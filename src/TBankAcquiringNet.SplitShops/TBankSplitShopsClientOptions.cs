namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Настройки клиента регистрации точек T-Bank Multisplit
/// </summary>
public sealed class TBankSplitShopsClientOptions
{
    /// <summary>Логин партнера, выданный банком для OAuth-запроса.</summary>
    public required string Username { get; init; }

    /// <summary>Пароль партнера, выданный банком для OAuth-запроса.</summary>
    public required string Password { get; init; }

    /// <summary>
    /// Среда API. Задаётся вместо <see cref="BaseAddress"/>, но не вместе с ним.
    /// </summary>
    /// <remarks>
    /// Без значения по умолчанию намеренно: умолчание <c>Production</c> означало, что забытая
    /// настройка молча регистрирует настоящие точки в боевом контуре.
    /// </remarks>
    public TBankSplitShopsEnvironment? Environment { get; init; }

    /// <summary>
    /// Явный базовый URL API. Задаётся вместо <see cref="Environment"/>, но не вместе с ним.
    /// </summary>
    public Uri? BaseAddress { get; init; }

    /// <summary>Сохранять сырое тело ответа в Metadata.RawResponseBody.</summary>
    public bool CaptureRawResponseBody { get; init; }

    /// <summary>
    /// Разрешает базовый адрес ровно из одного источника.
    /// </summary>
    /// <remarks>
    /// Раньше можно было задать и среду, и адрес, и адрес молча побеждал: настройка
    /// <c>Environment = Test</c> рядом с боевым <c>BaseAddress</c> не делала ничего и выглядела
    /// работающей. Противоречие теперь отвергается, как и отсутствие выбора.
    /// </remarks>
    /// <exception cref="ArgumentException">Заданы оба источника или не задан ни один.</exception>
    internal Uri ResolveBaseAddress()
    {
        if (BaseAddress is not null && Environment is not null)
        {
            throw new ArgumentException(
                "Set either BaseAddress or Environment, not both: they name the same thing and only one can win.",
                nameof(BaseAddress));
        }

        if (BaseAddress is not null)
        {
            return BaseAddress;
        }

        return Environment switch
        {
            TBankSplitShopsEnvironment.Test => new Uri("https://acqapi-test.tinkoff.ru/"),
            TBankSplitShopsEnvironment.Production => new Uri("https://acqapi.tinkoff.ru/"),
            null => throw new ArgumentException(
                "Set either Environment or BaseAddress: the API host is never assumed.",
                nameof(Environment)),
            _ => throw new ArgumentException(
                $"Unsupported T-Bank multisplit shops environment: {Environment}.",
                nameof(Environment))
        };
    }
}
