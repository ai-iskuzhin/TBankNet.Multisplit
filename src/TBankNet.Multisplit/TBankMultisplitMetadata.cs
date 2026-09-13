using System.Net;

namespace TBankNet.Multisplit;

/// <summary>
/// HTTP-метаданные ответа API регистрации точек T-Bank Multisplit
/// </summary>
public sealed record TBankMultisplitResponseMetadata(
    HttpStatusCode HttpStatusCode,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Headers,
    string? RawResponseBody);
