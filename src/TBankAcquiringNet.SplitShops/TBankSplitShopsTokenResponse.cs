using System.Text.Json.Serialization;

namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Ответ OAuth-авторизации для API регистрации точек T-Bank Split
/// </summary>
public sealed record TBankSplitShopsTokenResponse
    : ITBankSplitShopsResponse<TBankSplitShopsTokenResponse>
{
    /// <summary>Access token для заголовка Authorization: Bearer.</summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    /// <summary>Тип токена.</summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    /// <summary>Refresh token.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>Время жизни access token в секундах.</summary>
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }

    /// <summary>OAuth scope.</summary>
    public string? Scope { get; init; }

    /// <summary>Идентификатор токена.</summary>
    public string? Jti { get; init; }

    /// <summary>
    /// Момент истечения токена, рассчитанный клиентом из <see cref="ExpiresIn"/>.
    /// </summary>
    /// <remarks>
    /// Само по себе <c>expires_in</c> — длительность без точки отсчёта, и кэшировать по ней нечего.
    /// Клиент отсчитывает срок от момента отправки запроса.
    /// </remarks>
    [JsonIgnore]
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>HTTP-метаданные ответа.</summary>
    [JsonIgnore]
    public TBankSplitShopsResponseMetadata? Metadata { get; init; }

    /// <summary>Токен в виде, который принимают методы клиента.</summary>
    public TBankSplitShopsAccessToken ToAccessToken() => new(AccessToken, ExpiresAt);

    TBankSplitShopsTokenResponse ITBankSplitShopsResponse<TBankSplitShopsTokenResponse>.WithMetadata(
        TBankSplitShopsResponseMetadata metadata) => this with { Metadata = metadata };
}
