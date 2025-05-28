using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Abp.UI;
using Microsoft.AspNet.Identity;

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
                throw new UserFriendlyException(L("FormIsNotValidMessage"));
            }
        }
        //uncommentthis method if you want to use it
        //protected void CheckErrors(IdentityResult identityResult)
        //{
        //    identityResult.CheckErrors(LocalizationManager);
        //}
    }
}