using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Contact;
using Sayarah.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Contact.Dto

{
    [AutoMapFrom(typeof(ContactMessage)), AutoMapTo(typeof(ContactMessage))]
    public class ContactMessageDto : AuditedEntityDto<long>
    {
        //[Required]
        //[StringLength(50)]
        public string Email { get; set; }
        [Required]
        public ContactsType ContactType { get; set; }
        public virtual SubjectType SubjectType { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; } // رقم الإعلان
        public string Message { get; set; }
        public string FilePath { get; set; }
       
        public string Reply { get; set; }
        public bool IsSeen { get; set; }
        public bool IsRead { get; set; }
        public string ErrorMsg { get; set; }
        public UserDto CreatorUser { get;set;} 
    }


    [AutoMapTo(typeof(ContactMessage))]
    public class CreateContactMessageDto
    {
        //[Required]
        //[StringLength(50)]
        public string Email { get; set; }
        [Required]
        public ContactsType ContactType { get; set; }
        public virtual SubjectType SubjectType { get; set; } 
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; } // رقم الإعلان
        public string Message { get; set; }
        public string FilePath { get; set; }
        public bool IsSeen { get; set; }
        public bool IsRead { get; set; } 
    }

    [AutoMapTo(typeof(ContactMessage))]
    public class UpdateContactMessageDto : EntityDto
    {
        public string Email { get; set; }
        [Required]
        public ContactsType ContactType { get; set; }
        public virtual SubjectType SubjectType { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Reply { get; set; }
        public bool IsSeen { get; set; }
        public bool IsRead { get; set; }
        public DateTime? Date { get; set; } 

    }

    public class GetContactMessagesInput : DataTableInputDto
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public virtual SubjectType SubjectType { get; set; }

        public string Message { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string BuildingName { get; set; }
        public ContactsType ContactType { get; set; } 
        public long? CreatorUserId { get; set; } 
        public long? Id { get; set; }
        public bool MatchExact { get; set; }
       
    }


    public class GetAllContactMessages : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public List<long> NewsLetterIds { get; set; }
        public string Message { get; set; }
    }
    public class GetContactMessageByType
    {
        public ContactsType ContactType { get; set; }
        public virtual SubjectType SubjectType { get; set; }

        public int? StartNo { get; set; }
        public int? LimitNo { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DateFrom { get; set; }
    }
    public class GetContactMessagesOutput
    {
        public List<ContactMessageDto> ContactMessages { get; set; }
        public long AllMessagesCount { get; set; }
    }

    public class SendNewsLetterInput
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Subject { get; set; }
        public string GovernorateName { get; set; }
        public string CityName { get; set; }
        public string CategoryName { get; set; }
    }
    public class GetSettingDataDto
    {

        public string Sender { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
   

}
