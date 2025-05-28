using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sayarah.Authorization.Users;
using static Sayarah.SayarahConsts;

namespace Sayarah.Web.Controllers
{
    [AbpMvcAuthorize]
    public class AdminController : SayarahControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        public AdminController(UserManager userManager, IRepository<User, long> userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [DisableAuditing]
        public async Task<ActionResult> Index()
        {
            if (AbpSession.UserId.HasValue && AbpSession.UserId.Value > 0)
            {
                IList<string> _userRoles = await _userManager.GetRolesAsync(
                    await _userManager.FindByIdAsync(AbpSession.UserId.Value.ToString())
                );
                var _user = await _userRepository.GetAll()
                    .Include(x => x.Company.User)
                    .Include(x => x.Branch.User)
                    .Include(x => x.MainProvider.User)
                    .Include(x => x.Provider.User)
                    .FirstOrDefaultAsync(a=>a.Id == AbpSession.UserId.Value);

                if (_user == null || _user.IsActive == false) {
                    return RedirectToAction("Logout", "Account");
                }
                if (_user.CompanyId.HasValue == true && (_user.Company == null ||  _user.Company.IsDeleted == true || _user.Company.User.IsActive == false)) { 
                    return RedirectToAction("Logout", "Account");
                }
                if (_user.BranchId.HasValue == true && (_user.Branch == null ||_user.Branch.IsDeleted == true || _user.Branch.User.IsActive == false))
                {
                    return RedirectToAction("Logout", "Account");
                }


                if (_user.MainProviderId.HasValue == true && (_user.MainProvider == null || _user.MainProvider.IsDeleted == true || _user.MainProvider.User.IsActive == false))
                {
                    return RedirectToAction("Logout", "Account");
                }
                if (_user.ProviderId.HasValue == true && (_user.Provider == null || _user.Provider.IsDeleted == true || _user.Provider.User.IsActive == false))
                {
                    return RedirectToAction("Logout", "Account");
                }


                if (_userRoles != null && _userRoles.Count > 0)
                {
                    switch (_userRoles[0])
                    {
                        case RolesNames.Admin:
                            return View("~/app/admin/layout/layout.cshtml");
                        case RolesNames.Company:
                            return View("~/app/Company/layout/layout.cshtml");
                        case RolesNames.Branch:
                            return View("~/app/Branch/layout/layout.cshtml");

                        case RolesNames.Employee:
                            if (_user.BranchId.HasValue)
                                return View("~/app/Branch/layout/layout.cshtml");
                            else if (_user.CompanyId.HasValue)
                                return View("~/app/Company/layout/layout.cshtml");
                            else if (_user.MainProviderId.HasValue)
                                return View("~/app/MainProvider/layout/layout.cshtml");
                            else  
                                return View("~/app/Provider/layout/layout.cshtml");

                        case RolesNames.MainProvider:
                            return View("~/app/MainProvider/layout/layout.cshtml");

                        case RolesNames.Provider:
                            return View("~/app/Provider/layout/layout.cshtml");
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}