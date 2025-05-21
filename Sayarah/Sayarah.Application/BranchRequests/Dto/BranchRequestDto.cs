using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.BranchRequests;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.BranchRequests.Dto;

[AutoMapFrom(typeof(BranchRequest)), AutoMapTo(typeof(BranchRequest))]
public class BranchRequestDto : EntityDto<long>
{
    public long? BranchId { get; set; }
    public BranchDto Branch { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus Status { get; set; }
    public FuelType FuelType { get; set; }
}

[AutoMapFrom(typeof(BranchRequest))]
public class ApiBranchRequestDto : EntityDto<long>
{
    public long? BranchId { get; set; }
    public BranchDto Branch { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus Status { get; set; }
    public FuelType FuelType { get; set; }
}

[AutoMapTo(typeof(BranchRequest))]
public class CreateBranchRequestDto
{
    public long? BranchId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus Status { get; set; }
    public FuelType FuelType { get; set; }
}

[AutoMapTo(typeof(BranchRequest))]
public class UpdateBranchRequestDto : EntityDto<long>
{
    public long? BranchId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus Status { get; set; }
    public FuelType FuelType { get; set; }
}
public class GetBranchRequestInput : DataTableInputDto
{
    public long? BranchId { get; set; }
    public long? CompanyId { get; set; }
    public long? Id { get; set; }
    public string Code { get; set; }
    public string BranchName { get; set; }
    public string CompanyName { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus? Status { get; set; }
    public FuelType? FuelType { get; set; }
    public bool? IsEmployee { get; set; }
    public List<long> BranchesIds { get; set; }

}
public class GetAllBranchRequest : PagedResultRequestDto
{
    public long? BranchId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public DeliveyWays? DeliveyWay { get; set; }
    public DateTime? DeliveyDate { get; set; }
    public string Notes { get; set; }
    public RequestStatus? Status { get; set; }
    public FuelType? FuelType { get; set; }
    public bool? MaxCount { get; set; }
}
