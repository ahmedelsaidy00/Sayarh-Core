using Abp.Localization.Sources;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Web.Mvc.Views;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Sayarah.Web.Views
{
    public abstract class SayarahWebViewPageBase<TModel> : RazorPage<TModel>
    {
        public ILocalizationManager LocalizationManager { get; set; }
        public IAbpSession AbpSession { get; set; }

        protected ILocalizationSource L => LocalizationManager.GetSource("Sayarah");

        // Optional: additional helper methods or properties
    }
}