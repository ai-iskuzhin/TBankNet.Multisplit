using System.Net;

namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// HTTP-метаданные ответа API регистрации точек T-Bank Split
/// </summary>
public sealed record TBankSplitShopsResponseMetadata(
    HttpStatusCode HttpStatusCode,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Headers,
    string? RawResponseBody);
