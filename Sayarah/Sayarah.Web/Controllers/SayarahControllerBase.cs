using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sayarah.Web.Controllers
{
    /// <summary>
    /// Derive all Controllers from this class.
    /// </summary>
    public abstract class SayarahControllerBase : AbpController
    {
        protected SayarahControllerBase()
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        }

        protected virtual void CheckModelState()
        {
            if (!ModelState.IsValid)
            {
                // Collect all validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                throw new UserFriendlyException(
                    L("FormIsNotValidMessage") + ": " + string.Join("; ", errors)
                );
            }
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}