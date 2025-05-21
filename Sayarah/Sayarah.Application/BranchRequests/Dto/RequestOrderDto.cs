using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.BranchRequests;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.BranchRequests.Dto;

[AutoMapFrom(typeof(RequestOrder)), AutoMapTo(typeof(RequestOrder))]
public class RequestOrderDto : EntityDto<long>
{
    public long? BranchRequestId { get; set; }
    public BranchRequestDto BranchRequest { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType FuelType { get; set; }

}

[AutoMapFrom(typeof(RequestOrder))]
public class ApiRequestOrderDto : EntityDto<long>
{
    public long? BranchRequestId { get; set; }
    public BranchRequestDto BranchRequest { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType FuelType { get; set; }
}

[AutoMapTo(typeof(RequestOrder))]
public class CreateRequestOrderDto
{
    public long? BranchRequestId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType FuelType { get; set; }
}

[AutoMapTo(typeof(RequestOrder))]
public class UpdateRequestOrderDto : EntityDto<long>
{
    public long? BranchRequestId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType FuelType { get; set; }
}
public class GetRequestOrderInput : DataTableInputDto
{
    public long? BranchRequestId { get; set; }
    public long? BranchId { get; set; }
    public long? Id { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod? PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType? FuelType { get; set; }

}
public class GetAllRequestOrder : PagedResultRequestDto
{
    public long? BranchRequestId { get; set; }
    public string Code { get; set; }
    public string Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; }
    public PayMethod? PayMethod { get; set; }
    public string Note { get; set; }
    public FuelType? FuelType { get; set; }
    public bool? MaxCount { get; set; }
}
