using System.Net;

namespace TBankNet.Multisplit;

/// <summary>
/// Базовое исключение SDK для регистрации точек T-Bank Multisplit
/// </summary>
public abstract class TBankMultisplitException : Exception
{
    /// <summary>Создает исключение SDK.</summary>
    protected TBankMultisplitException(string message)
        : base(message)
    {
    }

    /// <summary>Создает исключение SDK с внутренней причиной.</summary>
    protected TBankMultisplitException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка транспорта: запрос не получил корректный HTTP-ответ.
/// </summary>
public sealed class TBankMultisplitTransportException : TBankMultisplitException
{
    /// <summary>Создает исключение транспортного уровня.</summary>
    public TBankMultisplitTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка протокола: ответ получен, но его невозможно безопасно разобрать.
/// </summary>
public sealed class TBankMultisplitProtocolException : TBankMultisplitException
{
    /// <summary>Создает исключение протокола T-Bank.</summary>
    public TBankMultisplitProtocolException(
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
public sealed class TBankMultisplitValidationException : TBankMultisplitException
{
    /// <summary>Создает исключение локальной валидации.</summary>
    public TBankMultisplitValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Ошибка API регистрации точек T-Bank Multisplit
/// </summary>
public sealed class TBankMultisplitApiException : TBankMultisplitException
{
    /// <summary>Создает исключение API T-Bank.</summary>
    public TBankMultisplitApiException(
        string message,
        HttpStatusCode httpStatusCode,
        TBankMultisplitErrorResponse? errorResponse,
        TBankMultisplitResponseMetadata metadata)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        ErrorResponse = errorResponse;
        Metadata = metadata;
    }

    /// <summary>HTTP-статус ответа.</summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>Типизированное тело ошибки, если его удалось разобрать.</summary>
    public TBankMultisplitErrorResponse? ErrorResponse { get; }

    /// <summary>HTTP-метаданные ответа.</summary>
    public TBankMultisplitResponseMetadata Metadata { get; }
}
