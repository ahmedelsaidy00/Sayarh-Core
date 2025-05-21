using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.SendingMails;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Wallets;
using System.Globalization;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Companies
{
    public class CompanyAppService : AsyncCrudAppService<Company, CompanyDto, long, GetCompaniesInput, CreateCompanyDto, UpdateCompanyDto>, ICompanyAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly ICommonAppService _commonService;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        private readonly IRepository<CompanyWalletTransaction, long> _companyWalletTransactionRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly ISendingMailsAppService _sendingMailsAppService;


        CultureInfo new_lang = new CultureInfo("ar");

        public CompanyAppService(
            IRepository<Company, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
             ICommonAppService commonService,
             AbpNotificationHelper abpNotificationHelper,
                IRepository<UserDevice, long> userDevicesRepository,
                IRepository<CompanyWalletTransaction, long> companyWalletTransactionRepository,
            ISendingMailsAppService sendingMailsAppService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _commonService = commonService;
            _userDevicesRepository = userDevicesRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _sendingMailsAppService = sendingMailsAppService;
            _companyWalletTransactionRepository = companyWalletTransactionRepository;

        }

        public override async Task<CompanyDto> GetAsync(EntityDto<long> input)
        {
            var Company = await Repository.GetAll()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Company);
        }

        [AbpAuthorize]
        public override async Task<CompanyDto> CreateAsync(CreateCompanyDto input)
        {
            var user = new UserDto();

            if (input.User != null)
            {
                input.User.Name ??= input.NameAr;
                input.User.Surname ??= input.NameAr;
                input.User.UserName ??= input.User.EmailAddress;
                input.User.UserType = UserTypes.Company;
                input.User.PhoneNumber = input.PhoneNumber;

                user = await _userService.CreateNewUser(input.User);
            }

            if (user.Id <= 0)
            {
                throw new UserFriendlyException(L("Pages.Companies.Error.CantCreateUser"));
            }

            input.UserId = user.Id;
            var company = ObjectMapper.Map<Company>(input);
            company.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Companies", CodeField = "Code" });

            company = await Repository.InsertAsync(company);
            await CurrentUnitOfWork.SaveChangesAsync();

            var dbUser = await _userRepository.FirstOrDefaultAsync(user.Id);
            dbUser.CompanyId = company.Id;
            await _userRepository.UpdateAsync(dbUser);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToEntityDto(company);
        }

        [AbpAuthorize]
        public override async Task<CompanyDto> UpdateAsync(UpdateCompanyDto input)
        {

            var company = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(company.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !company.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(3, company.Avatar, new string[] { "600x600_" });


            if (input.User != null)
            {
                company.User.Name = input.NameAr;
                company.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                company.User.Surname = input.NameEn;
                company.User.PhoneNumber = input.PhoneNumber;
                input.User.PhoneNumber = input.PhoneNumber;
                company.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                company.User.Password : _userManager.PasswordHasher.HashPassword(company.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                     company.User.Password : _userManager.PasswordHasher.HashPassword(company.User, input.User.Password);

                company.User.EmailAddress = input.User.EmailAddress;


                if (!string.IsNullOrEmpty(company.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == company.User.UserName && x.Id != company.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }


                if (!string.IsNullOrEmpty(company.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == company.User.EmailAddress && x.Id != company.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }
                await _userManager.UpdateAsync(company.User);



            }
            ObjectMapper.Map(input, company);

            await Repository.UpdateAsync(company);
            return MapToEntityDto(company);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var company = await Repository.GetAsync(input.Id);
            if (company == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(company);
        }
        public override async Task<PagedResultDto<CompanyDto>> GetAllAsync(GetCompaniesInput input)
        {
            var query = Repository.GetAll().Include(at => at.User).AsQueryable();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);


            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var companies = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<CompanyDto>>(companies);
            return new PagedResultDto<CompanyDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<CompanyDto>> GetPaged(GetCompaniesPagedInput input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var ids = input.ids?.Select(int.Parse).ToList() ?? [];

                if (input.actionType == "GroupAction")
                {
                    foreach (var id in ids)
                    {
                        var company = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                        if (company == null) continue;

                        switch (input.action)
                        {
                            case "Delete":
                                await SendLogoutNotification(new EntityDto<long> { Id = company.Id });
                                company.User.IsDeleted = true;
                                company.User.DeletionTime = DateTime.Now;
                                company.User.DeleterUserId = AbpSession.UserId;
                                company.IsDeleted = true;
                                company.DeletionTime = DateTime.Now;
                                company.DeleterUserId = AbpSession.UserId;
                                await Repository.UpdateAsync(company);
                                break;
                            case "Active":
                                var activeUser = await _userManager.FindByIdAsync(company.UserId.Value.ToString());
                                activeUser.IsActive = true;
                                activeUser.LockoutEndDateUtc = null;
                                await _userManager.UpdateAsync(activeUser);
                                await Repository.UpdateAsync(company);
                                break;
                            case "Deactive":
                                await SendLogoutNotification(new EntityDto<long> { Id = company.Id });
                                var deactiveUser = await _userManager.FindByIdAsync(company.UserId.Value.ToString());
                                deactiveUser.IsActive = false;
                                await _userManager.UpdateAsync(deactiveUser);
                                await Repository.UpdateAsync(company);
                                break;
                        }
                    }

                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction" && ids.Any())
                {
                    var id = ids.First();
                    var company = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                    if (company != null)
                    {
                        switch (input.action)
                        {
                            case "Delete":
                                await SendLogoutNotification(new EntityDto<long> { Id = company.Id });
                                company.User.IsDeleted = true;
                                company.User.DeletionTime = DateTime.Now;
                                company.User.DeleterUserId = AbpSession.UserId;
                                company.IsDeleted = true;
                                company.DeletionTime = DateTime.Now;
                                company.DeleterUserId = AbpSession.UserId;
                                await Repository.UpdateAsync(company);
                                break;
                            case "Active":
                                var activeUser = await _userManager.FindByIdAsync(company.UserId.Value.ToString());
                                activeUser.IsActive = true;
                                activeUser.LockoutEndDateUtc = null;
                                await _userManager.UpdateAsync(activeUser);
                                break;
                            case "Deactive":
                                await SendLogoutNotification(new EntityDto<long> { Id = company.Id });
                                var deactiveUser = await _userManager.FindByIdAsync(company.UserId.Value.ToString());
                                deactiveUser.IsActive = false;
                                await _userManager.UpdateAsync(deactiveUser);
                                break;
                        }

                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                }

                var query = Repository.GetAll()
                    .Include(a => a.User)
                    .Where(at => at.User.UserType == UserTypes.Company)
                    .FilterDataTable(input)
                    .WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code))
                    .WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name))
                    .WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.User.PhoneNumber.Contains(input.PhoneNumber))
                    .WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive)
                    .WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                var totalCount = await Repository.CountAsync();
                var filteredCount = await query.CountAsync();

                var companies = await query.Include(x => x.CreatorUser)
                                           .Include(x => x.LastModifierUser)
                                           .OrderBy($"{input.columns[input.order[0].column].name} {input.order[0].dir}")
                                           .Skip(input.start)
                                           .Take(input.length)
                                           .ToListAsync();

                return new DataTableOutputDto<CompanyDto>
                {
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = filteredCount,
                    aaData = ObjectMapper.Map<List<CompanyDto>>(companies)
                };
            }
        }

        [AbpAuthorize]
        public async Task<CompanyDto> GetByUserId(EntityDto<long> input)
        {
            var company = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(company);
        }
        public async Task SendLogoutNotification(EntityDto<long> input)
        {
            var users = await _userRepository.GetAll().Where(a => a.CompanyId == input.Id).ToListAsync();
            foreach (var user in users)
            {
                var userLang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, user.Id)) ?? new_lang.ToString();

                var userDevices = await _userDevicesRepository.GetAll().Where(x => x.UserId == user.Id).ToListAsync();
                foreach (var item in userDevices)
                {
                    var fcmClient = new FCMPushNotification();
                    await fcmClient.SendNotification(new FcmNotificationInput
                    {
                        RegistrationToken = item.RegistrationToken,
                        Title = L("Common.SystemTitle"),
                        Message = L("Pages.Notifications.LogOut"),
                        Type = FcmNotificationType.Logout
                    });
                }

                var targets = new[] { new UserIdentifier(AbpSession.TenantId, user.Id) };
                var notification = new CreateNotificationDto
                {
                    Message = L("Pages.Notifications.LogOut"),
                    EntityType = Entity_Type.Logout
                };
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Logout, notification, targets);
            }
        }

        [AbpAuthorize]
        public async Task<GetWalletDetailsDto> GetWalletDetails(EntityDto<long> input)
        {
            var company = await Repository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);

            var last = await _companyWalletTransactionRepository.GetAll()
                .Where(a => a.CompanyId == input.Id && a.TransactionType == TransactionType.Deposit && a.BankId.HasValue && a.DepositStatus == DepositStatus.Accepted)
                .OrderByDescending(a => a.CreationTime)
                .FirstOrDefaultAsync();

            var output = ObjectMapper.Map<GetWalletDetailsDto>(company);
            if (last != null)
            {
                output.LastDepositCreationTime = last.CreationTime;
            }

            return output;
        }

        [AbpAuthorize]
        public async Task<UserDto> HandleEmailAddress(ChangeEmailAndPhone input)
        {
            var existUser = await _userRepository.CountAsync(a => a.EmailAddress == input.EmailAddress && !a.IsDeleted);
            if (existUser > 0)
            {
                throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
            }

            var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            user.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
            await _userRepository.UpdateAsync(user);

            var emails = new[] { input.EmailAddress };
            await _sendingMailsAppService.SendEmail(new SendEmailRequest
            {
                Emails = emails,
                datalst = [L("MobileApi.Messages.CodeMessage", user.EmailConfirmationCode)]
            });

            return ObjectMapper.Map<UserDto>(user);
        }

        [AbpAuthorize]
        public async Task<ChangeEmailAndPhone> HandleConfirmEmailAddress(ChangeEmailAndPhone input)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == AbpSession.UserId);

            if (user.EmailConfirmationCode == input.ConfirmationCode)
            {
                user.EmailAddress = input.EmailAddress;
                user.IsEmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                return new ChangeEmailAndPhone { EmailAddress = input.EmailAddress, Id = input.Id };
            }

            throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
        }

        [AbpAuthorize]
        public async Task<UserDto> HandlePhoneNumber(ChangeEmailAndPhone input)
        {
            // Check if phone exists
            var existUser = await _userRepository.CountAsync(a => a.PhoneNumber == input.PhoneNumber && !a.IsDeleted);
            if (existUser > 0)
            {
                throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistPhone"));
            }

            var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            user.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
            await _userRepository.UpdateAsync(user);

            var smsHelper = new SMSHelper();
            var data = new SendMessageInput
            {
                MessageText = L("MobileApi.Messages.CodeMessage", user.EmailConfirmationCode),
                PhoneNumbers = input.PhoneNumber
            };
            _ = smsHelper.SendMessage(data);

            return ObjectMapper.Map<UserDto>(user);
        }

        [AbpAuthorize]
        public async Task<ChangeEmailAndPhone> HandleConfirmPhoneNumber(ChangeEmailAndPhone input)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == AbpSession.UserId);

            if (user.EmailConfirmationCode == input.ConfirmationCode)
            {
                user.PhoneNumber = input.PhoneNumber;
                user.IsPhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                return new ChangeEmailAndPhone
                {
                    PhoneNumber = input.PhoneNumber,
                    Id = input.Id
                };
            }

            throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
        }

        [AbpAuthorize]
        public async Task<List<CompanyNameDto>> GetAllCompanies(GetCompaniesInput input)
        {
            var query = Repository.GetAll()
                .Include(at => at.User)
                .WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name))
                .WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);

            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = await query.CountAsync();
            }

            var companies = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return ObjectMapper.Map<List<CompanyNameDto>>(companies);
        }
    }
}
