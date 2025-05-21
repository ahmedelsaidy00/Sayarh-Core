using Abp.Application.Services;
using Sayarah.Application.Veichles.Dto;

namespace Sayarah.Application.Veichles
{
    
    public interface IVeichlePicAppService : IAsyncCrudAppService<VeichlePicDto, long, GetAllVeichlePic, CreateVeichlePicDto, UpdateVeichlePicDto>
    {
        Task<bool> SaveVeichlePic(SaveVeichlePicDto input);
        Task DeleteMedia(DeleteVeichlePicInput input);
    }
}
