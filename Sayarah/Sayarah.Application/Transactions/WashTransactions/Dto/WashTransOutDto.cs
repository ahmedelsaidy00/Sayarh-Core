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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Transactions.WashTransactions.Dto
{
    [AutoMapFrom(typeof(WashTransOut)), AutoMapTo(typeof(WashTransOut))]
    public class WashTransOutDto : FullAuditedEntityDto<long>
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

        public string BeforeCarPic { get; set; }


        public string FullBeforeCarPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeCarPic) && Utilities.CheckExistImage(12, BeforeCarPic))
                    return FilesPath.WashTransOut.ServerImagePath + BeforeCarPic;
                else
                    return FilesPath.WashTransOut.DefaultImagePath;
            }
        }

        public string AfterCarPic { get; set; }

        public string FullAfterCarPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterCarPic) && Utilities.CheckExistImage(12, AfterCarPic))
                    return FilesPath.WashTransOut.ServerImagePath + AfterCarPic;
                else
                    return FilesPath.WashTransOut.DefaultImagePath;
            }
        }


    }

    [AutoMapTo(typeof(WashTransOut))]
    public class CreateWashTransOutDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }

        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }

        public int Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string BeforeCarPic { get; set; }
        public string AfterCarPic { get; set; }
    }


    [AutoMapTo(typeof(WashTransOut))]
    public class UpdateWashTransOutDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public int Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string BeforeCarPic { get; set; }
        public string AfterCarPic { get; set; }
    }


    public class GetWashTransOutsPagedInput : DataTableInputDto
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
        public bool? IsProviderEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }


    public class GetWashTransOutsInput : PagedResultRequestDto
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