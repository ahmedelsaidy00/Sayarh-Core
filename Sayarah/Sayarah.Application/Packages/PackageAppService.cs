using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.Helpers;
using Sayarah.Application.Packages.Dto;
using Sayarah.Companies;
using Sayarah.Packages;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;


namespace Sayarah.Application.Packages
{
    [AbpAuthorize]
    public class PackageAppService : AsyncCrudAppService<Package, PackageDto, long, GetAllPackages, CreatePackageDto, UpdatePackageDto>, IPackageAppService
    {
        private readonly IRepository<Package, long> _packageRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PackageAppService(
            IRepository<Package, long> repository,
            IRepository<Company, long> companyRepository,
            IUnitOfWorkManager unitOfWorkManager
        ) : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _packageRepository = repository;
            _companyRepository = companyRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task<PackageDto> GetAsync(EntityDto<long> input)
        {
            var package = await Repository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);
            return ObjectMapper.Map<PackageDto>(package);
        }

        public override async Task<PackageDto> CreateAsync(CreatePackageDto input)
        {
            var existingPackage = await _packageRepository.FirstOrDefaultAsync(at => at.VeichlesFrom <= input.VeichlesFrom && at.VeichlesTo >= input.VeichlesFrom);
            if (existingPackage != null)
                throw new UserFriendlyException(L("Pages.Packages.Error.InconsistencyInVehicleNumbers"));

            existingPackage = await _packageRepository.FirstOrDefaultAsync(at => at.VeichlesFrom <= input.VeichlesTo && at.VeichlesTo >= input.VeichlesTo);
            if (existingPackage != null)
                throw new UserFriendlyException(L("Pages.Packages.Error.InconsistencyInVehicleNumbers"));

            var package = ObjectMapper.Map<Package>(input);
            await Repository.InsertAsync(package);
            return MapToEntityDto(package);
        }

        public override async Task<PackageDto> UpdateAsync(UpdatePackageDto input)
        {
            var existingPackage = await _packageRepository.FirstOrDefaultAsync(at => at.Id != input.Id && at.VeichlesFrom <= input.VeichlesFrom && at.VeichlesTo >= input.VeichlesFrom);
            if (existingPackage != null)
                throw new UserFriendlyException(L("Pages.Packages.Error.InconsistencyInVehicleNumbers"));

            existingPackage = await _packageRepository.FirstOrDefaultAsync(at => at.Id != input.Id && at.VeichlesFrom <= input.VeichlesTo && at.VeichlesTo >= input.VeichlesTo);
            if (existingPackage != null)
                throw new UserFriendlyException(L("Pages.Packages.Error.InconsistencyInVehicleNumbers"));

            var package = await Repository.GetAsync(input.Id);
            ObjectMapper.Map(input, package);
            await Repository.UpdateAsync(package);
            return MapToEntityDto(package);
        }

        public async Task Delete(EntityDto<int> input)
        {
            var package = await Repository.FirstOrDefaultAsync(input.Id);
            if (package == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            await Repository.DeleteAsync(package);
        }

        public override async Task<PagedResultDto<PackageDto>> GetAllAsync(GetAllPackages input)
        {
            var query = Repository.GetAll().Where(x => x.Visible);
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
            query = query.WhereIf(input.VeichlesFrom.HasValue, at => at.VeichlesFrom >= input.VeichlesFrom);
            query = query.WhereIf(input.VeichlesTo.HasValue, at => at.VeichlesTo <= input.VeichlesTo);

            int _count = await query.CountAsync();
            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = _count;
            }
            var packages = await query.OrderBy(a => a.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            return new PagedResultDto<PackageDto>(_count, ObjectMapper.Map<List<PackageDto>>(packages));
        }

        public async Task<PackageDto> GetPackageByOptions(GetAllPackages input)
        {
            var query = Repository.GetAll().Where(x => x.Visible);
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
            query = query.WhereIf(input.VeichlesFrom.HasValue, at => at.VeichlesFrom >= input.VeichlesFrom);
            query = query.WhereIf(input.VeichlesTo.HasValue, at => at.VeichlesTo <= input.VeichlesTo);
            query = query.WhereIf(input.VeichlesCount.HasValue, at => at.VeichlesTo >= input.VeichlesCount && at.VeichlesFrom <= input.VeichlesCount);

            var package = await query.FirstOrDefaultAsync();
            return ObjectMapper.Map<PackageDto>(package);
        }

        public async Task<DataTableOutputDto<PackageDto>> GetPaged(GetPackagePagedInput input)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                int id = 0;
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        id = Convert.ToInt32(input.ids[i]);
                        var package = await Repository.FirstOrDefaultAsync(id);
                        if (package != null)
                        {
                            if (input.action == "Delete")
                            {
                                await Repository.DeleteAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "Visible")
                            {
                                package.Visible = true;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "NotVisible")
                            {
                                package.Visible = false;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "Free")
                            {
                                package.Free = true;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "NotFree")
                            {
                                package.Free = false;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                        }
                    }
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        id = Convert.ToInt32(input.ids[0]);
                        var package = await Repository.FirstOrDefaultAsync(id);
                        if (package != null)
                        {
                            if (input.action == "Delete")
                            {
                                await Repository.DeleteAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "Visible")
                            {
                                package.Visible = true;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "NotVisible")
                            {
                                package.Visible = false;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "Free")
                            {
                                package.Free = true;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                            else if (input.action == "NotFree")
                            {
                                package.Free = false;
                                await Repository.UpdateAsync(package);
                                await _unitOfWorkManager.Current.SaveChangesAsync();
                            }
                        }
                    }
                }

                var query = Repository.GetAll();
                int count = await query.CountAsync();
                query = query.FilterDataTable(input);
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), m => m.NameAr.Contains(input.NameAr));
                query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), m => m.NameEn.Contains(input.NameEn));
                query = query.WhereIf(input.VeichlesFrom.HasValue, at => at.VeichlesFrom >= input.VeichlesFrom);
                query = query.WhereIf(input.VeichlesTo.HasValue, at => at.VeichlesTo <= input.VeichlesTo);
                query = query.WhereIf(input.PackageId.HasValue, at => at.Id == input.PackageId);
                query = query.WhereIf(input.Visible.HasValue, at => at.Visible == input.Visible);
                query = query.WhereIf(input.Free.HasValue, at => at.Free == input.Free);

                int filteredCount = await query.CountAsync();
                var packages = await query.Include(x => x.CreatorUser)
                    .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                    .Skip(input.start).Take(input.length)
                    .ToListAsync();

                return new DataTableOutputDto<PackageDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<PackageDto>>(packages)
                };
            }
        }
    }
}
