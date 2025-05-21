using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Providers;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.ComponentModel.DataAnnotations.Schema;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Transactions.OilTransactions.Dto
{
    [AutoMapFrom(typeof(OilTransOut)), AutoMapTo(typeof(OilTransOut))]
    public class OilTransOutDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }
        public long? DriverId { get; set; }
        public DriverDto Driver { get; set; }

        public long? ProviderId { get; set; }
        public ApiProviderDto Provider { get; set; }
              
        public long? WorkerId { get; set; }
        public ApiWorkerDto Worker { get; set; }


        public int Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string CounterPic { get; set; }

        
        public string FullCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(CounterPic) && Utilities.CheckExistImage(13, CounterPic))
                    return FilesPath.OilTransOut.ServerImagePath + CounterPic;
                else
                    return FilesPath.OilTransOut.DefaultImagePath;
            }
        }
        
    }

    [AutoMapTo(typeof(OilTransOut))]
    public class CreateOilTransOutDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }

        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }

        public int Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string CounterPic { get; set; }
    }


    [AutoMapTo(typeof(OilTransOut))]
    public class UpdateOilTransOutDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public int Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string CounterPic { get; set; }
    }


    public class GetOilTransOutsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public int? Quantity { get; set; } // litre
        public string Code { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string VeichleName { get; set; }
        public string DriverName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string WorkerName { get; set; }

        public decimal? Price { get; set; }
       
    }


    public class GetOilTransOutsInput : PagedResultRequestDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public int Quantity { get; set; } // litre
        public string Code { get; set; }
        public decimal Price { get; set; }
        public bool MaxCount { get; set; }
    }
}