using System.Net;

namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Базовое исключение SDK для регистрации точек T-Bank Split
/// </summary>
public abstract class TBankSplitShopsException : Exception
{
    /// <summary>Создает исключение SDK.</summary>
    protected TBankSplitShopsException(string message)
        : base(message)
    {
    }

    /// <summary>Создает исключение SDK с внутренней причиной.</summary>
    protected TBankSplitShopsException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка транспорта: запрос не получил корректный HTTP-ответ.
/// </summary>
public sealed class TBankSplitShopsTransportException : TBankSplitShopsException
{
    /// <summary>Создает исключение транспортного уровня.</summary>
    public TBankSplitShopsTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка протокола: ответ получен, но его невозможно безопасно разобрать.
/// </summary>
public sealed class TBankSplitShopsProtocolException : TBankSplitShopsException
{
    /// <summary>Создает исключение протокола T-Bank.</summary>
    public TBankSplitShopsProtocolException(
        string message,
        HttpStatusCode? httpStatusCode = null,
        string? responseBodyPreview = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
        ResponseBodyPreview = responseBodyPreview;
    }

    /// <summary>HTTP-статус ответа, если он был получен.</summary>
    public HttpStatusCode? HttpStatusCode { get; }

    /// <summary>Короткий отредактированный фрагмент тела ответа для диагностики.</summary>
    public string? ResponseBodyPreview { get; }
}

/// <summary>
/// Ошибка локальной валидации запроса до отправки в T-Bank.
/// </summary>
public sealed class TBankSplitShopsValidationException : TBankSplitShopsException
{
    /// <summary>Создает исключение локальной валидации.</summary>
    public TBankSplitShopsValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Ошибка API регистрации точек T-Bank Split
/// </summary>
public sealed class TBankSplitShopsApiException : TBankSplitShopsException
{
    /// <summary>Создает исключение API T-Bank.</summary>
    public TBankSplitShopsApiException(
        string message,
        HttpStatusCode httpStatusCode,
        TBankSplitShopsErrorResponse? errorResponse,
        TBankSplitShopsResponseMetadata metadata)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        ErrorResponse = errorResponse;
        Metadata = metadata;
    }

    /// <summary>HTTP-статус ответа.</summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>Типизированное тело ошибки, если его удалось разобрать.</summary>
    public TBankSplitShopsErrorResponse? ErrorResponse { get; }

    /// <summary>HTTP-метаданные ответа.</summary>
    public TBankSplitShopsResponseMetadata Metadata { get; }
}
