# T-Bank Multisplit API: Provider Registration And Update

## Purpose

This document keeps the full working documentation for the provider-registration API used in the T-Bank multisplit flow.

Unlike the shorter project notes in adjacent files, this document is intentionally fuller and implementation-oriented.

Source basis:

- original registration and partner-update material dated `2026-03-03`

## Scope

This document covers:

- authorization for registration API usage
- provider shop registration
- reading shop data by `shopCode`
- updating provider banking details
- request and response examples
- validation and integration notes relevant for Sportgearhub

## Integration Flow

Recommended working sequence:

1. Obtain bearer token via `POST /oauth/token`.
2. Register provider shop via `POST /sm-register/register`.
3. Persist returned `shopCode` in the Sportgearhub provider-connection model.
4. Read current shop state via `GET /sm-register/register/shop/{shopCode}` when needed.
5. Update payout-related banking details via `PATCH /sm-register/register/{shopCode}`.

## Environments

| Environment | Base URL |
| --- | --- |
| Test | `https://acqapi-test.tinkoff.ru` |
| Production | `https://acqapi.tinkoff.ru` |

Additional access requirements:

- mTLS certificate
- IP whitelist request via `acq_help@tbank.ru`

## Transport And General Rules

- registration methods are called over HTTP `POST`, `GET`, and `PATCH`
- JSON is used for registration and update requests
- parameters are case-sensitive
- `Content-Type: application/json` is required for JSON endpoints
- `POST /oauth/token` is the exception and uses form-style body instead of JSON

## 1. Authorization

### Endpoint

`POST /oauth/token`

### Purpose

Returns bearer token for subsequent registration API calls.

### Auth Mode

Basic auth:

- username: `partner`
- password: `partner`

### Request Body

### Примечание: grant_type=password имеет постоянное значение. Переменная часть запроса 
«username=login&password=password». Username и password выдает банк  

```bash
curl -X POST https://sm-register-test.tcsbank.ru/oauth/token \
-d "grant_type=password&username=login&password=password" \
-H "Authorization: Basic cGFydG5lcjpwYXJ0bmVy"
```
Ответ:

```json
{
  "access_token": "...",
  "token_type": "bearer",
  "refresh_token": "...",
  "expires_in": 43199,
  "scope": "partner",
  "jti": "uuid"
}
```

Использовать:
Authorization: Bearer <access_token>

## 1.3 Регистрация точки для партнера
Тестовый URL: `https://acqapi-test.tinkoff.ru/sm-register/register`
Боевой URL: `https://acqapi.tinkoff.ru/sm-register/register`

