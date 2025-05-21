using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Tickets;
using static Sayarah.SayarahConsts;
namespace Sayarah.Application.Tickets.Dto
{
    [AutoMapFrom(typeof(TicketDetail)), AutoMapTo(typeof(TicketDetail))]
    public class TicketDetailDto : FullAuditedEntityDto<long>
    {
        [DisableAuditing]
        public string Code { get; set; }

        public long? TicketId { get; set; }
        //public Ticket Ticket { get; set; }


        public TicketFrom TicketFrom { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(17, /*"600x600_" +*/ FilePath))
                    return FilesPath.Tickets.ServerImagePath + /*"600x600_" +*/ FilePath;
                else
                    return FilesPath.Tickets.DefaultImagePath;
            }
        }
        public string CreatorUserName { get; set; }

    }


    [AutoMapTo(typeof(TicketDetail))]
    public class CreateTicketDetailDto
    {
        public string Code { get; set; }
        public long? TicketId { get; set; }
        public TicketFrom TicketFrom { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }

    [AutoMapTo(typeof(TicketDetail))]
    public class UpdateTicketDetailDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? TicketId { get; set; }
        public TicketFrom TicketFrom { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }
    public class GetTicketDetailInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? TicketId { get; set; }
        public TicketFrom? TicketFrom { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }

    }
    public class GetAllTicketDetail : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? TicketId { get; set; }
        public TicketFrom? TicketFrom { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }
}
