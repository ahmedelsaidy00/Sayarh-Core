using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Drivers;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Drivers;

public class DriverVeichleAppService : AsyncCrudAppService<DriverVeichle, DriverVeichleDto, long, GetDriverVeichlesInput, CreateDriverVeichleDto, UpdateDriverVeichleDto>, IDriverVeichleAppService
{
    private readonly IRepository<Veichle, long> _veichleRepository;
    public DriverVeichleAppService(
         IRepository<DriverVeichle, long> repository,
         IRepository<Veichle, long> veichleRepository
        )
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _veichleRepository = veichleRepository;

    }
    public override async Task<DriverVeichleDto> GetAsync(EntityDto<long> input)
    {
        var DriverVeichle = await Repository.GetAll()
            .Include(x => x.Driver.Branch.Company)
            .Include(x => x.Veichle)
            .FirstOrDefaultAsync(x => x.Id == input.Id);
        return MapToEntityDto(DriverVeichle);
    }
    [AbpAuthorize]
    public override async Task<DriverVeichleDto> CreateAsync(CreateDriverVeichleDto input)
    {
        try
        {
            var driverVeichle = ObjectMapper.Map<DriverVeichle>(input);
            driverVeichle = await Repository.InsertAsync(driverVeichle);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToEntityDto(driverVeichle);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    [AbpAuthorize]
    public override async Task<DriverVeichleDto> UpdateAsync(UpdateDriverVeichleDto input)
    {

        var driverVeichle = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
        ObjectMapper.Map(input, driverVeichle);
        await Repository.UpdateAsync(driverVeichle);
        return MapToEntityDto(driverVeichle);
    }
    [AbpAuthorize]
    public override async Task DeleteAsync(EntityDto<long> input)
    {
        var driverVeichle = await Repository.GetAsync(input.Id);
        if (driverVeichle == null)
            throw new UserFriendlyException("Common.Message.ElementNotFound");
        await Repository.DeleteAsync(driverVeichle);
    }
    public override async Task<PagedResultDto<DriverVeichleDto>> GetAllAsync(GetDriverVeichlesInput input)
    {
        var query = Repository.GetAll()
            .Include(at => at.Veichle.Branch)
            .Include(at => at.Veichle.Brand)
            .Include(at => at.Veichle.Model)
            .Include(at => at.Driver.Branch).AsQueryable();

        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
        query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
        query = query.WhereIf(input.DriverUserId.HasValue, at => at.Driver.UserId == input.DriverUserId);
        query = query.WhereIf(input.BranchId.HasValue, at => at.Driver.BranchId == input.BranchId);
        query = query.WhereIf(input.CompanyId.HasValue, at => at.Driver.Branch.CompanyId == input.CompanyId);

        int count = query.Count();
        if (input.MaxCount == true)
        {
            input.SkipCount = 0;
            input.MaxResultCount = query.Count();
        }

        var driverVeichles = await query.OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount).Take(input.MaxResultCount)
            .ToListAsync();

        var _mappedList = ObjectMapper.Map<List<DriverVeichleDto>>(driverVeichles);
        return new PagedResultDto<DriverVeichleDto>(count, _mappedList);
    }
    [AbpAuthorize]
    public async Task<DataTableOutputDto<DriverVeichleDto>> GetPaged(GetDriverVeichlesPagedInput input)
    {
        try
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                int id = 0;
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        id = Convert.ToInt32(input.ids[i]);
                        DriverVeichle driverVeichle = await Repository.FirstOrDefaultAsync(id);
                        if (driverVeichle != null)
                        {
                            if (input.action == "Delete")
                            {
                                // update driverId in veichle
                                var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.VeichleId && a.DriverId == driverVeichle.DriverId);
                                if (veichle != null)
                                {
                                    veichle.DriverId = null;
                                    await _veichleRepository.UpdateAsync(veichle);
                                }

                                await Repository.DeleteAsync(driverVeichle);
                                await UnitOfWorkManager.Current.SaveChangesAsync();

                            }
                            if (input.action == "Current")
                            {

                                if (driverVeichle.IsCurrent == false)
                                {
                                    // set all to false and set row to true

                                    await Repository.GetAll().Where(a => a.VeichleId == driverVeichle.VeichleId).ForEachAsync(a => a.IsCurrent = false);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                    driverVeichle.IsCurrent = !driverVeichle.IsCurrent;
                                    await Repository.UpdateAsync(driverVeichle);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();


                                    // update driverId
                                    var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.VeichleId);
                                    veichle.DriverId = input.DriverId;
                                    await _veichleRepository.UpdateAsync(veichle);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        id = Convert.ToInt32(input.ids[0]);
                        DriverVeichle driverVeichle = await Repository.FirstOrDefaultAsync(id);
                        if (driverVeichle != null)
                        {
                            if (input.action == "Delete")
                            {

                                // update driverId in veichle
                                var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.VeichleId && a.DriverId == driverVeichle.DriverId);
                                if (veichle != null)
                                {
                                    veichle.DriverId = null;
                                    await _veichleRepository.UpdateAsync(veichle);
                                }

                                await Repository.DeleteAsync(driverVeichle);
                                await UnitOfWorkManager.Current.SaveChangesAsync();

                            }
                            if (input.action == "Current")
                            {

                                if (driverVeichle.IsCurrent == false)
                                {
                                    // set all to false and set row to true
                                    await Repository.GetAll().Where(a => a.VeichleId == driverVeichle.VeichleId).ForEachAsync(a => a.IsCurrent = false);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                    driverVeichle.IsCurrent = !driverVeichle.IsCurrent;
                                    await Repository.UpdateAsync(driverVeichle);

                                    // update driverId
                                    var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.VeichleId);
                                    veichle.DriverId = input.DriverId;
                                    await _veichleRepository.UpdateAsync(veichle);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                            }
                        }
                    }
                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }

                if (input.CompanyId.HasValue == false)
                    return new DataTableOutputDto<DriverVeichleDto>
                    {
                        iTotalDisplayRecords = 0,
                        iTotalRecords = 0,
                        aaData = []
                    };

                var query = Repository.GetAll()
                    .Include(a => a.Driver.Branch.Company)
                    .Include(a => a.Veichle).AsQueryable();

                query = query.WhereIf(input.CompanyId.HasValue, a => a.Driver.Branch.CompanyId == input.CompanyId);
                query = query.WhereIf(input.BranchId.HasValue, at => at.Driver.BranchId == input.BranchId);
                query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at => at.Veichle.NameAr.Contains(input.VeichleName) || at.Veichle.NameEn.Contains(input.VeichleName));
                int count = await query.CountAsync();
                query = query.FilterDataTable((DataTableInputDto)input);
                query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                int filteredCount = await query.CountAsync();
                var driverVeichles = await query.Include(a => a.Driver.Branch.Company)
                    .Include(a => a.Veichle)
                    .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                return new DataTableOutputDto<DriverVeichleDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<DriverVeichleDto>>(driverVeichles)
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    [AbpAuthorize]
    public async Task<bool> UpdateIsCurrent(EntityDto<long> input)
    {
        try
        {

            var driverVeichle = await Repository.FirstOrDefaultAsync(a => a.Id == input.Id);
            if (driverVeichle == null)
                throw new UserFriendlyException(L(""));

            await Repository.GetAll().Where(a => a.VeichleId == driverVeichle.VeichleId).ForEachAsync(a => a.IsCurrent = false);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            driverVeichle.IsCurrent = !driverVeichle.IsCurrent;
            await Repository.UpdateAsync(driverVeichle);
            await UnitOfWorkManager.Current.SaveChangesAsync();


            // update driverId
            var veichle = await _veichleRepository.FirstOrDefaultAsync(a => a.Id == driverVeichle.VeichleId);
            veichle.DriverId = driverVeichle.DriverId;
            await _veichleRepository.UpdateAsync(veichle);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
