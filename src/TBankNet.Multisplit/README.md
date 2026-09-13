# TBankNet.Multisplit

.NET SDK package for T-Bank Multisplit provider shop registration and updates.

## Install

```bash
dotnet add package TBankNet.Multisplit --prerelease
```

## Supported API

`TBankNet.Multisplit` currently includes typed support for:

- OAuth token acquisition via `/oauth/token`
- provider shop registration via `POST /sm-register/register`
- shop lookup by `shopCode` via `GET /sm-register/register/shop/{shopCode}`
- provider banking detail updates via `PATCH /sm-register/register/{shopCode}`
- typed validation and API error responses

## Quick Start

```csharp
using TBankNet.Multisplit;

using var httpClient = new HttpClient();

var client = new TBankMultisplitClient(httpClient, new TBankMultisplitClientOptions
{
    Username = "login-from-bank",
    Password = "...",
    Environment = TBankMultisplitEnvironment.Test
});

// The SDK issues tokens; it does not hold them. Cache and refresh where you know how many
// requests share one credential.
var token = (await client.GetAccessTokenAsync()).ToAccessToken();

var shop = await client.GetShopAsync("111111111", token);

Console.WriteLine(shop.Name);
Console.WriteLine(shop.BankAccount?.Bik);
```

Set exactly one of `Environment` or `BaseAddress` — they name the same thing, and setting both is rejected rather than silently resolved. There is no default: the API host is never assumed.

## Access Tokens

`GetAccessTokenAsync` is the only call that talks to `/oauth/token`. Every other method takes the
token you pass it, so one token serves many calls and the refresh policy stays yours:

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

`ExpiresAt` is computed from `expires_in` against the moment the request was sent, so it is never
optimistic. When the bank reports no lifetime it stays `null` and `IsExpired()` returns `false` —
better a 401 you can retry than a working token thrown away on a guess.

## TLS

T-Bank serves `*.tinkoff.ru` from the Russian Trusted CA (Минцифры), whose root ships in no common
OS or container image. A default `HttpClient` fails the handshake with `UntrustedRoot` before any
request is sent.

Install the root into the system trust store and no code is needed:

```bash
# Debian/Ubuntu
sudo cp russian_trusted_root_ca_pem.crt /usr/local/share/ca-certificates/russian_trusted_root_ca.crt
sudo update-ca-certificates
```

Otherwise extend trust on the `HttpClient` you pass to the client — keep the system check first and
add the Минцифры root only as a fallback anchor, so a hostname mismatch or an expired certificate is
still rejected. The repository README has a full example.

The GOST certificates from the same distribution are of no use here: .NET verifies neither
GOST R 34.10-2012 signatures nor GOST TLS cipher suites. Use the RSA chain.

Production access to `acqapi.tinkoff.ru` may also require an mTLS client certificate and IP
allow-listing according to T-Bank registration API requirements.

## Registration

Registration uses `TBankRegisterShopRequest` with typed nested models for addresses, phones, founders, CEO data, licenses, and bank account details.

The client performs conservative local validation for required fields and sends JSON with T-Bank wire names and casing.

## Error Handling

Non-success HTTP responses are thrown as `TBankMultisplitApiException` with typed `TBankMultisplitErrorResponse` data when the response body can be parsed.

Transport, protocol, and local validation failures are thrown as SDK exceptions.

## Repository

Source, issue tracking, and full documentation live in the repository:

https://github.com/ai-iskuzhin/TBankNet.Multisplit
