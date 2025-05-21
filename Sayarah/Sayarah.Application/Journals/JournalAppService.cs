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
    public class JournalAppService : AsyncCrudAppService<Journal, JournalDto, long, GetAllJournals, CreateJournalDto, UpdateJournalDto>, IJournalAppService
    {
        private readonly IRepository<Journal, long> _journalRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        public ICommonAppService _commonAppService { get; set; }
        public JournalAppService(IRepository<Journal, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _journalRepository = repository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<JournalDto>> GetPaged(GetJournalsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int journalId = Convert.ToInt32(input.ids[i]);
                            Journal journal = await _journalRepository.GetAsync(journalId);
                            if (journal != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await _journalRepository.DeleteAsync(journal);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int journalId = Convert.ToInt32(input.ids[0]);
                            Journal journal = await _journalRepository.GetAsync(journalId);
                            if (journal != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    
                                    await _journalRepository.DeleteAsync(journal);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _journalRepository.CountAsync();
                    var query = _journalRepository.GetAll().Include(a => a.Provider.MainProvider)
                        .Include(a => a.Branch.Company).AsQueryable();


                    query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, m => m.Provider.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, m => m.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, m => m.Branch.CompanyId == input.CompanyId);
                   
                    count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr) );
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));

                    query = query.WhereIf(input.JournalType.HasValue, m => m.JournalType == input.JournalType);


                    int filteredCount = await query.CountAsync();
                    var journals =
                          await query/*.Include(q => q.CreatorUser)*/
                           .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                              .ToListAsync();
                    return new DataTableOutputDto<JournalDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<JournalDto>>(journals)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        [AbpAuthorize]
        public override async Task<JournalDto> GetAsync(EntityDto<long> input)
        {
            var journal = _journalRepository.GetAll()
               .Include(x => x.Branch.Company)
               .Include(x => x.Provider.MainProvider)
               .Include(x => x.JournalDetails)
               .FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<JournalDto>(journal));
        }

        [AbpAuthorize]
        public override async Task<JournalDto> CreateAsync(CreateJournalDto input)
        {
            try
            {
                var journal = ObjectMapper.Map<Journal>(input);
                await _journalRepository.InsertAsync(journal);
                return MapToEntityDto(journal);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<JournalDto> UpdateAsync(UpdateJournalDto input)
        {
            try
            {
                var journal = await _journalRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, journal);
                await _journalRepository.UpdateAsync(journal);
                return MapToEntityDto(journal);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<PagedResultDto<JournalDto>> GetAllAsync(GetAllJournals input)
        {
            try
            {
                var query = _journalRepository.GetAll().Include(a => a.Provider.MainProvider)
                        .Include(a => a.Branch.Company).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.Provider.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.BranchId.HasValue, m => m.BranchId == input.BranchId);
                query = query.WhereIf(input.CompanyId.HasValue, m => m.Branch.CompanyId == input.CompanyId);
                query = query.WhereIf(input.JournalType.HasValue, m => m.JournalType == input.JournalType);


                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var journals = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<JournalDto>(
                   query.Count(), ObjectMapper.Map<List<JournalDto>>(journals)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public async Task<PagedResultDto<ApiJournalDto>> GetAllJournals(GetAllJournals input)
        {
            try
            {
                var query = _journalRepository.GetAll().Include(a => a.Provider.MainProvider)
                        .Include(a => a.Branch.Company).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.Provider.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.BranchId.HasValue, m => m.BranchId == input.BranchId);
                query = query.WhereIf(input.CompanyId.HasValue, m => m.Branch.CompanyId == input.CompanyId);
                query = query.WhereIf(input.JournalType.HasValue, m => m.JournalType == input.JournalType);


                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var journals = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<ApiJournalDto>(
                   query.Count(), ObjectMapper.Map<List<ApiJournalDto>>(journals)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
