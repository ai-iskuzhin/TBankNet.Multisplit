# TBankAcquiringNet.SplitShops

.NET SDK package for T-Bank Multisplit provider shop registration and updates.

## Install

```bash
dotnet add package TBankAcquiringNet.SplitShops --prerelease
```

## Supported API

`TBankAcquiringNet.SplitShops` currently includes typed support for:

- OAuth token acquisition via `/oauth/token`
- provider shop registration via `POST /sm-register/register`
- shop lookup by `shopCode` via `GET /sm-register/register/shop/{shopCode}`
- provider banking detail updates via `PATCH /sm-register/register/{shopCode}`
- typed validation and API error responses

## Quick Start

```csharp
using TBankAcquiringNet.SplitShops;

using var httpClient = new HttpClient();

var client = new TBankSplitShopsClient(httpClient, new TBankSplitShopsClientOptions
{
    Username = "login-from-bank",
    Password = "...",
    Environment = TBankSplitShopsEnvironment.Test
});

var shop = await client.GetShopAsync("111111111");

Console.WriteLine(shop.Name);
Console.WriteLine(shop.BankAccount?.Bik);
```

Production access to `acqapi.tinkoff.ru` may require mTLS certificates and IP allow-listing according to T-Bank registration API requirements.

## Registration

Registration uses `TBankRegisterShopRequest` with typed nested models for addresses, phones, founders, CEO data, licenses, and bank account details.

The client performs conservative local validation for required fields and sends JSON with T-Bank wire names and casing.

## Error Handling

Non-success HTTP responses are thrown as `TBankSplitShopsApiException` with typed `TBankSplitShopsErrorResponse` data when the response body can be parsed.

Transport, protocol, and local validation failures are thrown as SDK exceptions.

## Repository

Source, issue tracking, and full documentation live in the repository:

https://github.com/ai-iskuzhin/TBankAcquiringNet
