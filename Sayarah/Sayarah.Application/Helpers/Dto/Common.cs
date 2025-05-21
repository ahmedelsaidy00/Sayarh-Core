using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System;
using System.Collections.Generic;

namespace Sayarah.Application.Helpers.Dto
{
    public class MostActiveDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ItemsCount { get; set; }
        public int ItemType { get; set; }
    }
    public class ExistJobDto
    {
        public long Id { get; set; }
        public string JobType { get; set; }
        public string JobArgs { get; set; }
        public short TryCount { get; set; }

    }
    public class IndexStatisticsDto
    {
        public int OldProjectsCount { get; set; }
        public int CurrentProjectsCount { get; set; }
        public int SoonProjectsCount { get; set; }
    }

    //public class GetDashboardStatisticsInput
    //{
    //    public ProjectType ProjectType { get; set; }
      
    //    public int Lang { get; set; }
    //}

    public class GetDashboardStatisticsOutput
    {
        public IndexStatisticsDto HomePageData { get; set; }
        public List<MostActiveDto> MostActiveItems { get; set; }
    }
   


    public class BestActives
    {
        public List<BestActiveDto> BestActivesUsers { get; set; }
    }

    public class BestActiveDto
    {
        public long ClientId { get; set; }
        public string Name { get; set; }
        public int AdvertisementsCount { get; set; }
    }


    public class BestActiveProducts
    {
        public List<BestActiveProductDto> BestActivesProducts { get; set; }
    }

    public class BestActiveProductDto
    {
        public long StoreId { get; set; }
        public string ProductNameAr { get; set; }
        public string ProductNameEn { get; set; }
        public int OrderCount { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
    }

    public class NotifyInputDto
    {
        public Veichle veichle { get; set; }
        public long? BranchId { get; set; }
        public long? BranchUserId { get; set; }
        public long? CompanyId { get; set; }
        public long? CompanyUserId { get; set; }
        public long? DriverId { get; set; }
        public long? DriverUserId { get; set; }
        public long? VeichleId { get; set; }
        public long? ProviderId { get; set; }
        public long? ProviderUserId { get; set; }
        public long? MainProviderId { get; set; }
        public long? MainProviderUserId { get; set; }
        public long? WorkerId { get; set; }
        public long? WorkerUserId { get; set; }
                  
        public long? EntityId { get; set; }

        public string BranchName { get; set; }
        public string CompanyName { get; set; }

        public string DriverName { get; set; }
        public string VeichleName { get; set; }
        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }
        public string WorkerName { get; set; }

        public string MessageAr { get; set; }
        public string MessageEn { get; set; }

    }

    public class FrequencyOutput
    {
        public long? RowNumber { get; set; }
        public string BrancheNameAr { get; set; }
        public string FullPlateNumberAr { get; set; }
        public VeichleType VeichleType { get; set; }
        public string DriverName{ get; set; }
        public FuelType FuelType { get; set; }
        public int? RepeatsCount { get; set; }
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreationTime { get; set; }
        public int? DriversSum { get; set; }
        public decimal? Quantity { get; set; }
        public string ExcelTitle { get; set; }
        public string FullPlateNumber { get; set; }
        public string BrancheNameEn { get; set; }
        public string BrandNameEn { get; set; }
        public string BrandNameAr { get; set; }
        public bool IsDeleted { get; set; }

    }
    public class FrequencyOutputDto
    {
        public long? RowNumber { get; set; }
        public string BrancheName { get; set; }
        public string FullPlateNumber { get; set; }
        public int? DriversSum { get; set; }
        public string VeichleTypeString { get; set; }
        public string BrandName { get; set; }
        public string FuelTypeString { get; set; }
        public int? RepeatsCount { get; set; }
        public decimal? Quantity { get; set; }
        public decimal Cost { get; set; }
        public string IsActiveString { get; set; }
        public string CreationTime { get; set; }
        public string ExcelTitle { get; set; }

    }
    public class ConsumptionOutput
    {
        public long? RowNumber { get; set; }
        public string BrancheNameAr { get; set; }
        public string BrancheNameEn { get; set; }
        public string FullPlateNumber { get; set; }

        public string FullPlateNumberAr { get; set; }
        public int? DriversSum { get; set; }
        public FuelType FuelType { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Cost { get; set; }

        public bool IsActive { get; set; }
        public DateTime? CreationTime { get; set; }
        public decimal? ConsumptionRate { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ConsumptionOutputDto
    {
        public long? RowNumber { get; set; }
        public string BrancheName { get; set; }
        public string FullPlateNumber { get; set; }
        public int? DriversSum { get; set; }
        public string FuelTypeString { get; set; }
        public int? RepeatsCount { get; set; }
        public string IsActiveString { get; set; }
        public decimal? Quantity { get; set; }
        public string ConsumptionRate { get; set; }
        public string CreationTime { get; set; }
    }
    public class VeichleConsumptionOutput
    {
        public long? RowNumber { get; set; }
        public string BrancheNameAr { get; set; }
        public string BrancheNameEn { get; set; }
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Cost { get; set; }
        public bool IsDeleted { get; set; }
        public FuelType FuelType { get; set; }
    }
    public class VeichleConsumptionOutputDto
    {
        public long? RowNumber { get; set; }
        public string BrancheName { get; set; }
        public string FullPlateNumber { get; set; }
        public string FuelTypeString { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Cost { get; set; }
    }

}
