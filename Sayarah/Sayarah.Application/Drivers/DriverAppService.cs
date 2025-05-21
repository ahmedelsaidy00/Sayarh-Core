using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Drivers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Drivers
{
    public class DriverAppService : AsyncCrudAppService<Driver, DriverDto, long, GetDriversInput, CreateDriverDto, UpdateDriverDto>, IDriverAppService
    {

        private readonly IUserAppService _userService;
        public UserManager UserManager { get; set; }

        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly ICommonAppService _commonService;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<DriverVeichle, long> _driverVeichleRepository;

        public DriverAppService(
            IRepository<Driver, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
             ICommonAppService commonService,
             IRepository<Branch, long> branchRepository,
             IRepository<Company, long> companyRepository,
             IRepository<DriverVeichle, long> driverVeichleRepository,
              RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _commonService = commonService;
            _branchRepository = branchRepository;
            _companyRepository = companyRepository;
            _roleManager = roleManager;
            _driverVeichleRepository = driverVeichleRepository;
        }

        public override async Task<DriverDto> GetAsync(EntityDto<long> input)
        {
            var Driver = await Repository.GetAll()
                .Include(x => x.User)
                .Include(x => x.Branch.Company)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Driver);
        }

        [AbpAuthorize]
        public override async Task<DriverDto> CreateAsync(CreateDriverDto input)
        {
            try
            {


                var user = new UserDto();
                if (input.User != null)
                {
                    input.User.Name = input.Name;
                    input.User.Surname = input.Name;
                    input.User.PhoneNumber = input.PhoneNumber;
                    input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                    input.User.UserType = UserTypes.Driver;
                    input.User.Avatar = input.Avatar;

                    input.User.EmailAddress = input.EmailAddress;

                    user = await _userService.CreateNewUser(input.User);
                }
                if (user.Id > 0)
                {
                    input.UserId = user.Id;
                    var driver = ObjectMapper.Map<Driver>(input);
                    driver.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Drivers", CodeField = "Code" });
                    driver = await Repository.InsertAsync(driver);

                    await CurrentUnitOfWork.SaveChangesAsync();


                    // update user with comapny id 
                    var _user = await _userRepository.FirstOrDefaultAsync(user.Id);
                    _user.BranchId = driver.BranchId;
                    _user.CompanyId = input.CompanyId;
                    await _userRepository.UpdateAsync(_user);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    return MapToEntityDto(driver);

                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Drivers.Error.CantCreateUser"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<DriverDto> UpdateMobile(UpdateDriverProfileInput input)
        {
            try
            {
                var driver = await Repository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);


                var user = await UserManager.GetUserByIdAsync(AbpSession.UserId.Value);
                user.Name = input.Name;
                user.Surname = input.Name;

                await UserManager.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                input.Id = driver.Id;
                ObjectMapper.Map(input, driver);

                await Repository.UpdateAsync(driver);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                return MapToEntityDto(driver);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // public async Task<DriverDto> UpdateDriverCode(UpdateDriverCodeInput input)
        //{
        //    try
        //    {
        //        var driver = await Repository.FirstOrDefaultAsync(a => a.Id == input.DriverId);

        //        driver.DriverCode = input.DriverCode; 
        //        ObjectMapper.Map(input, driver); 
        //        await Repository.UpdateAsync(driver);
        //        await UnitOfWorkManager.Current.SaveChangesAsync();
        //        return MapToEntityDto(driver);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [AbpAuthorize]
        public override async Task<DriverDto> UpdateAsync(UpdateDriverDto input)
        {

            var driver = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(driver.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !driver.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(5, driver.Avatar, new string[] { "600x600_" });

            if (!string.IsNullOrEmpty(driver.Licence) && (string.IsNullOrEmpty(input.Licence) || !driver.Licence.Equals(input.Licence)))
                Utilities.DeleteImage(5, driver.Licence, new string[] { "600x600_" });


            if (input.User != null)
            {
                driver.User.Name = input.Name;
                driver.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                driver.User.Surname = input.Name;
                driver.User.PhoneNumber = input.PhoneNumber;
                input.User.PhoneNumber = input.PhoneNumber;
                driver.User.Avatar = input.Avatar;
                driver.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                driver.User.Password : _userManager.PasswordHasher.HashPassword(driver.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                driver.User.Password : _userManager.PasswordHasher.HashPassword(driver.User, input.User.Password);

                driver.User.EmailAddress = string.IsNullOrEmpty(input.EmailAddress) ? input.User.EmailAddress : input.EmailAddress;

                if (!string.IsNullOrEmpty(driver.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == driver.User.UserName && x.Id != driver.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }


                if (!string.IsNullOrEmpty(driver.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == driver.User.EmailAddress && x.Id != driver.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }


                await _userManager.UpdateAsync(driver.User);

            }
            ObjectMapper.Map(input, driver);
            await Repository.UpdateAsync(driver);
            return MapToEntityDto(driver);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var driver = await Repository.GetAsync(input.Id);
            if (driver == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(driver);
        }

        public override async Task<PagedResultDto<DriverDto>> GetAllAsync(GetDriversInput input)
        {
            try
            {

                var query = Repository.GetAll().Include(at => at.User).AsQueryable();

                query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                query = query.WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);
                query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                query = query.WhereIf(input.ExcludedList != null && input.ExcludedList.Count > 0, at => input.ExcludedList.Any(a => a == at.Id) == false);
                if (input.IsEmployee.HasValue && input.IsEmployee == true)
                {
                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                    }
                    else
                        return new PagedResultDto<DriverDto>
                        (
                           0, new List<DriverDto>()
                        );
                }

                int count = query.Count();
                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }

                var drivers = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                var _mappedList = ObjectMapper.Map<List<DriverDto>>(drivers);
                return new PagedResultDto<DriverDto>(count, _mappedList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<DriverDto>> GetPaged(GetDriversPagedInput input)
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

                            Driver driver = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (driver != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _driverClinicRepository.CountAsync(a => a.DriverId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Drivers.HasClinics"));

                                    driver.User.IsDeleted = true;
                                    driver.User.DeletionTime = DateTime.Now;
                                    driver.User.DeleterUserId = AbpSession.UserId;
                                    driver.IsDeleted = true;
                                    driver.DeletionTime = DateTime.Now;
                                    driver.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(driver);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    driver.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(driver.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(driver);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(driver.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(driver);
                                }
                                else if (input.action == "Restore")
                                {
                                    driver.UnDelete();
                                    driver.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Driver;

                                    driver.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        driver.User.Roles.Add(new UserRole(AbpSession.TenantId, driver.UserId.Value, role.Id));

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
                            Driver driver = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (driver != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _driverClinicRepository.CountAsync(a => a.DriverId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Drivers.HasClinics"));

                                    driver.User.IsDeleted = true;
                                    driver.User.DeletionTime = DateTime.Now;
                                    driver.User.DeleterUserId = AbpSession.UserId;
                                    driver.IsDeleted = true;
                                    driver.DeletionTime = DateTime.Now;
                                    driver.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(driver);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();


                                }
                                //else if (input.action == "Restore")
                                //    driver.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(driver.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(driver.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                }

                                else if (input.action == "Restore")
                                {
                                    driver.UnDelete();
                                    driver.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Driver;

                                    driver.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        driver.User.Roles.Add(new UserRole(AbpSession.TenantId, driver.UserId.Value, role.Id));

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    if (input.CompanyId.HasValue == false)
                        return new DataTableOutputDto<DriverDto>
                        {
                            iTotalDisplayRecords = 0,
                            iTotalRecords = 0,
                            aaData = new List<DriverDto>()
                        };
                    List<long?> driverIds = new List<long?>();
                    bool filterByDriver = false;

                    if (string.IsNullOrEmpty(input.FullPlateNumber) == false)
                    {
                        filterByDriver = true;
                        //string exampleTrimmed = String.Concat(input.FullPlateNumber.Where(c => !Char.IsWhiteSpace(c)));
                        //string exampleTrimmed = Regex.Replace(input.FullPlateNumber, @"(?<=\D)\s+|\s+(?=\D)", "").ToLower();

                        string letters = Regex.Replace(input.FullPlateNumber, @"[^\p{L}]", "").ToLower();  // Get only letters and convert to lowercase
                        string numbers = Regex.Replace(input.FullPlateNumber, @"\D", ""); // Get only digits

                        string exampleTrimmed = letters + " " + numbers; // Combine them with a single space
                        driverIds = await _driverVeichleRepository.GetAll().Where(a => a.Veichle.FullPlateNumber.Contains(exampleTrimmed) || a.Veichle.FullPlateNumberAr.Contains(exampleTrimmed)).Select(a => a.DriverId).ToListAsync();
                    }

                    var query = Repository.GetAll()
                        .Include(a => a.DriverVeichles.Select(aa => aa.Veichle))
                        .Include(a => a.Branch.Company)
                        .Include(a => a.User).Where(at => at.User.UserType == UserTypes.Driver);

                    query = query.WhereIf(input.CompanyId.HasValue, a => a.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(filterByDriver, at => driverIds.Contains(at.Id));


                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            return new DataTableOutputDto<DriverDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<DriverDto>()
                            };
                    }


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchCode), at => at.Branch.Code.Contains(input.BranchCode));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName));
                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                    int filteredCount = await query.CountAsync();
                    var drivers = await query.Include(x => x.User)
                        .Include(x => x.CreatorUser)
                        .Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<DriverDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<DriverDto>>(drivers)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<DriverDto> GetByUserId(EntityDto<long> input)
        {
            var Driver = await Repository.GetAllIncluding(x => x.User)
                .Include(a => a.Branch.Company.User)
                .Include(a => a.Branch.User)
                .FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(Driver);
        }

        [AbpAuthorize]
        public async Task<DriverDto> UpdateDriverPhotoAsync(UpdateDriverDto input)
        {
            var driver = await Repository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);

            if (!string.IsNullOrEmpty(driver.Avatar))
                Utilities.DeleteImage(6, driver.Avatar, new string[] { "600x600_" });

            driver.Avatar = input.Avatar;

            await Repository.UpdateAsync(driver);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            if (user != null)
            {
                user.Avatar = input.Avatar;
                await _userRepository.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();
            }

            return MapToEntityDto(driver);
        }




        [AbpAuthorize]
        public async Task<string> ExportExcel(RequestDriversExcelDtoInput input)
        {
            try
            {

                try
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {
                        var query = Repository.GetAll()
                          .Include(a => a.DriverVeichles.Select(aa => aa.Veichle))
                          .Include(a => a.Branch.Company)
                          .Include(a => a.User).Where(at => at.User.UserType == UserTypes.Driver);

                        query = query.WhereIf(input.CompanyId.HasValue, a => a.Branch.CompanyId == input.CompanyId);
                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);


                        if (input.IsEmployee.HasValue && input.IsEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                            }
                            else
                                return "";
                        }

                        int count = await query.CountAsync();

                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                        query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);

                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<DriverDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {
                            var excelData = ObjectMapper.Map<List<DriverExcelDto>>(cBList.Select(m => new DriverExcelDto
                            {
                                Code = m.Code,
                                Name = m.Name,
                                UserName = m.User.UserName,
                                Branch = m.Branch != null ? m.Branch.Name : "",
                                VeichlesCount = m.DriverVeichlesCount.ToString(),
                                FullPlateNumbers = m.PlateNumbers.JoinAsString(" , "),
                                Status = m.User.IsActive == true ? L("Common.Active") : L("Common.NotActive")
                            }));

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();
                            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();
                            if (input.CompanyId.HasValue)
                            {

                                var _company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.Drivers.Title")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "تاريخ الإصدار",
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "الشركة",
                                    Value = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn,
                                    ExcelType = L("Pages.Drivers.Title"),
                                    KeyValues = _keyValues
                                };
                            }

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.FuelTransactions;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.Drivers.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

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