| Наименование                       | Тип          | Обязательность | Описание                                                             |
| ---------------------------------- | ------------ | -------------- | -------------------------------------------------------------------- |
| serviceProviderEmail               | String       | Нет            | Email магазина                                                       |
| mcc1                               | Integer      | Нет            | MCC-код торговой группы                                              |
| shopArticleId                      | String (32)  | Нет            | Код точки на стороне магазина. Если не передан, присваивается банком |
| billingDescriptor                  | String       | Да             | Название магазина в СМС и на странице 3DS (на иностранном языке)     |
| fullName                           | String       | Да             | Полное наименование организации                                      |
| name2                              | String (512) | Да             | Сокращенное наименование организации                                 |
| inn                                | String       | Да             | ИНН                                                                  |
| kpp3                               | String       | Да             | КПП                                                                  |
| okved                              | String       | Нет            | ОКВЭД                                                                |
| ogrn                               | Integer      | Да             | Основной регистрационный номер                                       |
| regDepartment                      | String       | Нет            | Орган государственной регистрации                                    |
| regDate                            | String       | Нет            | Дата присвоения ОГРН                                                 |
| addresses4                         | Array        | Да             | Адреса организации                                                   |
| addresses[].type                   | String       | Да             | Тип адреса: legal / actual / post / other                            |
| addresses[].zip                    | String       | Да             | Почтовый индекс                                                      |
| addresses[].country                | String       | Да             | Трехбуквенный код страны ISO                                         |
| addresses[].city                   | String       | Да             | Город                                                                |
| addresses[].street                 | String       | Да             | Улица, дом                                                           |
| addresses[].description            | String       | Нет            | Дополнительное описание                                              |
| phones                             | Array        | Нет            | Телефоны организации                                                 |
| phones[].type                      | String       | Нет            | Тип: common / fax / other                                            |
| phones[].phone                     | String       | Нет            | Телефон                                                              |
| phones[].description               | String       | Нет            | Дополнительное описание                                              |
| email                              | String       | Да             | Email партнера                                                       |
| assets                             | String       | Нет            | Размер уставного капитала                                            |
| founders                           | Object       | Нет            | Сведения об учредителях                                              |
| founders.individuals               | Array        | Да             | Физические лица                                                      |
| founders.individuals[].firstName   | String       | Да             | Имя                                                                  |
| founders.individuals[].lastName    | String       | Да             | Фамилия                                                              |
| founders.individuals[].middleName  | String       | Нет            | Отчество                                                             |
| founders.individuals[].birthDate   | String       | Нет            | Дата рождения                                                        |
| founders.individuals[].birthPlace  | String       | Нет            | Место рождения                                                       |
| founders.individuals[].citizenship | String       | Да             | Гражданство                                                          |
| founders.individuals[].docType     | String       | Нет            | Тип документа                                                        |
| founders.individuals[].docNumber   | String       | Нет            | Серия и номер                                                        |
| founders.individuals[].issueDate   | String       | Нет            | Дата выдачи                                                          |
| founders.individuals[].issuedBy    | String       | Нет            | Кем выдан                                                            |
| founders.individuals[].address     | String       | Да             | Адрес                                                                |
| ceo                                | Object       | Да             | Руководитель                                                         |
| ceo.firstName                      | String       | Да             | Имя                                                                  |
| ceo.lastName                       | String       | Да             | Фамилия                                                              |
| ceo.middleName                     | String       | Нет            | Отчество                                                             |
| ceo.birthDate                      | String       | Нет            | Дата рождения                                                        |
| ceo.birthPlace                     | String       | Нет            | Место рождения                                                       |
| ceo.docType                        | String       | Нет            | Тип документа                                                        |
| ceo.docNumber                      | String       | Нет            | Серия и номер                                                        |
| ceo.issueDate                      | String       | Нет            | Дата выдачи                                                          |
| ceo.issuedBy                       | String       | Нет            | Кем выдан                                                            |
| ceo.address                        | String       | Нет            | Адрес                                                                |
| ceo.phone                          | String       | Да             | Телефон                                                              |
| ceo.country                        | String       | Да             | Код страны ISO Alpha-3                                               |
| licenses                           | Array        | Нет            | Лицензии                                                             |
| licenses[].type                    | String       | Нет            | Вид                                                                  |
| licenses[].number                  | String       | Нет            | Номер                                                                |
| licenses[].issueDate               | String       | Нет            | Дата выдачи                                                          |
| licenses[].issuedBy                | String       | Нет            | Кем выдана                                                           |
| licenses[].expiryDate              | String       | Нет            | Срок действия                                                        |
| licenses[].description             | String       | Нет            | Описание деятельности                                                |
| siteUrl                            | String       | Да             | Сайт                                                                 |
| primaryActivities                  | String       | Нет            | Основные виды деятельности                                           |
| bankAccount                        | Object       | Да             | Банковские реквизиты                                                 |
| bankAccount.account                | String       | Да             | Счет                                                                 |
| bankAccount.korAccount             | String       | Нет            | Корреспондентский счет                                               |
| bankAccount.bankName               | String       | Да             | Банк                                                                 |
| bankAccount.bik                    | String       | Да             | БИК                                                                  |
| bankAccount.kbk5                   | String       | Нет            | КБК                                                                  |
| bankAccount.oktmo5                 | String       | Нет            | ОКТМО                                                                |
| bankAccount.details                | String       | Да             | Назначение платежа                                                   |
| comment                            | String       | Нет            | Комментарий                                                          |
| nonResident                        | Boolean      | Нет            | Признак нерезидента (всегда false)                                   |

