using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TBankNet.Multisplit;

/// <summary>
/// Клиент регистрации и обновления точек T-Bank Multisplit
/// </summary>
public sealed class TBankMultisplitClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient httpClient;
    private readonly TBankMultisplitClientOptions options;
    private readonly Uri baseAddress;

    /// <summary>
    /// Создает клиент API регистрации точек T-Bank Multisplit
    /// </summary>
    public TBankMultisplitClient(HttpClient httpClient, TBankMultisplitClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Username))
        {
            throw new ArgumentException("OAuth username must be configured.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new ArgumentException("OAuth password must be configured.", nameof(options));
        }

        this.httpClient = httpClient;
        this.options = options;

        // Resolved once, at construction: a bad host is a configuration mistake and should surface
        // when the client is built, not on the first call that happens to need it.
        baseAddress = options.ResolveBaseAddress();
    }

    /// <summary>
    /// Выпускает OAuth access_token для методов регистрации и обновления точек.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Единственное место, где SDK обращается к <c>oauth/token</c>. Остальные методы токен только
    /// принимают: хранение, продление и разделение между запросами — дело вызывающей стороны.
    /// </para>
    /// <para>
    /// Раньше каждый вызов начинался со скрытого получения токена, то есть стоил два обращения к
    /// банку вместо одного, а <c>expires_in</c> разбирался и не использовался.
    /// </para>
    /// </remarks>
    public async Task<TBankMultisplitTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Отсчёт от отправки, а не от разбора ответа: так оценка срока годности заведомо не длиннее
        // настоящей.
        var issuedAt = DateTimeOffset.UtcNow;

        var endpoint = new Uri(baseAddress, "oauth/token");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", options.Username),
                new KeyValuePair<string, string>("password", options.Password)
            ])
        };

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes("partner:partner")));

        var token = await SendAsync<TBankMultisplitTokenResponse>("oauth/token", request, cancellationToken)
            .ConfigureAwait(false);

        return token.ExpiresIn is { } expiresInSeconds
            ? token with { ExpiresAt = issuedAt.AddSeconds(expiresInSeconds) }
            : token;
    }

    /// <summary>
    /// Регистрирует точку партнера.
    /// </summary>
    /// <param name="request">Данные регистрируемой точки.</param>
    /// <param name="accessToken">
    /// Токен, выпущенный <see cref="GetAccessTokenAsync"/>. Передаётся явно, чтобы вызывающая сторона
    /// могла переиспользовать его между вызовами.
    /// </param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<TBankShopMutationResponse> RegisterShopAsync(
        TBankRegisterShopRequest request,
        TBankMultisplitAccessToken accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAccessToken(accessToken);
        TBankMultisplitRequestValidator.Validate(request);

        using var httpRequest = CreateJsonRequest(HttpMethod.Post, "sm-register/register", request, accessToken);

        return await SendAsync<TBankShopMutationResponse>("sm-register/register", httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Получает информацию по точке партнера.
    /// </summary>
    /// <param name="shopCode">Код точки, выданный банком при регистрации.</param>
    /// <param name="accessToken">Токен, выпущенный <see cref="GetAccessTokenAsync"/>.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<TBankShopInfoResponse> GetShopAsync(
        string shopCode,
        TBankMultisplitAccessToken accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopCode))
        {
            throw new TBankMultisplitValidationException("Shop code must be provided.");
        }

        EnsureAccessToken(accessToken);

        var path = $"sm-register/register/shop/{Uri.EscapeDataString(shopCode)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress, path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

        return await SendAsync<TBankShopInfoResponse>(path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Обновляет информацию о точке партнера.
    /// </summary>
    /// <param name="shopCode">Код точки, выданный банком при регистрации.</param>
    /// <param name="request">Обновляемые данные точки.</param>
    /// <param name="accessToken">Токен, выпущенный <see cref="GetAccessTokenAsync"/>.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<TBankShopMutationResponse> UpdateShopAsync(
        string shopCode,
        TBankUpdateShopRequest request,
        TBankMultisplitAccessToken accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopCode))
        {
            throw new TBankMultisplitValidationException("Shop code must be provided.");
        }

        ArgumentNullException.ThrowIfNull(request);
        EnsureAccessToken(accessToken);
        TBankMultisplitRequestValidator.Validate(request);

        var path = $"sm-register/register/{Uri.EscapeDataString(shopCode)}";
        using var httpRequest = CreateJsonRequest(HttpMethod.Patch, path, request, accessToken);

        return await SendAsync<TBankShopMutationResponse>(path, httpRequest, cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage CreateJsonRequest<TRequest>(
        HttpMethod method,
        string path,
        TRequest body,
        TBankMultisplitAccessToken accessToken)
    {
        var request = new HttpRequestMessage(method, new Uri(baseAddress, path))
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

        return request;
    }

    private static void EnsureAccessToken(TBankMultisplitAccessToken accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken.Value))
        {
            throw new TBankMultisplitValidationException(
                "An access token must be provided. Issue one with GetAccessTokenAsync.");
        }
    }

    private async Task<TResponse> SendAsync<TResponse>(
        string operation,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        where TResponse : ITBankMultisplitResponse<TResponse>
    {
        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new TBankMultisplitTransportException(
                $"T-Bank multisplit shops {operation} request failed before a response was received.",
                exception);
        }

        using (response)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var metadata = CreateResponseMetadata(response, responseBody, options.CaptureRawResponseBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = DeserializeError(operation, response.StatusCode, responseBody);
                throw new TBankMultisplitApiException(
                    $"T-Bank multisplit shops {operation} returned HTTP {(int)response.StatusCode} ({response.StatusCode}).",
                    response.StatusCode,
                    errorResponse,
                    metadata);
            }

            var result = DeserializeResponse<TResponse>(operation, response.StatusCode, responseBody);
            return result.WithMetadata(metadata);
        }
    }

    private static TResponse DeserializeResponse<TResponse>(
        string operation,
        HttpStatusCode statusCode,
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new TBankMultisplitProtocolException(
                $"T-Bank multisplit shops {operation} response body was empty. HTTP {(int)statusCode} ({statusCode}).",
                statusCode);
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseBody, JsonOptions)
                ?? throw new TBankMultisplitProtocolException(
                    $"T-Bank multisplit shops {operation} response body was empty after deserialization.",
                    statusCode,
                    CreateBodyPreview(responseBody));
        }
        catch (JsonException exception)
        {
            var responseBodyPreview = CreateBodyPreview(responseBody);
            throw new TBankMultisplitProtocolException(
                $"T-Bank multisplit shops {operation} response body was not valid JSON for the expected response model. HTTP {(int)statusCode} ({statusCode}). Response preview: {responseBodyPreview}",
                statusCode,
                responseBodyPreview,
                exception);
        }
    }

    private static TBankMultisplitErrorResponse? DeserializeError(
        string operation,
        HttpStatusCode statusCode,
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TBankMultisplitErrorResponse>(responseBody, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new TBankMultisplitProtocolException(
                $"T-Bank multisplit shops {operation} error response body was not valid JSON. HTTP {(int)statusCode} ({statusCode}). Response preview: {CreateBodyPreview(responseBody)}",
                statusCode,
                CreateBodyPreview(responseBody),
                exception);
        }
    }

    private static TBankMultisplitResponseMetadata CreateResponseMetadata(
        HttpResponseMessage response,
        string responseBody,
        bool captureRawResponseBody)
    {
        var headers = response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(static header => header.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => (IReadOnlyList<string>)group.SelectMany(static header => header.Value).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return new TBankMultisplitResponseMetadata(
            response.StatusCode,
            headers,
            captureRawResponseBody ? responseBody : null);
    }

    private static string CreateBodyPreview(string responseBody)
    {
        var preview = RedactSensitiveFields(responseBody);
        const int maxLength = 512;

        return preview.Length <= maxLength ? preview : preview[..maxLength];
    }

    private static string RedactSensitiveFields(string value)
    {
        return Regex.Replace(
            value,
            "(\"(?:access_token|refresh_token|password|Password|token|Token)\"\\s*:\\s*\")([^\"]*)(\")",
            "$1***REDACTED***$3",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        jsonOptions.Converters.Add(new TBankMultisplitStringJsonConverter());

        return jsonOptions;
    }
}
