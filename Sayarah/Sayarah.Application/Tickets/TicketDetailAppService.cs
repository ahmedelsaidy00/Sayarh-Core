using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Tickets.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using Sayarah.Tickets;
using System.Globalization;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Tickets
{
    [DisableAuditing]
    public class TicketDetailAppService : AsyncCrudAppService<TicketDetail, TicketDetailDto, long, GetAllTicketDetail, CreateTicketDetailDto, UpdateTicketDetailDto>, ITicketDetailAppService
    {
        private readonly IRepository<TicketDetail, long> _ticketDetailRepository;
        private readonly IRepository<Ticket, long> _ticketRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        private readonly ICommonAppService _commonService;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        CultureInfo new_lang = new CultureInfo("ar");
        public TicketDetailAppService(
            IRepository<TicketDetail, long> repository,
            IRepository<Ticket, long> ticketRepository,
            IRepository<User, long> userRepository,
            IRepository<Company, long> companyRepository,
            IRepository<MainProvider, long> mainProviderRepository,
            IRepository<Provider, long> providerRepository,
            IRepository<Branch, long> branchRepository,
            ICommonAppService commonService,
            AbpNotificationHelper abpNotificationHelper,
            UserManager userManager,
            RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _ticketDetailRepository = repository;
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _companyRepository = companyRepository;
            _mainProviderRepository = mainProviderRepository;
            _providerRepository = providerRepository;
            _branchRepository = branchRepository;
            _commonService = commonService;
            _abpNotificationHelper = abpNotificationHelper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

     

        [AbpAuthorize]
        public override async Task<TicketDetailDto> UpdateAsync(UpdateTicketDetailDto input)
        {
            try
            {
                var ticketDetail = await _ticketDetailRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, ticketDetail);
                await _ticketDetailRepository.UpdateAsync(ticketDetail);
                return MapToEntityDto(ticketDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            try
            {
                var ticketDetail = await _ticketDetailRepository.GetAsync(input.Id);

                // delete file first 
                if (ticketDetail != null && !string.IsNullOrEmpty(ticketDetail.FilePath)) Utilities.DeleteImage(17, ticketDetail.FilePath, new string[] {  });
              
                await _ticketDetailRepository.DeleteAsync(ticketDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<TicketDetailDto> CreateTicketDetail(CreateTicketDetailDto input)
        {
            try
            {
                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "TicketDetails", CodeField = "Code", AddWhere = "" });
                var ticketDetail = ObjectMapper.Map<TicketDetail>(input);
                await _ticketDetailRepository.InsertAsync(ticketDetail);

                string senderUserName = string.Empty;

                // get ticket 
                var ticket = await _ticketRepository
                    .GetAllIncluding(a => a.Company)
                    .Include(a => a.Branch)
                    .Include(a => a.MainProvider)
                    .Include(a => a.Provider)
                    .FirstOrDefaultAsync(a => a.Id == input.TicketId);


                long targetUserId = 0;

                if (input.TicketFrom == TicketFrom.Company)
                    senderUserName = ticket.Company.NameAr;
                else if (input.TicketFrom == TicketFrom.MainProvider)
                    senderUserName = ticket.MainProvider.NameAr;
                else if (input.TicketFrom == TicketFrom.Provider)
                    senderUserName = ticket.Provider.NameAr;
                else if (input.TicketFrom == TicketFrom.Branch)
                    senderUserName = ticket.Branch.NameAr;
                else if (input.TicketFrom == TicketFrom.Admin)
                {
                    senderUserName = L("Common.SystemTitle");

                    if (ticket.TicketFrom == TicketFrom.Company)
                        targetUserId = ticket.Company.UserId.Value;
                    else if (ticket.TicketFrom == TicketFrom.MainProvider)
                        targetUserId = ticket.MainProvider.UserId.Value;
                    else if (ticket.TicketFrom == TicketFrom.Provider)
                         targetUserId = ticket.Provider.UserId.Value;
                    else if (ticket.TicketFrom == TicketFrom.Branch)
                           targetUserId = ticket.Branch.UserId.Value;
                }


                if (input.TicketFrom == TicketFrom.Admin)
                {
                    #region ///////  Send Abp Notifications from Branch To Admin ///////
                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: targetUserId));

                    CreateNotificationDto CreateAdminNotificationData = new CreateNotificationDto
                    {
                        SenderUserName = senderUserName,
                        Message = "Pages.Notifications.NewTicketDetail",
                        EntityType = Entity_Type.NewTicket,
                        EntityId = ticket.Id,
                        TicketFrom = input.TicketFrom,
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewTicket, CreateAdminNotificationData, targetUsersId.ToArray());

                    #endregion
                }
                else
                {
                    #region ///////  Send Abp Notifications from Branch To Admin ///////
                    List<UserIdentifier> adminTargetUsersId = new List<UserIdentifier>();
                    var role = await _roleManager.GetRoleByNameAsync(RolesNames.Admin);
                    if (role != null)
                    {
                        var admins = _userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
                        foreach (var usr in admins)
                        {
                            adminTargetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                        }

                        CreateNotificationDto CreateAdminNotificationData = new CreateNotificationDto
                        {
                            SenderUserName = senderUserName,
                            Message = "Pages.Notifications.NewTicketDetail",
                            EntityType = Entity_Type.NewTicket,
                            EntityId = ticket.Id,
                            TicketFrom = input.TicketFrom,
                        };
                        //Publish Notification Data
                        await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewTicket, CreateAdminNotificationData, adminTargetUsersId.ToArray());
                    }
                    #endregion
                }



                return ObjectMapper.Map<TicketDetailDto>(ticketDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }




    }
}
