using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using Sayarah.Helpers.Enums;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Sayarah.Security
{
    public class AppSession : ClaimsAbpSession, ITransientDependency
    {
        public AppSession(IPrincipalAccessor principalAccessor, IMultiTenancyConfig multiTenancy, ITenantResolver tenantResolver,
            IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider)
            : base(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider)
        { }
    
        public UserTypes? UserType
        {
            get
            {
                if (!(Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal))
                {
                    return null;
                }

                var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.UserType);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return null;
                }

                return (UserTypes)Enum.Parse(typeof(UserTypes), claim.Value, true) ;
            }
        }

        public long? CompanyId
        {
            get
            {
                if (!(Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal))
                {
                    return null;
                }

                var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.CompanyId);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return null;
                }

                return claim.Value != null ? Convert.ToInt64(claim.Value) : 0;
            }
        }

        public long? BranchId
        {
            get
            {
                if (!(Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal))
                {
                    return null;
                }

                var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.BranchId);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return null;
                }

                return claim.Value != null ? Convert.ToInt64(claim.Value) : 0;
            }
        }

        public long? MainProviderId
        {
            get
            {
                if (!(Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal))
                {
                    return null;
                }

                var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.MainProviderId);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return null;
                }

                return claim.Value != null ? Convert.ToInt64(claim.Value) : 0;
            }
        }
        public long? ProviderId
        {
            get
            {
                if (!(Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal))
                {
                    return null;
                }

                var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.ProviderId);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return null;
                }

                return claim.Value != null ? Convert.ToInt64(claim.Value) : 0;
            }
        }
    }
}
