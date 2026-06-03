using System.Text.Json.Serialization;

namespace TBankAcquiringNet.SplitShops;

/// <summary>
/// Запрос регистрации точки партнера в T-Bank Split
/// </summary>
public sealed record TBankRegisterShopRequest
{
    /// <summary>Email магазина.</summary>
    public string? ServiceProviderEmail { get; init; }

    /// <summary>MCC-код торговой группы.</summary>
    public int? Mcc { get; init; }

    /// <summary>Код точки на стороне магазина Мультирасчетов.</summary>
    public string? ShopArticleId { get; init; }

    /// <summary>Название магазина в СМС и на странице проверки 3DS на иностранном языке.</summary>
    public required string BillingDescriptor { get; init; }

    /// <summary>Полное наименование организации.</summary>
    public required string FullName { get; init; }

    /// <summary>Сокращенное наименование организации.</summary>
    public required string Name { get; init; }

    /// <summary>ИНН.</summary>
    public required string Inn { get; init; }

    /// <summary>КПП.</summary>
    public string? Kpp { get; init; }

    /// <summary>ОКВЭД.</summary>
    public string? Okved { get; init; }

    /// <summary>Основной регистрационный номер.</summary>
    public required long Ogrn { get; init; }

    /// <summary>Орган государственной регистрации.</summary>
    public string? RegDepartment { get; init; }

    /// <summary>Дата присвоения ОГРН.</summary>
    public string? RegDate { get; init; }

    /// <summary>Адреса организации.</summary>
    public required IReadOnlyList<TBankShopAddress> Addresses { get; init; }

    /// <summary>Телефоны организации.</summary>
    public IReadOnlyList<TBankShopPhone>? Phones { get; init; }

    /// <summary>Email партнера.</summary>
    public required string Email { get; init; }

    /// <summary>Сведения о величине зарегистрированного и оплаченного капитала.</summary>
    public string? Assets { get; init; }

    /// <summary>Сведения об учредителях.</summary>
    public TBankShopFounders? Founders { get; init; }

    /// <summary>Сведения о руководителе.</summary>
    public required TBankShopCeo Ceo { get; init; }

    /// <summary>Лицензии.</summary>
    public IReadOnlyList<TBankShopLicense>? Licenses { get; init; }

    /// <summary>Адрес интернет сайта.</summary>
    public required string SiteUrl { get; init; }

    /// <summary>Основные виды деятельности.</summary>
    public string? PrimaryActivities { get; init; }

    /// <summary>Реквизиты партнера магазина Мультирасчетов для перечисления возмещения.</summary>
    public required TBankShopBankAccount BankAccount { get; init; }

    /// <summary>Комментарий.</summary>
    public string? Comment { get; init; }

    /// <summary>Признак нерезидента.</summary>
    public bool? NonResident { get; init; }
}

/// <summary>
/// Запрос обновления информации о точке партнера.
/// </summary>
public sealed record TBankUpdateShopRequest
{
    /// <summary>Реквизиты партнера магазина Мультирасчетов для перечисления возмещения.</summary>
    public TBankShopBankAccountUpdate? BankAccount { get; init; }
}

/// <summary>
/// Адрес организации.
/// </summary>
public sealed record TBankShopAddress
{
    /// <summary>Тип адреса: legal, actual, post или other.</summary>
    public required string Type { get; init; }

    /// <summary>Почтовый индекс.</summary>
    public required string Zip { get; init; }

    /// <summary>Трехбуквенный код страны по ISO.</summary>
    public required string Country { get; init; }

    /// <summary>Город или населенный пункт.</summary>
    public required string City { get; init; }

    /// <summary>Улица, дом.</summary>
    public required string Street { get; init; }

    /// <summary>Дополнительное описание.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Телефон организации.
/// </summary>
public sealed record TBankShopPhone
{
    /// <summary>Тип телефона: common, fax или other.</summary>
    public string? Type { get; init; }

