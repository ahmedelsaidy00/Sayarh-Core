using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Authorization.Users;

using Sayarah.Companies;
using Sayarah.Lookups;
using System.Linq.Dynamic.Core;
namespace Sayarah.Application.Lookups
{
    [DisableAuditing]
    public class CityAppService : AsyncCrudAppService<City, CityDto, long, GetAllCities, CreateCityDto, UpdateCityDto>, ICityAppService
    {
        private readonly IRepository<City, long> _cityRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        public ICommonAppService _commonAppService { get; set; }

        public CityAppService(
            IRepository<City, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Branch, long> branchRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _cityRepository = repository;
            _userRepository = userRepository;
            _branchRepository = branchRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<CityDto>> GetPaged(GetCitiesInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int cityId = Convert.ToInt32(input.ids[i]);
                        var city = await _cityRepository.FirstOrDefaultAsync(cityId);
                        if (city != null && input.action == "Delete")
                        {
                            var centers = await _branchRepository.GetAll().Where(at => at.CityId == cityId).ToListAsync();
                            if (centers != null && centers.Count > 0)
                            {
                                foreach (var item in centers)
                                {
                                    item.CityId = null;
                                    await _branchRepository.UpdateAsync(item);
                                }
                                await CurrentUnitOfWork.SaveChangesAsync();
                            }
                            await _cityRepository.DeleteAsync(city);
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction" && input.ids.Length > 0)
                {
                    int cityId = Convert.ToInt32(input.ids[0]);
                    var city = await _cityRepository.FirstOrDefaultAsync(cityId);
                    if (city != null && input.action == "Delete")
                    {
                        var centers = await _branchRepository.GetAll().Where(at => at.CityId == cityId).ToListAsync();
                        if (centers != null)
                        {
                            foreach (var item in centers)
                            {
                                item.CityId = null;
                                await _branchRepository.UpdateAsync(item);
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await _cityRepository.DeleteAsync(city);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                var query = _cityRepository.GetAll();
                int count = await query.CountAsync();
                query = query.FilterDataTable(input);
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr));
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));
                int filteredCount = await query.CountAsync();
                var cities = await query
                    .OrderBy($"{input.columns[input.order[0].column].name} {input.order[0].dir}")
                    .Skip(input.start)
                    .Take(input.length)
                    .ToListAsync();

                return new DataTableOutputDto<CityDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<CityDto>>(cities)
                };
            }
        }

        public override async Task<CityDto> GetAsync(EntityDto<long> input)
        {
            var city = await _cityRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
            return ObjectMapper.Map<CityDto>(city);
        }

        [AbpAuthorize]
        public override async Task<CityDto> CreateAsync(CreateCityDto input)
        {
            int existingCount = await _cityRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn);
            if (existingCount > 0)
                throw new UserFriendlyException(L("Pages.Cities.Error.AlreadyExist"));

            input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Cities", CodeField = "Code" });
            var city = ObjectMapper.Map<City>(input);
            await _cityRepository.InsertAsync(city);
            return MapToEntityDto(city);
        }

        [AbpAuthorize]
        public override async Task<CityDto> UpdateAsync(UpdateCityDto input)
        {
            int existingCount = await _cityRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id);
            if (existingCount > 0)
                throw new UserFriendlyException(L("Pages.Cities.Error.AlreadyExist"));
            var city = await _cityRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, city);
            await _cityRepository.UpdateAsync(city);
            return MapToEntityDto(city);
        }

        public override async Task<PagedResultDto<CityDto>> GetAllAsync(GetAllCities input)
        {
            var query = _cityRepository.GetAll();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));

            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = await query.CountAsync();
            }
            var cities = await query
                .OrderBy(x => x.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();
            return new PagedResultDto<CityDto>(
                await query.CountAsync(),
                ObjectMapper.Map<List<CityDto>>(cities)
            );
        }
    }
}
