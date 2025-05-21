using Abp.Auditing;
using Abp.Domain.Repositories;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Sessions.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Packages;
using Sayarah.Providers;
using Sayarah.Security;
using System.Globalization;

namespace Sayarah.Application.Sessions
{
    public class SessionAppService : SayarahAppServiceBase, ISessionAppService
    {
        private readonly IObjectMapper _objectMapper;
        public AppSession AppSession { get; set; }
        private readonly IRepository<Subscription, long> _subscriptionRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<UserDashboard, long> _userDashboardRepository;
        private readonly IRepository<User, long> _userRepository;

        public SessionAppService(
            IObjectMapper objectMapper,
            IRepository<Subscription, long> subscriptionRepository,
            IRepository<Provider, long> providerRepository,
            IRepository<MainProvider, long> mainProviderRepository,
            IRepository<Company, long> companyRepository,
            IRepository<UserDashboard, long> userDashboardRepository,
            IRepository<User, long> userRepository
            )
        {
            _objectMapper = objectMapper;
            _subscriptionRepository = subscriptionRepository;
            _providerRepository = providerRepository;
            _mainProviderRepository = mainProviderRepository;
            _companyRepository = companyRepository;
            _userDashboardRepository = userDashboardRepository;
            _userRepository = userRepository;
        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            var output = new GetCurrentLoginInformationsOutput();

            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            if (AbpSession.UserId.HasValue)
            {
                var user = await UserManager.GetUserByIdAsync(AbpSession.UserId.Value);
                output.User = _objectMapper.Map<UserLoginInfoDto>(await GetCurrentUserAsync());
                var roles = await UserManager.GetRolesAsync(user);
                if (roles != null && roles.Count > 0)
                {
                    output.User.UserRole = roles[0];
                }

                output.User.CompanyId = AppSession.CompanyId;
                output.User.BranchId = AppSession.BranchId;
                output.User.ProviderId = AppSession.ProviderId;
                output.User.MainProviderId = AppSession.MainProviderId;

                if (output.User.CompanyId > 0)
                {
                    var subscriptionPackageCount = await _subscriptionRepository.CountAsync(at => at.CompanyId == output.User.CompanyId && DateTime.Now <= at.EndDate && at.IsPaid);
                    if (subscriptionPackageCount > 0)
                    {
                        output.User.InPackage = true;
                    }
                }

                if (output.User.UserType == UserTypes.Provider)
                {
                    var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == output.User.ProviderId);
                    if (provider != null)
                    {
                        output.User.IsFuel = provider.IsFuel;
                        output.User.IsOil = provider.IsOil;
                        output.User.IsClean = provider.IsClean;
                        output.User.IsMaintain = provider.IsMaintain;
                        output.User.AddExternalInvoice = provider.AddExternalInvoice;
                        output.User.Avatar = provider.Avatar;
                        output.User.FuelTypes = provider.FuelTypes;
                    }
                }

                if (output.User.UserType == UserTypes.MainProvider)
                {
                    var main = await _mainProviderRepository.FirstOrDefaultAsync(a => a.Id == output.User.MainProviderId);
                    if (main != null)
                    {
                        output.User.MainProviderName = currentCulture.Name.Contains("ar") ? main.NameAr : main.NameEn;
                        output.User.IsFuel = main.IsFuel;
                        output.User.IsClean = main.IsClean;
                        output.User.IsMaintain = main.IsMaintain;
                        output.User.AddExternalInvoice = main.AddExternalInvoice;
                        output.User.Avatar = main.Avatar;
                    }
                }

                if (output.User.UserType == UserTypes.Company)
                {
                    var company = await _companyRepository.FirstOrDefaultAsync(a => a.Id == output.User.CompanyId);
                    if (company != null)
                    {
                        output.User.CompanyName = currentCulture.Name.Contains("ar") ? company.NameAr : company.NameEn;
                        output.User.IsFuel = company.IsFuel;
                        output.User.IsClean = company.IsClean;
                        output.User.IsMaintain = company.IsMaintain;
                        output.User.Avatar = company.Avatar;
                    }
                }

                if (output.User.UserType == UserTypes.Employee)
                {
                    List<long> _ids = new List<long>();

                    if (output.User.MainProviderId > 0)
                    {
                        var users = await _userDashboardRepository.GetAll().Where(a => a.UserId == output.User.Id && a.ProviderId.HasValue).ToListAsync();
                        if (users != null && users.Count > 0)
                        {
                            _ids = users.Select(a => a.ProviderId.Value).ToList();
                        }

                        var main = await _mainProviderRepository.FirstOrDefaultAsync(a => a.Id == output.User.MainProviderId);
                        if (main != null)
                        {
                            output.User.MainProviderName = currentCulture.Name.Contains("ar") ? main.NameAr : main.NameEn;
                            output.User.AddExternalInvoice = main.AddExternalInvoice;
                            output.User.IsFuel = main.IsFuel;
                            output.User.IsClean = main.IsClean;
                            output.User.IsMaintain = main.IsMaintain;
                        }
                    }
                    else if (output.User.CompanyId > 0)
                    {
                        var users = await _userDashboardRepository.GetAll().Where(a => a.UserId == output.User.Id && a.BranchId.HasValue).ToListAsync();
                        if (users != null && users.Count > 0)
                        {
                            _ids = users.Select(a => a.BranchId.Value).ToList();
                        }

                        var company = await _companyRepository.FirstOrDefaultAsync(a => a.Id == output.User.CompanyId);
                        if (company != null)
                        {
                            output.User.CompanyName = currentCulture.Name.Contains("ar") ? company.NameAr : company.NameEn;
                            output.User.IsFuel = company.IsFuel;
                            output.User.IsClean = company.IsClean;
                            output.User.IsMaintain = company.IsMaintain;
                        }
                    }

                    output.User.BranchesIds = _ids;
                }
            }

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = _objectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }
            return output;
        }
    }
}