using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Sayarah.Application.Helpers;
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
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Tickets
{
    [DisableAuditing]
    public class TicketAppService : AsyncCrudAppService<Ticket, TicketDto, long, GetAllTicket, CreateTicketDto, UpdateTicketDto>, ITicketAppService
    {
        private readonly IRepository<Ticket, long> _ticketRepository;
        private readonly IRepository<TicketDetail, long> _ticketDetailRepository;
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
        public TicketAppService(
            IRepository<Ticket, long> repository,
            IRepository<TicketDetail, long> ticketDetailRepository,
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
            _ticketRepository = repository;
            _ticketDetailRepository = ticketDetailRepository;
            _userRepository = userRepository;
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
        public async Task<DataTableOutputDto<TicketDto>> GetPaged(GetTicketInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int ticketId = Convert.ToInt32(input.ids[i]);
                            Ticket ticket = await _ticketRepository.GetAllIncluding(a => a.Company)
                                .Include(a => a.MainProvider)
                                .Include(a => a.Provider)
                                .Include(a => a.Branch)
                                .FirstOrDefaultAsync(a => a.Id == ticketId);

                            if (ticket != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await _ticketRepository.DeleteAsync(ticket);
                                }

                                else if (input.action == "Complete")//Delete
                                {
                                    await CompleteTicket(ticket);
                                }

                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int ticketId = Convert.ToInt32(input.ids[0]);
                            Ticket ticket = await _ticketRepository.GetAllIncluding(a => a.Company)
                               .Include(a => a.MainProvider)
                               .Include(a => a.Provider)
                               .Include(a => a.Branch)
                               .FirstOrDefaultAsync(a => a.Id == ticketId);
                            if (ticket != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await _ticketRepository.DeleteAsync(ticket);
                                }

                                else if (input.action == "Complete")//Delete
                                {
                                    await CompleteTicket(ticket);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    //int count = await _ticketRepository.CountAsync();
                    var query = _ticketRepository.GetAll()
                        .Include(a => a.Provider.User)
                        .Include(a => a.Branch.User)
                        .Include(a => a.MainProvider.User)
                        .Include(a => a.Company.User).AsQueryable();


                    query = query.WhereIf(input.CompanyId.HasValue, at => at.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    // query = query.WhereIf(input.TicketType.HasValue && input.TicketType == Contact.TicketTypes.Companies, at => at.MainProviderId.HasValue == false && at.CompanyId.HasValue == true);
                    // query = query.WhereIf(input.TicketType.HasValue && input.TicketType == Contact.TicketTypes.MainProviders, at => at.MainProviderId.HasValue == true && at.CompanyId.HasValue == false);
                    query = query.WhereIf(input.TicketFrom.HasValue, at => at.TicketFrom == input.TicketFrom);


                    int count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code == input.Code);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Description), at => at.Description == input.Description);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Company.NameAr.Contains(input.CompanyName) || at.Company.NameEn.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.MainProvider.NameAr.Contains(input.MainProviderName) || at.MainProvider.NameEn.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName));
                    query = query.WhereIf(input.TicketStatus.HasValue, at => at.TicketStatus == input.TicketStatus);
                    query = query.WhereIf(input.ProblemCategory.HasValue, at => at.ProblemCategory == input.ProblemCategory);


                    int filteredCount = await query.CountAsync();
                    List<Ticket> tickets = new List<Ticket>();
                    if (input.columns[input.order[0].column].name.Equals("Code"))
                    {
                        if (input.order[0].dir.Equals("asc"))
                            tickets = await query.Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                                .Include(x => x.Company.User)
                                .Include(x => x.MainProvider.User)
                                .OrderBy(x => x.Code.Length).ThenBy(x => x.Code).Skip(input.start).Take(input.length).ToListAsync();
                        else
                            tickets = await query.Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                                .Include(x => x.Company.User)
                                .Include(x => x.MainProvider.User)
                                .OrderByDescending(x => x.Code.Length).ThenByDescending(x => x.Code).Skip(input.start).Take(input.length).ToListAsync();
                    }
                    else
                    {
                        tickets = await query.Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                            .Include(x => x.Company.User)
                            .Include(x => x.MainProvider.User)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();
                    }
                    return new DataTableOutputDto<TicketDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<TicketDto>>(tickets)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<TicketDto> GetAsync(EntityDto<long> input)
        {
            var ticket = _ticketRepository.GetAll()
                .Include(x => x.Branch.User)
                .Include(x => x.Provider.User)
                .Include(x => x.Company.User)
                .Include(x => x.MainProvider.User)
                .Include(x => x.CreatorUser)
                .Include(x => x.TicketDetails.Select(a=>a.CreatorUser))
                .FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<TicketDto>(ticket));
        }

        [AbpAuthorize]
        public override async Task<TicketDto> CreateAsync(CreateTicketDto input)
        {
            try
            {
                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Tickets", CodeField = "Code", AddWhere = "" });
                var ticket = ObjectMapper.Map<Ticket>(input);
                var ticketId = await _ticketRepository.InsertAndGetIdAsync(ticket);

                string senderUserName = string.Empty;

                if (input.TicketFrom == TicketFrom.Company)
                {
                    var company = await _companyRepository.FirstOrDefaultAsync(x => x.Id == input.CompanyId);
                    senderUserName = company.NameAr;
                }
                else if (input.TicketFrom == TicketFrom.MainProvider)
                {
                    var mainProvider = await _mainProviderRepository.FirstOrDefaultAsync(x => x.Id == input.MainProviderId);
                    senderUserName = mainProvider.NameAr;
                }
                else if (input.TicketFrom == TicketFrom.Provider)
                {
                    var provider = await _providerRepository.FirstOrDefaultAsync(x => x.Id == input.ProviderId);
                    senderUserName = provider.NameAr;
                }
                else if (input.TicketFrom == TicketFrom.Branch)
                {
                    var branch = await _branchRepository.FirstOrDefaultAsync(x => x.Id == input.BranchId);
                    senderUserName = branch.NameAr;
                }
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
                        Message = "Pages.Notifications.NewTicket",
                        EntityType = Entity_Type.NewTicket,
                        EntityId = ticket.Id,
                        TicketFrom = input.TicketFrom,
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewTicket, CreateAdminNotificationData, adminTargetUsersId.ToArray());
                }
                #endregion


                return MapToEntityDto(ticket);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<TicketDto> UpdateAsync(UpdateTicketDto input)
        {
            try
            {
                var ticket = await _ticketRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, ticket);
                await _ticketRepository.UpdateAsync(ticket);
                return MapToEntityDto(ticket);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<TicketDto>> GetAllAsync(GetAllTicket input)
        {
            try
            {
                var query = _ticketRepository.GetAll()
                    .Include(a => a.Provider)
                    .Include(a => a.Branch)
                    .Include(a => a.MainProvider)
                    .Include(a => a.Company).AsQueryable();

                query = query.WhereIf(input.CompanyId.HasValue, at => at.CompanyId == input.CompanyId);
                query = query.WhereIf(input.MainProviderId.HasValue, at => at.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code == input.Code);
                query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Company.NameAr.Contains(input.CompanyName) || at.Company.NameEn.Contains(input.CompanyName));
                query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.MainProvider.NameAr.Contains(input.MainProviderName) || at.MainProvider.NameEn.Contains(input.MainProviderName));
                query = query.WhereIf(input.TicketStatus.HasValue, at => at.TicketStatus == input.TicketStatus);
                query = query.WhereIf(input.ProblemCategory.HasValue, at => at.ProblemCategory == input.ProblemCategory);



                if (input.MaxCount.HasValue && input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var tickets = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<TicketDto>(
                   query.Count(), ObjectMapper.Map<List<TicketDto>>(tickets)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<TicketDto> CompleteTicket(Ticket ticket)
        {
            try
            {

                ticket.TicketStatus = TicketStatus.Completed;

                await _ticketRepository.UpdateAsync(ticket);
                await CurrentUnitOfWork.SaveChangesAsync();

                // user id 
                long userId = 0;
                if (ticket.TicketFrom == TicketFrom.Company)
                    userId = ticket.Company.UserId.Value;
                else if (ticket.TicketFrom == TicketFrom.MainProvider)
                    userId = ticket.MainProvider.UserId.Value;
                else if (ticket.TicketFrom == TicketFrom.Provider)
                    userId = ticket.Provider.UserId.Value;
                else if (ticket.TicketFrom == TicketFrom.Branch)
                    userId = ticket.Branch.UserId.Value;


                #region ///////  Send Abp Notifications To Client  ///////
                CreateNotificationDto createNotificationData;
                var _lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, userId));
                if (_lang == null)
                    _lang = new_lang.ToString();
                createNotificationData = new CreateNotificationDto
                {
                    Message = "Pages.Notifications.TicketCompleted",
                    EntityType = Entity_Type.TicketCompleted,
                    EntityId = ticket.Id,
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.TicketCompleted, createNotificationData, new List<UserIdentifier> { new UserIdentifier(tenantId: AbpSession.TenantId.Value, userId: userId) }.ToArray());
                #endregion
                return MapToEntityDto(ticket);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        //[AbpAuthorize]
        //public async Task<TicketDetailDto> CreateTicketDetail(CreateTicketDetailDto input)
        //{
        //    try
        //    {
        //        input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "TicketDetails", CodeField = "Code", AddWhere = "" });
        //        var ticketDetail = ObjectMapper.Map<TicketDetail>(input);
        //        await _ticketDetailRepository.InsertAsync(ticketDetail);

        //        string senderUserName = string.Empty;

        //        // get ticket 
        //        var ticket = await _ticketRepository
        //            .GetAllIncluding(a => a.Company)
        //            .Include(a => a.Branch)
        //            .Include(a => a.MainProvider)
        //            .Include(a => a.Provider)
        //            .FirstOrDefaultAsync(a => a.Id == input.TicketId);


        //        long targetUserId = 0;

        //        if (input.TicketFrom == TicketFrom.Company)
        //        {
        //            senderUserName = ticket.Company.NameAr;
        //            targetUserId = ticket.Company.UserId.Value;
        //        }
        //        else if (input.TicketFrom == TicketFrom.MainProvider)
        //        {
        //            senderUserName = ticket.MainProvider.NameAr;
        //            targetUserId = ticket.MainProvider.UserId.Value;
        //        }
        //        else if (input.TicketFrom == TicketFrom.Provider)
        //        {
        //            senderUserName = ticket.Provider.NameAr;
        //            targetUserId = ticket.Provider.UserId.Value;
        //        }
        //        else if (input.TicketFrom == TicketFrom.Branch)
        //        {
        //            senderUserName = ticket.Branch.NameAr;
        //            targetUserId = ticket.Branch.UserId.Value;
        //        }
        //        else if (input.TicketFrom == TicketFrom.Admin)
        //        {
        //            senderUserName = L("Common.SystemTitle");
        //        }


        //        if (input.TicketFrom == TicketFrom.Admin)
        //        {
        //            #region ///////  Send Abp Notifications from Branch To Admin ///////
        //            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
        //            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: targetUserId));

        //            CreateNotificationDto CreateAdminNotificationData = new CreateNotificationDto
        //            {
        //                SenderUserName = senderUserName,
        //                Message = "Pages.Notifications.NewTicketDetail",
        //                EntityType = Entity_Type.NewTicket,
        //                EntityId = ticket.Id,
        //                TicketFrom = input.TicketFrom,
        //            };
        //            //Publish Notification Data
        //            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewTicket, CreateAdminNotificationData, targetUsersId.ToArray());

        //            #endregion
        //        }
        //        else
        //        {
        //            #region ///////  Send Abp Notifications from Branch To Admin ///////
        //            List<UserIdentifier> adminTargetUsersId = new List<UserIdentifier>();
        //            var role = await _roleManager.GetRoleByNameAsync(RolesNames.Admin);
        //            if (role != null)
        //            {
        //                var admins = _userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
        //                foreach (var usr in admins)
        //                {
        //                    adminTargetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
        //                }

        //                CreateNotificationDto CreateAdminNotificationData = new CreateNotificationDto
        //                {
        //                    SenderUserName = senderUserName,
        //                    Message = "Pages.Notifications.NewTicketDetail",
        //                    EntityType = Entity_Type.NewTicket,
        //                    EntityId = ticket.Id,
        //                    TicketFrom = input.TicketFrom,
        //                };
        //                //Publish Notification Data
        //                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewTicket, CreateAdminNotificationData, adminTargetUsersId.ToArray());
        //            }
        //            #endregion
        //        }



        //        return ObjectMapper.Map<TicketDetailDto>(ticketDetail);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}



    }
}