    /// <summary>Телефон.</summary>
    public string? Phone { get; init; }

    /// <summary>Дополнительное описание.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Сведения об учредителях.
/// </summary>
public sealed record TBankShopFounders
{
    /// <summary>Физические лица.</summary>
    public required IReadOnlyList<TBankShopIndividualFounder> Individuals { get; init; }
}

/// <summary>
/// Физическое лицо среди учредителей.
/// </summary>
public sealed record TBankShopIndividualFounder
{
    /// <summary>Имя.</summary>
    public required string FirstName { get; init; }

    /// <summary>Фамилия.</summary>
    public required string LastName { get; init; }

    /// <summary>Отчество.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Дата рождения.</summary>
    public string? BirthDate { get; init; }

    /// <summary>Место рождения.</summary>
    public string? BirthPlace { get; init; }

    /// <summary>Гражданство.</summary>
    public required string Citizenship { get; init; }

    /// <summary>Вид документа, удостоверяющего личность.</summary>
    public string? DocType { get; init; }

    /// <summary>Серия и номер документа.</summary>
    public string? DocNumber { get; init; }

    /// <summary>Дата выдачи.</summary>
    public string? IssueDate { get; init; }

    /// <summary>Кем выдан.</summary>
    public string? IssuedBy { get; init; }

    /// <summary>Адрес регистрации или проживания.</summary>
    public required string Address { get; init; }
}

/// <summary>
/// Сведения о руководителе.
/// </summary>
public sealed record TBankShopCeo
{
    /// <summary>Имя.</summary>
    public required string FirstName { get; init; }

    /// <summary>Фамилия.</summary>
    public required string LastName { get; init; }

    /// <summary>Отчество.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Дата рождения.</summary>
    public string? BirthDate { get; init; }

    /// <summary>Место рождения.</summary>
    public string? BirthPlace { get; init; }

    /// <summary>Вид документа, удостоверяющего личность.</summary>
    public string? DocType { get; init; }

    /// <summary>Серия и номер документа.</summary>
    public string? DocNumber { get; init; }

    /// <summary>Дата выдачи.</summary>
    public string? IssueDate { get; init; }

    /// <summary>Кем выдан.</summary>
    public string? IssuedBy { get; init; }

    /// <summary>Адрес регистрации или проживания.</summary>
    public string? Address { get; init; }

    /// <summary>Контактный телефон.</summary>
    public required string Phone { get; init; }

    /// <summary>Страна гражданства по ISO 3166-1 Alpha-3.</summary>
    public required string Country { get; init; }
}

/// <summary>
/// Лицензия организации.
/// </summary>
public sealed record TBankShopLicense
{
    /// <summary>Вид.</summary>
    public string? Type { get; init; }

    /// <summary>Номер.</summary>
    public string? Number { get; init; }

    /// <summary>Дата выдачи.</summary>
    public string? IssueDate { get; init; }

    /// <summary>Кем выдана.</summary>
    public string? IssuedBy { get; init; }

    /// <summary>Срок действия.</summary>
    public string? ExpiryDate { get; init; }

    /// <summary>Перечень лицензируемой деятельности.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Банковские реквизиты для регистрации точки.
/// </summary>
public sealed record TBankShopBankAccount
{
    /// <summary>Расчетный или казначейский счет.</summary>
    public required string Account { get; init; }

    /// <summary>Корреспондентский счет.</summary>
    public string? KorAccount { get; init; }

    /// <summary>Наименование банка.</summary>
    public required string BankName { get; init; }

    /// <summary>БИК.</summary>
    public required string Bik { get; init; }

    /// <summary>КБК.</summary>
    public string? Kbk { get; init; }

    /// <summary>ОКТМО.</summary>
    public string? Oktmo { get; init; }

    /// <summary>Назначение платежа.</summary>
    public required string Details { get; init; }
}

/// <summary>
/// Банковские реквизиты для обновления точки.
/// </summary>
public sealed record TBankShopBankAccountUpdate
{
    /// <summary>Расчетный или казначейский счет.</summary>
    public required string Account { get; init; }

