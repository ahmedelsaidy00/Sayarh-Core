using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Transactions;

namespace Sayarah.Application.Transactions.WashTransactions.Dto
{
    [AutoMapFrom(typeof(WashTransIn)) , AutoMapTo(typeof(WashTransIn))]
    public class WashTransInDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }

        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
      
    [AutoMapTo(typeof(WashTransIn))]
    public class CreateWashTransInDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

 
    [AutoMapTo(typeof(WashTransIn))]
    public class UpdateWashTransInDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

     
    public class GetWashTransInsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public int? Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
 
  
    public class GetWashTransInsInput : PagedResultRequestDto
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