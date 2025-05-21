using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Packages.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Packages
{
    public interface IPackageAppService : IAsyncCrudAppService<PackageDto, long, GetAllPackages, CreatePackageDto, UpdatePackageDto>
    {
        Task<DataTableOutputDto<PackageDto>> GetPaged(GetPackagePagedInput input);
        Task<PackageDto> GetPackageByOptions(GetAllPackages input);
    }
}
