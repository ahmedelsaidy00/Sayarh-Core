using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Contact.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Contact;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Contact
{
    public class ContactMessageAppService : AsyncCrudAppService<ContactMessage, ContactMessageDto, long, GetAllContactMessages, CreateContactMessageDto, ContactMessageDto>, IContactMessageAppService
    {
        private readonly IRepository<ContactMessage, long> _contactMessageRepository;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly ICommonAppService _commonAppService;
        public UserManager UserManager { get; set; }
        public ContactMessageAppService(IRepository<ContactMessage, long> contactMessageRepository
            , UserManager userManager, RoleManager roleManager
            , AbpNotificationHelper abpNotificationHelper, ICommonAppService commonAppService)
            : base(contactMessageRepository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _contactMessageRepository = contactMessageRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _abpNotificationHelper = abpNotificationHelper;
            _commonAppService = commonAppService;
        }
        public override async Task<ContactMessageDto> GetAsync(EntityDto<long> input)
        {
            var message = Repository.GetAll()
                .Include(a=>a.CreatorUser)
              
                .FirstOrDefault(x => x.Id == input.Id);
            return await Task.FromResult(ObjectMapper.Map<ContactMessageDto>(message));
        }
        [AbpAuthorize]
        public async Task<DataTableOutputDto<ContactMessageDto>> GetPaged(GetContactMessagesInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        ContactMessage contactMessage = await _contactMessageRepository.GetAsync(Convert.ToInt32(input.ids[i]));
                        if (contactMessage != null)
                        {
                            if (input.action == "Delete")//Delete
                                await _contactMessageRepository.DeleteAsync(contactMessage);
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        ContactMessage contactMessage = await _contactMessageRepository.GetAsync(Convert.ToInt32(input.ids[0]));
                        if (contactMessage != null)
                        {
                            if (input.action == "Delete")//Delete
                                await _contactMessageRepository.DeleteAsync(contactMessage);

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                int count = await _contactMessageRepository.CountAsync();

                var query = _contactMessageRepository.GetAll()
                    .Include(a=>a.CreatorUser)
                  
                    .Where(a=>a.ContactType == ContactsType.Complaint);

                query = query.FilterDataTable((DataTableInputDto)input);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Subject), at => at.Subject.Contains(input.Subject));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Message), at => at.Message.Contains(input.Message));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.CreatorUser.Name.Contains(input.UserName));
           
                query = query.WhereIf(input.CreatorUserId.HasValue, at => at.CreatorUserId == input.CreatorUserId);
                query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                int filteredCount = await query.CountAsync();

                var categories =
                      await query
                       .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start)
                        .Take(input.length)
                          .ToListAsync();
                return new DataTableOutputDto<ContactMessageDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<ContactMessageDto>>(categories)
                };
            }
        }
        public override async Task<ContactMessageDto> CreateAsync(CreateContactMessageDto input)
        {
            try
            {
                string message = "Pages.Notifications.NewMessage";
                //Check if contactMessage exists
                if (input.ContactType == ContactsType.NewsLetter)
                {
                    input.Name = input.Email;
                    message = "Pages.Notifications.NewsLetter";
                    int existingCount = await _contactMessageRepository.CountAsync(at => at.Email == input.Email && at.ContactType == ContactsType.NewsLetter);
                    if (existingCount > 0)
                        return new ContactMessageDto { ErrorMsg = L("Pages.Subscribers.Error.AlreadyExistEmail") };
                }

                var contactMessage = ObjectMapper.Map<ContactMessage>(input);
                await _contactMessageRepository.InsertAsync(contactMessage);
                await CurrentUnitOfWork.SaveChangesAsync();
                #region ///////  Send Abp Notifications from Client To Admin ///////
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                var role = await _roleManager.GetRoleByNameAsync(RolesNames.Admin);
                if (role != null)
                {
                    var admins = _userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
                    foreach (var usr in admins)
                    {
                        targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                    }

                    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                    {
                        // SenderUserId = AbpSession.UserId.Value,
                        SenderUserName = input.Name,
                        Message = message,
                        EntityType = Entity_Type.ContactMsg,
                        ContactType = contactMessage.ContactType,
                        EntityId = contactMessage.Id
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewContactMsg, CreateNotificationData, targetUsersId.ToArray());
                }
                #endregion
                return MapToEntityDto(contactMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var contactMessage = await _contactMessageRepository.GetAsync(input.Id);
            if (contactMessage == null)
                throw new UserFriendlyException(L("Common.Message.ElementNotFound"));
            await _contactMessageRepository.DeleteAsync(contactMessage);
        }
        [AbpAuthorize]
        public override async Task<PagedResultDto<ContactMessageDto>> GetAllAsync(GetAllContactMessages input)
        {
            var query = await _contactMessageRepository.GetAll().ToListAsync();
            return new PagedResultDto<ContactMessageDto>(query.Count, ObjectMapper.Map<List<ContactMessageDto>>(query));
        }
        [AbpAuthorize]
        public async Task<GetContactMessagesOutput> GetAllMessages(GetContactMessageByType input)
        {
            try
            {
                var query = _contactMessageRepository.GetAll().Include(at => at.CreatorUser).AsQueryable();
                query = query.Where(at => at.ContactType == input.ContactType);
                query = query.WhereIf(input.DateFrom.HasValue, at => at.CreationTime >= input.DateFrom);
                if (input.DateTo.HasValue)
                {
                    input.DateTo = input.DateTo.Value.Add(TimeSpan.FromSeconds(86399));
                    query = query.WhereIf(input.DateTo.HasValue, at => at.CreationTime <= input.DateTo);
                }
                int count = query.ToList().Count;
                query = query.OrderBy(at => at.IsSeen).OrderByDescending(x => x.CreationTime);
                if (input.StartNo.HasValue && input.LimitNo.HasValue)
                {
                    query = query.Skip(input.StartNo.Value).Take(input.LimitNo.Value);
                }

                var contactMessages = await query.ToListAsync();
                return new GetContactMessagesOutput { ContactMessages = ObjectMapper.Map<List<ContactMessageDto>>(contactMessages), AllMessagesCount = count };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [AbpAuthorize]
        public async Task<ContactMessageDto> SetAsSeen(EntityDto<long> input)
        {
            ContactMessage ContactMessage = await _contactMessageRepository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);
            if (ContactMessage == null)
                throw new UserFriendlyException(L("Common.ErrorOccurred"));
            if (!ContactMessage.IsRead)
            {
                ContactMessage.IsSeen = true;
                ContactMessage.IsRead = true;
                ContactMessage = await _contactMessageRepository.UpdateAsync(ContactMessage);
            }
            return ObjectMapper.Map<ContactMessageDto>(ContactMessage);

        }
        #region NewsLetter
        [AbpAuthorize]
        public async Task<PagedResultDto<ContactMessageDto>> GetAllNewsLetter(GetAllContactMessages input)
        {
            var query = _contactMessageRepository.GetAll().Where(x => x.ContactType == ContactsType.NewsLetter);
            int count = query.Count();
            query = query.OrderByDescending(m => m.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount);
            var newsLetter = await query.ToListAsync();
            return new PagedResultDto<ContactMessageDto>(count, ObjectMapper.Map<List<ContactMessageDto>>(newsLetter));
        }
        [AbpAuthorize]
        public async Task<bool> DeleteAllNewsLetter()
        {
            var newsLetter = await _contactMessageRepository.GetAll().Where(x => x.ContactType == ContactsType.NewsLetter).ToListAsync();
            if (newsLetter != null && newsLetter.Count > 0)
            {
                foreach (var item in newsLetter)
                {
                    await _contactMessageRepository.DeleteAsync(item);
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task<bool> DeleteSelected(GetAllContactMessages input)
        {
            try
            {
                if (input.NewsLetterIds != null && input.NewsLetterIds.Count > 0)
                {
                    foreach (var itemId in input.NewsLetterIds)
                    {
                        await DeleteAsync(new EntityDto<long> { Id = itemId });
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<bool> MakeAllAsRead(GetAllContactMessages input)
        {
            var newsLetter = await _contactMessageRepository.GetAll().Where(x => x.ContactType == ContactsType.NewsLetter).ToListAsync();
            if (newsLetter != null && newsLetter.Count > 0)
            {
                foreach (var item in newsLetter)
                {
                    item.IsRead = true;
                    item.IsSeen = true;
                    await _contactMessageRepository.UpdateAsync(item);
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task<bool> SendToAll(GetAllContactMessages input)
        {
            var newsLetter = await _contactMessageRepository.GetAll().Where(x => x.ContactType == ContactsType.NewsLetter).ToListAsync();
            if (newsLetter != null && newsLetter.Count > 0)
            {
                bool result = await _commonAppService.SendEmail(new SendEmailRequest
                {
                    Subject = L("Common.SystemTitle"),
                    datalst = new[] { input.Message },
                    Emails = new[] { string.Join(",", newsLetter.Select(x => x.Email)) },
                    IsBodyHtml = true
                });
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task<bool> SendToSelected(GetAllContactMessages input)
        {
            var newsLetter = await _contactMessageRepository.GetAll().Where(x => x.ContactType == ContactsType.NewsLetter && input.NewsLetterIds.Any(id => id == x.Id)).ToListAsync();
            if (newsLetter != null && newsLetter.Count > 0)
            {
                bool result = await _commonAppService.SendEmail(new SendEmailRequest
                {
                    Subject = L("Common.SystemTitle"),
                    datalst = new[] { input.Message },
                    Emails = new[] { string.Join(",", newsLetter.Select(x => x.Email)) },
                    IsBodyHtml = true
                });
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion
    }
}
