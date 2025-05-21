using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Journals.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Journals;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Journals
{
    public class JournalDetailAppService : AsyncCrudAppService<JournalDetail, JournalDetailDto, long, GetAllJournalDetails, CreateJournalDetailDto, UpdateJournalDetailDto>, IJournalDetailAppService
    {
        private readonly IRepository<JournalDetail, long> _journalDetailRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        public ICommonAppService _commonAppService { get; set; }
        public JournalDetailAppService(IRepository<JournalDetail, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _journalDetailRepository = repository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<JournalDetailDto>> GetPaged(GetJournalDetailsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int journalDetailId = Convert.ToInt32(input.ids[i]);
                            JournalDetail journalDetail = await _journalDetailRepository.GetAsync(journalDetailId);
                            if (journalDetail != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await _journalDetailRepository.DeleteAsync(journalDetail);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int journalDetailId = Convert.ToInt32(input.ids[0]);
                            JournalDetail journalDetail = await _journalDetailRepository.GetAsync(journalDetailId);
                            if (journalDetail != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await _journalDetailRepository.DeleteAsync(journalDetail);
                                }
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _journalDetailRepository.CountAsync();
                    var query = _journalDetailRepository.GetAll()
                        .Include(a => a.Journal.Provider.MainProvider)
                        .Include(a => a.Journal.Branch.Company).AsQueryable();

                    query = query.WhereIf(input.ProviderId.HasValue, m => m.Journal.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, m => m.Journal.Provider.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, m => m.Journal.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, m => m.Journal.Branch.CompanyId == input.CompanyId);

                    count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    int filteredCount = await query.CountAsync();

                    // Fix: Use System.Linq.Dynamic.Core for dynamic ordering
                    var orderBy = $"{input.columns[input.order[0].column].name} {input.order[0].dir}";
                    var journalDetails = await query
                        .OrderBy(orderBy)
                        .Skip(input.start)
                        .Take(input.length)
                        .ToListAsync();

                    return new DataTableOutputDto<JournalDetailDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<JournalDetailDto>>(journalDetails)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<JournalDetailDto> GetAsync(EntityDto<long> input)
        {
            var journalDetail = _journalDetailRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<JournalDetailDto>(journalDetail));
        }

        [AbpAuthorize]
        public override async Task<JournalDetailDto> CreateAsync(CreateJournalDetailDto input)
        {
            try
            {
                var journalDetail = ObjectMapper.Map<JournalDetail>(input);
                await _journalDetailRepository.InsertAsync(journalDetail);
                return MapToEntityDto(journalDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<JournalDetailDto> UpdateAsync(UpdateJournalDetailDto input)
        {
            try
            {
                ////Check if JournalDetail exists
                //int existingCount = await _journalDetailRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.JournalDetails.Error.AlreadyExist"));
                var journalDetail = await _journalDetailRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, journalDetail);
                await _journalDetailRepository.UpdateAsync(journalDetail);
                return MapToEntityDto(journalDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<JournalDetailDto>> GetAllAsync(GetAllJournalDetails input)
        {
            try
            {
                var query = _journalDetailRepository.GetAll() 
                        .Include(a => a.Journal.Provider.MainProvider)
                        .Include(a => a.Journal.Branch.Company).AsQueryable();

                query = query.WhereIf(input.ProviderId.HasValue, m => m.Journal.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.Journal.Provider.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.BranchId.HasValue, m => m.Journal.BranchId == input.BranchId);
                query = query.WhereIf(input.CompanyId.HasValue, m => m.Journal.Branch.CompanyId == input.CompanyId);
               


                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var journalDetails = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<JournalDetailDto>(
                   query.Count(), ObjectMapper.Map<List<JournalDetailDto>>(journalDetails)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