### Примечание:  
1. Если значение параметра mcc не равно mcc-коду, соответствующему переданной торговой группы 
(merchantIds), то значение merchantIds игнорируется, и точка зарегистрируется на ту торговую группу, 
которой соответствует значение параметра mcc.   
2. Параметр name необходимо заполнить кириллицей и обязательно указать организационно-правовую 
форму (например ОАО, ЗАО, ИП). 
3. В случае, если у регистрируемой точки нет КПП, то необходимо передать нули: 000000000. 
4. В параметре addresses допустимы только символы («.» и «,»). При наличии других спецсимволов запрос 
отобьется ошибкой. 
5. Параметры kbk и oktmo оба обязательны для заполнения, если указан 1 (один) из этих параметров. 

Пример запроса 
```json
{ 
    "serviceProviderEmail": "333@mail.ru", 
    "shopArticleId": "test_tochka", 
    "billingDescriptor": "test_tochka", 
    "fullName": "Общество с ограниченной ответственностью «Компания»", 
    "name": "ООО «Компания»", 
    "inn": "3333333333", 
    "kpp": "333333333", 
    "okved": "64.92.7", 
    "ogrn": 333333333333, 
    "regDepartment": "ФНС №1 по г. Москве", 
    "regDate": "2003-03-03", 
    "addresses": [ 
        { 
            "type": "legal", 
            "zip": "108809", 
            "country": "RUS", 
            "city": "Москва", 
            "street": "Маяковского, 3", 
            "description": "Юридический адрес" 
        }, 
        { 
            "type": "actual", 
            "zip": "108809", 
            "country": "RUS", 
            "city": "Москва", 
            "street": "Маяковского, 5" 
        } 
    ], 
    "phones": [ 
        { 
            "type": "common", 
            "phone": "+7(495)333-3333", 
            "description": "основной" 
        } 
    ], 
    "email": "333@mail.ru", 
    "assets": "3000000", 
    "founders" : { 
        "individuals" : [ { 
          "firstName" : "Семен", 
          "lastName" : "Семенов", 
          "middleName" : "Семенович", 
          "birthDate" : "1970-02-02", 
          "birthPlace" : "Рязань", 
          "citizenship" : "Россия", 
          "docType" : "Паспорт", 
          "docNumber" : "2222 222222", 
          "issueDate" : "2009-07-21", 
          "issuedBy" : "Отделом УФМС России по Рязанской области", 
          "address" : "214031, г. Рязань, ул. Ленина, д. 1, кв. 1" 
        }, {      
          "firstName" : "Имя", 
          "lastName" : "Фамилия", 
          "middleName" : "Отчество", 
          "birthDate" : "1993-01-01", 
          "birthPlace" : "г. Москва", 
          "citizenship" : "Россия", 
          "docType" : "Паспорт", 
          "docNumber" : "2222 333333", 
          "issueDate" : "2012-09-13", 
          "issuedBy" : "Отделом УФМС России по гор. Москве", 
          "address" : "125413, г. Москва, ул.Ленина, д. 1, кв. 1" 
        } ] 
      }, 
    "ceo": { 
        "address": "108809, г. Москва, Маяковского, 3", 
        "phone": "+79853333333", 
        "firstName": "Иван", 
        "lastName": "Иванов", 
        "middleName": "Иванович", 
        "birthDate": "1980-03-03", 
        "birthPlace": "Москва", 
        "docType": "Паспорт", 
        "docNumber": "333 333333", 
        "issueDate": "2020-09-16", 
        "issuedBy": "УМВД России по Московской области", 
        "country": "RUS"
        }, 
  "licenses": [ { 
  "type" : "type", 
  "number" : "3333-654", 
  "issueDate" : "2010-01-01", 
  "issuedBy" : "issuedBy", 
  "expiryDate" : "2020-01-01", 
  "description" : "лицензия продлена" 
  } ],  
  "siteUrl": "http://yandex.ru/", 
  "primaryActivities": "Торговля", 
  "bankAccount": { 
  "account": "40702810838170023076", 
  "korAccount": "30101810400000000225", 
  "bankName": "ПАО «Сбербанк России»", 
  "bik": "044525225", 
  "kbk": "18210501011011000110", 
  "oktmo": "45286575000", 
  "details": "Перевод средств по договору № 3333-3333 от 16.09.2021 по Реестру Операций от ${date}. Сумма комиссии 
  ${rub} руб. ${kop} коп." 
  }, 
  "comment": "Комментарий", 
  "nonnResident": false
}
```
Ответ 
Формат ответа: JSON
| Параметр  | Тип    | Описание                                                                      |
| --------- | ------ | ----------------------------------------------------------------------------- |
| code      | String | Код точки на стороне партнера (значение из shopArticleId)                     |
| shopCode  | String | Идентификатор точки на стороне банка. Используется как PartnerId при выплатах |
| terminals | Array  | Массив объектов с информацией о терминалах (для Мультирасчетов — пустой)      |