    /// <summary>Корреспондентский счет.</summary>
    public string? KorAccount { get; init; }

    /// <summary>Наименование банка.</summary>
    public required string BankName { get; init; }

    /// <summary>БИК.</summary>
    public required string Bik { get; init; }

    /// <summary>КБК.</summary>
    public string? Kbk { get; init; }

    /// <summary>ОКТМО.</summary>
    public string? Oktmo { get; init; }

    /// <summary>Назначение платежа.</summary>
    public required string Details { get; init; }

    /// <summary>Заблокировать возмещения у торговой точки.</summary>
    public bool? DisableReimbursement { get; init; }
}

/// <summary>
/// Ответ регистрации или обновления точки партнера.
/// </summary>
public sealed record TBankShopMutationResponse
{
    /// <summary>Код точки на стороне партнера.</summary>
    public string? Code { get; init; }

    /// <summary>Присвоенный идентификатор точки на стороне банка.</summary>
    [JsonConverter(typeof(TBankSplitShopStringJsonConverter))]
    public string? ShopCode { get; init; }

    /// <summary>Информация о зарегистрированных терминалах.</summary>
    public IReadOnlyList<TBankShopTerminal> Terminals { get; init; } = [];

    /// <summary>HTTP-метаданные ответа.</summary>
    [JsonIgnore]
    public TBankSplitShopsResponseMetadata? Metadata { get; init; }
}

/// <summary>
/// Терминал точки партнера.
/// </summary>
public sealed record TBankShopTerminal
{
    /// <summary>Идентификатор терминала, если банк вернул его в ответе.</summary>
    [JsonConverter(typeof(TBankSplitShopStringJsonConverter))]
    public string? TerminalId { get; init; }
}

/// <summary>
/// Ответ получения информации по точке.
/// </summary>
public sealed record TBankShopInfoResponse
{
    /// <summary>Идентификаторы агрегированных мерчантов.</summary>
    public IReadOnlyList<long> MerchantIds { get; init; } = [];

    /// <summary>Идентификаторы терминалов.</summary>
    public IReadOnlyList<long> TerminalIds { get; init; } = [];

    /// <summary>Типы терминалов.</summary>
    public IReadOnlyList<int> TerminalTypes { get; init; } = [];

    /// <summary>MCC-код торговой группы.</summary>
    public int? Mcc { get; init; }

    /// <summary>Сокращенное наименование организации.</summary>
    public string? Name { get; init; }

    /// <summary>ИНН.</summary>
    public string? Inn { get; init; }

    /// <summary>КПП.</summary>
    public string? Kpp { get; init; }

    /// <summary>Email организации.</summary>
    public string? Email { get; init; }

    /// <summary>Банковские реквизиты партнера агрегатора.</summary>
    public TBankShopInfoBankAccount? BankAccount { get; init; }

    /// <summary>Признак нерезидента.</summary>
    public bool? NonResident { get; init; }

    /// <summary>Атрибуты точки для платежных систем.</summary>
    public IReadOnlyList<TBankShopPaymentSystemAttribute> PaymentSystemAttributes { get; init; } = [];

    /// <summary>HTTP-метаданные ответа.</summary>
    [JsonIgnore]
    public TBankSplitShopsResponseMetadata? Metadata { get; init; }
}

/// <summary>
/// Банковские реквизиты в ответе получения информации по точке.
/// </summary>
public sealed record TBankShopInfoBankAccount
{
    /// <summary>Расчетный счет.</summary>
    public string? Account { get; init; }

    /// <summary>Корреспондентский счет.</summary>
    public string? KorAccount { get; init; }

    /// <summary>Наименование банка.</summary>
    public string? BankName { get; init; }

    /// <summary>БИК.</summary>
    public string? Bik { get; init; }

    /// <summary>Назначение платежа.</summary>
    public string? Details { get; init; }

