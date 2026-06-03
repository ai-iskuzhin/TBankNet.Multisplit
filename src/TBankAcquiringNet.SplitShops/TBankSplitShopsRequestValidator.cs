namespace TBankAcquiringNet.SplitShops;

internal static class TBankSplitShopsRequestValidator
{
    public static void Validate(TBankRegisterShopRequest request)
    {
        Require(request.BillingDescriptor, "Billing descriptor must be provided.");
        Require(request.FullName, "Full name must be provided.");
        Require(request.Name, "Name must be provided.");
        Require(request.Inn, "INN must be provided.");
        Require(request.Email, "Email must be provided.");
        Require(request.SiteUrl, "Site URL must be provided.");

        if (request.Ogrn <= 0)
        {
            throw new TBankSplitShopsValidationException("OGRN must be greater than zero.");
        }

        if (request.Addresses is null || request.Addresses.Count == 0)
        {
            throw new TBankSplitShopsValidationException("At least one address must be provided.");
        }

        foreach (var address in request.Addresses)
        {
            Require(address.Type, "Address type must be provided.");
            Require(address.Zip, "Address ZIP must be provided.");
            Require(address.Country, "Address country must be provided.");
            Require(address.City, "Address city must be provided.");
            Require(address.Street, "Address street must be provided.");
        }

        if (request.Ceo is null)
        {
            throw new TBankSplitShopsValidationException("CEO must be provided.");
        }

        Require(request.Ceo.FirstName, "CEO first name must be provided.");
        Require(request.Ceo.LastName, "CEO last name must be provided.");
        Require(request.Ceo.Phone, "CEO phone must be provided.");
        Require(request.Ceo.Country, "CEO country must be provided.");

        if (request.BankAccount is null)
        {
            throw new TBankSplitShopsValidationException("Bank account must be provided.");
        }

        Validate(request.BankAccount);
    }

    public static void Validate(TBankUpdateShopRequest request)
    {
        if (request.BankAccount is not null)
        {
            Validate(request.BankAccount);
        }
    }

    private static void Validate(TBankShopBankAccount bankAccount)
    {
        Require(bankAccount.Account, "Bank account must be provided.");
        Require(bankAccount.BankName, "Bank name must be provided.");
        Require(bankAccount.Bik, "BIK must be provided.");
        Require(bankAccount.Details, "Payment details must be provided.");
        ValidateKbkOktmoPair(bankAccount.Kbk, bankAccount.Oktmo);
    }

    private static void Validate(TBankShopBankAccountUpdate bankAccount)
    {
        Require(bankAccount.Account, "Bank account must be provided.");
        Require(bankAccount.BankName, "Bank name must be provided.");
        Require(bankAccount.Bik, "BIK must be provided.");
        Require(bankAccount.Details, "Payment details must be provided.");
        ValidateKbkOktmoPair(bankAccount.Kbk, bankAccount.Oktmo);
    }

    private static void ValidateKbkOktmoPair(string? kbk, string? oktmo)
    {
        if (kbk is null && oktmo is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(kbk) || string.IsNullOrWhiteSpace(oktmo))
        {
            throw new TBankSplitShopsValidationException("KBK and OKTMO must be provided together.");
        }
    }

    private static void Require(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new TBankSplitShopsValidationException(message);
        }
    }
}
