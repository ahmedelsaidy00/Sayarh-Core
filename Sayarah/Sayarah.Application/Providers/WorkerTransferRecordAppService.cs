using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users;
using Sayarah.Authorization.Users;
using Sayarah.Providers;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.Providers
{
    public class WorkerTransferRecordAppService : AsyncCrudAppService<WorkerTransferRecord, WorkerTransferRecordDto, long, GetWorkerTransferRecordsInput, CreateWorkerTransferRecordDto, UpdateWorkerTransferRecordDto>, IWorkerTransferRecordAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Worker, long> _workerRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly ICommonAppService _commonService;
        private readonly IRepository<UserDevice, long> _userDeviceRepository;


        public WorkerTransferRecordAppService(
            IRepository<WorkerTransferRecord, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Worker, long> workerRepository,
             IRepository<Provider, long> providerRepository,
              IRepository<UserDevice, long> userDeviceRepository,
             ICommonAppService commonService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _workerRepository = workerRepository;
            _providerRepository = providerRepository;
            _userDeviceRepository = userDeviceRepository;
            _commonService = commonService;
        }

        public override async Task<WorkerTransferRecordDto> GetAsync(EntityDto<long> input)
        {
            var WorkerTransferRecord = await Repository.GetAll()
                .Include(x => x.TargetProvider)
                .Include(x => x.SourceProvider)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(WorkerTransferRecord);
        }


        [AbpAuthorize]
        public override async Task<WorkerTransferRecordDto> CreateAsync(CreateWorkerTransferRecordDto input)
        {
            try
            {
                //Check if workerTransferRecord exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "WorkerTransferRecords", CodeField = "Code" });
                var workerTransferRecord = ObjectMapper.Map<WorkerTransferRecord>(input);
                workerTransferRecord = await Repository.InsertAsync(workerTransferRecord);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(workerTransferRecord);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<WorkerTransferRecordDto> UpdateAsync(UpdateWorkerTransferRecordDto input)
        {

            var workerTransferRecord = await Repository.GetAll()
                .Include(x => x.TargetProvider)
                .Include(x => x.SourceProvider)
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, workerTransferRecord);
            await Repository.UpdateAsync(workerTransferRecord);
            return MapToEntityDto(workerTransferRecord);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var workerTransferRecord = await Repository.GetAsync(input.Id);
            if (workerTransferRecord == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            //await Repository.DeleteAsync(workerTransferRecord);
        }


        [AbpAuthorize]
        public override async Task<PagedResultDto<WorkerTransferRecordDto>> GetAllAsync(GetWorkerTransferRecordsInput input)
        {
            var query = Repository.GetAll()
                .Include(x => x.TargetProvider)
                .Include(x => x.SourceProvider)
                .Include(x => x.Worker).AsQueryable();

            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Worker.Name.Contains(input.Name));
            query = query.WhereIf(input.SourceProviderId.HasValue, at => at.SourceProviderId == input.SourceProviderId);
            query = query.WhereIf(input.TargetProviderId.HasValue, at => at.TargetProviderId == input.TargetProviderId);
            query = query.WhereIf(input.MainProviderId.HasValue, at => at.Worker.Provider.MainProviderId == input.MainProviderId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var workerTransferRecords = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<WorkerTransferRecordDto>>(workerTransferRecords);
            return new PagedResultDto<WorkerTransferRecordDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<WorkerTransferRecordDto>> GetPaged(GetWorkerTransferRecordsPagedInput input)
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
                            WorkerTransferRecord workerTransferRecord = await Repository.FirstOrDefaultAsync(id);
                            if (workerTransferRecord != null)
                            {
                                //if (input.action == "Delete")
                                //{

                                //    //// check before delete 
                                //    //int clinicsCount = await _workerTransferRecordClinicRepository.CountAsync(a => a.WorkerTransferRecordId == id);
                                //    //if (clinicsCount > 0)
                                //    //    throw new UserFriendlyException(L("Pages.WorkerTransferRecords.HasClinics"));

                                //    await Repository.DeleteAsync(workerTransferRecord);
                                //    await UnitOfWorkManager.Current.SaveChangesAsync();

                                //}

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            WorkerTransferRecord workerTransferRecord = await Repository.FirstOrDefaultAsync(id);
                            if (workerTransferRecord != null)
                            {
                                //if (input.action == "Delete")
                                //{

                                //    // check before delete 
                                //    //int clinicsCount = await _workerTransferRecordClinicRepository.CountAsync(a => a.WorkerTransferRecordId == id);
                                //    //if (clinicsCount > 0)
                                //    //    throw new UserFriendlyException(L("Pages.WorkerTransferRecords.HasClinics"));

                                //    await Repository.DeleteAsync(workerTransferRecord);
                                //    await UnitOfWorkManager.Current.SaveChangesAsync();

                                //}

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                         .Include(x => x.TargetProvider)
                         .Include(x => x.SourceProvider)
                         .Include(x => x.Worker).AsQueryable();

                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Worker.Provider.MainProviderId == input.MainProviderId);

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(input.SourceProviderId.HasValue, at => at.SourceProviderId == input.SourceProviderId);
                    query = query.WhereIf(input.TargetProviderId.HasValue, at => at.TargetProviderId == input.TargetProviderId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.SourceProviderName), at => at.SourceProvider.NameAr.Contains(input.SourceProviderName) || at.SourceProvider.NameEn.Contains(input.SourceProviderName) || at.SourceProvider.Code.Contains(input.SourceProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.TargetProviderName), at => at.TargetProvider.NameAr.Contains(input.TargetProviderName) || at.TargetProvider.NameEn.Contains(input.TargetProviderName) || at.TargetProvider.Code.Contains(input.TargetProviderName));

                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                    int filteredCount = await query.CountAsync();
                    var workerTransferRecords = await query.Include(x => x.TargetProvider).Include(x => x.SourceProvider).Include(x => x.Worker)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<WorkerTransferRecordDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<WorkerTransferRecordDto>>(workerTransferRecords)
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

                if (input.WorkersId != null && input.WorkersId.Count > 0 && input.TargetProviderId.HasValue)
                {

                    foreach (var item in input.WorkersId.ToList())
                    {

                        // get worker 
                        var worker = await _workerRepository.FirstOrDefaultAsync(item);

                        // check if source and target provideres are equals
                        if (worker.ProviderId != input.TargetProviderId)
                        {
                            // create a row in records 
                            WorkerTransferRecord _record = new WorkerTransferRecord();

                            _record.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "WorkerTransferRecords", CodeField = "Code" });
                            _record.Status = TransferStatus.Accepted;
                            _record.WorkerId = item;
                            _record.SourceProviderId = worker.ProviderId;
                            _record.TargetProviderId = input.TargetProviderId;
                            await Repository.InsertAsync(_record);

                            // update worker  

                            worker.ProviderId = input.TargetProviderId;
                            await _workerRepository.UpdateAsync(worker);

                            // get user 
                            var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == worker.UserId);
                            user.ProviderId = input.TargetProviderId;
                            await _userRepository.UpdateAsync(user);

                            await CurrentUnitOfWork.SaveChangesAsync();

                            var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == _record.SourceProviderId);
                            var workersCount = await _workerRepository.CountAsync(a => a.ProviderId == _record.SourceProviderId && a.IsDeleted == false);
                            provider.WorkersCount = workersCount;
                            await _providerRepository.UpdateAsync(provider);


                            var targetProvider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == _record.TargetProviderId);
                            var targetWorkersCount = await _workerRepository.CountAsync(a => a.ProviderId == _record.TargetProviderId && a.IsDeleted == false);
                            targetProvider.WorkersCount = targetWorkersCount;
                            await _providerRepository.UpdateAsync(targetProvider);

                            await CurrentUnitOfWork.SaveChangesAsync();


                            var userDevicesByUser = await _userDeviceRepository
                                .GetAllListAsync(x => x.UserId == worker.UserId);

                            if (userDevicesByUser != null && userDevicesByUser.Count > 0)
                            {
                                foreach (var device in userDevicesByUser)
                                {
                                    FCMPushNotification fcmPushClient = new FCMPushNotification();
                                    FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                                    {
                                        RegistrationToken = device.RegistrationToken,
                                        Title = L("Common.SystemTitle"),
                                        Type = FcmNotificationType.Logout,
                                        Body = L("MobileApi.Messages.TransferedToAnotherProvider")
                                    });
                                    await _userDeviceRepository.DeleteAsync(device);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }


    }
}