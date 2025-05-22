using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Transactions;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Transactions.FuelTransactions.Dto
{
    [AutoMapFrom(typeof(FuelTransOut)), AutoMapTo(typeof(FuelTransOut))]
    public class FuelTransOutDto : FullAuditedEntityDto<long>
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


        public decimal Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string BeforeBoxPic { get; set; }
        public string FullBeforeBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeBoxPic) && Utilities.CheckExistImage(9, BeforeBoxPic))
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeBoxPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string AfterBoxPic { get; set; }
        public string FullAfterBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterBoxPic) && Utilities.CheckExistImage(9, AfterBoxPic))
                    return FilesPath.FuelTransOut.ServerImagePath + AfterBoxPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string BeforeCounterPic { get; set; }
        public string FullBeforeCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeCounterPic) && Utilities.CheckExistImage(9, BeforeCounterPic))
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeCounterPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string AfterCounterPic { get; set; }
        public string FullAfterCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterCounterPic) && Utilities.CheckExistImage(9, AfterCounterPic))
                    return FilesPath.FuelTransOut.ServerImagePath + AfterCounterPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }

        public bool Completed { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public virtual string CompanyInvoiceCode { get; set; } //  رقم فاتورة الشركة
        public bool InvoiceStatus { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual string QrCode { get; set; }

        public virtual CancelNote CancelNote { get; set; }
        public string MappedFullPlateNumber { get; set; }
    }


    [AutoMapFrom(typeof(FuelTransOut)), AutoMapTo(typeof(FuelTransOut))]
    public class ApiFuelTransOutDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCompanyName { get; set; }
        public long? VeichleId { get; set; }

        // public ApiVeichleDto Veichle { get; set; }
        public decimal Quantity { get; set; } // litre
        public decimal Price { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool Completed { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool InvoiceStatus { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote CancelNote { get; set; }
        public virtual string CancelReason { get; set; }
    }



    [AutoMapTo(typeof(FuelTransOut))]
    public class CreateFuelTransOutDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }

        public long? DriverId { get; set; }

        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }

        public decimal Quantity { get; set; } // litre
        public string Code { get; set; }

        public decimal Price { get; set; }
        public string BeforeBoxPic { get; set; }
        public string AfterBoxPic { get; set; }
        public string BeforeCounterPic { get; set; }
        public string AfterCounterPic { get; set; }
        public bool Completed { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool InvoiceStatus { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote? CancelNote { get; set; }
        public OperationType? OperationType { get; set; }
        public virtual string CancelReason { get; set; }
    }


    [AutoMapTo(typeof(FuelTransOut))]
    public class UpdateFuelTransOutDto : EntityDto<long>
    {
        //public long? BranchId { get; set; }
        //public long? VeichleId { get; set; }
        //public long? DriverId { get; set; }
        //public long? ProviderId { get; set; }
        //public long? WorkerId { get; set; }
        public decimal Quantity { get; set; } // litre
                                              // public string Code { get; set; }

        public decimal Price { get; set; }
        //public string BeforeBoxPic { get; set; }
        public string AfterBoxPic { get; set; }
        //public string BeforeCounterPic { get; set; }
        public string AfterCounterPic { get; set; }  
        public OperationType? OperationType { get; set; }

        public virtual string CancelReason { get; set; }
    }

    
    [AutoMapTo(typeof(FuelTransOut))]
    public class CancelFuelTransOutDto : EntityDto<long>
    { 
        public CancelNote? CancelNote { get; set; }
        public string CancelReason { get; set; }
    }




    public class GetFuelTransOutsPagedInput : DataTableInputDto
    {
        public string ExcelTitle { get; set; }

        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }

        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public decimal? Quantity { get; set; } // litre
        public string Code { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string VeichleName { get; set; }
        public string DriverName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string WorkerName { get; set; }

        public decimal? Price { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool? InvoiceStatus { get; set; }

        public long? BrandId { get; set; }

        public long? ModelId { get; set; }



        public bool? IsProviderEmployee { get; set; }
        public bool? IsCompanyEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote? CancelNote { get; set; }        public virtual string CancelReason { get; set; }

    }


    public class GetFuelTransOutsInput : PagedResultRequestDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }

        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public decimal Quantity { get; set; } // litre
        public string Code { get; set; }
        public decimal Price { get; set; }
        public bool MaxCount { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool? InvoiceStatus { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote? CancelNote { get; set; }
        public virtual string CancelReason { get; set; }
    }


    public class GetFuelPriceInput
    {
        public long? VeichleId { get; set; }
        public FuelType FuelType { get; set; }
    }


    public class UpdateVeichleMoneyBalanceInput
    {
        public long? VeichleId { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
    }


    public class GetFuelTransoutOutput
    {
        public List<FuelTransOutDto> Transactions { get; set; }
        public int AllCount { get; set; }
        public int FilterCount { get; set; }
        public float TotalRates { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal TotalQuantity { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote? CancelNote { get; set; }
    }



    //public class RequestFuelExcelDtoInput : ExcelBaseInput
    //{
    //    public string ExcelTitle { get; set; }

    //    public long? Id { get; set; }
    //    public long? BranchId { get; set; }
    //    public long? CompanyId { get; set; }
    //    public long? VeichleId { get; set; }

    //    public long? MainProviderId { get; set; }
    //    public long? DriverId { get; set; }
    //    public long? ProviderId { get; set; }
    //    public long? WorkerId { get; set; }
    //    public decimal? Quantity { get; set; } // litre
    //    public string Code { get; set; }
    //    public string CompanyName { get; set; }
    //    public string BranchName { get; set; }
    //    public string VeichleName { get; set; }
    //    public string DriverName { get; set; }
    //    public string MainProviderName { get; set; }
    //    public string ProviderName { get; set; }
    //    public string WorkerName { get; set; }

    //    public decimal? Price { get; set; }

    //    public FuelType? FuelType { get; set; }
    //    public decimal FuelPrice { get; set; } // سعر لتر الوقود
    //    public string InvoiceCode { get; set; } // رقم الفاتورة
    //    public bool? InvoiceStatus { get; set; }

    //    public long? BrandId { get; set; }

    //    public long? ModelId { get; set; }


    //    public bool? IsProviderEmployee { get; set; }


    //    public bool? IsCompanyEmployee { get; set; }
    //    public List<long> BranchesIds { get; set; }
    //}



    public class RequestFuelExcelDtoInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }        public string FullPeriodFromString { get; set; }        public string FullPeriodToString { get; set; }
        public string Date { get; set; }
        public int? TransactionType { get; set; }
        public FuelType? FuelType { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? WorkerId { get; set; }

        public bool? IsProviderEmployee { get; set; }
        public bool? IsCompanyEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

        public bool? NewTransactions { get; set; }
        public bool? MaxCount { get; set; }
        public bool? Paginated { get; set; }        public int SkipCount { get; set; }        public int MaxResultCount { get; set; }        public long? Id { get; set; }
        public string Code { get; set; }
        public decimal? Quantity { get; set; } // litre
        public string Notes { get; set; }        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string VeichleName { get; set; }
        public string DriverName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string WorkerName { get; set; }
        public decimal? Price { get; set; }        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool? InvoiceStatus { get; set; }        public long? BrandId { get; set; }
        public long? ModelId { get; set; }
        public bool? IsBranch { get; set; }

    }


    public class RequestFuelExcelDto
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public string Branch { get; set; }
        public string Veichle { get; set; }
        public string VeichleTypeString { get; set; }
        public string Driver { get; set; }
        public string Provider { get; set; }
        public string Worker { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string FuelType { get; set; }
        public string MainProviderName { get; set; }
        public string CreationTime { get; set; }
        public string ExcelTitle { get; set; }

    }

    public class RequestProviderFuelExcelDto
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public string Veichle { get; set; }
        public string Worker { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string FuelType { get; set; }
        public string CreationTime { get; set; }
        public string ExcelTitle { get; set; }

    }

    public class RequestFuelExcelCompanyDto
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public string Veichle { get; set; }
        public string Branch { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string FuelType { get; set; }
        public string CreationTime { get; set; }
        public string ExcelTitle { get; set; }

    }


    public class AdminRequestFuelExcelDto
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public string MainProvider { get; set; }
        public string Provider { get; set; }
        public string Company { get; set; }
        public string Branch { get; set; }
        public string Veichle { get; set; }
        public string Driver { get; set; } 
        public string Worker { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string FuelType { get; set; }
        public string CreationTime { get; set; }
        public string ExcelTitle { get; set; }
    }




    public class RequestFuelExcelOptionsDto
    {        public string ProviderName { get; set; }
        public string ExcelType { get; set; }
        public string ExcelDate { get; set; }

        public List<RequestFuelExcelOptionKeyValue> KeyValues { get; set; }

    }


    public class RequestFuelExcelOptionKeyValue
    {

        public string Key { get; set; }
        public string Value { get; set; }

    }



    public class GetBranchConsumptionReportOutput
    {
        public List<BranchConsumption> Transactions { get; set; }
        public int AllCount { get; set; }
        public int FilterCount { get; set; } 
        public decimal TotalPrice { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalBranchBalance { get; set; }

    }

    public class BranchConsumption
    {
        public long BranchId { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public FuelType TransFuelType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BranchFuelBalance { get; set; }
        public bool BranchStatus { get; set; } 
        public bool BranchIsDeleted { get; set; }
    }

    public class BranchConsumptionExcel
    {
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string TransFuelType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BranchFuelBalance { get; set; }
        public string BranchStatus { get; set; }
    }


    [AutoMapTo(typeof(FuelTransOut))]
    public class UpdateFuelAndQuantityDto : EntityDto<long>
    {
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; }
        public decimal FuelPrice { get; set; }
    }

    public class NewFuelTransOutDto : FullAuditedEntityDto<long>
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
        public decimal Quantity { get; set; } // litre
        public string Code { get; set; }
        public decimal Price { get; set; }
        public string BeforeBoxPic { get; set; }
        public string FullBeforeBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeBoxPic) && Utilities.CheckExistImage(9, BeforeBoxPic))
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeBoxPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string AfterBoxPic { get; set; }
        public string FullAfterBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterBoxPic) && Utilities.CheckExistImage(9, AfterBoxPic))
                    return FilesPath.FuelTransOut.ServerImagePath + AfterBoxPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string BeforeCounterPic { get; set; }
        public string FullBeforeCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeCounterPic) && Utilities.CheckExistImage(9, BeforeCounterPic))
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeCounterPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }
        public string AfterCounterPic { get; set; }
        public string FullAfterCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterCounterPic) && Utilities.CheckExistImage(9, AfterCounterPic))
                    return FilesPath.FuelTransOut.ServerImagePath + AfterCounterPic;
                else
                    return FilesPath.FuelTransOut.DefaultImagePath;
            }
        }

        public bool Completed { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public virtual string CompanyInvoiceCode { get; set; } //  رقم فاتورة الشركة
        public bool InvoiceStatus { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote CancelNote { get; set; }
        public string MappedFullPlateNumber { get; set; }
        public string VeichleTypeString { get; set; }
    }

    public class GetProviderConsumptionReportOutput
    {
        public List<ProviderConsumption> Transactions { get; set; }        public int AllCount { get; set; }        public int FilterCount { get; set; }
        public decimal TotalPrice { get; set; }        public decimal TotalQuantity { get; set; }        public decimal TotalProviderBalance { get; set; }    }
    public class ProviderConsumption
    {        public long ProviderId { get; set; }        public string MainProviderName { get; set; }        public string ProviderName { get; set; }        public FuelType TransFuelType { get; set; }        public decimal TotalQuantity { get; set; }        public decimal TotalPrice { get; set; }        public decimal ProviderFuelBalance { get; set; }        public bool ProviderStatus { get; set; }
        public bool ProviderIsDeleted { get; set; }        public decimal FuelPrice { get; set; }
    }
    public class ProviderConsumptionExcel
    {        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string TransFuelType { get; set; }
        public string FuelPrice { get; set; }                public decimal BranchFuelBalance { get; set; }              public string TotalQuantity { get; set; }        public string TotalPrice { get; set; }
        public string BranchStatus { get; set; }    }

}