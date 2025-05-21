using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Drivers;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Veichles
{
    [AbpAuthorize]
    public class VeichleRouteAppService : AsyncCrudAppService<VeichleRoute, VeichleRouteDto, long, GetVeichleRoutesInput, CreateVeichleRouteDto, UpdateVeichleRouteDto>, IVeichleRouteAppService
    {
        private readonly ICommonAppService _commonService;
        private readonly IRepository<DriverVeichle, long> _driverVeichleRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;

        public VeichleRouteAppService(
            IRepository<VeichleRoute, long> repository,
            ICommonAppService commonService,
            IRepository<DriverVeichle, long> driverVeichleRepository,
            IRepository<Veichle, long> veichleRepository
        ) : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _driverVeichleRepository = driverVeichleRepository;
            _veichleRepository = veichleRepository;
        }

        public override async Task<VeichleRouteDto> GetAsync(EntityDto<long> input)
        {
            var veichleRoute = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(veichleRoute);
        }

        public override async Task<VeichleRouteDto> CreateAsync(CreateVeichleRouteDto input)
        {
            input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleRoutes", CodeField = "Code" });

            var veichleRoute = ObjectMapper.Map<VeichleRoute>(input);
            veichleRoute = await Repository.InsertAsync(veichleRoute);
            await CurrentUnitOfWork.SaveChangesAsync();

            if (input.StartDate.HasValue && input.EndDate.HasValue)
            {
                var existVeichle = await _driverVeichleRepository.FirstOrDefaultAsync(a => a.VeichleId == input.VeichleId && a.DriverId == input.DriverId);
                if (existVeichle == null)
                {
                    existVeichle = await _driverVeichleRepository.InsertAsync(new DriverVeichle
                    {
                        DriverId = input.DriverId,
                        VeichleId = input.VeichleId
                    });
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.StartDate <= DateTime.Now && input.EndDate >= DateTime.Now)
                {
                    await _driverVeichleRepository.GetAll().Where(a => a.VeichleId == input.VeichleId).ForEachAsync(a => a.IsCurrent = false);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    existVeichle.IsCurrent = true;
                    await _driverVeichleRepository.UpdateAsync(existVeichle);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var veichles = await _veichleRepository.GetAll().Where(a => a.DriverId == input.DriverId).ToListAsync();
                    if (veichles != null)
                    {
                        foreach (var item in veichles)
                        {
                            item.DriverId = null;
                            await _veichleRepository.UpdateAsync(item);
                        }
                    }

                    var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == input.VeichleId);
                    veichle.DriverId = input.DriverId;
                    await _veichleRepository.UpdateAsync(veichle);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            return MapToEntityDto(veichleRoute);
        }

        public override async Task<VeichleRouteDto> UpdateAsync(UpdateVeichleRouteDto input)
        {
            var veichleRoute = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, veichleRoute);
            await Repository.UpdateAsync(veichleRoute);
            return MapToEntityDto(veichleRoute);
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var veichleRoute = await Repository.GetAsync(input.Id);
            if (veichleRoute == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(veichleRoute);
        }

        public override async Task<PagedResultDto<VeichleRouteDto>> GetAllAsync(GetVeichleRoutesInput input)
        {
            var query = Repository.GetAll()
                .Include(at => at.Driver)
                .Include(at => at.Veichle)
                .Include(at => at.Branch.Company)
                .AsQueryable();

            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

            int count = await query.CountAsync();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = count;
            }

            var veichleRoutes = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var mappedList = ObjectMapper.Map<List<VeichleRouteDto>>(veichleRoutes);
            return new PagedResultDto<VeichleRouteDto>(count, mappedList);
        }

        public async Task<DataTableOutputDto<VeichleRouteDto>> GetPaged(GetVeichleRoutesPagedInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                int id = 0;
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        id = Convert.ToInt32(input.ids[i]);
                        var veichleRoute = await Repository.FirstOrDefaultAsync(id);
                        if (veichleRoute != null && input.action == "Delete")
                        {
                            await Repository.DeleteAsync(veichleRoute);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        id = Convert.ToInt32(input.ids[0]);
                        var veichleRoute = await Repository.FirstOrDefaultAsync(id);
                        if (veichleRoute != null && input.action == "Delete")
                        {
                            await Repository.DeleteAsync(veichleRoute);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                var query = Repository.GetAll()
                    .Include(a => a.Branch.Company)
                    .Include(at => at.Driver)
                    .Include(at => at.Veichle)
                    .AsQueryable();

                query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

                int count = await query.CountAsync();
                query = query.FilterDataTable((DataTableInputDto)input);
                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName));

                int filteredCount = await query.CountAsync();
                var veichleRoutes = await query.Include(x => x.Branch)
                    .Include(x => x.CreatorUser)
                    .Include(x => x.LastModifierUser)
                    .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                    .Skip(input.start)
                    .Take(input.length)
                    .ToListAsync();

                return new DataTableOutputDto<VeichleRouteDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<VeichleRouteDto>>(veichleRoutes)
                };
            }
        }
    }
}
