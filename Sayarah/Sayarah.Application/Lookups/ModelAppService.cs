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
using Sayarah.Lookups;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Lookups
{
    [DisableAuditing]
    public class ModelAppService : AsyncCrudAppService<Model, ModelDto, long, GetAllModels, CreateModelDto, UpdateModelDto>, IModelAppService
    {
        private readonly IRepository<Model, long> _modelRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly ICommonAppService _commonAppService;

        public ModelAppService(
            IRepository<Model, long> repository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _modelRepository = repository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<ModelDto>> GetPaged(GetModelsInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    foreach (var id in input.ids)
                    {
                        if (long.TryParse(id, out var modelId))
                        {
                            var model = await _modelRepository.FirstOrDefaultAsync(modelId);
                            if (model != null && input.action == "Delete")
                            {
                                var veichles = await _veichleRepository.GetAll()
                                    .Where(v => v.ModelId == modelId)
                                    .ToListAsync();

                                if (veichles.Any())
                                {
                                    foreach (var veichle in veichles)
                                    {
                                        veichle.ModelId = null;
                                        await _veichleRepository.UpdateAsync(veichle);
                                    }
                                    await CurrentUnitOfWork.SaveChangesAsync();
                                }
                                await _modelRepository.DeleteAsync(model);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction" && input.ids.Length > 0)
                {
                    if (long.TryParse(input.ids[0], out var modelId))
                    {
                        var model = await _modelRepository.FirstOrDefaultAsync(modelId);
                        if (model != null && input.action == "Delete")
                        {
                            var veichles = await _veichleRepository.GetAll()
                                .Where(v => v.ModelId == modelId)
                                .ToListAsync();

                            if (veichles.Any())
                            {
                                foreach (var veichle in veichles)
                                {
                                    veichle.ModelId = null;
                                    await _veichleRepository.UpdateAsync(veichle);
                                }
                                await CurrentUnitOfWork.SaveChangesAsync();
                            }
                            await _modelRepository.DeleteAsync(model);
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                var query = _modelRepository.GetAll();
                var totalCount = await query.CountAsync();

                query = query.WhereIf(input.BrandId.HasValue, m => m.BrandId == input.BrandId);
                query = query.FilterDataTable(input);
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), m => m.NameAr.Contains(input.NameAr));
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), m => m.NameEn.Contains(input.NameEn));

                var filteredCount = await query.CountAsync();

                var orderColumn = input.columns[input.order[0].column].name;
                var orderDir = input.order[0].dir;
                var models = await query
                    .OrderBy($"{orderColumn} {orderDir}")
                    .Skip(input.start)
                    .Take(input.length)
                    .ToListAsync();

                return new DataTableOutputDto<ModelDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = totalCount,
                    aaData = ObjectMapper.Map<List<ModelDto>>(models)
                };
            }
        }

        public override async Task<ModelDto> GetAsync(EntityDto<long> input)
        {
            var model = await _modelRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
            return ObjectMapper.Map<ModelDto>(model);
        }

        [AbpAuthorize]
        public override async Task<ModelDto> CreateAsync(CreateModelDto input)
        {
            var exists = await _modelRepository.CountAsync(m =>
                m.NameAr == input.NameAr &&
                m.NameEn == input.NameEn &&
                m.BrandId == input.BrandId
            ) > 0;

            if (exists)
                throw new UserFriendlyException(L("Pages.Models.Error.AlreadyExist"));

            input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Models", CodeField = "Code" });

            var model = ObjectMapper.Map<Model>(input);
            await _modelRepository.InsertAsync(model);
            return MapToEntityDto(model);
        }

        [AbpAuthorize]
        public override async Task<ModelDto> UpdateAsync(UpdateModelDto input)
        {
            var exists = await _modelRepository.CountAsync(m =>
                m.NameAr == input.NameAr &&
                m.NameEn == input.NameEn &&
                m.BrandId == input.BrandId &&
                m.Id != input.Id
            ) > 0;

            if (exists)
                throw new UserFriendlyException(L("Pages.Models.Error.AlreadyExist"));

            var model = await _modelRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, model);
            await _modelRepository.UpdateAsync(model);
            return MapToEntityDto(model);
        }

        public override async Task<PagedResultDto<ModelDto>> GetAllAsync(GetAllModels input)
        {
            var query = _modelRepository.GetAll();

            if (!string.IsNullOrEmpty(input.Name))
            {
                query = query.Where(m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
            }

            if (input.BrandId.HasValue)
            {
                query = query.Where(m => m.BrandId == input.BrandId);
            }

            var totalCount = await query.CountAsync();

            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = totalCount;
            }

            var models = await query
                .OrderBy(m => m.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<ModelDto>(
                totalCount,
                ObjectMapper.Map<List<ModelDto>>(models)
            );
        }
    }
}
