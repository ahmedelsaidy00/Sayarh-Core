using Abp.Application.Services;
using Abp.Application.Services.Dto;
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
using Sayarah.Lookups;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Lookups;

public class BrandAppService : AsyncCrudAppService<Brand, BrandDto, long, GetAllBrands, CreateBrandDto, UpdateBrandDto>, IBrandAppService
{
    private readonly IRepository<Brand, long> _brandRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<Veichle, long> _veichleRepository;
    private readonly ICommonAppService _commonAppService;
    public BrandAppService(
        IRepository<Brand, long> repository,
        IRepository<User, long> userRepository,
        IRepository<Veichle, long> veichleRepository,
        ICommonAppService commonAppService)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _brandRepository = repository;
        _userRepository = userRepository;
        _veichleRepository = veichleRepository;
        _commonAppService = commonAppService;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<BrandDto>> GetPaged(GetBrandsInput input)
    {
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
        {
            if (input.actionType == "GroupAction")
            {
                for (int i = 0; i < input.ids.Length; i++)
                {
                    int brandId = Convert.ToInt32(input.ids[i]);
                    var brand = await _brandRepository.FirstOrDefaultAsync(brandId);
                    if (brand != null && input.action == "Delete")
                    {
                        var veichles = await _veichleRepository.GetAll().Where(at => at.BrandId == brandId).ToListAsync();
                        if (veichles != null && veichles.Count > 0)
                        {
                            foreach (var item in veichles)
                            {
                                item.BrandId = null;
                                await _veichleRepository.UpdateAsync(item);
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        await _brandRepository.DeleteAsync(brand);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            else if (input.actionType == "SingleAction")
            {
                if (input.ids.Length > 0)
                {
                    int brandId = Convert.ToInt32(input.ids[0]);
                    var brand = await _brandRepository.FirstOrDefaultAsync(brandId);
                    if (brand != null && input.action == "Delete")
                    {
                        var veichles = await _veichleRepository.GetAll().Where(at => at.BrandId == brandId).ToListAsync();
                        if (veichles != null && veichles.Count > 0)
                        {
                            foreach (var item in veichles)
                            {
                                item.BrandId = null;
                                await _veichleRepository.UpdateAsync(item);
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        await _brandRepository.DeleteAsync(brand);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            var query = _brandRepository.GetAll();
            int count = await query.CountAsync();
            query = query.FilterDataTable(input);
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr));
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));
            int filteredCount = await query.CountAsync();
            var brands = await query
                .OrderBy($"{input.columns[input.order[0].column].name} {input.order[0].dir}")
                .Skip(input.start)
                .Take(input.length)
                .ToListAsync();
            return new DataTableOutputDto<BrandDto>
            {
                iTotalDisplayRecords = filteredCount,
                iTotalRecords = count,
                aaData = ObjectMapper.Map<List<BrandDto>>(brands)
            };
        }
    }

    public override async Task<BrandDto> GetAsync(EntityDto<long> input)
    {
        var brand = await _brandRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
        return ObjectMapper.Map<BrandDto>(brand);
    }

    [AbpAuthorize]
    public override async Task<BrandDto> CreateAsync(CreateBrandDto input)
    {
        int existingCount = await _brandRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn);
        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.Brands.Error.AlreadyExist"));

        input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Brands", CodeField = "Code" });
        var brand = ObjectMapper.Map<Brand>(input);
        await _brandRepository.InsertAsync(brand);
        return MapToEntityDto(brand);
    }

    [AbpAuthorize]
    public override async Task<BrandDto> UpdateAsync(UpdateBrandDto input)
    {
        int existingCount = await _brandRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id);

        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.Brands.Error.AlreadyExist"));

        var brand = await _brandRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, brand);
        await _brandRepository.UpdateAsync(brand);
        return MapToEntityDto(brand);
    }

    public override async Task<PagedResultDto<BrandDto>> GetAllAsync(GetAllBrands input)
    {
        var query = _brandRepository.GetAll();
        query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));

        if (input.MaxCount)
        {
            input.SkipCount = 0;
            input.MaxResultCount = await query.CountAsync();
        }

        var brands = await query
            .OrderBy(x => x.Id)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        return new PagedResultDto<BrandDto>(
            await query.CountAsync(),
            ObjectMapper.Map<List<BrandDto>>(brands)
        );
    }
}
