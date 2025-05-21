using Abp.Configuration;
using Abp.Localization;
using Abp.Net.Mail;
using Sayarah.Configuration;

namespace Sayarah.Application.Configuration;
public class SayarahSettingProvider : SettingProvider
{
    public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
    {
        var displaySettings = new SettingDefinitionGroup("DisplaySettings", L("Settings.Groups.Display"));
        var emailSettings = new SettingDefinitionGroup("EmailSettings", L("Settings.Groups.Email"));
        var listSettings = new SettingDefinitionGroup("ListSettings", L("Settings.Groups.List"));
        var publicSettings = new SettingDefinitionGroup("PublicSettings", L("Settings.Groups.Public"));
        var invoiceSettings = new SettingDefinitionGroup("InvoiceSettings", L("Settings.InvoiceSettings"));
        var fuelSettings = new SettingDefinitionGroup("FuelSettings", L("Settings.FuelSettings"));

        return
        [
                new SettingDefinition("FavouriteLanguage", "en", L("Settings.FavouriteLanguage"), displaySettings, L("Settings.FavouriteLanguage.Description"),
                    SettingScopes.User | SettingScopes.Tenant | SettingScopes.Application, isVisibleToClients: true),

                new SettingDefinition(Abp.Zero.Configuration.AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin, "true",
                    L("Settings.EmailConfirmationRequiredForLogin"), emailSettings, L("Settings.EmailConfirmationRequiredForLogin.Description"),
                    SettingScopes.Tenant),

                new SettingDefinition("SaveDataTableState", "false", L("Settings.SaveDataTableState"), listSettings, L("Settings.SaveDataTableState.Description"),
                    SettingScopes.User | SettingScopes.Tenant | SettingScopes.Application, isVisibleToClients: true),

                new SettingDefinition(EmailSettingNames.Smtp.Host, "al7osamcompany.com", L("Settings.Mail.Host"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.Port, "587", L("Settings.Mail.Port"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.UserName, "info@al7osamcompany.com", L("Settings.Mail.UserName"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.Password, "Ve%n6n77", L("Settings.Mail.Password"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.Domain, "al7osamcompany.com", L("Settings.Mail.Domain"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.EnableSsl, "false", L("Settings.Mail.EnableSsl"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.Smtp.UseDefaultCredentials, "true", L("Settings.Mail.UseDefaultCredentials"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.DefaultFromAddress, "info@al7osamcompany.com", L("Settings.Mail.DefaultFromAddress"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(EmailSettingNames.DefaultFromDisplayName, "Syarah app", L("Settings.Mail.DefaultFromDisplayName"), emailSettings, null,
                    SettingScopes.Tenant | SettingScopes.Application),

                new SettingDefinition(AppSettingNames.WebApiKey, "YOUR_API_KEY", L("Settings.WebApiKey"), null, L("Settings.WebApiKey.Description"),
                    SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, true),

                new SettingDefinition(AppSettingNames._91_PriceForLitre, "0", L("Settings._91_PriceForLitre"), null, L("Settings._91_PriceForLitre"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames._95_PriceForLitre, "0", L("Settings._95_PriceForLitre"), null, L("Settings._95_PriceForLitre"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames._Diesel_PriceForLitre, "0", L("Settings._Diesel_PriceForLitre"), null, L("Settings._Diesel_PriceForLitre"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.CompanyName, "", L("Settings.CompanyName"), invoiceSettings, L("Settings.CompanyName"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.TaxNumber, "", L("Settings.TaxNumber"), invoiceSettings, L("Settings.TaxNumber"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.CompanyAddress, "", L("Settings.CompanyAddress"), invoiceSettings, L("Settings.CompanyAddress"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.RegNo, "", L("Settings.RegNo"), invoiceSettings, L("Settings.RegNo"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.TaxNumberLength, "15", L("Settings.TaxNumberLength"), publicSettings, L("Settings.TaxNumberLength"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.RegNoLength, "15", L("Settings.RegNoLength"), publicSettings, L("Settings.RegNoLength"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.BankAccountPrintText, "false", L("Settings.BankAccountPrintText"), publicSettings, L("Settings.BankAccountPrintText"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.ForceUpdate, "false", L("Settings.ForceUpdate"), publicSettings, L("Settings.ForceUpdate"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames._91color, "", L("Settings._91color"), null, L("Settings._91color"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames._95color, "", L("Settings._95color"), null, L("Settings._95color"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames._Dieselcolor, "", L("Settings._Dieselcolor"), null, L("Settings._Dieselcolor"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.ActivateTimeBetweenFuelTransaction, "false", L("Settings.ActivateTimeBetweenFuelTransaction"), fuelSettings, L("Settings.ActivateTimeBetweenFuelTransaction"),
                    SettingScopes.All, true),

                new SettingDefinition(AppSettingNames.TimeBetweenFuelTransaction, "5", L("Settings.TimeBetweenFuelTransaction"), fuelSettings, L("Settings.TimeBetweenFuelTransaction"),
                    SettingScopes.All, true)
            ];
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, SayarahConsts.LocalizationSourceName);
    }
}
