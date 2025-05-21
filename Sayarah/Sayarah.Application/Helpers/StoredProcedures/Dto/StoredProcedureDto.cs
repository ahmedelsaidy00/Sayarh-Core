

using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Helpers.StoredProcedures.Dto
{

    public class GetTransactionsReportInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }

        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }

        public string Date { get; set; }

        public int? TransactionType { get; set; }
        public int? FuelType { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? WorkerId { get; set; }

        public bool? IsProviderEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

        public bool? NewTransactions { get; set; }
        public bool? MaxCount { get; set; }
        public bool? Paginated { get; set; }

        public int? SkipCount { get; set; }
        public int? MaxResultCount { get; set; }
    }


    public class GetTransactionsReportOutput
    {
        public List<TransactionDto> Transactions { get; set; }

        public int? filterCount { get; set; }
        public int TotalCount { get; set; }

    }


    public class GetTransactionsReportPagedOutput
    {
        public List<TransactionDto> Transactions { get; set; }

        public int? filterCount { get; set; }
        public int TotalCount { get; set; }

    }


    public class TransactionDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }

        public string WorkerName { get; set; }
        public string DriverName { get; set; }
        public string VeichleName { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public string FullPlateNumber { get; set; }
        public string BrandName
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? BrandNameAr : BrandNameEn;
            }
        }
        public string BrandNameAr { get; set; }
        public string BrandNameEn { get; set; }
        public string ModelName
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? ModelNameAr : ModelNameEn;
            }
        }
        public string ModelNameAr { get; set; }
        public string ModelNameEn { get; set; }
        public VeichleType? VeichleType { get; set; }
        public TransOutTypes TransactionType { get; set; }
        public FuelType FuelType { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreationTime { get; set; }

        public FuelType? TransFuelType { get; set; }
        public decimal? FuelPrice { get; set; } // سعر لتر الوقود


        public string BeforeBoxPic { get; set; }
        public string FullBeforeBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeBoxPic) && Utilities.CheckExistImage(9, BeforeBoxPic) && TransactionType == TransOutTypes.Fuel)
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeBoxPic;
                else
                    return "";
            }
        }
        public string AfterBoxPic { get; set; }
        public string FullAfterBoxPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterBoxPic) && Utilities.CheckExistImage(9, AfterBoxPic) && TransactionType == TransOutTypes.Fuel)
                    return FilesPath.FuelTransOut.ServerImagePath + AfterBoxPic;
                else
                    return "";
            }
        }
        public string BeforeCounterPic { get; set; }
        public string FullBeforeCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(BeforeCounterPic) && Utilities.CheckExistImage(9, BeforeCounterPic) && TransactionType == TransOutTypes.Fuel)
                    return FilesPath.FuelTransOut.ServerImagePath + BeforeCounterPic;
                else
                    return "";
            }
        }
        public string AfterCounterPic { get; set; }
        public string FullAfterCounterPic
        {
            get
            {
                if (!string.IsNullOrEmpty(AfterCounterPic) && Utilities.CheckExistImage(9, AfterCounterPic) && TransactionType == TransOutTypes.Fuel)
                    return FilesPath.FuelTransOut.ServerImagePath + AfterCounterPic;
                else
                    return "";
            }
        }

        public string CounterPic { get; set; }


        public string FullCounterPic
        {
            get
            {

                if (TransactionType == TransOutTypes.Maintain)
                {


                    if (!string.IsNullOrEmpty(CounterPic) && Utilities.CheckExistImage(11, CounterPic))
                        return FilesPath.MaintainTransOut.ServerImagePath + CounterPic;
                    else
                        return "";
                }
                if (TransactionType == TransOutTypes.Oil)
                {
                    if (!string.IsNullOrEmpty(CounterPic) && Utilities.CheckExistImage(13, CounterPic))
                        return FilesPath.OilTransOut.ServerImagePath + CounterPic;
                    else
                        return "";
                }

                return "";
            }
        }



        public string BeforeCarPic { get; set; }


        public string FullBeforeCarPic
        {
            get
            {
                if (TransactionType == TransOutTypes.Wash)
                {
                    if (!string.IsNullOrEmpty(BeforeCarPic) && Utilities.CheckExistImage(12, BeforeCarPic))
                        return FilesPath.WashTransOut.ServerImagePath + BeforeCarPic;
                    else
                        return "";
                }
                return "";

            }
        }

        public string AfterCarPic { get; set; }

        public string FullAfterCarPic
        {
            get
            {
                if (TransactionType == TransOutTypes.Wash)
                {
                    if (!string.IsNullOrEmpty(AfterCarPic) && Utilities.CheckExistImage(12, AfterCarPic))
                        return FilesPath.WashTransOut.ServerImagePath + AfterCarPic;
                    else
                        return "";
                }
                return "";

            }
        }


    }

    public class MappedTransactionDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string InvoiceCode { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string DriverName { get; set; }
        public string VeichleName { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public string FullPlateNumber { get; set; }
        public string BrandName { get; set; }
        public string BrandNameAr { get; set; }
        public string BrandNameEn { get; set; }
        public string ModelName { get; set; }
        public string ModelNameAr { get; set; }
        public string ModelNameEn { get; set; }
        public VeichleType? VeichleType { get; set; }
        public TransOutTypes TransactionType { get; set; }
        public FuelType FuelType { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreationTime { get; set; }
        public FuelType? TransFuelType { get; set; }
        public decimal? FuelPrice { get; set; } // سعر لتر الوقود
        public string BeforeBoxPic { get; set; }
        public string FullBeforeBoxPic { get; set; }
        public string AfterBoxPic { get; set; }
        public string FullAfterBoxPic { get; set; }
        public string BeforeCounterPic { get; set; }
        public string FullBeforeCounterPic { get; set; }
        public string AfterCounterPic { get; set; }
        public string FullAfterCounterPic { get; set; }
        public string CounterPic { get; set; }
        public string FullCounterPic { get; set; }
        public string BeforeCarPic { get; set; }
        public string FullBeforeCarPic { get; set; }
        public string AfterCarPic { get; set; }
        public string FullAfterCarPic { get; set; }

    }


    public class GetInvoicesReportOutput
    {
        public List<InvoiceReportDto> Invoices { get; set; }
    }

    public class InvoiceReportDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public long? ProviderId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? CreationTime { get; set; }
    }



    public class GetVouchersReportOutput
    {
        public List<VoucherReportDto> Vouchers { get; set; }
    }

    public class VoucherReportDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public long? ProviderId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreationTime { get; set; }
    }

    public enum FiltrationTimes
    {
        None = 0,
        Today = 1,
        LastWeek = 2,
        LastMonth = 3,
        LastYear = 4
    }
    public class GetDashboardStatisticsInput : PagedAndSortedResultRequestDto
    {
        public FiltrationTimes FiltrationTime { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }
        public int? FuelType { get; set; }
        public string Date { get; set; }
        public long? CityId { get; set; }
        public long? WorkerId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public int Lang { get; set; }
        public int? Year { get; set; }
        public bool? IsEmployee { get; set; }
        public bool? IsProviderEmployee { get; set; }
        public string ProviderIds { get; set; }
        public bool? MaxCount { get; set; }


    }

    public class GetMonthlyFuelConsumptionReportInput : PagedAndSortedResultRequestDto
    {         
        public long? CompanyId { get; set; }
        public string Month { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }




    public class GetProviderDashboardStatisticsOutput
    {
        public int InvoicesCount { get; set; }
        public decimal InvoicesAmount { get; set; }
        public decimal VouchersAmount { get; set; }
        public int ActiveWorkers { get; set; }
        public int NotActiveWorkers { get; set; }
        public int ActiveProviders { get; set; }
        public int NotActiveProviders { get; set; }

        public decimal FuelTransactionAmount { get; set; }
        public int FuelTransactionCount { get; set; }
        public int WashTransactionCount { get; set; }
        public decimal WashTransactionAmount { get; set; }

        public int MaintainTransactionCount { get; set; }
        public decimal MaintainTransactionAmount { get; set; }

        public int OilTransactionCount { get; set; }
        public decimal OilTransactionAmount { get; set; }


        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }

    }



    public class GetAdminDashboardStatisticsOutput
    {
        public int CompaniesCount { get; set; }
        public int BranchesCount { get; set; }
        public int MainProvidersCount { get; set; }
        public int ProvidersCount { get; set; }
        public int DriversCount { get; set; }
        public int VeichlesCount { get; set; }
        public int WorkersCount { get; set; }
        public int PendingCompaniesTransactionsCount { get; set; }
        public int FuelChangePricesCount { get; set; }


        public int InvoicesCount { get; set; }
        public decimal InvoicesAmount { get; set; }
        public decimal FuelTransactionAmount { get; set; }
        public int FuelTransactionCount { get; set; }
        public int WashTransactionCount { get; set; }
        public decimal WashTransactionAmount { get; set; }

        public int MaintainTransactionCount { get; set; }
        public decimal MaintainTransactionAmount { get; set; }

        public int OilTransactionCount { get; set; }
        public decimal OilTransactionAmount { get; set; }


        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }


        public int LiveSubscriptionCount { get; set; }
        public int PendingSubscriptionCount { get; set; }
        public int ExpiredSubscriptionCount { get; set; }
        public decimal SubscriptionAmount { get; set; }




    }



    public class GetBranchDashboardStatisticsOutput
    {
        public int DriversCount { get; set; }
        public int ActiveDriversCount { get; set; }
        public int NotActiveDriversCount { get; set; }
        public int VeichlesCount { get; set; }
        public int ActiveVeichlesCount { get; set; }
        public int NotActiveVeichlesCount { get; set; }
        public int ActiveChipsCount { get; set; }
        public int NotActiveChipsCount { get; set; }
        public decimal WalletAmount { get; set; }
        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }

    }


    public class GetCompanyDashboardStatisticsOutput
    {
        public GetCompanyDashboardStatisticsObject CompanyStatistics { get; set; }
        public List<StatisticsBranchDto> Branches { get; set; }
    }

    public class GetCompanyDashboardStatisticsObject
    {
        public int BranchesCount { get; set; }
        public int ActiveBranchesCount { get; set; }
        public int NotActiveBranchesCount { get; set; }
        public int DriversCount { get; set; }
        public int ActiveDriversCount { get; set; }
        public int NotActiveDriversCount { get; set; }
        public int VeichlesCount { get; set; }
        public int ActiveVeichlesCount { get; set; }
        public int NotActiveVeichlesCount { get; set; }
        public int ActiveChipsCount { get; set; }
        public int NotActiveChipsCount { get; set; }
        public bool HasOldSubscription { get; set; }
        public decimal WalletAmount { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }

        public string PackageName { get; set; }
        public string PackageNameEn { get; set; }
        public string _PackageName { get; set; }

        public decimal BranchesFuelWalletAmount { get; set; }
        public decimal BranchesCleanWalletAmount { get; set; }
        public decimal BranchesMaintainWalletAmount { get; set; }
        public decimal TotalAmount { get; set; }


    }



    public class GetFuelTypesStatisticsOutput
    {
        public FuelTypesStatisticsObject FuelTypesStatistics { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class FuelTypesStatisticsObject
    {
        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }
        public decimal Fuel91Quantity { get; set; }
        public decimal Fuel95Quantity { get; set; }
        public decimal FuelDesielQuantity { get; set; }
    }


    [AutoMapFrom(typeof(Branch)), AutoMapTo(typeof(Branch))]
    public class StatisticsBranchDto : EntityDto<long>
    {

        public string Name { get; set; }
        public decimal WalletAmount { get; set; }
    }


    public class UpdateInvoiceCodeInFuelTransOutsInput
    {
        public string InvoiceCode { get; set; }
        public string Ids { get; set; }
    }



    public class GetProviderStatmentReportOutput
    {
        public List<ProviderStatmentDto> Invoices { get; set; }
    }

    public class ProviderStatmentDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Note { get; set; }
        public decimal RecordPrice { get; set; }
        public decimal Balance { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? TransactionDate { get; set; }
        public ProviderStatmentTypes StatmentType { get; set; }

        public string FilePath { get; set; }
        public string FullFilePath
        {
            get
            {
                if (StatmentType == ProviderStatmentTypes.Voucher)
                {
                    if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(18, FilePath))
                        return FilesPath.Vouchers.ServerImagePath + FilePath;
                    else
                        return null;
                }
                return null;
            }
        }
    }

    public enum ProviderStatmentTypes
    {
        Invoice = 1,
        Voucher = 2
    }



    public enum ExcelSource
    {
        AccountStatment = 0,
        FuelTransactions = 1,
        Revenue = 2,
        BankInfo = 3,
        BranchConsumption = 4
    }
    public class ProviderStatmentExcelDto
    {
        //public string Code { get; set; }
        public string CreationTime { get; set; }
        public string StatmentType { get; set; }
        public string DepitPrice { get; set; }
        public string CreditPrice { get; set; }
        public string Balance { get; set; }

    }



    public class ExportProviderStatmentInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }

        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }

        public int? TransactionType { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? WorkerId { get; set; }

        public long? Id { get; set; }

        public string Code { get; set; }

        public decimal? Price { get; set; }

        public FuelType? FuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool? InvoiceStatus { get; set; }

    }


    public class GetMonthlyFuelStatsByTypeOutput
    {
        public List<MonthlyFuelStats> Months { get; set; }
    }

    public class MonthlyFuelStats
    {
        public int Month { get; set; }
        public decimal _91_QuantityCount { get; set; }
        public decimal _91_PricesAmount { get; set; }

        public decimal _95_QuantityCount { get; set; }
        public decimal _95_PricesAmount { get; set; }

        public decimal Diesel_QuantityCount { get; set; }
        public decimal Diesel_PricesAmount { get; set; }

    }


    public class CopySubscriptionToTransactionsInput
    {
        public long SubscriptionId { get; set; }
        public SubscriptionTransactionType TransactionType { get; set; }
    }



    public class GetProviderRevenueOutput
    {
        public List<ProviderRevenue> List { get; set; }
        public decimal TotalTransactions { get; set; }
        public decimal TotalVouchers { get; set; }
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    
    public class GetMonthlyFuelConsumptionReportOutput
    {
         
        public List<GetTransactionsListOutput> Transactions { get; set; } 
        public decimal TotalRowsVatValue { get; set; } 
        public decimal TotalRowsWithoutVat { get; set; } 
        public decimal TotalRowsWithVat { get; set; } 
        public DateTime PeriodFrom { get; set; } 
        public DateTime PeriodTo { get; set; } 
    }
    
    public class GetTransactionsListOutput
    {
        public FuelType FuelType { get; set; }
        public decimal FuelPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalWithVat { get; set; }
        public decimal TotalWithoutVat { get; set; }
        public decimal VatValue { get; set; }
    }


    public class ProviderRevenue
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public decimal TransactionsAmount { get; set; }
        public decimal VouchersAmount { get; set; }
    }

    public class GetProviderRevenueExcelDtoInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public long? MainProviderId { get; set; }
        public int SkipCount { get; set; }
        public int MaxResultCount { get; set; }
    }

    public class AdminRevenueFuelExcelDto
    {
        public string Code { get; set; }
        public string NameAr { get; set; }

        public decimal TransactionsAmount { get; set; }
        public decimal VouchersAmount { get; set; }
        public decimal ClaimBalance { get; set; }
        public string ExcelTitle { get; set; }
    }



    public class GetCompanyStatmentReportOutput
    {
        public List<CompanyStatmentDto> Invoices { get; set; }
    }

    public enum CompanyStatmentTypes
    {
        Invoice = 1,
        Wallet = 2,
        Package = 3
    }

    public class CompanyStatmentDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Note { get; set; }
        public decimal RecordPrice { get; set; }
        public decimal Balance { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? TransactionDate { get; set; }
        public CompanyStatmentTypes StatmentType { get; set; }

        public string FilePath { get; set; }
        public string FullFilePath
        {
            get
            {
                if (StatmentType == CompanyStatmentTypes.Wallet)
                {
                    if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(15, "600x600_" + FilePath))
                        return FilesPath.Wallet.ServerImagePath + "600x600_" + FilePath;
                    else
                        return "";
                }
                else if (StatmentType == CompanyStatmentTypes.Package)
                {
                    if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(15, "600x600_" + FilePath))
                        return FilesPath.Wallet.ServerImagePath + "600x600_" + FilePath;
                    else
                        return "";
                }

                return null;
            }
        }
    }

    public class GetFrequencyStationInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }
        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public int? FuelType { get; set; }
        public int? VeichleType { get; set; }
        public long? CompanyId { get; set; }
        public string DateTimeNow { get; set; }
    }

    public class GetConsumptionInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }
        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public long? DriverId { get; set; }
        public int? FuelType { get; set; }
        public int? VeichleType { get; set; }
        public string DateTimeNow { get; set; }

    }
    public class GetVeichleConsumptionInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }
        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }

    }
    public class WalletsPendingTrans

    {
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public long? CompanyId { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }
    }
    public class WalletsPendingTransOutput
    {
        public decimal RecordPrice { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string CompanyName { get; set; }
        public string RecordCode { get; set; }
        public long? RecordId { get; set; }
        public string Note { get; set; }
        public long? CompanyId { get; set; }
    }

    public class MappedGetFuelTypesStatisticsOutput
    {
        public FuelTypesStatisticsObject FuelTypesStatistics { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class MappedFuelTypesStatisticsObject
    {
        public decimal Fuel91Amount { get; set; }
        public decimal Fuel95Amount { get; set; }
        public decimal FuelDesielAmount { get; set; }
        public decimal Fuel91Quantity { get; set; }
        public decimal Fuel95Quantity { get; set; }
        public decimal FuelDesielQuantity { get; set; }
    }


    //public class GetBranchConsumptionReportOutput
    //{
    //    public List<BranchConsumption> List { get; set; }
    //    public decimal TotalPrice { get; set; }
    //    public decimal TotalQuantity { get; set; }
    //    public int TotalCount { get; set; }
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //}




}
