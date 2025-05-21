using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System.Globalization;
using System.Linq.Dynamic.Core;


namespace Sayarah.Application.Veichles
{
    public class VeichleTripAppService : AsyncCrudAppService<VeichleTrip, VeichleTripDto, long, GetVeichleTripsInput, CreateVeichleTripDto, UpdateVeichleTripDto>, IVeichleTripAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Company, long> _companyRepository;
       
        public VeichleTripAppService(
            IRepository<VeichleTrip, long> repository,
             ICommonAppService commonService,
             IRepository<Company, long> companyRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _companyRepository = companyRepository;
        }

        public override async Task<VeichleTripDto> GetAsync(EntityDto<long> input)
        {
            var VeichleTrip = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(VeichleTrip);
        }

        [AbpAuthorize]
        public override async Task<VeichleTripDto> CreateAsync(CreateVeichleTripDto input)
        {
            try
            {
                //Check if veichleTrip exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTrips", CodeField = "Code" });

                var veichleTrip = ObjectMapper.Map<VeichleTrip>(input);
                veichleTrip = await Repository.InsertAsync(veichleTrip);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(veichleTrip);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        [AbpAuthorize]
        public override async Task<VeichleTripDto> UpdateAsync(UpdateVeichleTripDto input)
        {

            var veichleTrip = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, veichleTrip);
            await Repository.UpdateAsync(veichleTrip);
            return MapToEntityDto(veichleTrip);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var veichleTrip = await Repository.GetAsync(input.Id);
            if (veichleTrip == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(veichleTrip);
        }

        [AbpAuthorize]
        public override async Task<PagedResultDto<VeichleTripDto>> GetAllAsync(GetVeichleTripsInput input)
        {
            var query = Repository.GetAll()
                .Include(at=>at.Veichle)
                .Include(at => at.Branch.Company).AsQueryable();

            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            //query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
            //query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var veichleTrips = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<VeichleTripDto>>(veichleTrips);
            return new PagedResultDto<VeichleTripDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<VeichleTripDto>> GetPaged(GetVeichleTripsPagedInput input)
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
                            VeichleTrip veichleTrip = await Repository.FirstOrDefaultAsync(id);
                            if (veichleTrip != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _veichleTripClinicRepository.CountAsync(a => a.VeichleTripId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.VeichleTrips.HasClinics"));

                                    await Repository.DeleteAsync(veichleTrip);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

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
                            VeichleTrip veichleTrip = await Repository.FirstOrDefaultAsync(id);
                            if (veichleTrip != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _veichleTripClinicRepository.CountAsync(a => a.VeichleTripId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.VeichleTrips.HasClinics"));

                                    await Repository.DeleteAsync(veichleTrip);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                           
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(a => a.Branch.Company)
                        .Include(at => at.Veichle)
                        .Include(at => at.CreatorUser).AsQueryable();

                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    //query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);


                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            return new DataTableOutputDto<VeichleTripDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<VeichleTripDto>()
                            };
                    }


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at => at.Veichle.NameAr.Contains(input.VeichleName) || at.Veichle.NameEn.Contains(input.VeichleName) || at.Veichle.Code.Contains(input.VeichleName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.TripNumber), at => at.TripNumber.Contains(input.TripNumber));


                    if (input.StartDateFrom.HasValue || input.StartDateTo.HasValue)
                    {
                        DateTime startDateFrom = new DateTime();
                        DateTime startDateTo = new DateTime();
                        //if (input.StartDateFrom.HasValue)
                        //{
                        //    startDateFrom = (input.StartDateFrom.Value.Date).AddDays(1);
                        //}
                        if (input.StartDateTo.HasValue)
                        {
                            startDateTo = input.StartDateTo.Value.Date.AddSeconds(86399).AddDays(1);
                        }
                        query = query.WhereIf(input.StartDateFrom.HasValue, at => at.StartDate >= startDateFrom);
                        query = query.WhereIf(input.StartDateTo.HasValue, at => at.StartDate <= startDateTo);
                    }


                    if (input.EndDateFrom.HasValue || input.EndDateTo.HasValue)
                    {
                        DateTime endDateFrom = new DateTime();
                        DateTime endDateTo = new DateTime();
                        //if (input.EndDateFrom.HasValue)
                        //{
                        //    endDateFrom = (input.EndDateFrom.Value.Date).AddDays(1);
                        //}
                        if (input.EndDateTo.HasValue)
                        {
                            endDateTo = input.EndDateTo.Value.Date.AddSeconds(86399).AddDays(1);
                        }
                        query = query.WhereIf(input.EndDateFrom.HasValue, at => at.EndDate >= endDateFrom);
                        query = query.WhereIf(input.EndDateTo.HasValue, at => at.EndDate <= endDateTo);
                    }


                    if (input.lastModificationTimeFrom.HasValue || input.lastModificationTimeTo.HasValue)
                    {
                        DateTime lastModificationTimeFrom = new DateTime();
                        DateTime lastModificationTimeTo = new DateTime();
                        if (input.lastModificationTimeFrom.HasValue)
                        {
                            lastModificationTimeFrom = input.lastModificationTimeFrom.Value.Date.AddDays(1);
                        }
                        if (input.lastModificationTimeTo.HasValue)
                        {
                            lastModificationTimeTo = input.lastModificationTimeTo.Value.Date.AddSeconds(86399).AddDays(1);
                        }
                        query = query.WhereIf(input.lastModificationTimeFrom.HasValue, at => at.LastModificationTime >= lastModificationTimeFrom);
                        query = query.WhereIf(input.lastModificationTimeTo.HasValue, at => at.LastModificationTime <= lastModificationTimeTo);
                    }



                    int filteredCount = await query.CountAsync();
                    var veichleTrips = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();


                    return new DataTableOutputDto<VeichleTripDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<VeichleTripDto>>(veichleTrips)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<string> ExportExcel(RequestTripsExcelDtoInput input)
        {
            try
            {

                try
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {
                        var query = Repository.GetAll()
                          .Include(a => a.Branch.Company)
                          .Include(at => at.Veichle)
                          .Include(at => at.CreatorUser).AsQueryable();

                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                        query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                        //query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);


                        if (input.IsEmployee.HasValue && input.IsEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                            }
                            else return string.Empty;
                        }


                        int count = await query.CountAsync();
                      
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at => at.Veichle.NameAr.Contains(input.VeichleName) || at.Veichle.NameEn.Contains(input.VeichleName) || at.Veichle.Code.Contains(input.VeichleName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.TripNumber), at => at.TripNumber.Contains(input.TripNumber));


                        if (input.StartDateFrom.HasValue || input.StartDateTo.HasValue)
                        {
                            DateTime startDateFrom = new DateTime();
                            DateTime startDateTo = new DateTime();
                            //if (input.StartDateFrom.HasValue)
                            //{
                            //    startDateFrom = (input.StartDateFrom.Value.Date).AddDays(1);
                            //}
                            if (input.StartDateTo.HasValue)
                            {
                                startDateTo = input.StartDateTo.Value.Date.AddSeconds(86399).AddDays(1);
                            }
                            query = query.WhereIf(input.StartDateFrom.HasValue, at => at.StartDate >= startDateFrom);
                            query = query.WhereIf(input.StartDateTo.HasValue, at => at.StartDate <= startDateTo);
                        }


                        if (input.EndDateFrom.HasValue || input.EndDateTo.HasValue)
                        {
                            DateTime endDateFrom = new DateTime();
                            DateTime endDateTo = new DateTime();
                            //if (input.EndDateFrom.HasValue)
                            //{
                            //    endDateFrom = (input.EndDateFrom.Value.Date).AddDays(1);
                            //}
                            if (input.EndDateTo.HasValue)
                            {
                                endDateTo = input.EndDateTo.Value.Date.AddSeconds(86399).AddDays(1);
                            }
                            query = query.WhereIf(input.EndDateFrom.HasValue, at => at.EndDate >= endDateFrom);
                            query = query.WhereIf(input.EndDateTo.HasValue, at => at.EndDate <= endDateTo);
                        }

                      
                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 //.Skip(input.SkipCount)
                 //.Take(input.MaxResultCount)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<VeichleTripDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {
                            var excelData = ObjectMapper.Map<List<RequestTripExcelCompanyDto>>(cBList.Select(m => new RequestTripExcelCompanyDto
                            {
                                Code = m.Code,
                                Branch = m.Branch != null ? m.Branch.Name : "",
                                Veichle = m.Veichle != null ? m.Veichle.FullPlateNumber + " " + m.Veichle.FullPlateNumberAr : "",
                                FuelType = m.Veichle != null ? m.Veichle.FuelType == FuelType._91 ? "91" : m.Veichle.FuelType == FuelType._95 ? "95" : "ديزل" : "",
                                MaxLitersCount = m.MaxLitersCount.ToString(),
                                CurrentConsumption = m.CurrentConsumption.ToString(),
                                StartDate = m.StartDate.ToString(),
                                EndDate = m.EndDate.ToString(),
                                CreationTime = m.CreationTime.ToString(),
                            }));
                             
                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();
                            if (input.CompanyId.HasValue)
                            {

                                var _company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);
                                
                                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.Trips.TripsHistory")
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
                                    ExcelType = L("Pages.Trips.TripsHistory"),
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
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.VeichleTripsRecords.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

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
