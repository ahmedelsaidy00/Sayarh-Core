using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users.Dto;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(Worker)) , AutoMapTo(typeof(Worker))]
    public class WorkerDto : FullAuditedEntityDto<long>
    {
        public long? UserId { get; set; }
        public UserDto User { get; set; }
        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(8, "400x400_" + Avatar))
                    return FilesPath.Workers.ServerImagePath + "400x400_" + Avatar;
                else
                    return FilesPath.Workers.DefaultImagePath;
            }
        }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
    }

    [AutoMapFrom(typeof(Worker)), AutoMapTo(typeof(Worker))]
    public class ApiWorkerDto : EntityDto<long>
    {
        public long? UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(8, "400x400_" + Avatar))
                    return FilesPath.Workers.ServerImagePath + "400x400_" + Avatar;
                else
                    return FilesPath.Workers.DefaultImagePath;
            }
        }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
    }

    [AutoMapTo(typeof(Worker))]
    public class CreateWorkerDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public string Avatar { get; set; }
        public string Notes { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }

        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }
    }

 
    [AutoMapTo(typeof(Worker))]
    public class UpdateWorkerDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public long? ProviderId { get; set; }
        public string Avatar { get; set; }
        public string Notes { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }

        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }

    }

     
    public class GetWorkersPagedInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProviderName { get; set; }
        public string EmailAddress { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }

        public bool? IsEmployee { get; set; }
        public bool? IsActive { get; set; }
        public List<long> BranchesIds { get; set; }
    }
 
  
    public class GetWorkersInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public bool MaxCount { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

    }

    [AutoMapFrom(typeof(Worker)), AutoMapTo(typeof(Worker))]
    public class UpdateWorkerProfileInput : EntityDto<long>
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
    }

}