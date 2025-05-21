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
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using System.Globalization;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Providers
{
    public class MainProviderAppService : AsyncCrudAppService<MainProvider, MainProviderDto, long, GetMainProvidersInput, CreateMainProviderDto, UpdateMainProviderDto>, IMainProviderAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly ICommonAppService _commonService;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly ISendingMailsAppService _sendingMailsAppService;

        CultureInfo new_lang = new CultureInfo("ar");

        public MainProviderAppService(
            IRepository<MainProvider, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
             ICommonAppService commonService,
             AbpNotificationHelper abpNotificationHelper,
                IRepository<UserDevice, long> userDevicesRepository,
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

        }

        public override async Task<MainProviderDto> GetAsync(EntityDto<long> input)
        {
            var MainProvider = await Repository.GetAll()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(MainProvider);
        }

        [AbpAuthorize]
        public override async Task<MainProviderDto> CreateAsync(CreateMainProviderDto input)
        {
            try
            {
                //Check if mainProvider exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                var user = new UserDto();
                if (input.User != null)
                {
                    input.User.Name = string.IsNullOrEmpty(input.User.Name) ? input.NameAr : input.User.Name;
                    input.User.Surname = string.IsNullOrEmpty(input.User.Surname) ? input.NameAr : input.User.Surname;
                    input.User.PhoneNumber = input.PhoneNumber;
                    input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                    input.User.UserType = UserTypes.MainProvider;
                    user = await _userService.CreateNewUser(input.User);
                }
                if (user.Id > 0)
                {
                    input.UserId = user.Id;
                    var mainProvider = ObjectMapper.Map<MainProvider>(input);
                    mainProvider.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "MainProviders", CodeField = "Code" });
                    mainProvider = await Repository.InsertAsync(mainProvider);
                    await CurrentUnitOfWork.SaveChangesAsync();


                    // update user with comapny id 
                    var _user = await _userRepository.FirstOrDefaultAsync(user.Id);
                    _user.MainProviderId = mainProvider.Id;
                    await _userRepository.UpdateAsync(_user);
                    await CurrentUnitOfWork.SaveChangesAsync();


                    return MapToEntityDto(mainProvider);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.MainProviders.Error.CantCreateUser"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [AbpAuthorize]
        public override async Task<MainProviderDto> UpdateAsync(UpdateMainProviderDto input)
        {

            var mainProvider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(mainProvider.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !mainProvider.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(7, mainProvider.Avatar, new string[] { "600x600_" });


            if (input.User != null)
            {
                mainProvider.User.Name = string.IsNullOrEmpty(input.User.Name) ? input.NameAr : input.User.Name;
                mainProvider.User.UserName = !string.IsNullOrEmpty(input.User.UserName) ? input.User.UserName : input.User.EmailAddress;
                mainProvider.User.Surname = string.IsNullOrEmpty(input.User.Surname) ? input.NameAr : input.User.Surname;
                mainProvider.User.PhoneNumber = input.PhoneNumber;
                input.User.PhoneNumber = input.PhoneNumber;

                mainProvider.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                            mainProvider.User.Password : _userManager.PasswordHasher.HashPassword(mainProvider.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                   mainProvider.User.Password : _userManager.PasswordHasher.HashPassword(mainProvider.User, input.User.Password);


                mainProvider.User.EmailAddress = input.User.EmailAddress;


                if (!string.IsNullOrEmpty(mainProvider.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == mainProvider.User.UserName && x.Id != mainProvider.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }


                if (!string.IsNullOrEmpty(mainProvider.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == mainProvider.User.EmailAddress && x.Id != mainProvider.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }

                await _userManager.UpdateAsync(mainProvider.User);


            }
            ObjectMapper.Map(input, mainProvider);

            await Repository.UpdateAsync(mainProvider);
            return MapToEntityDto(mainProvider);
        }



        [AbpAuthorize]
        public async Task<MainProviderDto> UpdateDocuments(UpdateMainProviderDocumentsDto input)
        {

            var mainProvider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, mainProvider);
            await Repository.UpdateAsync(mainProvider);
            return MapToEntityDto(mainProvider);
        }



        [AbpAuthorize]
        public async Task<MainProviderDto> UpdateBankInfo(UpdateMainProviderBankInfoDto input)
        {

            var mainProvider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, mainProvider);
            await Repository.UpdateAsync(mainProvider);
            return MapToEntityDto(mainProvider);
        }

        [AbpAuthorize]
        public async Task<MainProviderDto> UpdateNationalAddress(UpdateMainProviderNationalAddressDto input)
        {

            var mainProvider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, mainProvider);
            await Repository.UpdateAsync(mainProvider);
            return MapToEntityDto(mainProvider);
        }


        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var mainProvider = await Repository.GetAsync(input.Id);
            if (mainProvider == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(mainProvider);
        }

        public override async Task<PagedResultDto<MainProviderDto>> GetAllAsync(GetMainProvidersInput input)
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

            var mainProviders = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<MainProviderDto>>(mainProviders);
            return new PagedResultDto<MainProviderDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<MainProviderDto>> GetPaged(GetMainProvidersPagedInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    int id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            id = Convert.ToInt32(input.ids[i]);

                            MainProvider mainProvider = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);

                            if (mainProvider != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _mainProviderClinicRepository.CountAsync(a => a.MainProviderId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.MainProviders.HasClinics"));

                                    await SendLogoutNotification(new EntityDto<long> { Id = mainProvider.Id });


                                    mainProvider.User.IsDeleted = true;
                                    mainProvider.User.DeletionTime = DateTime.Now;
                                    mainProvider.User.DeleterUserId = AbpSession.UserId;
                                    mainProvider.IsDeleted = true;
                                    mainProvider.DeletionTime = DateTime.Now;
                                    mainProvider.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(mainProvider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    mainProvider.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(mainProvider.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(mainProvider);
                                }
                                else if (input.action == "Deactive")
                                {

                                    await SendLogoutNotification(new EntityDto<long> { Id = mainProvider.Id });


                                    var user = await _userManager.FindByIdAsync(mainProvider.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(mainProvider);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            MainProvider mainProvider = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (mainProvider != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _mainProviderClinicRepository.CountAsync(a => a.MainProviderId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.MainProviders.HasClinics"));
                                    await SendLogoutNotification(new EntityDto<long> { Id = mainProvider.Id });


                                    mainProvider.User.IsDeleted = true;
                                    mainProvider.User.DeletionTime = DateTime.Now;
                                    mainProvider.User.DeleterUserId = AbpSession.UserId;
                                    mainProvider.IsDeleted = true;
                                    mainProvider.DeletionTime = DateTime.Now;
                                    mainProvider.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(mainProvider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    mainProvider.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(mainProvider.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Deactive")
                                {
                                    await SendLogoutNotification(new EntityDto<long> { Id = mainProvider.Id });


                                    var user = await _userManager.FindByIdAsync(mainProvider.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll().Include(a => a.User).Where(at => at.User.UserType == UserTypes.MainProvider);
                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.User.PhoneNumber.Contains(input.PhoneNumber));
                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                    int filteredCount = await query.CountAsync();
                    var mainProviders = await query.Include(x => x.User)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<MainProviderDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<MainProviderDto>>(mainProviders)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<MainProviderBankInfoDto>> GetBankInfoPaged(GetMainProvidersPagedInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {

                    var query = Repository.GetAll()
                        .Where(at => at.User.UserType == UserTypes.MainProvider);
                    int count = await query.CountAsync();
                    query = query.FilterDataTable(input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.User.PhoneNumber.Contains(input.PhoneNumber));
                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.Id == input.MainProviderId);

                    int filteredCount = await query.CountAsync();
                    var mainProviders = await query.OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();

                    return new DataTableOutputDto<MainProviderBankInfoDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<MainProviderBankInfoDto>>(mainProviders)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<MainProviderDto> GetByUserId(EntityDto<long> input)
        {
            var MainProvider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(MainProvider);
        }



        public async Task SendLogoutNotification(EntityDto<long> input)
        {
            var users = await _userRepository.GetAll().Where(a => a.MainProviderId == input.Id).ToListAsync();
            if (users.Count > 0)
            {

                foreach (var user in users)
                {


                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();


                    var _lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, user.Id));
                    if (_lang == null)
                        _lang = new_lang.ToString();



                    FCMPushNotification fcmPushClient = new FCMPushNotification();
                    var userDevices = await _userDevicesRepository.GetAll().Where(x => x.UserId == user.Id).ToListAsync();


                    if (userDevices != null && userDevices.Count > 0)
                    {
                        foreach (var item in userDevices)
                        {
                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                            {
                                RegistrationToken = item.RegistrationToken,
                                Title = L("Common.SystemTitle"),
                                Message = L("Pages.Notifications.LogOut"),
                                Type = FcmNotificationType.Logout

                            });
                        }
                    }

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: user.Id));


                    CreateNotificationDto _createUserNotificationData = new CreateNotificationDto
                    {
                        //SenderUserName = (await _userManager.GetUserByIdAsync(AbpSession.UserId.Value)).Name,
                        Message = L("Pages.Notifications.LogOut"),
                        EntityType = Entity_Type.Logout,
                        //EntityId = input.AdvertisementId,

                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Logout, _createUserNotificationData, targetUsersId.ToArray());

                }

            }

        }



        [AbpAuthorize]
        public async Task<UserDto> HandleEmailAddress(ChangeEmailAndPhone input)
        {
            try
            {

                //if (input.Id > 0)
                //{
                // check if phone exists 
                var existUser = await _userRepository.CountAsync(a => a.EmailAddress == input.EmailAddress && a.IsDeleted == false);
                if (existUser > 0)
                    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));

                var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);

                //var mainProvider = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.Id);
                user.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
                await _userRepository.UpdateAsync(user);

                List<string> emails = new List<string>();
                emails.Add(input.EmailAddress);

                string[] ownerEmails = emails.ToArray();

                var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                {
                    Emails = ownerEmails,
                    datalst = new[] { L("MobileApi.Messages.CodeMessage" ,user.EmailConfirmationCode )
                    }
                });

                return ObjectMapper.Map<UserDto>(user);
                //}
                //else
                //{
                //    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<ChangeEmailAndPhone> HandleConfirmEmailAddress(ChangeEmailAndPhone input)
        {
            try
            {

                //if (input.Id > 0)
                //{
                var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == AbpSession.UserId);

                if (user.EmailConfirmationCode == input.ConfirmationCode)
                {
                    // set phone is confirmed true
                    user.EmailAddress = input.EmailAddress;
                    user.IsEmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    return new ChangeEmailAndPhone { EmailAddress = input.EmailAddress, Id = input.Id };
                }
                else
                {
                    throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                }
                //}
                //else
                //{
                //    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<UserDto> HandlePhoneNumber(ChangeEmailAndPhone input)
        {
            try
            {

                //if (input.Id > 0)
                //{
                // check if phone exists 
                var existUser = await _userRepository.CountAsync(a => a.PhoneNumber == input.PhoneNumber && a.IsDeleted == false);
                if (existUser > 0)
                    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistPhone"));

                var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);

                user.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
                await _userRepository.UpdateAsync(user);

                SMSHelper smsHelper = new SMSHelper();
                SendMessageInput data = new SendMessageInput
                {
                    MessageText = L("MobileApi.Messages.CodeMessage", user.EmailConfirmationCode),
                    PhoneNumbers = input.PhoneNumber
                };
                var sendResult = smsHelper.SendMessage(data);


                return ObjectMapper.Map<UserDto>(user);
                //}
                //else
                //{
                //    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<ChangeEmailAndPhone> HandleConfirmPhoneNumber(ChangeEmailAndPhone input)
        {
            try
            {

                //if (input.Id > 0)
                //{
                var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == AbpSession.UserId);

                if (user.EmailConfirmationCode == input.ConfirmationCode)
                {
                    // set phone is confirmed true
                    user.PhoneNumber = input.PhoneNumber;
                    user.IsPhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    return new ChangeEmailAndPhone { PhoneNumber = input.PhoneNumber, Id = input.Id };
                }
                else
                {
                    throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                }
                //}
                //else
                //{
                //    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [AbpAuthorize]
        public async Task<string> ExportBankInfoExcel(GetMainProvidersExcelInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {


                        //var _transactions = await _storedProcedureAppService.GetTransactionsReport(input);

                        var query = Repository.GetAll();

                        query = query.WhereIf(input.MainProviderId.HasValue, at => at.Id == input.MainProviderId);

                        int count = query.Count();

                        var transactions = await query.OrderByDescending(x => x.Id)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<MainProviderBankInfoDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {

                            var excelData = new List<MainProviderBankInfoExcelDto>();


                            excelData = ObjectMapper.Map<List<MainProviderBankInfoExcelDto>>(cBList.Select(m => new MainProviderBankInfoExcelDto
                            {
                                MainProvider = m.Name,
                                BankName = m.BankName,
                                AccountName = m.AccountName,
                                Iban = m.Iban,
                                AccountNumber = m.AccountNumber
                            }));

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();



                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "نوع الملف",
                                Value = "البيانات البنكية"
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "تاريخ الإصدار",
                                Value = DateTime.Now.ToString()
                            });


                            _options = new RequestFuelExcelOptionsDto
                            {
                                ExcelDate = DateTime.Now.ToString(),
                                ExcelType = "البيانات البنكية",
                                KeyValues = _keyValues
                            };

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            ExcelSource source = ExcelSource.BankInfo;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.MainProvidersBankInfo.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }
}