Пример ответа: 
```json
{  
  "code": "test_tochka", 
  "shopCode": 111111111, 
  "terminals": []
} 
```
При неуспешном ответе в объекте errors передается перечень ошибок валидации, которые были найдены в переданном запросе. 

| Наименование   | Тип    | Описание                                         |
| -------------- | ------ | ------------------------------------------------ |
| field          | String | Имя параметра запроса, в котором допущена ошибка |
| defaultMessage | String | Сообщение об ошибке                              |
| rejectedValue  | String | Значение, переданное в запросе                   |
| code           | String | Тип формата, которому значение не соответствует  |

Существуют следующие причины ошибок:	 
• Ошибки валидации и формата сообщения 
• Ошибки бизнес-логики

Примеры неуспешного ответа: 
1) По причине бизнес-логики или по технических проблемам 
```json
{ 
    "timestamp": "2018-07-16T13:10:11.158+0000", 
    "status": 400, 
    "error": "Bad Request", 
    "message": "Ошибка регистрации точки billingDescriptor[shopArticleId]\nуказаны неверные банковские реквизиты. БИК : 
044583999; р/с : 000000000000000000000", 
    "path": "/register" 
}
```
Status заполняется http-кодом, которым завершился запрос. Если в ответе сообщения присутствует данный параметр, то это означает, что регистрация точки завершилась ошибкой.

2) При ошибках формата сообщения 
```json
{ 
    "timestamp": "2018-07-25T13:23:18.160+0000", 
    "status": 400, 
    "error": "Bad Request", 
    "errors": [ 
       { 
           "field": "billingDescriptor", 
           "defaultMessage": "не может быть пусто", 
           "rejectedValue": "", 
           "code": "NotEmpty" 
       }, 
       { 
           "field": "serviceProviderEmail", 
           "defaultMessage": "email определен в неверном формате", 
           "rejectedValue": "bademeil", 
           "code": "Email" 
       }, 
       { 
           "field": "billingDescriptor", 
           "defaultMessage": "должно соответствовать шаблону \"[A-z0-9.\\-_ ]+\"", 
           "rejectedValue": "", 
           "code": "Pattern" 
       }, 
       { 
           "field": "billingDescriptor", 
           "defaultMessage": "размер должен быть между 1 и 14", 
           "rejectedValue": "", 
           "code": "Size" 
       } 
   ], 
    "message": "Validation failed for object='merchant'. Error count: 1", 
    "path": "/register" 
}
```
Перечень возможных ошибок 
• Адрес не задан. 
• The billingDescriptor is not specified 
• Точка уже активирована. 
• ShopArticleId не передан 
• Параметр [name] не задaн. 
• Параметр [inn] не задaн. 
• Параметр [ogrn] не задaн. 
• Параметр [address.zip] не задaн. 
• Параметр [address.city] не задaн. 
• Параметр [address.country] не задaн. 
• Параметр [address.country] не задaн. 
• Параметр [ceo.firstName] не задaн. 
• Параметр [ceo.lastName] не задaн. 
• Параметр [bankAccount] не задaн. 
• Параметр назначение платежа не задано 
• Параметр назначение платежа не задано 
• The ShopArticleId is not specified 
• Шаблон назначение платежа не задан 
• Указаны неверные банковские реквизиты. БИК: ${bankAccount.bik}; р/с: ${bankAccount.account} 
• Поле КБК не соответствует заданному шаблону: 20 цифр  
• Поле КБК должно быть задано вместе с ОКТМО 
• Поле ОКТМО не соответствует заданному шаблону: 8 или 11 цифр 
• Поле ОКТМО должно быть задано вместе с КБК 
• Ошибка регистрации точки. Параметр ogrn не задан 

