using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Veichles;

namespace Sayarah.Application.Veichles
{
    public class VeichlePicAppService : AsyncCrudAppService<VeichlePic, VeichlePicDto, long, GetAllVeichlePic, CreateVeichlePicDto, UpdateVeichlePicDto>, IVeichlePicAppService
    {
        private readonly IRepository<VeichlePic, long> _VeichlePicRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly ICommonAppService _commonService;

        public VeichlePicAppService(IRepository<VeichlePic, long> repository,
            IRepository<Veichle, long> veichleRepository,
             ICommonAppService commonService

            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _VeichlePicRepository = repository;
            _veichleRepository = veichleRepository;
            _commonService = commonService;

        }


        [AbpAuthorize]
        public override async Task<VeichlePicDto> GetAsync(EntityDto<long> input)
        {
            var VeichlePic = _VeichlePicRepository.GetAll().FirstOrDefault(x => x.Id == input.Id);
            return await Task.FromResult(ObjectMapper.Map<VeichlePicDto>(VeichlePic));
        }

        //[AbpAuthorize]
        public override async Task<VeichlePicDto> CreateAsync(CreateVeichlePicDto input)
        {
            try
            {
                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichlePics", CodeField = "Code" });
                var VeichlePic = ObjectMapper.Map<VeichlePic>(input);
                await _VeichlePicRepository.InsertAsync(VeichlePic);
                return MapToEntityDto(VeichlePic);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //[AbpAuthorize]
        public override async Task<VeichlePicDto> UpdateAsync(UpdateVeichlePicDto input)
        {
            try
            {
                var VeichlePic = await _VeichlePicRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, VeichlePic);
                await _VeichlePicRepository.UpdateAsync(VeichlePic);
                //await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(VeichlePic);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task<PagedResultDto<VeichlePicDto>> GetAllAsync(GetAllVeichlePic input)
        {
            var query = _VeichlePicRepository.GetAll();
            query = query.WhereIf(input.VeichleId.HasValue && input.VeichleId.Value > 0, at => at.VeichleId == input.VeichleId);
            var Veichles = await query.ToListAsync();
            return new PagedResultDto<VeichlePicDto>(
               Veichles.Count, ObjectMapper.Map<List<VeichlePicDto>>(Veichles)
                );
        }


        //=================Custom Methodts================


        [AbpAuthorize]
        public async Task<bool> SaveVeichlePic(SaveVeichlePicDto input)
        {
            try
            {
                if (input.VeichlePicList != null && input.VeichlePicList.Count > 0)
                {
                    foreach (VeichlePic item in input.VeichlePicList)
                    {
                        var activityMedia = new VeichlePic
                        {
                            FilePath = item.FilePath,
                            VeichleId = item.VeichleId
                        };
                        if (item.Id > 0)
                        {
                            activityMedia.Id = item.Id;
                            await Repository.UpdateAsync(activityMedia);
                        }
                        else
                            await Repository.InsertAsync(activityMedia);

                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        [AbpAuthorize]

        public async Task DeleteMedia(DeleteVeichlePicInput input)
        {
            var MediaItem = await Repository.GetAsync(input.Id);
            if (MediaItem != null && !string.IsNullOrEmpty(MediaItem.FilePath)) Utilities.DeleteImage(4, MediaItem.FilePath, new string[] { "800x600_" });
            await Repository.DeleteAsync(input.Id);

            await CurrentUnitOfWork.SaveChangesAsync();

        }





    }
}
