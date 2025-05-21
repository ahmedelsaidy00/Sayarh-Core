using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Drivers;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Veichles
{
    public class VeichleTransferRecordAppService : AsyncCrudAppService<VeichleTransferRecord, VeichleTransferRecordDto, long, GetVeichleTransferRecordsInput, CreateVeichleTransferRecordDto, UpdateVeichleTransferRecordDto>, IVeichleTransferRecordAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<Driver, long> _driverRepository;
        private readonly IRepository<DriverVeichle, long> _driverVeichleRepository;
        private readonly IRepository<VeichleTransferRecordDriver, long> _veichleTransferRecordDriverRepository;
        private readonly ICommonAppService _commonService;

        public VeichleTransferRecordAppService(
            IRepository<VeichleTransferRecord, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
             IRepository<Driver, long> driverRepository,
             IRepository<DriverVeichle, long> driverVeichleRepository,
             IRepository<VeichleTransferRecordDriver, long> veichleTransferRecordDriverRepository,
             ICommonAppService commonService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _driverRepository = driverRepository;
            _driverVeichleRepository = driverVeichleRepository;
            _veichleTransferRecordDriverRepository = veichleTransferRecordDriverRepository;
            _commonService = commonService;
        }

        public override async Task<VeichleTransferRecordDto> GetAsync(EntityDto<long> input)
        {
            var VeichleTransferRecord = await Repository.GetAll()
                .Include(x => x.TargetBranch)
                .Include(x => x.SourceBranch)
                .Include(x => x.VeichleTransferRecordDrivers.Select(a=>a.Driver))
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(VeichleTransferRecord);
        }


        [AbpAuthorize]
        public override async Task<VeichleTransferRecordDto> CreateAsync(CreateVeichleTransferRecordDto input)
        {
            try
            {
                //Check if veichleTransferRecord exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTransferRecords", CodeField = "Code" });
                var veichleTransferRecord = ObjectMapper.Map<VeichleTransferRecord>(input);
                veichleTransferRecord = await Repository.InsertAsync(veichleTransferRecord);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(veichleTransferRecord);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<VeichleTransferRecordDto> UpdateAsync(UpdateVeichleTransferRecordDto input)
        {

            var veichleTransferRecord = await Repository.GetAll()
                .Include(x => x.TargetBranch)
                .Include(x => x.SourceBranch)
                .Include(x => x.Veichle)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, veichleTransferRecord);
            await Repository.UpdateAsync(veichleTransferRecord);
            return MapToEntityDto(veichleTransferRecord);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var veichleTransferRecord = await Repository.GetAsync(input.Id);
            if (veichleTransferRecord == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(veichleTransferRecord);
        }
        [AbpAuthorize]
        public override async Task<PagedResultDto<VeichleTransferRecordDto>> GetAllAsync(GetVeichleTransferRecordsInput input)
        {
            var query = Repository.GetAll()
                .Include(x => x.TargetBranch)
                .Include(x => x.SourceBranch)
                .Include(x => x.Veichle).AsQueryable();

            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Veichle.NameAr.Contains(input.Name) || at.Veichle.NameEn.Contains(input.Name));
            query = query.WhereIf(input.SourceBranchId.HasValue, at => at.SourceBranchId == input.SourceBranchId);
            query = query.WhereIf(input.TargetBranchId.HasValue, at => at.TargetBranchId == input.TargetBranchId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Veichle.Branch.CompanyId == input.CompanyId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var veichleTransferRecords = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<VeichleTransferRecordDto>>(veichleTransferRecords);
            return new PagedResultDto<VeichleTransferRecordDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<VeichleTransferRecordDto>> GetPaged(GetVeichleTransferRecordsPagedInput input)
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
                            VeichleTransferRecord veichleTransferRecord = await Repository.FirstOrDefaultAsync(id);
                            if (veichleTransferRecord != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _veichleTransferRecordClinicRepository.CountAsync(a => a.VeichleTransferRecordId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.VeichleTransferRecords.HasClinics"));

                                    await Repository.DeleteAsync(veichleTransferRecord);
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
                            VeichleTransferRecord veichleTransferRecord = await Repository.FirstOrDefaultAsync(id);
                            if (veichleTransferRecord != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _veichleTransferRecordClinicRepository.CountAsync(a => a.VeichleTransferRecordId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.VeichleTransferRecords.HasClinics"));

                                    await Repository.DeleteAsync(veichleTransferRecord);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                         .Include(x => x.TargetBranch)
                         .Include(x => x.SourceBranch)
                         .Include(x => x.VeichleTransferRecordDrivers)
                         .Include(x => x.Veichle).AsQueryable();

                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Veichle.Branch.CompanyId == input.CompanyId);

                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.SourceBranchId) || input.BranchesIds.Any(_id => _id == a.TargetBranchId));
                        }
                        else
                            return new DataTableOutputDto<VeichleTransferRecordDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<VeichleTransferRecordDto>()
                            };
                    }



                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(input.SourceBranchId.HasValue, at => at.SourceBranchId == input.SourceBranchId);
                    query = query.WhereIf(input.TargetBranchId.HasValue, at => at.TargetBranchId == input.TargetBranchId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at => at.Veichle.NameAr.Contains(input.VeichleName) || at.Veichle.NameEn.Contains(input.VeichleName) || at.Veichle.Code.Contains(input.VeichleName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.SourceBranchName), at => at.SourceBranch.NameAr.Contains(input.SourceBranchName) || at.SourceBranch.NameEn.Contains(input.SourceBranchName) || at.SourceBranch.Code.Contains(input.SourceBranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.TargetBranchName), at => at.TargetBranch.NameAr.Contains(input.TargetBranchName) || at.TargetBranch.NameEn.Contains(input.TargetBranchName) || at.TargetBranch.Code.Contains(input.TargetBranchName));

                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                    int filteredCount = await query.CountAsync();
                    var veichleTransferRecords = await query.Include(x => x.TargetBranch).Include(x => x.SourceBranch).Include(x => x.Veichle)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<VeichleTransferRecordDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<VeichleTransferRecordDto>>(veichleTransferRecords)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [AbpAuthorize]
        public async Task<bool> ManageTransfers(ManageTransfersInput input)
        {
            try
            {

                if (input.Veichles != null && input.Veichles.Count > 0 && input.TargetBranchId.HasValue)
                {

                    foreach (var item in input.Veichles.ToList())
                    {

                        // get veichle 
                        var veichle = await _veichleRepository.FirstOrDefaultAsync(item.Id);

                        // check if source and target branches are equals
                        if (veichle.BranchId != input.TargetBranchId)
                        {


                            #region transfer veichle first

                            VeichleTransferRecord _record = new VeichleTransferRecord();

                            _record.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTransferRecords", CodeField = "Code" });
                            _record.Status = TransferStatus.Accepted;
                            _record.VeichleId = item.Id;
                            _record.SourceBranchId = veichle.BranchId;
                            _record.TargetBranchId = input.TargetBranchId;
                            _record.TransferDrivers = item.TransferDrivers;

                            await Repository.InsertAsync(_record);
                            await CurrentUnitOfWork.SaveChangesAsync();

                            // update veichle  

                            veichle.BranchId = input.TargetBranchId;
                            await _veichleRepository.UpdateAsync(veichle);
                            await CurrentUnitOfWork.SaveChangesAsync();

                            #endregion


                            if (item.TransferDrivers == true)
                            {


                                // loop over all 
                                foreach (var driverVeichle in item.DriverVeichles)
                                {

                                    var driver = await _driverRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.DriverId);

                                    if (driverVeichle.IsSelected)
                                    {
                                        // get driver and change branch 
                                        driver.BranchId = input.TargetBranchId;
                                        await _driverRepository.UpdateAsync(driver);

                                        //create a row in veichleTransferRecordDrivers
                                        await _veichleTransferRecordDriverRepository.InsertAsync(new VeichleTransferRecordDriver
                                        {
                                            DriverId = driverVeichle.DriverId,
                                            VeichleTransferRecordId = _record.Id,
                                        });

                                        // if this driver has another veichle ???
                                        // هنا ممكن السائق مع مركبه تانيه أكون بنقلها هي كمان -_-
                                        //var driverExistsWithAnotherVeichles = await _driverVeichleRepository.GetAll()
                                        //                                                                    .Where(a => a.DriverId == driverVeichle.DriverId && a.VeichleId != veichle.Id)
                                        //                                                                    .ToListAsync();


                                        //if(driverExistsWithAnotherVeichles != null && driverExistsWithAnotherVeichles.Count > 0)
                                        //{

                                        //}
                                    }
                                    else
                                    {
                                        // remove relationship

                                        if (veichle.DriverId == driverVeichle.DriverId)
                                        {
                                            veichle.DriverId = null;
                                            await _veichleRepository.UpdateAsync(veichle);
                                        }

                                        // delete all 
                                        await _driverVeichleRepository.DeleteAsync(a => a.DriverId == driverVeichle.DriverId && a.VeichleId == veichle.Id);
                                    }

                                }
                            }
                            else
                            {
                                veichle.DriverId = null;
                                await _veichleRepository.UpdateAsync(veichle);

                                // remove relationship for this car with any drivers 
                                await _driverVeichleRepository.DeleteAsync(a => a.VeichleId == veichle.Id);
                            }
                        }
                    }
              
                
                    await UnitOfWorkManager.Current.SaveChangesAsync(); 

                }

                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }


        //[AbpAuthorize]
        //public async Task<bool> ManageTransfers(ManageTransfersInput input)
        //{
        //    try
        //    {

        //        if (input.VeichlesId != null && input.VeichlesId.Count > 0 && input.TargetBranchId.HasValue)
        //        {

        //            foreach (var item in input.VeichlesId.ToList())
        //            {

        //                // get veichle 
        //                var veichle = await _veichleRepository.FirstOrDefaultAsync(item);

        //                // check if source and target branches are equals
        //                if (veichle.BranchId != input.TargetBranchId)
        //                {

        //                    // create a row in records 

        //                    VeichleTransferRecord _record = new VeichleTransferRecord();

        //                    _record.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTransferRecords", CodeField = "Code" });
        //                    _record.Status = Helpers.Enums.TransferStatus.Accepted;
        //                    _record.VeichleId = item;
        //                    _record.SourceBranchId = veichle.BranchId;
        //                    _record.TargetBranchId = input.TargetBranchId;
        //                    await Repository.InsertAsync(_record);
        //                    await CurrentUnitOfWork.SaveChangesAsync();

        //                    // update veichle  

        //                    veichle.BranchId = input.TargetBranchId;
        //                    await _veichleRepository.UpdateAsync(veichle);
        //                    await CurrentUnitOfWork.SaveChangesAsync();
        //                }
        //            }
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserFriendlyException(ex.Message);
        //    }
        //}



    }
}