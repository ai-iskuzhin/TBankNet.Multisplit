using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Клиент регистрации и обновления точек T-Bank Split
/// </summary>
public sealed class TBankSplitShopsClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient httpClient;
    private readonly TBankSplitShopsClientOptions options;

    /// <summary>
    /// Создает клиент API регистрации точек T-Bank Split
    /// </summary>
    public TBankSplitShopsClient(HttpClient httpClient, TBankSplitShopsClientOptions options)
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
    }

    /// <summary>
    /// Получает OAuth access_token для методов регистрации и обновления точек.
    /// </summary>
    public async Task<TBankSplitShopsTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = new Uri(options.ResolveBaseAddress(), "oauth/token");
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

        return await SendAsync<TBankSplitShopsTokenResponse>("oauth/token", request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Регистрирует точку партнера.
    /// </summary>
    public async Task<TBankShopMutationResponse> RegisterShopAsync(
        TBankRegisterShopRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        TBankSplitShopsRequestValidator.Validate(request);

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var httpRequest = CreateJsonRequest(HttpMethod.Post, "sm-register/register", request, token.AccessToken);

        return await SendAsync<TBankShopMutationResponse>("sm-register/register", httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Получает информацию по точке партнера.
    /// </summary>
    public async Task<TBankShopInfoResponse> GetShopAsync(
        string shopCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopCode))
        {
            throw new TBankSplitShopsValidationException("Shop code must be provided.");
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var path = $"sm-register/register/shop/{Uri.EscapeDataString(shopCode)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(options.ResolveBaseAddress(), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return await SendAsync<TBankShopInfoResponse>(path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Обновляет информацию о точке партнера.
    /// </summary>
    public async Task<TBankShopMutationResponse> UpdateShopAsync(
        string shopCode,
        TBankUpdateShopRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopCode))
        {
            throw new TBankSplitShopsValidationException("Shop code must be provided.");
        }

        ArgumentNullException.ThrowIfNull(request);
        TBankSplitShopsRequestValidator.Validate(request);

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var path = $"sm-register/register/{Uri.EscapeDataString(shopCode)}";
        using var httpRequest = CreateJsonRequest(HttpMethod.Patch, path, request, token.AccessToken);

        return await SendAsync<TBankShopMutationResponse>(path, httpRequest, cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage CreateJsonRequest<TRequest>(
        HttpMethod method,
        string path,
        TRequest body,
        string accessToken)
    {
        var request = new HttpRequestMessage(method, new Uri(options.ResolveBaseAddress(), path))
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }

    private async Task<TResponse> SendAsync<TResponse>(
        string operation,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new TBankSplitShopsTransportException(
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
                throw new TBankSplitShopsApiException(
                    $"T-Bank multisplit shops {operation} returned HTTP {(int)response.StatusCode} ({response.StatusCode}).",
                    response.StatusCode,
                    errorResponse,
                    metadata);
            }

            var result = DeserializeResponse<TResponse>(operation, response.StatusCode, responseBody);
            return AttachMetadata(result, metadata);
        }
    }

    private static TResponse AttachMetadata<TResponse>(
        TResponse response,
        TBankSplitShopsResponseMetadata metadata)
    {
        return response switch
        {
            TBankSplitShopsTokenResponse token => (TResponse)(object)(token with { Metadata = metadata }),
            TBankShopMutationResponse mutation => (TResponse)(object)(mutation with { Metadata = metadata }),
            TBankShopInfoResponse info => (TResponse)(object)(info with { Metadata = metadata }),
            _ => response
        };
    }

    private static TResponse DeserializeResponse<TResponse>(
        string operation,
        HttpStatusCode statusCode,
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new TBankSplitShopsProtocolException(
                $"T-Bank multisplit shops {operation} response body was empty. HTTP {(int)statusCode} ({statusCode}).",
                statusCode);
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseBody, JsonOptions)
                ?? throw new TBankSplitShopsProtocolException(
                    $"T-Bank multisplit shops {operation} response body was empty after deserialization.",
                    statusCode,
                    CreateBodyPreview(responseBody));
        }
        catch (JsonException exception)
        {
            var responseBodyPreview = CreateBodyPreview(responseBody);
            throw new TBankSplitShopsProtocolException(
                $"T-Bank multisplit shops {operation} response body was not valid JSON for the expected response model. HTTP {(int)statusCode} ({statusCode}). Response preview: {responseBodyPreview}",
                statusCode,
                responseBodyPreview,
                exception);
        }
    }

    private static TBankSplitShopsErrorResponse? DeserializeError(
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
            return JsonSerializer.Deserialize<TBankSplitShopsErrorResponse>(responseBody, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new TBankSplitShopsProtocolException(
                $"T-Bank multisplit shops {operation} error response body was not valid JSON. HTTP {(int)statusCode} ({statusCode}). Response preview: {CreateBodyPreview(responseBody)}",
                statusCode,
                CreateBodyPreview(responseBody),
                exception);
        }
    }

    private static TBankSplitShopsResponseMetadata CreateResponseMetadata(
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

        return new TBankSplitShopsResponseMetadata(
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

        jsonOptions.Converters.Add(new TBankSplitShopStringJsonConverter());

        return jsonOptions;
    }
}
