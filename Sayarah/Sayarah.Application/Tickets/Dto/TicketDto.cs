using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Contact;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using Sayarah.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static Sayarah.SayarahConsts;
namespace Sayarah.Application.Tickets.Dto
{
    [AutoMapFrom(typeof(Ticket)), AutoMapTo(typeof(Ticket))]
    public class TicketDto :FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }
        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public virtual long? ProviderId { get; set; }
        public virtual ProviderDto Provider { get; set; }
        public virtual long? BranchId { get; set; }
        public virtual BranchDto Branch { get; set; }

        public virtual UserDto CreatorUser { get; set; }

        public string CreatorName { get { 
        
                string creatorName = string.Empty;
                switch (TicketFrom)
                {
                    case TicketFrom.Company:
                        creatorName = Company != null ? Company.Name :string.Empty;
                        break;
                    case TicketFrom.MainProvider:
                        creatorName = MainProvider != null ? MainProvider.Name :string.Empty;
                        break;
                    case TicketFrom.Provider:
                        creatorName = Provider != null ? Provider.Name : string.Empty; 
                        break;
                    case TicketFrom.Branch:
                        creatorName = Branch != null ? Branch.Name :string.Empty;
                        break;
                    case TicketFrom.Admin:
                        creatorName = CreatorUser != null ? CreatorUser.Name : string.Empty;
                        break;
                    default:
                        break;
                }
                return creatorName;
                
            }
        }


        public ProblemCategory ProblemCategory { get; set; }
        public TicketStatus TicketStatus { get; set; }
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


        public string Comment { get; set; }
        public decimal Rate { get; set; }
        public bool IsRated { get; set; }
        public TicketFrom TicketFrom { get; set; }

        public List<TicketDetailDto> TicketDetails { get; set; }
    }
   

    [AutoMapTo(typeof(Ticket))]
    public class CreateTicketDto
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; } 
        public long? BranchId { get; set; } 

        public ProblemCategory ProblemCategory { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string Comment { get; set; }
        public decimal Rate { get; set; }
        public bool IsRated { get; set; }
        public TicketFrom TicketFrom { get; set; }
    }

    [AutoMapTo(typeof(Ticket))]
    public class UpdateTicketDto : EntityDto<long>
    {
        public string Code { get; set; }

        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }

        public ProblemCategory ProblemCategory { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string Comment { get; set; }
        public decimal Rate { get; set; }
        public bool IsRated { get; set; }
        public TicketFrom TicketFrom { get; set; }
    }
    public class GetTicketInput : DataTableInputDto
    { 
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }

        public string CompanyName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string BranchName { get; set; }

        public ProblemCategory? ProblemCategory { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string Comment { get; set; }
        public decimal? Rate { get; set; }
        public bool? IsRated { get; set; }
        public long? Id { get; set; }

       // public TicketTypes? TicketType { get; set; }
        public TicketFrom? TicketFrom { get; set; }

    }
    public class GetAllTicket : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }

        public string CompanyName { get; set; }
        public long? MainProviderId { get; set; }
        public string MainProviderName { get; set; }

        public long? ProviderId { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string ProviderName { get; set; }

        public ProblemCategory? ProblemCategory { get; set; }
        public TicketFrom? TicketFrom { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string Comment { get; set; }
        public decimal Rate { get; set; }
        public bool? IsRated { get; set; }
        public bool? MaxCount { get; set; }
    }
}
