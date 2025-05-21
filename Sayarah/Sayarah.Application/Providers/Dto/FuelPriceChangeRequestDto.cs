using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(FuelPriceChangeRequest)), AutoMapTo(typeof(FuelPriceChangeRequest))]
    public class FuelPriceChangeRequestDto : FullAuditedEntityDto<long>
    {
        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
              
        public string Code { get; set; }
        public FuelType FuelType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string FilePath { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(10, FilePath))
                    return FilesPath.FuelPriceChangeRequests.ServerImagePath + FilePath;
                else
                    return FilesPath.FuelPriceChangeRequests.DefaultImagePath;
            }
        }

        public ChangeRequestStatus Status { get; set; }
        public string CreatorUserName { get; set; }
    }


    [AutoMapFrom(typeof(FuelPriceChangeRequest)), AutoMapTo(typeof(FuelPriceChangeRequest))]
    public class ApiFuelPriceChangeRequestDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public ApiProviderDto Provider { get; set; }

        public string Code { get; set; }
        public FuelType FuelType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string FilePath { get; set; }
        public string FullFilePath { get; set; }
        public ChangeRequestStatus Status { get; set; }
    }


    [AutoMapTo(typeof(FuelPriceChangeRequest))]
    public class CreateFuelPriceChangeRequestDto
    {
        public long? ProviderId { get; set; } 

        public string Code { get; set; }
        public FuelType FuelType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string FilePath { get; set; } 
        public ChangeRequestStatus Status { get; set; }
    }


    [AutoMapTo(typeof(FuelPriceChangeRequest))]
    public class UpdateFuelPriceChangeRequestDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }

        public string Code { get; set; }
        public FuelType FuelType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string FilePath { get; set; }
        public ChangeRequestStatus Status { get; set; }
    }


    
    public class UpdateProviderFuelPrice  
    {
        public long? RequestId { get; set; }
        public long? ProviderId { get; set; }
        public FuelType FuelType { get; set; }
        public decimal NewPrice { get; set; }
    }



    public class GetFuelPriceChangeRequestsPagedInput : DataTableInputDto
    {

        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public string Code { get; set; }
        public FuelType? FuelType { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string FilePath { get; set; }
        public ChangeRequestStatus? Status { get; set; }

        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public string Name { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

    }


    public class GetFuelPriceChangeRequestsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public string ProviderName { get; set; }
        public long? ProviderId { get; set; }

        public string Code { get; set; }
        public FuelType? FuelType { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string FilePath { get; set; }
        public ChangeRequestStatus? Status { get; set; }


        public bool MaxCount { get; set; }
    }

 
}