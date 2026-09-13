using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using TBankNet.Multisplit;

namespace TBankNet.Multisplit.Tests;

public sealed class TBankMultisplitClientTests
{
    [Fact]
    public async Task GetAccessTokenAsync_PostsFormRequestWithBasicAuthorization()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""
            {
              "access_token": "access-token",
              "token_type": "bearer",
              "refresh_token": "refresh-token",
              "expires_in": 43199,
              "scope": "partner",
              "jti": "token-id"
            }
            """);
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.GetAccessTokenAsync();

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("bearer", response.TokenType);
        Assert.Equal(43199, response.ExpiresIn);
        Assert.Equal("token-id", response.Jti);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://example.test/oauth/token", request.RequestUri?.ToString());
        Assert.Equal("Basic", request.Authorization?.Scheme);
        Assert.Equal("cGFydG5lcjpwYXJ0bmVy", request.Authorization?.Parameter);
        Assert.Equal("application/x-www-form-urlencoded", request.ContentType?.MediaType);
        Assert.Equal("grant_type=password&username=login&password=password", request.Body);
    }

    [Fact]
    public async Task RegisterShopAsync_GetsTokenAndPostsJsonToRegisterEndpoint()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""
            {
              "code": "test_tochka",
              "shopCode": 111111111,
              "terminals": []
            }
            """);
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.RegisterShopAsync(CreateRegisterRequest(), TestAccessToken);

        Assert.Equal("test_tochka", response.Code);
        Assert.Equal("111111111", response.ShopCode);
        Assert.Empty(response.Terminals);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://example.test/sm-register/register", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Authorization?.Scheme);
        Assert.Equal("access-token", request.Authorization?.Parameter);
        Assert.Equal("application/json", request.ContentType?.MediaType);

        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;

        Assert.Equal("333@mail.ru", root.GetProperty("serviceProviderEmail").GetString());
        Assert.Equal("test_tochka", root.GetProperty("shopArticleId").GetString());
        Assert.Equal("test_tochka", root.GetProperty("billingDescriptor").GetString());
        Assert.Equal("ООО Компания", root.GetProperty("name").GetString());
        Assert.Equal(333333333333, root.GetProperty("ogrn").GetInt64());
        Assert.Equal("legal", root.GetProperty("addresses")[0].GetProperty("type").GetString());
        Assert.Equal("Иван", root.GetProperty("ceo").GetProperty("firstName").GetString());
        Assert.Equal("40702810838170023076", root.GetProperty("bankAccount").GetProperty("account").GetString());
        Assert.False(root.GetProperty("nonResident").GetBoolean());
    }

    [Fact]
    public async Task GetShopAsync_GetsTokenAndParsesShopInfo()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""
            {
              "merchantIds": [1000000000000, 1000000000001],
              "terminalIds": [7000000, 7111111],
              "terminalTypes": [0, 1],
              "name": "OOO Moya kompaniya",
              "inn": "1111111111",
              "kpp": "111000001",
              "email": "11@mail.ru",
              "bankAccount": {
                "account": "111111111111111111",
                "korAccount": "30101810400000000225",
                "bankName": "ПАО Сбербанк России",
                "bik": "044525225",
                "details": "Перевод средств",
                "userDefinedFees": [
                  {
                    "tax": { "percent": 1, "min": 0 },
                    "rule": { "operationType": 0 },
                    "isAFT": true,
                    "startDate": "2022-03-03 21:07:57"
                  }
                ],
                "disableReimbursement": false,
                "feeType": "DOWN"
              },
              "paymentSystemAttributes": [
                { "mcc": "6012", "mid": "200000001111111", "tid": "11111111" }
              ]
            }
            """);
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.GetShopAsync("111111111", TestAccessToken);

        Assert.Equal("OOO Moya kompaniya", response.Name);
        Assert.Equal([1000000000000, 1000000000001], response.MerchantIds);
        Assert.Equal([7000000, 7111111], response.TerminalIds);
        Assert.Equal([0, 1], response.TerminalTypes);
        Assert.Equal("044525225", response.BankAccount?.Bik);
        Assert.False(response.BankAccount?.DisableReimbursement);
        Assert.True(response.BankAccount?.UserDefinedFees[0].IsAft);
        Assert.Equal(0, response.BankAccount?.UserDefinedFees[0].Rule?.OperationType);
        Assert.Equal("6012", response.PaymentSystemAttributes[0].Mcc);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://example.test/sm-register/register/shop/111111111", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Authorization?.Scheme);
        Assert.Equal("access-token", request.Authorization?.Parameter);
        Assert.Null(request.Body);
    }

    [Fact]
    public async Task UpdateShopAsync_GetsTokenAndSendsPatchRequest()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""
            {
              "code": "test_tochka",
              "shopCode": "111111111",
              "terminals": []
            }
            """);
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.UpdateShopAsync("111111111", new TBankUpdateShopRequest
        {
            BankAccount = new TBankShopBankAccountUpdate
            {
                Account = "40702810838170023076",
                KorAccount = "30101810400000000225",
                BankName = "ПАО Сбербанк России",
                Bik = "044525225",
                Kbk = "18210501011011000110",
                Oktmo = "45286575000",
                Details = "Перевод средств",
                DisableReimbursement = true
            }
        }, TestAccessToken);

        Assert.Equal("111111111", response.ShopCode);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("https://example.test/sm-register/register/111111111", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Authorization?.Scheme);

        using var document = JsonDocument.Parse(request.Body!);
        var bankAccount = document.RootElement.GetProperty("bankAccount");

        Assert.Equal("40702810838170023076", bankAccount.GetProperty("account").GetString());
        Assert.Equal("18210501011011000110", bankAccount.GetProperty("kbk").GetString());
        Assert.Equal("45286575000", bankAccount.GetProperty("oktmo").GetString());
        Assert.True(bankAccount.GetProperty("disableReimbursement").GetBoolean());
    }

    [Fact]
    public async Task RegisterShopAsync_ThrowsApiExceptionForValidationErrorResponse()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""
            {
              "timestamp": "2018-07-25T13:23:18.160+0000",
              "status": 400,
              "error": "Bad Request",
              "errors": [
                {
                  "field": "billingDescriptor",
                  "defaultMessage": "не может быть пусто",
                  "rejectedValue": "",
                  "code": "NotEmpty"
                }
              ],
              "message": "Validation failed",
              "path": "/register"
            }
            """, HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var exception = await Assert.ThrowsAsync<TBankMultisplitApiException>(
            () => client.RegisterShopAsync(CreateRegisterRequest(), TestAccessToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.HttpStatusCode);
        Assert.Equal("Validation failed", exception.ErrorResponse?.Message);
        var error = Assert.Single(exception.ErrorResponse?.Errors ?? []);
        Assert.Equal("billingDescriptor", error.Field);
        Assert.Equal("NotEmpty", error.Code);
        Assert.Null(exception.Metadata.RawResponseBody);
    }

    [Fact]
    public async Task RegisterShopAsync_ValidatesKbkAndOktmoPairBeforeSending()
    {
        using var httpClient = new HttpClient(new QueueingHandler());
        var client = CreateClient(httpClient);
        var request = CreateRegisterRequest() with
        {
            BankAccount = new TBankShopBankAccount
            {
                Account = "40702810838170023076",
                BankName = "ПАО Сбербанк России",
                Bik = "044525225",
                Kbk = "18210501011011000110",
                Details = "Перевод средств"
            }
        };

        var exception = await Assert.ThrowsAsync<TBankMultisplitValidationException>(
            () => client.RegisterShopAsync(request, TestAccessToken));

        Assert.Equal("KBK and OKTMO must be provided together.", exception.Message);
    }

    [Fact]
    public async Task RegisterShopAsync_ValidatesBillingDescriptorLengthBeforeSending()
    {
        using var httpClient = new HttpClient(new QueueingHandler());
        var client = CreateClient(httpClient);
        var request = CreateRegisterRequest() with
        {
            BillingDescriptor = "megaprokat-test" // 15 chars, max is 14
        };

        var exception = await Assert.ThrowsAsync<TBankMultisplitValidationException>(
            () => client.RegisterShopAsync(request, TestAccessToken));

        Assert.Equal("Billing descriptor size must be between 1 and 14.", exception.Message);
    }

    [Fact]
    public async Task GetAccessTokenAsync_StampsExpiryFromExpiresIn()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""{"access_token":"access-token","token_type":"bearer","expires_in":43199}""");
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var before = DateTimeOffset.UtcNow;
        var response = await client.GetAccessTokenAsync();

        // expires_in on its own is a duration with no origin; the caller can only cache against an
        // absolute moment, counted from when the request went out.
        Assert.NotNull(response.ExpiresAt);
        Assert.InRange(
            response.ExpiresAt!.Value,
            before.AddSeconds(43199),
            DateTimeOffset.UtcNow.AddSeconds(43199));

        Assert.False(response.ToAccessToken().IsExpired());
    }

    [Fact]
    public async Task GetAccessTokenAsync_LeavesExpiryUnsetWhenBankOmitsIt()
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""{"access_token":"access-token","token_type":"bearer"}""");
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.GetAccessTokenAsync();

        Assert.Null(response.ExpiresAt);
        // Nothing was said about the lifetime, so discarding a working token on a guess would be worse.
        Assert.False(response.ToAccessToken().IsExpired());
    }

    [Fact]
    public void AccessToken_IsExpired_RespectsLeeway()
    {
        var token = new TBankMultisplitAccessToken("access-token", DateTimeOffset.UtcNow.AddSeconds(30));

        Assert.False(token.IsExpired(TimeSpan.FromSeconds(5)));
        Assert.True(token.IsExpired(TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void AccessToken_ToString_DoesNotLeakTheValue()
    {
        var token = new TBankMultisplitAccessToken("super-secret");

        Assert.DoesNotContain("super-secret", token.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegisterShopAsync_RejectsAnEmptyToken()
    {
        using var handler = new QueueingHandler();
        using var httpClient = new HttpClient(handler);
        var client = CreateClient(httpClient);

        var exception = await Assert.ThrowsAsync<TBankMultisplitValidationException>(
            () => client.RegisterShopAsync(CreateRegisterRequest(), default));

        Assert.Contains("GetAccessTokenAsync", exception.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void Options_RejectBothEnvironmentAndBaseAddress()
    {
        using var httpClient = new HttpClient();

        // Both used to be accepted, with BaseAddress silently winning — so Environment = Test beside a
        // production BaseAddress looked configured and talked to production.
        var exception = Assert.Throws<ArgumentException>(() => new TBankMultisplitClient(
            httpClient,
            new TBankMultisplitClientOptions
            {
                Username = "login",
                Password = "password",
                Environment = TBankMultisplitEnvironment.Test,
                BaseAddress = new Uri("https://acqapi.tinkoff.ru/")
            }));

        Assert.Contains("not both", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Options_RejectNeitherEnvironmentNorBaseAddress()
    {
        using var httpClient = new HttpClient();

        // Production is never assumed: the old default silently registered real shops.
        Assert.Throws<ArgumentException>(() => new TBankMultisplitClient(
            httpClient,
            new TBankMultisplitClientOptions { Username = "login", Password = "password" }));
    }

    [Theory]
    [InlineData(TBankMultisplitEnvironment.Test, "https://acqapi-test.tinkoff.ru/oauth/token")]
    [InlineData(TBankMultisplitEnvironment.Production, "https://acqapi.tinkoff.ru/oauth/token")]
    public async Task Options_ResolveEnvironmentToItsHost(TBankMultisplitEnvironment environment, string expected)
    {
        using var handler = new QueueingHandler();
        handler.Enqueue("""{"access_token":"access-token","token_type":"bearer"}""");
        using var httpClient = new HttpClient(handler);
        var client = new TBankMultisplitClient(httpClient, new TBankMultisplitClientOptions
        {
            Username = "login",
            Password = "password",
            Environment = environment
        });

        await client.GetAccessTokenAsync();

        Assert.Equal(expected, Assert.Single(handler.Requests).RequestUri?.ToString());
    }

    private static readonly TBankMultisplitAccessToken TestAccessToken = new("access-token");

    private static TBankMultisplitClient CreateClient(HttpClient httpClient)
    {
        return new TBankMultisplitClient(httpClient, new TBankMultisplitClientOptions
        {
            Username = "login",
            Password = "password",
            BaseAddress = new Uri("https://example.test/")
        });
    }

    private static TBankRegisterShopRequest CreateRegisterRequest()
    {
        return new TBankRegisterShopRequest
        {
            ServiceProviderEmail = "333@mail.ru",
            ShopArticleId = "test_tochka",
            BillingDescriptor = "test_tochka",
            FullName = "Общество с ограниченной ответственностью Компания",
            Name = "ООО Компания",
            Inn = "3333333333",
            Okved = "64.92.7",
            Ogrn = 333333333333,
            RegDepartment = "ФНС N1 по г. Москве",
            RegDate = "2003-03-03",
            Addresses =
            [
                new TBankShopAddress
                {
                    Type = "legal",
                    Zip = "108809",
                    Country = "RUS",
                    City = "Москва",
                    Street = "Маяковского, 3",
                    Description = "Юридический адрес"
                }
            ],
            Phones =
            [
                new TBankShopPhone
                {
                    Type = "common",
                    Phone = "+7(495)333-3333",
                    Description = "основной"
                }
            ],
            Email = "333@mail.ru",
            Assets = "3000000",
            Founders = new TBankShopFounders
            {
                Individuals =
                [
                    new TBankShopIndividualFounder
                    {
                        FirstName = "Семен",
                        LastName = "Семенов",
                        MiddleName = "Семенович",
                        BirthDate = "1970-02-02",
                        BirthPlace = "Рязань",
                        Citizenship = "Россия",
                        DocType = "Паспорт",
                        DocNumber = "2222 222222",
                        IssueDate = "2009-07-21",
                        IssuedBy = "Отделом УФМС России",
                        Address = "214031, г. Рязань, ул. Ленина, д. 1"
                    }
                ]
            },
            Ceo = new TBankShopCeo
            {
                Address = "108809, г. Москва, Маяковского, 3",
                Phone = "+79853333333",
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                BirthDate = "1980-03-03",
                BirthPlace = "Москва",
                DocType = "Паспорт",
                DocNumber = "333 333333",
                IssueDate = "2020-09-16",
                IssuedBy = "УМВД России",
                Country = "RUS"
            },
            SiteUrl = "https://example.test/",
            PrimaryActivities = "Торговля",
            BankAccount = new TBankShopBankAccount
            {
                Account = "40702810838170023076",
                KorAccount = "30101810400000000225",
                BankName = "ПАО Сбербанк России",
                Bik = "044525225",
                Kbk = "18210501011011000110",
                Oktmo = "45286575000",
                Details = "Перевод средств"
            },
            Comment = "Комментарий",
            NonResident = false
        };
    }

    private sealed class QueueingHandler : HttpMessageHandler
    {
        private readonly Queue<ResponseSpec> responses = new();

        public List<RequestRecord> Requests { get; } = [];

        public void Enqueue(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            responses.Enqueue(new ResponseSpec(responseBody, statusCode));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!responses.TryDequeue(out var responseSpec))
            {
                throw new InvalidOperationException("No queued HTTP response.");
            }

            Requests.Add(new RequestRecord(
                request.Method,
                request.RequestUri,
                request.Headers.Authorization,
                request.Content?.Headers.ContentType,
                request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));

            return new HttpResponseMessage(responseSpec.StatusCode)
            {
                Content = new StringContent(responseSpec.Body)
            };
        }
    }

    private sealed record ResponseSpec(string Body, HttpStatusCode StatusCode);

    private sealed record RequestRecord(
        HttpMethod Method,
        Uri? RequestUri,
        AuthenticationHeaderValue? Authorization,
        MediaTypeHeaderValue? ContentType,
        string? Body);
}
