namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Ответ, к которому клиент прикрепляет HTTP-метаданные.
/// </summary>
/// <remarks>
/// Самоссылающийся параметр <typeparamref name="TSelf"/> позволяет вернуть точный тип без приведения,
/// а ограничение на <c>SendAsync</c> делает реализацию обязательной: новый тип ответа либо участвует
/// в контракте, либо не компилируется. Прежний вариант — <c>switch</c> по конкретным типам с
/// непроверяемым приведением <c>(TResponse)(object)</c> — терял метаданные молча.
/// </remarks>
/// <typeparam name="TSelf">Тип, реализующий интерфейс.</typeparam>
internal interface ITBankSplitShopsResponse<out TSelf>
    where TSelf : ITBankSplitShopsResponse<TSelf>
{
    /// <summary>Возвращает копию ответа с прикреплёнными метаданными.</summary>
    TSelf WithMetadata(TBankSplitShopsResponseMetadata metadata);
}
