using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.Companies;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.SendingMails;
using Sayarah.Application.Providers;
using Sayarah.Application.RegisterationRequests.Dto;
using Sayarah.Application.Users;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using Sayarah.RegisterationRequests;
using System.Globalization;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.RegisterationRequests
{
    public class RegisterationRequestAppService : AsyncCrudAppService<RegisterationRequest, RegisterationRequestDto, long, GetRegisterationRequestsInput, CreateRegisterationRequestDto, UpdateRegisterationRequestDto>, IRegisterationRequestAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IMainProviderAppService _mainProviderAppService;
        private readonly ICompanyAppService _companyAppService;
        private readonly ISendingMailsAppService _sendingMailsAppService;

        public ICommonAppService _commonAppService { get; set; }

        CultureInfo new_lang = new CultureInfo("ar");

        public RegisterationRequestAppService(
            IRepository<RegisterationRequest, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Provider, long> providerRepository,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService,
            IMainProviderAppService mainProviderAppService,
            ICompanyAppService companyAppService,
            ISendingMailsAppService sendingMailsAppService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _providerRepository = providerRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _commonAppService = commonAppService;
            _mainProviderAppService = mainProviderAppService;
            _companyAppService = companyAppService;
            _sendingMailsAppService = sendingMailsAppService;
        }

        public override async Task<RegisterationRequestDto> GetAsync(EntityDto<long> input)
        {
            var RegisterationRequest = await Repository.GetAll().Include(a => a.CompanyType)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(RegisterationRequest);
        }


        public override async Task<RegisterationRequestDto> CreateAsync(CreateRegisterationRequestDto input)
        {
            try
            {

                //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "RegisterationRequests", CodeField = "Code" });
                input.Status = RegisterationRequestStatus.DuringCreation;
                var registerationRequest = ObjectMapper.Map<RegisterationRequest>(input);
                registerationRequest = await Repository.InsertAsync(registerationRequest);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                // send to admin 
                //await SendNotificationToEmployees(new Dto.UpdateProviderFuelPrice
                //{
                //    RequestId = registerationRequest.Id,
                //    FuelType = registerationRequest.FuelType,
                //    NewPrice = registerationRequest.NewPrice,
                //    ProviderId = registerationRequest.ProviderId
                //});

                return MapToEntityDto(registerationRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<RegisterationRequestDto> ManageRequest(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {

                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == input.UserName && x.Id != input.Id && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));


                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
                    ObjectMapper.Map(input, registerationRequest);
                    await Repository.UpdateAsync(registerationRequest);
                    return MapToEntityDto(registerationRequest);
                }
                else
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == input.UserName && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));


                    //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "RegisterationRequests", CodeField = "Code" });
                    input.Status = RegisterationRequestStatus.DuringCreation;
                    if (input.EmailAddress != null)
                    {
                        input.EmailAddressConfirmed = true;
                    }
                    var registerationRequest = ObjectMapper.Map<RegisterationRequest>(input);
                    registerationRequest = await Repository.InsertAsync(registerationRequest);
                    await UnitOfWorkManager.Current.SaveChangesAsync();
                    return MapToEntityDto(registerationRequest);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<RegisterationRequestDto> HandlePhoneNumber(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    // check if phone exists 
                    var existUser = await _userRepository.CountAsync(a => a.PhoneNumber == input.PhoneNumber && a.IsDeleted == false);
                    if (existUser > 0)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistPhone"));

                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
                    registerationRequest.PhoneNumber = input.PhoneNumber;
                    registerationRequest.PhoneNumberConfirmationCode = new Random().Next(1000, 9999).ToString();
                    await Repository.UpdateAsync(registerationRequest);


                    SMSHelper smsHelper = new SMSHelper();

                    SendMessageInput data = new SendMessageInput
                    {
                        MessageText = L("MobileApi.Messages.CodeMessage", registerationRequest.PhoneNumberConfirmationCode),
                        PhoneNumbers = input.PhoneNumber
                    };
                    var sendResult = smsHelper.SendMessage(data);


                    var output = MapToEntityDto(registerationRequest);
                    output.PhoneNumberConfirmationCode = "";
                    return output;
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<RegisterationRequestDto> HandleConfirmPhoneNumber(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

                    if (registerationRequest.PhoneNumberConfirmationCode == input.PhoneNumberConfirmationCode)
                    {
                        // set phone is confirmed true
                        registerationRequest.PhoneNumberConfirmed = true;
                        await Repository.UpdateAsync(registerationRequest);
                        return MapToEntityDto(registerationRequest);
                    }
                    else
                    {
                        throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                    }
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<RegisterationRequestDto> HandleEmailAddress(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    // check if phone exists 
                    var existUser = await _userRepository.CountAsync(a => a.EmailAddress == input.EmailAddress && a.IsDeleted == false);
                    if (existUser > 0)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));


                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
                    registerationRequest.EmailAddress = input.EmailAddress;
                    registerationRequest.EmailAddressConfirmationCode = new Random().Next(1000, 9999).ToString();
                    await Repository.UpdateAsync(registerationRequest);


                    List<string> emails = new List<string>();
                    emails.Add(input.EmailAddress);

                    string[] ownerEmails = emails.ToArray();


                    var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                    {
                        Emails = ownerEmails,
                        datalst = new[] { L("MobileApi.Messages.CodeMessage" ,registerationRequest.EmailAddressConfirmationCode )
                    }
                    });

                    var output = MapToEntityDto(registerationRequest);
                    output.EmailAddressConfirmationCode = "";
                    return output;

                    //return MapToEntityDto(registerationRequest);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<RegisterationRequestDto> HandleConfirmEmailAddress(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

                    if (registerationRequest.EmailAddressConfirmationCode == input.EmailAddressConfirmationCode)
                    {
                        // set phone is confirmed true
                        registerationRequest.EmailAddressConfirmed = true;
                        await Repository.UpdateAsync(registerationRequest);
                        return MapToEntityDto(registerationRequest);
                    }
                    else
                    {
                        throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                    }
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<RegisterationRequestDto> CompleteRegisteration(UpdateRegisterationRequestDto input)
        {
            try
            {

                if (input.Id > 0)
                {

                    var registerationRequest = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
                    registerationRequest.Status = RegisterationRequestStatus.Pending;
                    await Repository.UpdateAsync(registerationRequest);
                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    // send notification to admin
                    await SendNotificationToEmployees(registerationRequest);

                    return MapToEntityDto(registerationRequest);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<bool> SendNotificationToEmployees(RegisterationRequest input)
        {
            try
            {

                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");


                // send notification  
                var users = await _userRepository.GetAll().Include(a => a.Permissions)
                    .Where(x => x.UserType == UserTypes.Admin).ToListAsync();

                if (users != null && users.Count > 0)
                {
                    //var currentUser = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
                    foreach (var item in users)
                    {
                        List<UserIdentifier> targetAdminUsersId = new List<UserIdentifier>();
                        targetAdminUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: item.Id));
                        CreateNotificationDto _createNotificationData = new CreateNotificationDto
                        {
                            EntityType = Entity_Type.NewRegisterationRequest,
                            EntityId = input.Id,
                            Message = L("Pages.RegisterationRequests.Messages.New")
                        };
                        //Publish Notification Data
                        await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewRegisterationRequest, _createNotificationData, targetAdminUsersId.ToArray());

                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public override async Task<RegisterationRequestDto> UpdateAsync(UpdateRegisterationRequestDto input)
        {

            var registerationRequest = await Repository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == input.Id);

            //if (!string.IsNullOrEmpty(registerationRequest.FilePath) && (string.IsNullOrEmpty(input.FilePath) || !registerationRequest.FilePath.Equals(input.FilePath)))
            //    Utilities.DeleteImage(10, registerationRequest.FilePath, new string[] {   });

            ObjectMapper.Map(input, registerationRequest);
            await Repository.UpdateAsync(registerationRequest);
            return MapToEntityDto(registerationRequest);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var registerationRequest = await Repository.GetAsync(input.Id);
            if (registerationRequest == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(registerationRequest);
        }

        public override async Task<PagedResultDto<RegisterationRequestDto>> GetAllAsync(GetRegisterationRequestsInput input)
        {
            var query = Repository.GetAll().Include(a => a.CompanyType).AsQueryable();

            //query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr));
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));
            query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.PhoneNumber.Contains(input.PhoneNumber));
            query = query.WhereIf(!string.IsNullOrEmpty(input.EmailAddress), at => at.EmailAddress.Contains(input.EmailAddress));
            query = query.WhereIf(input.AccountType.HasValue, at => at.AccountType == input.AccountType);
            query = query.WhereIf(input.Status.HasValue, at => at.Status == input.Status);
            query = query.WhereIf(input.HearAboutUs.HasValue, a => a.HearAboutUs == input.HearAboutUs);


            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var registerationRequests = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<RegisterationRequestDto>>(registerationRequests);
            return new PagedResultDto<RegisterationRequestDto>(count, _mappedList);
        }



        //[AbpAuthorize]
        //public async Task<bool> SendRefuseNotification(UpdateProviderFuelPrice input)
        //{

        //    var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
        //    if (provider == null)
        //        return false;

        //    // send notification to provider here 

        //    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
        //    string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

        //    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: provider.UserId.Value));

        //    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
        //    {
        //        SenderUserId = AbpSession.UserId.Value,
        //        Message = L("Pages.RegisterationRequests.Messages.Refused"),
        //        EntityType = Entity_Type.RefuseRegisterationRequest,
        //        EntityId = input.RequestId
        //    };
        //    //Publish Notification Data
        //    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.RefuseRegisterationRequest, CreateNotificationData, targetUsersId.ToArray());

        //    return true;
        //}


        [AbpAuthorize]
        public async Task<DataTableOutputDto<RegisterationRequestDto>> GetPaged(GetRegisterationRequestsPagedInput input)
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
                            RegisterationRequest registerationRequest = await Repository.GetAll().FirstOrDefaultAsync(a => a.Id == id);
                            if (registerationRequest != null)
                            {
                                if (input.action == "Delete")
                                {


                                    await Repository.DeleteAsync(registerationRequest);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                                else if (input.action == "Accept")
                                {
                                    registerationRequest.Status = RegisterationRequestStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await SendAcceptNotification(registerationRequest);


                                }
                                else if (input.action == "Refuse")
                                {
                                    registerationRequest.Status = RegisterationRequestStatus.Refused;

                                    // send refuse notification 

                                    await SendRefuseNotification(registerationRequest);

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
                            RegisterationRequest registerationRequest = await Repository.GetAll().FirstOrDefaultAsync(a => a.Id == id);
                            if (registerationRequest != null)
                            {
                                if (input.action == "Delete")
                                {

                                    await Repository.DeleteAsync(registerationRequest);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                else if (input.action == "Accept")
                                {
                                    registerationRequest.Status = RegisterationRequestStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await SendAcceptNotification(registerationRequest);

                                }
                                else if (input.action == "Refuse")
                                {
                                    registerationRequest.Status = RegisterationRequestStatus.Refused;

                                    // send refuse notification 

                                    await SendRefuseNotification(registerationRequest);

                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    var query = Repository.GetAll().Include(a => a.CompanyType).AsQueryable();



                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.PhoneNumber.Contains(input.PhoneNumber));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.EmailAddress), at => at.EmailAddress.Contains(input.EmailAddress));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.UserName.Contains(input.UserName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.RegNo), at => at.RegNo.Contains(input.RegNo));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.TaxNo), at => at.TaxNo.Contains(input.TaxNo));
                    query = query.WhereIf(input.AccountType.HasValue, at => at.AccountType == input.AccountType);
                    query = query.WhereIf(input.Status.HasValue, at => at.Status == input.Status);
                    query = query.WhereIf(input.HearAboutUs.HasValue, a => a.HearAboutUs == input.HearAboutUs);

                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    int filteredCount = await query.CountAsync();

                    var registerationRequests = await query
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();


                    return new DataTableOutputDto<RegisterationRequestDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<RegisterationRequestDto>>(registerationRequests)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<bool> SendAcceptNotification(RegisterationRequest input)
        {
            // first create provider account first 

            if (input.AccountType == AccountType.ServicesProvider || input.AccountType == AccountType.CleanProvider || input.AccountType == AccountType.FuelProvider)
            {
                var mainProvider = await _mainProviderAppService.CreateAsync(new CreateMainProviderDto
                {

                    Avatar = input.FilePath,
                    NameAr = input.NameAr,
                    NameEn = input.NameEn,
                    DescAr = input.DescAr,
                    DescEn = input.DescEn,
                    PhoneNumber = input.PhoneNumber,
                    RegNo = input.RegNo,
                    RegNoFilePath = input.RegNoFilePath,
                    AccountType = input.AccountType,

                    IsClean = input.AccountType == AccountType.CleanProvider ? true : false,
                    IsFuel = input.AccountType == AccountType.FuelProvider ? true : false,
                    IsMaintain = input.AccountType == AccountType.ServicesProvider ? true : false,

                    TaxNo = input.TaxNo,
                    TaxNoFilePath = input.TaxNoFilePath,

                    User = new Users.Dto.CreateNewUserInput
                    {
                        Name = input.NameAr,
                        IsActive = true,
                        Password = input.Password,
                        PhoneNumber = input?.PhoneNumber,
                        Avatar = input.FilePath,
                        EmailAddress = input.EmailAddress,
                        RoleName = RolesNames.MainProvider,
                        UserName = !string.IsNullOrEmpty(input.UserName) ? input.UserName : input.EmailAddress,
                        Surname = input.NameAr,
                        UserType = UserTypes.MainProvider,
                    }
                });

                input.AccountId = mainProvider.Id;
            }
            else if (input.AccountType == AccountType.Company)
            {
                var company = await _companyAppService.CreateAsync(new Companies.Dto.CreateCompanyDto
                {

                    Avatar = input.FilePath,
                    NameAr = input.NameAr,
                    NameEn = input.NameEn,
                    DescAr = input.DescAr,
                    DescEn = input.DescEn,
                    PhoneNumber = input.PhoneNumber,
                    CompanyTypeId = input.CompanyTypeId,
                    RegNoFilePath = input.RegNoFilePath,
                    RegNo = input.RegNo,
                    TaxNo = input.TaxNo,
                    TaxNoFilePath = input.TaxNoFilePath,

                    IsFuel = true,

                    User = new Users.Dto.CreateNewUserInput
                    {
                        Name = input.NameAr,
                        IsActive = true,
                        Password = input.Password,
                        PhoneNumber = input?.PhoneNumber,
                        Avatar = input.FilePath,
                        EmailAddress = input.EmailAddress,
                        RoleName = RolesNames.Company,
                        UserName = !string.IsNullOrEmpty(input.UserName) ? input.UserName : input.EmailAddress,
                        Surname = input.NameAr,
                        UserType = UserTypes.Company,
                    }
                });

                input.AccountId = company.Id;
            }

            await Repository.UpdateAsync(input);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            // send email and sms 
            #region SendEmail

            string callbackUrl = DomainUrl + "Account/Login";
            if (!string.IsNullOrEmpty(input.EmailAddress))
            {

                List<string> emails = new List<string>();
                emails.Add(input.EmailAddress);

                string[] ownerEmails = emails.ToArray();


                // send email here 
                var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                {
                    Emails = ownerEmails,
                    Url = callbackUrl,
                    UrlTitle = L("Pages.Register.Login"),
                    datalst = new[] { $"تم قبول طلب انضمامك لمنصة سيارة و يمكنك تسجيل الدخول الأن" ,
                                       $"البريد الإلكتروني : {input.EmailAddress}",
                                       $"كلمة المرور : {input.Password}"
                    }
                });
            }

            #endregion


            SMSHelper smsHelper = new SMSHelper();

            SendMessageInput data = new SendMessageInput
            {
                MessageText = $"تم قبول طلب انضمامك لمنصة سيارة و يمكنك تسجيل الدخول الأن عن طريق الضغط على الرابط التالي : {callbackUrl}",
                PhoneNumbers = input.PhoneNumber
            };
            var sendResult = smsHelper.SendMessage(data);


            return true;
        }



        [AbpAuthorize]
        public async Task<RegisterationRequestDto> RefuseRegisterationRequest(UpdateRegisterationRequestDto input)
        {
            try
            {

                var registerationRequest = await Repository.GetAsync(input.Id);
                registerationRequest.Status = RegisterationRequestStatus.Refused;
                registerationRequest.RefuseReason = input.RefuseReason;

                await Repository.UpdateAsync(registerationRequest);
                await UnitOfWorkManager.Current.SaveChangesAsync();


                // send notification to trainer 

                await SendRefuseNotification(registerationRequest);

                return MapToEntityDto(registerationRequest);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<RegisterationRequestDto> ApproveRegisterationRequest(EntityDto<long> input)
        {
            try
            {

                var registerationRequest = await Repository.GetAsync(input.Id);
                registerationRequest.Status = RegisterationRequestStatus.Accepted;

                await Repository.UpdateAsync(registerationRequest);
                await UnitOfWorkManager.Current.SaveChangesAsync();


                // send notification to trainer 
                await SendAcceptNotification(registerationRequest);

                return MapToEntityDto(registerationRequest);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [AbpAuthorize]
        public async Task<bool> SendRefuseNotification(RegisterationRequest input)
        {

            #region SendEmail

            if (!string.IsNullOrEmpty(input.EmailAddress))
            {

                List<string> emails = new List<string>();
                emails.Add(input.EmailAddress);

                string[] ownerEmails = emails.ToArray();


                // send email here 
                var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                {
                    Emails = ownerEmails,
                    datalst = new[] { $"تم رفض طلب انضمامك لمنصة سيارة " ,
                                       $"السبب : {input.RefuseReason}"
                    }
                });
            }

            #endregion



            SMSHelper smsHelper = new SMSHelper();

            SendMessageInput data = new SendMessageInput
            {
                MessageText = $"تم رفض طلب انضمامك لمنصة سيارة بسبب  : {input.RefuseReason}",
                PhoneNumbers = input.PhoneNumber
            };
            var sendResult = smsHelper.SendMessage(data);


            return true;
        }


        public async Task<bool> TestSendNotification(RegisterationRequest input)
        {
            try
            {


                #region SendEmail

                if (!string.IsNullOrEmpty(input.EmailAddress))
                {

                    List<string> emails = new List<string>();
                    emails.Add(input.EmailAddress);

                    string[] ownerEmails = emails.ToArray();


                    // send email here 
                    var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                    {
                        Emails = ownerEmails,
                        datalst = new[] { $"تم رفض طلب انضمامك لمنصة سيارة " ,
                                       $"السبب : {input.RefuseReason}"
                    }
                    });
                }

                #endregion


                //SMSHelper smsHelper = new SMSHelper();

                //SendMessageInput data = new SendMessageInput
                //{
                //    MessageText = $"تم رفض طلب انضمامك لمنصة سيارة بسبب  : {input.RefuseReason}",
                //    PhoneNumbers = input.PhoneNumber
                //};
                //var sendResult = smsHelper.SendMessage(data);


                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
