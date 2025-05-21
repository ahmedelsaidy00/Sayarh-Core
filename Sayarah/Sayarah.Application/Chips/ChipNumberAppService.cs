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
using Sayarah.Application.Chips.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Authorization.Users;
using Sayarah.Chips;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Chips
{
    [DisableAuditing]
    public class ChipNumberAppService : AsyncCrudAppService<ChipNumber, ChipNumberDto, long, GetAllChipNumbers, CreateChipNumberDto, UpdateChipNumberDto>, IChipNumberAppService
    {
        private readonly IRepository<ChipNumber, long> _chipNumberRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;

        public ICommonAppService _commonAppService { get; set; }

        public ChipNumberAppService(
            IRepository<ChipNumber, long> repository,
            IRepository<User, long> userRepository,
            ICommonAppService commonAppService,
            IRepository<Veichle, long> veichleRepository)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _chipNumberRepository = repository;
            _userRepository = userRepository;
            _commonAppService = commonAppService;
            _veichleRepository = veichleRepository;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<ChipNumberDto>> GetPaged(GetChipNumbersInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var ids = input.ids?.Select(long.Parse).ToList() ?? new List<long>();

                if (input.actionType == "GroupAction" && input.action == "Delete")
                {
                    foreach (var id in ids)
                    {
                        var chip = await _chipNumberRepository.GetAsync(id);
                        if (chip != null)
                            await _chipNumberRepository.DeleteAsync(chip);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction" && ids.Any())
                {
                    var chip = await _chipNumberRepository.GetAsync(ids.First());
                    if (chip != null)
                        await _chipNumberRepository.DeleteAsync(chip);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                var query = _chipNumberRepository.GetAll()
                    .Include(x => x.ChipDevice)
                    .Include(x => x.Veichle)
                    .Include(x => x.Company)
                    .Include(x => x.Branch).ThenInclude(b => b.Company)
                    .FilterDataTable(input)
                    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.Contains(input.Code))
                    .WhereIf(!string.IsNullOrEmpty(input.ChipDeviceCode), x => x.ChipDevice.Code.Contains(input.ChipDeviceCode))
                    .WhereIf(!string.IsNullOrEmpty(input.BranchName), x => x.Branch.NameAr.Contains(input.BranchName) || x.Branch.NameEn.Contains(input.BranchName) || x.Branch.Code.Contains(input.BranchName))
                    .WhereIf(!string.IsNullOrEmpty(input.CompanyName), x => x.Company.NameAr.Contains(input.CompanyName) || x.Company.NameEn.Contains(input.CompanyName) || x.Company.Code.Contains(input.CompanyName))
                    .WhereIf(!string.IsNullOrEmpty(input.VeichleName), x => x.Veichle.NameAr.Contains(input.VeichleName) || x.Veichle.NameEn.Contains(input.VeichleName) || x.Veichle.Code.Contains(input.VeichleName))
                    .WhereIf(input.ChipDeviceId.HasValue, x => x.ChipDeviceId == input.ChipDeviceId)
                    .WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId)
                    .WhereIf(input.CompanyId.HasValue, x => x.CompanyId == input.CompanyId)
                    .WhereIf(input.VeichleId.HasValue, x => x.VeichleId == input.VeichleId)
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

                var totalCount = await _chipNumberRepository.CountAsync();
                var filteredCount = await query.CountAsync();

                var orderBy = $"{input.columns[input.order[0].column].name} {input.order[0].dir}";
                var result = await query
                    .OrderBy(orderBy)
                    .Skip(input.start)
                    .Take(input.length)
                    .ToListAsync();

                return new DataTableOutputDto<ChipNumberDto>
                {
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = filteredCount,
                    aaData = ObjectMapper.Map<List<ChipNumberDto>>(result)
                };
            }
        }
        public override async Task<ChipNumberDto> GetAsync(EntityDto<long> input)
        {
            var chipNumber = _chipNumberRepository.GetAll()
                .Include(a => a.ChipDevice)
                .Include(a => a.Veichle)
                .Include(a => a.Company)
                .Include(a => a.Branch.Company)
                .FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<ChipNumberDto>(chipNumber));
        }

        [AbpAuthorize]
        public override async Task<ChipNumberDto> CreateAsync(CreateChipNumberDto input)
        {
            try
            {
                int existingCount = await _chipNumberRepository.CountAsync(at => at.Code == input.Code);
                if (existingCount > 0)
                    throw new UserFriendlyException(L("Pages.ChipNumbers.Error.AlreadyExist"));

                var chipNumber = ObjectMapper.Map<ChipNumber>(input);
                await _chipNumberRepository.InsertAsync(chipNumber);
                return MapToEntityDto(chipNumber);
            }
            catch (Exception ex)
            {
                throw ;
            }
        }

        [AbpAuthorize]
        public override async Task<ChipNumberDto> UpdateAsync(UpdateChipNumberDto input)
        {
            try
            {
                int exists = await _chipNumberRepository.CountAsync(at => at.Code == input.Code && at.Id != input.Id);
                if (exists > 0)
                    throw new UserFriendlyException(L("Pages.ChipNumbers.Error.AlreadyExist"));

                var chipNumber = await _chipNumberRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, chipNumber);
                await _chipNumberRepository.UpdateAsync(chipNumber);
                return MapToEntityDto(chipNumber);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override async Task<PagedResultDto<ChipNumberDto>> GetAllAsync(GetAllChipNumbers input)
        {
            var query = _chipNumberRepository.GetAll();

            query = query.WhereIf(input.ChipDeviceId.HasValue, x => x.ChipDeviceId == input.ChipDeviceId)
                         .WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId)
                         .WhereIf(input.CompanyId.HasValue, x => x.CompanyId == input.CompanyId)
                         .WhereIf(input.VeichleId.HasValue, x => x.VeichleId == input.VeichleId)
                         .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = await query.CountAsync();
            }

            var chipNumbers = await query
                .OrderBy(x => x.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var total = await query.CountAsync();
            return new PagedResultDto<ChipNumberDto>(total, ObjectMapper.Map<List<ChipNumberDto>>(chipNumbers));
        }

        [AbpAuthorize]
        public async Task<ChipNumberDto> LinkWithCompany(UpdateChipNumberDto input)
        {
            var chipNumber = await _chipNumberRepository.GetAsync(input.Id);
            chipNumber.CompanyId = input.CompanyId;

            if (chipNumber.BranchId != input.BranchId)
            {
                chipNumber.VeichleId = null;
                chipNumber.Status = ChipStatus.Used;
            }

            chipNumber.BranchId = input.BranchId;
            await _chipNumberRepository.UpdateAsync(chipNumber);
            return MapToEntityDto(chipNumber);
        }

        [AbpAuthorize]
        public async Task<ChipNumberDto> LinkWithVeichle(UpdateChipNumberDto input)
        {
            var veichle = await _veichleRepository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.VeichleId);

            var chipNumber = await _chipNumberRepository.GetAsync(input.Id);

            if (veichle != null)
            {
                if (chipNumber.CompanyId.HasValue && veichle.Branch.CompanyId != chipNumber.CompanyId)
                    throw new UserFriendlyException("Pages.ChipsNumbers.ItsRelatedToOtherCompany");

                if (chipNumber.BranchId.HasValue && veichle.BranchId != chipNumber.BranchId)
                    throw new UserFriendlyException("Pages.ChipsNumbers.ItsRelatedToOtherBranch");
            }

            chipNumber.VeichleId = input.VeichleId;

            if (input.VeichleId.HasValue)
                chipNumber.Status = ChipStatus.Linked;
            else if (input.BranchId.HasValue && chipNumber.Status != ChipStatus.Blocked && chipNumber.Status != ChipStatus.Archived)
                chipNumber.Status = ChipStatus.Used;

            chipNumber.ActivationDate = input.ActivationDate;
            chipNumber.ActivationUserId = AbpSession.UserId;

            await _chipNumberRepository.UpdateAsync(chipNumber);
            return MapToEntityDto(chipNumber);
        }

        [AbpAuthorize]
        public async Task<LinkByChipsEmployeeOutput> LinkByChipsEmployee(LinkByChipsEmployee input)
        {
            var linkedChips = new List<string>();
            var rejectedChips = new List<string>();
            var acceptedChips = new List<string>();

            foreach (var item in input.ChipsNumbers)
            {
                var chip = await _chipNumberRepository.FirstOrDefaultAsync(x => x.ReleaseNumber == item);

                if (chip == null)
                {
                    rejectedChips.Add(item);
                }
                else if (chip.CompanyId.HasValue && chip.CompanyId != input.CompanyId)
                {
                    linkedChips.Add(item);
                }
                else
                {
                    chip.BranchId = input.BranchId;
                    chip.CompanyId = input.CompanyId;
                    chip.Status = ChipStatus.Used;
                    await _chipNumberRepository.UpdateAsync(chip);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    acceptedChips.Add(item);
                }
            }

            return new LinkByChipsEmployeeOutput
            {
                LinkedChips = linkedChips,
                AcceptedChips = acceptedChips,
                RejectedChips = rejectedChips
            };
        }
    }
}