    /// <summary>Пользовательские правила комиссий.</summary>
    public IReadOnlyList<TBankShopUserDefinedFee> UserDefinedFees { get; init; } = [];

    /// <summary>Возмещения заблокированы у торговой точки.</summary>
    public bool DisableReimbursement { get; init; }

    /// <summary>Тип комиссии: UP или DOWN.</summary>
    public string? FeeType { get; init; }
}

/// <summary>
/// Пользовательское правило комиссии.
/// </summary>
public sealed record TBankShopUserDefinedFee
{
    /// <summary>Комиссия.</summary>
    public TBankShopFeeTax? Tax { get; init; }

    /// <summary>Правило применения комиссии.</summary>
    public TBankShopFeeRule? Rule { get; init; }

    /// <summary>Платежная система.</summary>
    public int? PaymentSystem { get; init; }

    /// <summary>Тип терминала.</summary>
    public int? TerminalType { get; init; }

    /// <summary>Карта выпущена в Т-Банке.</summary>
    public bool? TinkoffCard { get; init; }

    /// <summary>Комиссия за AFT операции.</summary>
    [JsonPropertyName("isAFT")]
    public bool? IsAft { get; init; }

    /// <summary>Дата вступления комиссии в силу.</summary>
    public string? StartDate { get; init; }

    /// <summary>Дата окончания действия комиссии.</summary>
    public string? EndDate { get; init; }
}

/// <summary>
/// Значения комиссии.
/// </summary>
public sealed record TBankShopFeeTax
{
    /// <summary>Процент от суммы операции.</summary>
    public decimal? Percent { get; init; }

    /// <summary>Фиксированная минимальная комиссия.</summary>
    public decimal? Min { get; init; }

    /// <summary>Фиксированная сумма, прибавляемая к комиссии.</summary>
    public decimal? Fix { get; init; }
}

/// <summary>
/// Правило применения комиссии.
/// </summary>
public sealed record TBankShopFeeRule
{
    /// <summary>Тип запроса: 0 Pay, 1 Fail pay, 2 Account verification.</summary>
    public int OperationType { get; init; }
}

/// <summary>
/// Атрибуты точки для платежных систем.
/// </summary>
public sealed record TBankShopPaymentSystemAttribute
{
    /// <summary>MCC-код торговой группы.</summary>
    public string? Mcc { get; init; }

    /// <summary>Уникальный МИД, зарегистрированный в платежной системе.</summary>
    public string? Mid { get; init; }

    /// <summary>Уникальный ТИД.</summary>
    public string? Tid { get; init; }
}

/// <summary>
/// Ошибка API регистрации точек T-Bank Split
/// </summary>
public sealed record TBankSplitShopsErrorResponse
{
    /// <summary>Время ошибки.</summary>
    public string? Timestamp { get; init; }

    /// <summary>HTTP-статус, возвращенный API.</summary>
    public int? Status { get; init; }

    /// <summary>Краткое описание HTTP-ошибки.</summary>
    public string? Error { get; init; }

    /// <summary>Сообщение ошибки.</summary>
    public string? Message { get; init; }

    /// <summary>Путь API.</summary>
    public string? Path { get; init; }

    /// <summary>Ошибки валидации полей.</summary>
    public IReadOnlyList<TBankSplitShopsFieldError> Errors { get; init; } = [];
}

/// <summary>
/// Ошибка валидации поля запроса регистрации или обновления точки.
/// </summary>
public sealed record TBankSplitShopsFieldError
{
    /// <summary>Имя параметра запроса, в котором допущена ошибка.</summary>
    public string? Field { get; init; }

    /// <summary>Сообщение об ошибке.</summary>
    public string? DefaultMessage { get; init; }

    /// <summary>Переданное значение.</summary>
    public string? RejectedValue { get; init; }

    /// <summary>Тип формата, которому значение не соответствует.</summary>
    public string? Code { get; init; }
}
