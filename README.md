<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="https://raw.githubusercontent.com/ai-iskuzhin/TBankNet.Multisplit/main/assets/icon.png" width="140" alt="Логотип TBankNet.Multisplit" />
    </td>
    <td valign="middle">
      <h1>TBankNet.Multisplit</h1>
      <p>Неофициальный .NET SDK для регистрации и обновления точек в схеме <a href="https://www.tbank.ru/kassa/develop/api/multisplit/">T-Bank Мультисплит</a>.</p>
      <p>
        <a href="https://github.com/ai-iskuzhin/TBankNet.Multisplit/blob/main/LICENSE"><img src="https://img.shields.io/github/license/ai-iskuzhin/TBankNet.Multisplit?style=flat-square" alt="License" /></a>
        <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/targets-net10.0-512BD4?logo=dotnet&amp;style=flat-square" alt="Targets" /></a>
        <a href="https://www.nuget.org/packages/TBankNet.Multisplit"><img src="https://img.shields.io/nuget/v/TBankNet.Multisplit?logo=nuget&amp;style=flat-square" alt="Версия NuGet" /></a>
      </p>
    </td>
  </tr>
</table>

> ⚠️ **Требуется доверие к корню Минцифры.** T-API отдаёт `*.tinkoff.ru` из Russian Trusted CA,
> которого нет ни в одном стандартном хранилище ОС и контейнеров: обычный `HttpClient` падает с
> `UntrustedRoot` ещё до отправки запроса. См. [TLS](#tls).

> Проект не аффилирован с T-Bank и разрабатывается независимо. «T-Bank», «Мультисплит» и связанные названия принадлежат их правообладателям.

## Установка

```bash
dotnet add package TBankNet.Multisplit --prerelease
```

## Поддерживаемые методы

| Метод T-API | API клиента | HTTP |
| --- | --- | --- |
| Получить токен | `GetAccessTokenAsync` | `POST /oauth/token` |
| Зарегистрировать точку | `RegisterShopAsync` | `POST /sm-register/register` |
| Получить точку по `shopCode` | `GetShopAsync` | `GET /sm-register/register/shop/{shopCode}` |
| Обновить реквизиты точки | `UpdateShopAsync` | `PATCH /sm-register/register/{shopCode}` |

Полное описание полей — в [docs/api_reg_upd_multisplit.md](docs/api_reg_upd_multisplit.md).

## Быстрый старт

```csharp
using TBankNet.Multisplit;

var client = new TBankMultisplitClient(httpClient, new TBankMultisplitClientOptions
{
    Username = "login-from-bank",
    Password = "...",
    Environment = TBankMultisplitEnvironment.Test
});

var token = (await client.GetAccessTokenAsync()).ToAccessToken();
var shop = await client.GetShopAsync("111111111", token);
```

Задаётся ровно одно из `Environment` или `BaseAddress`: они означают одно и то же, и указать оба —
ошибка, а не молчаливое разрешение в пользу одного. Умолчания нет: боевой хост никогда не
предполагается сам собой.

## Токены

`GetAccessTokenAsync` — единственный метод, который обращается к `/oauth/token`. Остальные принимают
токен, который вы передали, поэтому один токен обслуживает много вызовов, а политика обновления
остаётся вашей:

```csharp
private TBankMultisplitAccessToken cached;

private async Task<TBankMultisplitAccessToken> GetTokenAsync(CancellationToken ct)
{
    if (cached.IsExpired())
    {
        cached = (await client.GetAccessTokenAsync(ct)).ToAccessToken();
    }

    return cached;
}
```

`ExpiresAt` считается от момента отправки запроса, а не разбора ответа, поэтому оценка заведомо не
длиннее настоящей. Если банк не сообщил срок жизни, поле остаётся `null`, а `IsExpired()` возвращает
`false`: лучше получить 401 и обновить токен, чем выбросить рабочий по догадке.

## TLS

T-Bank отдаёт `*.tinkoff.ru` из Russian Trusted CA (Минцифры). Корня нет в стандартных хранилищах,
поэтому либо установите его в системное хранилище, либо передайте `HttpClient`, который ему доверяет:

```csharp
using TBankAcquiringNet;

var handler = new SocketsHttpHandler();
handler.SslOptions.RemoteCertificateValidationCallback = (_, certificate, chain, errors) =>
    TBankServerCertificateValidator.RussianTrustedCa.Validate(
        null, certificate as X509Certificate2, chain, errors);

using var httpClient = new HttpClient(handler);
```

Боевой доступ к `acqapi.tinkoff.ru` дополнительно может требовать клиентского mTLS-сертификата и
включения IP в список разрешённых — см. [samples/TBankAcquiringNet.MtlsExample](samples/TBankAcquiringNet.MtlsExample).

## Обработка ошибок

| Исключение | Когда |
| --- | --- |
| `TBankMultisplitValidationException` | Локальная проверка запроса до обращения к банку |
| `TBankMultisplitTransportException` | Ответ не получен: сеть, TLS, таймаут |
| `TBankMultisplitApiException` | HTTP не 2xx; тело разобрано в `TBankMultisplitErrorResponse` |
| `TBankMultisplitProtocolException` | Ответ получен, но не является ожидаемым JSON |

## Лицензия

[MIT](LICENSE)
