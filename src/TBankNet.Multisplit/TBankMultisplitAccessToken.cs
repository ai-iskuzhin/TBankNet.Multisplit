namespace TBankNet.Multisplit;

/// <summary>
/// OAuth access token для вызовов API регистрации точек.
/// </summary>
/// <remarks>
/// <para>
/// Отдельный тип, а не <see cref="string"/>: методы принимают рядом код точки и токен, и перепутать
/// местами два строковых параметра слишком легко — цена ошибки здесь равна утечке токена в путь URL.
/// </para>
/// <para>
/// Жизненным циклом токена SDK не управляет. Он выдаёт токен
/// (<see cref="TBankMultisplitClient.GetAccessTokenAsync"/>) и сообщает срок годности; кэширование,
/// продление и разделение между запросами остаются за вызывающей стороной, которая одна знает,
/// сколько процессов и сколько запросов делят одни учётные данные.
/// </para>
/// </remarks>
/// <param name="Value">Значение токена для заголовка <c>Authorization: Bearer</c>.</param>
/// <param name="ExpiresAt">
/// Момент истечения, если банк его сообщил. Считается от времени отправки запроса, а не ответа,
/// поэтому оценка заведомо консервативна.
/// </param>
public readonly record struct TBankMultisplitAccessToken(string Value, DateTimeOffset? ExpiresAt = null)
{
    /// <summary>
    /// Истёк ли токен с учётом запаса на дорогу до банка.
    /// </summary>
    /// <remarks>
    /// Без срока годности (<see cref="ExpiresAt"/> равен <c>null</c>) считается действующим: банк не
    /// сообщил ничего, и выбрасывать рабочий токен по догадке хуже, чем получить 401 и обновить его.
    /// </remarks>
    /// <param name="leeway">Запас, по умолчанию минута.</param>
    public bool IsExpired(TimeSpan? leeway = null)
    {
        // Токена нет вовсе — не тот случай, когда «срок не сообщили». default(T) даёт пустое
        // значение и пустой срок, и раньше такой токен отвечал «не истёк»: вызывающая сторона
        // пропускала выпуск и отправляла пустой bearer.
        if (!HasValue)
        {
            return true;
        }

        if (ExpiresAt is not { } expiresAt)
        {
            return false;
        }

        return DateTimeOffset.UtcNow + (leeway ?? TimeSpan.FromMinutes(1)) >= expiresAt;
    }

    /// <summary>Есть ли вообще значение, которое можно отправить.</summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value);

    /// <inheritdoc />
    public override string ToString() => "TBankMultisplitAccessToken(***)";
}
