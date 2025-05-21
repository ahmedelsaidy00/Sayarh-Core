using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Drivers;
using Sayarah.Lookups;
using Sayarah.Veichles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(Veichle)) , AutoMapTo(typeof(Veichle))]
    public class VeichleDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }

        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }
               
        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }

        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }


        //private string _FullPlateNumber;
        //public string FullPlateNumber
        //{
        //    get
        //    {
        //        if (VeichleType.Value == Helpers.Enums.VeichleType.HeavyEquipment)
        //            return "";
        //        else
        //            return _FullPlateNumber;
        //        //return _FullPlateNumber;
        //    }

        //    set { _FullPlateNumber = value; }
        //}


        public string BodyNumber { get; set; } 
        public string SimNumber { get; set; }
        public FuelType? FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }

        public decimal Fuel_In { get; set; }
        public decimal Fuel_Out { get; set; }
        public decimal Fuel_Balance { get; set; }

        public decimal Maintain_In { get; set; }
        public decimal Maintain_Out { get; set; }
        public decimal Maintain_Balance { get; set; } 
        public decimal Oil_In { get; set; }
        public decimal Oil_Out { get; set; }
        public decimal Oil_Balance { get; set; }

        public decimal Wash_In { get; set; }
        public decimal Wash_Out { get; set; }
        public decimal Wash_Balance { get; set; }


        public bool IsActive { get; set; }

        public long? FuelGroupId { get; set; }
        public FuelGroupDto FuelGroup { get; set; }

        public decimal MoneyBalance { get; set; }
        public decimal FuelLitreBalance { get; set; }
        public DateTime? MoneyBalanceStartDate { get; set; }
        public DateTime? MoneyBalanceEndDate { get; set; }


        public VeichleType? VeichleType { get; set; }
        public long? BrandId { get; set; }
        public BrandDto Brand { get; set; }

        public long? ModelId { get; set; }
        public ModelDto Model { get; set; }



        public string WorkingDays { get; set; }
        public string YearOfIndustry { get; set; }

        public ConsumptionType ConsumptionType { get; set; }
        public long? VeichleTripId { get; set; }
        public List<VeichleTripDto> VeichleTrips { get; set; }


        public long? DriverId { get; set; }
        public DriverDto Driver { get; set; }


        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }

        public string FullInternalNumberFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(InternalNumberFilePath) && Utilities.CheckExistImage(5, "600x600_" + InternalNumberFilePath))
                    return FilesPath.Veichles.ServerImagePath + "600x600_" + InternalNumberFilePath;
                else
                    return FilesPath.Veichles.DefaultImagePath;
            }
        }


        public bool ActivateTimeBetweenFuelTransaction { get; set; }
        public int TimeBetweenFuelTransaction { get; set; }
        public int DriversCount { get; set; }
        public string MappedFullPlateNumber { get; set; }

    }


    [AutoMapFrom(typeof(Veichle)), AutoMapTo(typeof(Veichle))]
    public class ApiVeichleDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }

        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }

        public decimal Fuel_Balance { get; set; }
        public decimal Maintain_Balance { get; set; }
        public decimal Oil_Balance { get; set; }
        public decimal Wash_Balance { get; set; }

        public FuelType? FuelType { get; set; }

        public string BrandName { get; set; }
        public string ModelName { get; set; }

        public long? DriverId { get; set; }
        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }
        public string WorkingDays { get; set; }
    }


    [AutoMapFrom(typeof(Veichle)), AutoMapTo(typeof(Veichle))]
    public class VeichleNumbersDto : EntityDto<long>
    {
        public string FullPlateNumber { get; set; }
    }

    [AutoMapFrom(typeof(Veichle)), AutoMapTo(typeof(Veichle))]
    public class ShortVeichleDto : EntityDto<long>
    {
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }
        public string BranchNameAr { get; set; }
        public long? DriverId { get; set; }
    }

    [AutoMapTo(typeof(Veichle))]
    public class CreateVeichleDto
    {
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }

        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }

        public string BodyNumber { get; set; }
        //public string Mark { get; set; }
        //public string Model { get; set; }
        public string SimNumber { get; set; }
        public FuelType? FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }
        public bool IsActive { get; set; }

        public long? FuelGroupId { get; set; }
        public decimal MoneyBalance { get; set; }
        public decimal FuelLitreBalance { get; set; }
        public DateTime? MoneyBalanceStartDate { get; set; }
        public DateTime? MoneyBalanceEndDate { get; set; }

        public VeichleType? VeichleType { get; set; }
        public long? BrandId { get; set; }

        public long? ModelId { get; set; }

        public string WorkingDays { get; set; }
        public string YearOfIndustry { get; set; }

        public ConsumptionType ConsumptionType { get; set; }

        public long? VeichleTripId { get; set; }
        public ManageVeichleTripDto Trip { get; set; }

        public long? DriverId { get; set; }
        public UpdateDriverDto Driver { get; set; }


        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }

        public bool AddDriver { get; set; }

    }


    [AutoMapTo(typeof(Veichle))]
    public class UpdateVeichleDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }

        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }

        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }


        public string BodyNumber { get; set; }
        //public string Mark { get; set; }
        //public string Model { get; set; }
        public string SimNumber { get; set; }
        public FuelType? FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }
        public bool IsActive { get; set; }
        public long? FuelGroupId { get; set; }

        public decimal MoneyBalance { get; set; }
        public decimal FuelLitreBalance { get; set; }
        public DateTime? MoneyBalanceStartDate { get; set; }
        public DateTime? MoneyBalanceEndDate { get; set; }

        public VeichleType? VeichleType { get; set; }
        public long? BrandId { get; set; }

        public long? ModelId { get; set; }

        public string WorkingDays { get; set; }
        public string YearOfIndustry { get; set; }

        public ConsumptionType ConsumptionType { get; set; }

        public long? VeichleTripId { get; set; }
        public ManageVeichleTripDto Trip { get; set; }

        public long? DriverId { get; set; }
        public UpdateDriverDto Driver { get; set; }

        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }
        public bool AddDriver { get; set; }

        public bool ActivateTimeBetweenFuelTransaction { get; set; }
        public int TimeBetweenFuelTransaction { get; set; }
    }

    [AutoMapTo(typeof(Veichle))]
    public class UpdateVeichleSimPicDto
    {
        public long DriverId { get; set; }
        public string PlateNumber { get; set; } 
        public string Number {
            get{

                //if (!string.IsNullOrEmpty(PlateNumber))
                //{
                //    return PlateNumber.Replace(" ", "");
                //}
                return PlateNumber;
            }
        }
        public string SimNumber { get; set; }

        public List<CreateVeichlePicDto> VeichleMedias { get; set; }

    }


    public class GetVeichlesPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }
        
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string BranchCode { get; set; }
        public string ModelName { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }

        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }

        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }

        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }


        public string BodyNumber { get; set; }
        //public string Mark { get; set; }
        //public string Model { get; set; }
        public string SimNumber { get; set; }
        public FuelType? FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }

        public decimal? Fuel_BalanceFrom { get; set; }
        public decimal? Fuel_BalanceTo { get; set; }
        public decimal? Maintain_BalanceFrom { get; set; }
        public decimal? Maintain_BalanceTo { get; set; }
        public decimal? Oil_BalanceFrom { get; set; }
        public decimal? Oil_BalanceTo { get; set; }
        public decimal? Wash_BalanceFrom { get; set; }
        public decimal? Wash_BalanceTo { get; set; }

        public bool? IsActive { get; set; }
        public long? FuelGroupId { get; set; }

        public VeichleType? VeichleType { get; set; }
        public long? BrandId { get; set; }

        public long? ModelId { get; set; }

        public ConsumptionType? ConsumptionType { get; set; }

        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }
        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }
 
  
    public class GetVeichlesInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; }

        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; }

        public string BodyNumber { get; set; }
        //public string Mark { get; set; }
        //public string Model { get; set; }
        public string SimNumber { get; set; }
        public FuelType? FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }
        public bool MaxCount { get; set; }

        public bool? IsActive { get; set; }
        public long? FuelGroupId { get; set; }

        public VeichleType? VeichleType { get; set; }
        public long? BrandId { get; set; }

        public long? ModelId { get; set; }

        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string InternalNumberFilePath { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
        public long? CompanyId { get; set; }
    }

    public class GetVeichleBySimOutput
    {
        public VeichleDto Veichle { get; set; }

        public GroupType GroupType { get; set; }
        public PeriodConsumptionType PeriodConsumptionType { get; set; }
        public decimal VeichleBalanceInLitre { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public decimal VeichleBalanceInSar { get; set; }
        public bool CounterPicIsRequired { get; set; }
        public bool Success { get; set; }
        public bool NotFoundSim { get; set; }
        public bool NotFoundVeichle { get; set; }
        public string Message { get; set; }
        public decimal MaximumRechargeAmountForOnce { get; set; } //أقصى مبلغ للتعبئة للمرة الوحدة

    }


    public class GetListByIdsInput
    {
        public long[] Ids { get; set; }
    }


    public class ImportExcelDataInput
    {
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public List<ImportExcelData> lst { get; set; }
    }


    public class ImportExcelDataOutput
    {
        public string Message { get; set; } 
        public ImportExcelData Veichle { get; set; }
    }


    public class ImportExcelData
    {
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }   
        public string PlateNumber { get; set; }
        public string PlateNumberEn { get; set; } 
        public string PlateLetters { get; set; }
        public string PlateLettersEn { get; set; }
        public string FullPlateNumber { get; set; }
        public string FullPlateNumberAr { get; set; } 
        public string BodyNumber { get; set; }  
        public string FuelType { get; set; }
        public int? TankSize { get; set; }
        public int? FuelAverageUsage { get; set; }
        public int? KiloCount { get; set; }  
        public string FuelGroup { get; set; }
        public string VeichleType { get; set; }
       
        public long? BrandId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public long? ModelId { get; set; }
        public string WorkingDays { get; set; }
        public string YearOfIndustry { get; set; }
        public string ConsumptionType { get; set; }
        public string DriverName { get; set; }
        public string DriverPhoneNumber { get; set; }
        public string DriverEmailAddress { get; set; }
        public string DriverUserName { get; set; }
        public string DriverPassword { get; set; }
        public string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public string Message { get; set; }  // رقم المركبة الداخلي
    }

    public class CreateDriverFromExcelInput
    {
        public Veichle Veichle { get; set; }
        public Driver Driver { get; set; }
        public CreateNewUserInput User { get; set; }
        public int RoleId { get; set; }
    }
    public class CreateDriverFromExcelOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class WorkingDaysInput
    {
        public string workingDays { get; set; }
    }
}