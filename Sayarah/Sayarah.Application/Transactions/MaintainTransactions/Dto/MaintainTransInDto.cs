using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Transactions;

namespace Sayarah.Application.Transactions.MaintainTransactions.Dto
{
    [AutoMapFrom(typeof(MaintainTransIn)) , AutoMapTo(typeof(MaintainTransIn))]
    public class MaintainTransInDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }

        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
      
    [AutoMapTo(typeof(MaintainTransIn))]
    public class CreateMaintainTransInDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

 
    [AutoMapTo(typeof(MaintainTransIn))]
    public class UpdateMaintainTransInDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

     
    public class GetMaintainTransInsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int? Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
 
  
    public class GetMaintainTransInsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int? Quantity { get; set; } // litre
        public string Notes { get; set; }
        public bool MaxCount { get; set; }
    }
}