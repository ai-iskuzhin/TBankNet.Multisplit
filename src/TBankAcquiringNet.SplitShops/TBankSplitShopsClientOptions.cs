namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Настройки клиента регистрации точек T-Bank Split
/// </summary>
public sealed class TBankSplitShopsClientOptions
{
    /// <summary>Логин партнера, выданный банком для OAuth-запроса.</summary>
    public required string Username { get; init; }

    /// <summary>Пароль партнера, выданный банком для OAuth-запроса.</summary>
    public required string Password { get; init; }

    /// <summary>Среда API, используемая при отсутствии BaseAddress.</summary>
    public TBankSplitShopsEnvironment Environment { get; init; } = TBankSplitShopsEnvironment.Production;

    /// <summary>Явный базовый URL API, если нужно переопределить среду.</summary>
    public Uri? BaseAddress { get; init; }

    /// <summary>Сохранять сырое тело ответа в Metadata.RawResponseBody.</summary>
    public bool CaptureRawResponseBody { get; init; }

    internal Uri ResolveBaseAddress()
    {
        if (BaseAddress is not null)
        {
            return BaseAddress;
        }

        return Environment switch
        {
            TBankSplitShopsEnvironment.Test => new Uri("https://acqapi-test.tinkoff.ru/"),
            TBankSplitShopsEnvironment.Production => new Uri("https://acqapi.tinkoff.ru/"),
            _ => throw new InvalidOperationException($"Unsupported T-Bank multisplit shops environment: {Environment}.")
        };
    }
}
