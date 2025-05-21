using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Authorization.Users;

namespace Sayarah.Application.Users.Dto
{
    [AutoMapFrom(typeof(UserDashboard))]
    public class UserDashboardDto : EntityDto<long>
    {
        public long? UserId { get; set; }
         
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
    }


    [AutoMapFrom(typeof(UserDashboard)) , AutoMapTo(typeof(UserDashboard))]
    public class ManageUserDashboardDto 
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public long? BranchId { get; set; }
        public long? ProviderId { get; set; }
        public EntityAction EntityAction { get; set; }
    }

}