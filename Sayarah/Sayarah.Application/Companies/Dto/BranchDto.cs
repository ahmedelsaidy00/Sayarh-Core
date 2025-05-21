using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Companies;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.Companies.Dto;

[AutoMapFrom(typeof(Branch)), AutoMapTo(typeof(Branch))]
public class BranchDto : FullAuditedEntityDto<long>
{
    public long? CompanyId { get; set; }
    public CompanyDto Company { get; set; }
    public long? UserId { get; set; }
    public UserDto User { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string Name { get; set; }
    public string DescAr { get; set; }
    public string DescEn { get; set; }
    public string Desc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string AddressOnMap { get; set; }
    public long? CityId { get; set; }
    public CityDto City { get; set; }
    public string Region { get; set; }
    public string District { get; set; }
    public decimal WalletAmount { get; set; }
    public decimal ConsumptionAmount { get; set; }
    public decimal FuelAmount { get; set; }
    public decimal CleanAmount { get; set; }
    public decimal MaintainAmount { get; set; }
    public int VeichlesCount { get; set; }
    public bool ActivateTimeBetweenFuelTransaction { get; set; }
    public int TimeBetweenFuelTransaction { get; set; }
    public virtual decimal Reserved { get; set; }
    public int ActVeichlesCount { get; set; }

}


[AutoMapFrom(typeof(Branch)), AutoMapTo(typeof(Branch))]
public class SmallBranchDto : EntityDto<long>
{
    public long? CompanyId { get; set; }
    public SmallCompanyDto Company { get; set; }
    public string Code { get; set; }
    public string Name { get; set; } 
}



[AutoMapFrom(typeof(Branch)), AutoMapTo(typeof(Branch))]
public class ApiBranchDto : EntityDto<long>
{
    
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string Name { get; set; }
    public int VeichlesCount { get; set; }
    public virtual decimal Reserved { get; set; }

}

[AutoMapTo(typeof(Branch))]
public class CreateBranchDto
{
    public long? CompanyId { get; set; }
    public long? UserId { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string DescAr { get; set; }
    public string DescEn { get; set; }

   
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string AddressOnMap { get; set; }

    public long? CityId { get; set; }
    public string Region { get; set; }
    public string District { get; set; }
    public CreateNewUserInput User { get; set; }
    public int VeichlesCount { get; set; }

    public bool? IsEmployee { get; set; }

    public bool ActivateTimeBetweenFuelTransaction { get; set; }
    public int TimeBetweenFuelTransaction { get; set; }
    public virtual decimal Reserved { get; set; }
}


[AutoMapTo(typeof(Branch))]
public class UpdateReservedBalanceBranchDto : EntityDto<long>
{
    public decimal Price { get; set; }  
    public decimal Reserved { get; set; }
    public OperationType? OperationType { get; set; }
    public bool IsTransOperation { get; set; }
}

[AutoMapTo(typeof(Branch))]
public class UpdateBranchDto : EntityDto<long>
{
    public long? CompanyId { get; set; }
    public long? UserId { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string Name { get; set; }
    public string DescAr { get; set; }
    public string DescEn { get; set; }
    public string Desc { get; set; }

   
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string AddressOnMap { get; set; }

    public long? CityId { get; set; }
    public string Region { get; set; }
    public string District { get; set; }
    public CreateNewUserInput User { get; set; }
    public int VeichlesCount { get; set; }
    public bool ActivateTimeBetweenFuelTransaction { get; set; }
    public int TimeBetweenFuelTransaction { get; set; }
    public virtual decimal Reserved { get; set; }
}


public class GetBranchesPagedInput : DataTableInputDto
{
    public string Code { get; set; }
    public long? CityId { get; set; }
    public string Region { get; set; }
    public string District { get; set; }
    public long? CompanyId { get; set; }
    public long? Id { get; set; }
    public long? BranchId { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public string ManagerName { get; set; }
    public bool? IsActive { get; set; }
    public decimal? FuelAmountFrom { get; set; }
    public decimal? FuelAmountTo { get; set; }
    public decimal? CleanAmountFrom { get; set; }
    public decimal? CleanAmountTo { get; set; }
    public decimal? MaintainAmountFrom { get; set; }
    public decimal? MaintainAmountTo { get; set; }
    public decimal? ConsumptionFrom { get; set; }
    public decimal? ConsumptionTo { get; set; }
    public decimal? VeichlesFrom { get; set; }
    public decimal? VeichlesTo { get; set; }
    public int? VeichlesCount { get; set; }
    public bool? IsEmployee { get; set; }
    public List<long> BranchesIds { get; set; }
    public virtual decimal Reserved { get; set; }
}


public class GetBranchesInput : PagedResultRequestDto
{
    public long? Id { get; set; }
    public long? CompanyId { get; set; }
    public long? CurrentBranchId { get; set; }
    public long? EmployeeId { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public bool? IsActive { get; set; }
    public bool MaxCount { get; set; }
    public int? VeichlesCount { get; set; }
    public List<long> BranchesIds { get; set; }
    public virtual decimal Reserved { get; set; }
}


[AutoMapFrom(typeof(Branch)), AutoMapTo(typeof(Branch))]
public class GetBranchWalletDetailsDto : EntityDto<long>
{
    public string NameAr { get; set; }
    public decimal? WalletAmount { get; set; }
    public decimal FuelAmount { get; set; }
    public decimal CleanAmount { get; set; }
    public decimal MaintainAmount { get; set; }
    public decimal? ConsumptionAmount { get; set; }
    public virtual decimal Reserved { get; set; }
}

public class ManageActiveOutput  
{
    public bool IsActive { get; set; }
    public long UserId { get; set; }
    public virtual decimal Reserved { get; set; }
}

public class BranchNameDto : EntityDto<long>
{
    public string Name { get; set; }
    public long? CompanyId { get; set; }
}