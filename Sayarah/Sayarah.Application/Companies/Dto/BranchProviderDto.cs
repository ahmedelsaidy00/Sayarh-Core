using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Companies;

namespace Sayarah.Application.Companies.Dto
{
    [AutoMapFrom(typeof(BranchProvider)) , AutoMapTo(typeof(BranchProvider))]
    public class BranchProviderDto : FullAuditedEntityDto<long>
    {
        public long? ProviderId { get; set; }
        public SmallProviderDto Provider { get; set; }
        public long? BranchId { get; set; }
        public SmallBranchDto Branch { get; set; }
        public long? MainProviderId { get; set; }
        public SmallMainProviderDto MainProvider { get; set; }
        public long? CompanyId { get; set; }
        public SmallCompanyDto Company { get; set; }
        public decimal BranchProvider_In { get; set; }
        public decimal BranchProvider_Out { get; set; }
        public decimal BranchProvider_Balance { get; set; }
    }
      
    [AutoMapTo(typeof(BranchProvider))]
    public class CreateBranchProviderDto
    {
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
    }

 
    [AutoMapTo(typeof(BranchProvider))]
    public class UpdateBranchProviderDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
    }


    public class GetBranchProviderProvidersPagedInput : DataTableInputDto
    {
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
        public long? Id { get; set; }
        public string ProviderName { get; set; }
        public bool? IsActive { get; set; }
    }
 
  
    public class GetBranchProviderProvidersInput : PagedResultRequestDto
    {
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
        public long? Id { get; set; }
        public bool MaxCount { get; set; }
    }
}