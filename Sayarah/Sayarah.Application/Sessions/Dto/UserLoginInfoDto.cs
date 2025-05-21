using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Helpers;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Sessions.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserLoginInfoDto : EntityDto<long>
    {
        public UserTypes UserType { get; set; }
        public string MainColor { get; set; }
        public bool DarkMode { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string MainProviderName { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string UserRole { get; set; }

        public string Avatar { get; set; }
        public string FuelTypes { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public string AvatarPath
        {
            get
            {

                switch (UserType)
                {
                    case UserTypes.Admin:
                        break;
                    case UserTypes.Client:
                        break;
                    case UserTypes.Employee:
                        break;
                    case UserTypes.Company:
                        if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(4, "600x600_" + Avatar))
                            return FilesPath.Companies.ServerImagePath + "600x600_" + Avatar;
                        else
                            return FilesPath.Companies.DefaultImagePath;
                       
                    case UserTypes.Branch:
                        break;
                    case UserTypes.Driver:
                        break;
                    case UserTypes.Provider:
                        break;
                    case UserTypes.Worker:
                        break;
                    case UserTypes.MainProvider:
                        if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(7, "600x600_" + Avatar))
                            return FilesPath.Providers.ServerImagePath + "600x600_" + Avatar;
                        else
                            return FilesPath.Providers.DefaultImagePath;
                    default:
                        if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(1, "400x400_" + Avatar))
                            return FilesPath.Users.ServerImagePath + "400x400_" + Avatar;
                        else
                            return FilesPath.Users.DefaultImagePath;
                        
                }

                return FilesPath.Users.DefaultImagePath;


            }
        }
        public bool InPackage { get; set; }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public bool AddExternalInvoice { get; set; }

        public List<long> BranchesIds { get; set; }

        //public bool IsSubscribed { get; set; }
        //public SessionSubscriptionDto Subscription { get; set; }


    }

    public class SessionSubscriptionDto
    {
        public DateTime? EndDate { get; set; }
        public long? PackageId { get; set; } 
        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }
        public int VeichlesCount { get; set; } // عدد السيارات الحالية
        public bool AttachNfc { get; set; }
        public PackageType? PackageType { get; set; }
        public bool AutoRenewal { get; set; }
    }
}