## 1.4 Получение информации по точке

## 3. Get Shop By `shopCode`

Тестовый URL: https://acqapi-test.tinkoff.ru/sm-register/register/shop/{shopCode} 
Боевой URL: https://acqapi.tinkoff.ru/sm-register/register/shop/{shopCode} 


Примечание Для возможности отправки запросов напишите на почту acq_help@tbank.ru c просьбой добавить ваши IP в WL. После чего сможете отправлять запросы.
| Наименование | Тип     | Обязательность | Описание                                                             |
| ------------ | ------- | -------------- | -------------------------------------------------------------------- |
| shopCode     | Integer | Да             | Идентификатор точки, полученный в ответе на запрос регистрации точки |

Ответ

| Наименование                       | Тип     | Обязательность | Описание                                                |
| ---------------------------------- | ------- | -------------- | ------------------------------------------------------- |
| merchantIds                        | Array   | Нет            | Идентификаторы агрегированных мерчантов                 |
| terminalIds                        | Array   | Нет            | Идентификаторы терминалов                               |
| terminalTypes                      | Array   | Нет            | Тип терминала: 0 (non3DS), 1 (3DS)                      |
| mcc                                | Integer | Нет            | MCC-код (если одна ТГ, иначе в paymentSystemAttributes) |
| name                               | String  | Да             | Сокращенное наименование организации                    |
| inn                                | String  | Нет            | ИНН                                                     |
| kpp                                | String  | Нет            | КПП                                                     |
| email                              | String  | Да             | Email организации                                       |
| bankAccount                        | Object  | Да             | Реквизиты партнера                                      |
| bankAccount.account                | String  | Нет*           | Расчетный счет                                          |
| bankAccount.korAccount             | String  | Нет*           | Корреспондентский счет                                  |
| bankAccount.bankName               | String  | Нет*           | Банк                                                    |
| bankAccount.bik                    | String  | Нет*           | БИК                                                     |
| bankAccount.details                | String  | Нет*           | Назначение платежа                                      |
| userDefinedFees                    | Object  | Да**           | Пользовательские комиссии                               |
| userDefinedFees.tax                | Object  | Да             | Комиссия (может быть пустой)                            |
| userDefinedFees.tax.percent        | Number  | Нет            | % от суммы                                              |
| userDefinedFees.tax.min            | Number  | Нет            | Минимальная комиссия                                    |
| userDefinedFees.tax.fix            | Number  | Нет            | Фиксированная сумма                                     |
| userDefinedFees.rule               | Object  | Да             | Правило применения                                      |
| userDefinedFees.paymentSystem      | Number  | Нет            | 0 (Visa), 1 (Mastercard), 2 (Mir)                       |
| userDefinedFees.terminalType       | Number  | Нет            | 0 (non-3DS), 1 (3DS)                                    |
| userDefinedFees.tinkoffCard        | Boolean | Нет            | Карта Т-Банка                                           |
| nonResident                        | Boolean | Нет            | Нерезидент                                              |
| userDefinedFees.rule.operationType | Number  | Да             | 0 (Pay), 1 (Fail pay), 2 (Account verification)         |
| userDefinedFees.isAft              | Boolean | Нет            | AFT комиссия                                            |
| userDefinedFees.startDate          | String  | Нет            | Дата начала (yyyy-MM-dd hh:mm:ss)                       |
| userDefinedFees.endDate            | String  | Нет            | Дата окончания (если null — без ограничения)            |
| bankAccount.disableReimbursement   | Boolean | Да             | Возмещения заблокированы                                |
| bankAccount.feeType                | String  | Да             | Тип комиссии: UP / DOWN                                 |
| paymentSystemAttributes            | Array   | Нет            | Атрибуты ПС                                             |
| paymentSystemAttributes.mcc        | String  | Нет            | MCC                                                     |
| paymentSystemAttributes.mid        | String  | Нет            | MID                                                     |
| paymentSystemAttributes.tid        | String  | Нет            | TID                                                     |
* Не приходит, если в запросе на регистрацию не был передан bankAccount. 
** Обязателен только для BPA и BRS. В других случаях необязателен (в ответе будет возвращаться пустой массив). 

