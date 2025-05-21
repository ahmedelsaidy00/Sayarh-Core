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

namespace Sayarah.Application.Lookups;

[DisableAuditing]
public class CompanyTypeAppService : AsyncCrudAppService<CompanyType, CompanyTypeDto, long, GetAllCompanyTypes, CreateCompanyTypeDto, UpdateCompanyTypeDto>, ICompanyTypeAppService
{
    private readonly IRepository<CompanyType, long> _companyTypeRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<Company, long> _companyRepository;
    public ICommonAppService _commonAppService { get; set; }

    public CompanyTypeAppService(
        IRepository<CompanyType, long> repository,
        IRepository<User, long> userRepository,
        IRepository<Company, long> companyRepository,
        ICommonAppService commonAppService)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _companyTypeRepository = repository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _commonAppService = commonAppService;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<CompanyTypeDto>> GetPaged(GetCompanyTypesInput input)
    {
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
        {
            if (input.actionType == "GroupAction")
            {
                for (int i = 0; i < input.ids.Length; i++)
                {
                    int companyTypeId = Convert.ToInt32(input.ids[i]);
                    var companyType = await _companyTypeRepository.GetAsync(companyTypeId);
                    if (companyType != null && input.action == "Delete")
                    {
                        var companies = await _companyRepository.GetAll().Where(at => at.CompanyTypeId == companyTypeId).ToListAsync();
                        if (companies != null && companies.Count > 0)
                        {
                            foreach (var item in companies)
                            {
                                item.CompanyTypeId = null;
                                await _companyRepository.UpdateAsync(item);
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        await _companyTypeRepository.DeleteAsync(companyType);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            else if (input.actionType == "SingleAction" && input.ids.Length > 0)
            {
                int companyTypeId = Convert.ToInt32(input.ids[0]);
                var companyType = await _companyTypeRepository.GetAsync(companyTypeId);
                if (companyType != null && input.action == "Delete")
                {
                    var companies = await _companyRepository.GetAll().Where(at => at.CompanyTypeId == companyTypeId).ToListAsync();
                    if (companies != null)
                    {
                        foreach (var item in companies)
                        {
                            item.CompanyTypeId = null;
                            await _companyRepository.UpdateAsync(item);
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _companyTypeRepository.DeleteAsync(companyType);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var query = _companyTypeRepository.GetAll();
            int count = await query.CountAsync();
            query = query.FilterDataTable(input);
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr));
            query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));
            int filteredCount = await query.CountAsync();
            var companyTypes = await query
                .OrderBy($"{input.columns[input.order[0].column].name} {input.order[0].dir}")
                .Skip(input.start)
                .Take(input.length)
                .ToListAsync();

            return new DataTableOutputDto<CompanyTypeDto>
            {
                iTotalDisplayRecords = filteredCount,
                iTotalRecords = count,
                aaData = ObjectMapper.Map<List<CompanyTypeDto>>(companyTypes)
            };
        }
    }

    public override async Task<CompanyTypeDto> GetAsync(EntityDto<long> input)
    {
        var companyType = await _companyTypeRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
        return ObjectMapper.Map<CompanyTypeDto>(companyType);
    }

    [AbpAuthorize]
    public override async Task<CompanyTypeDto> CreateAsync(CreateCompanyTypeDto input)
    {
        int existingCount = await _companyTypeRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn);
        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.CompanyTypes.Error.AlreadyExist"));

        input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyTypes", CodeField = "Code" });

        var companyType = ObjectMapper.Map<CompanyType>(input);
        await _companyTypeRepository.InsertAsync(companyType);
        return MapToEntityDto(companyType);
    }

    [AbpAuthorize]
    public override async Task<CompanyTypeDto> UpdateAsync(UpdateCompanyTypeDto input)
    {
        int existingCount = await _companyTypeRepository.CountAsync(at => at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id);
        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.CompanyTypes.Error.AlreadyExist"));

        var companyType = await _companyTypeRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, companyType);
        await _companyTypeRepository.UpdateAsync(companyType);
        return MapToEntityDto(companyType);
    }

    public override async Task<PagedResultDto<CompanyTypeDto>> GetAllAsync(GetAllCompanyTypes input)
    {
        var query = _companyTypeRepository.GetAll();
        query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));

        if (input.MaxCount)
        {
            input.SkipCount = 0;
            input.MaxResultCount = await query.CountAsync();
        }
        var companyTypes = await query
            .OrderBy(x => x.Id)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        int totalCount = await query.CountAsync();
        return new PagedResultDto<CompanyTypeDto>(
            totalCount, ObjectMapper.Map<List<CompanyTypeDto>>(companyTypes)
        );
    }
}