Пример ответа, если точка подключена к 1 торговой группе (merchantIds): 
```json
{ 
    "merchantIds": [0000000000000], 
    "terminalIds": [0000000, 11111111], 
    "terminalTypes": [0,1], 
    "mcc": 6012, 
    "name": "OOO «Moya kompaniya»", 
    "inn": "1111111111", 
    "kpp": "111000001", 
    "email": "11@mail.ru", 
    "bankAccount": { 
        "account": "111111111111111111", 
        "korAccount": "111111111111111111", 
        "bankName": "ПАО «Сбербанк России»", 
        "bik": "11111111111", 
        "details": "Перевод средств по договору № 202210-11111 от 01.09.2021 по Реестру Операций от ${date}. Сумма     комиссии 
         ${rub} руб. ${kop} коп.", 
        "userDefinedFees": [ 
            { 
                "tax": { 
                    "percent": 1, 
                    "min": 0 
                }, 
                "rule": { 
                    "operationType": 0 
                }, 
                "isAFT": false, 
                "startDate": "2022-03-03 21:07:57" 
        ], 
        "disableReimbursement": false, 
        "feeType": "DOWN" 
    }, 
    "paymentSystemAttributes": [ 
        { 
            "mid": "200000001111111", 
            "tid": "11111111" 
        } 
    ] 
} 
```
Пример ответа, если точка подключена к нескольким торговым группам (merchantIds): 
```json
{     
    "merchantIds": [0000000000000, 0000000000001], 
    "terminalIds": [0000000, 11111111, 0000001, 11111112], 
    "terminalTypes": [0,1, 0, 1],     
    "name": "OOO «Moya kompaniya»", 
    "inn": "1111111111", 
    "kpp": "111000001", 
    "email": "11@mail.ru", 
    "bankAccount": { 
        "account": "111111111111111111", 
        "korAccount": "111111111111111111", 
        "bankName": "ПАО «Сбербанк России»", 
        "bik": "11111111111", 
        "details": "Перевод средств по договору № 202210-11111 от 01.09.2021 по Реестру Операций от ${date}. Сумма комиссии ${rub} руб. 
${kop} коп.", 
        "userDefinedFees": [ 
            { 
                "tax": { 
                    "percent": 1, 
                    "min": 0 
                }, 
                "rule": { 
                    "operationType": 0 
                }, 
                "isAFT": false, 
                "startDate": "2022-03-03 21:07:57" 
            }, 
            { 
                "tax": { 
                    "percent": 1, 
                    "min": 0 
                }, 
                "rule": { 
                    "operationType": 0 
                }, 
                "isAFT": true, 
                "startDate": "2022-03-03 21:07:57" 
            } 
        ], 
        "disableReimbursement": false, 
        "feeType": "DOWN" 
    }, 
    "paymentSystemAttributes": [ 
        { 
            "mcc": "0000",             
            "mid": "200000001111111", 
            "tid": "11111111" 
        }, 
        { 
            "mcc": "0001",             
            "mid": "200000001111111", 
            "tid": "11111111" 
        } 
] 
} 
```
