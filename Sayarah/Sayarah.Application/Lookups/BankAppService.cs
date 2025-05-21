using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Lookups;
using Sayarah.Veichles;
using Sayarah.Wallets;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Lookups
{
    [DisableAuditing]
    public class BankAppService : AsyncCrudAppService<Bank, BankDto, long, GetAllBanks, CreateBankDto, UpdateBankDto>, IBankAppService
    {
        private readonly IRepository<Bank, long> _bankRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<CompanyWalletTransaction, long> _companyWalletTransactionRepository;
        public ICommonAppService _commonAppService { get; set; }
        public BankAppService(IRepository<Bank, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService,
            IRepository<CompanyWalletTransaction, long> companyWalletTransactionRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _bankRepository = repository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
            _companyWalletTransactionRepository = companyWalletTransactionRepository;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<BankDto>> GetPaged(GetBanksInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int bankId = Convert.ToInt32(input.ids[i]);
                            Bank bank = await _bankRepository.GetAsync(bankId);
                            if (bank != null)
                            {
                                if (input.action == "Delete")//Delete
                                {

                                    int existCount = await _companyWalletTransactionRepository.CountAsync(a => a.BankId == bankId);
                                    if (existCount > 0)
                                        throw new UserFriendlyException(L("Pages.Banks.Errors.HasTransactions"));

                                    await _bankRepository.DeleteAsync(bank);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int bankId = Convert.ToInt32(input.ids[0]);
                            Bank bank = await _bankRepository.GetAsync(bankId);
                            if (bank != null)
                            {
                                if (input.action == "Delete")//Delete
                                {

                                    int existCount = await _companyWalletTransactionRepository.CountAsync(a => a.BankId == bankId);
                                    if (existCount > 0)
                                        throw new UserFriendlyException(L("Pages.Banks.Errors.HasTransactions"));


                                    await _bankRepository.DeleteAsync(bank);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _bankRepository.CountAsync();
                    var query = _bankRepository.GetAll();
                    count = query.Count();
                    query = query.FilterDataTable(input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name) );
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Iban), at => at.Iban.Contains(input.Iban));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.AccountNumber), at => at.AccountNumber.Contains(input.AccountNumber));
                    int filteredCount = await query.CountAsync();
                    var banks =
                          await query/*.Include(q => q.CreatorUser)*/
                           .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                              .ToListAsync();
                    return new DataTableOutputDto<BankDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<BankDto>>(banks)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<BankDto> GetAsync(EntityDto<long> input)
        {
            var bank = _bankRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<BankDto>(bank));
        }

        [AbpAuthorize]
        public override async Task<BankDto> CreateAsync(CreateBankDto input)
        {
            try
            {
                int existingCount = await _bankRepository.CountAsync(at => at.Name == input.Name);
                if (existingCount > 0)
                    throw new UserFriendlyException(L("Pages.Banks.Error.AlreadyExist"));

                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Banks", CodeField = "Code" });

                var bank = ObjectMapper.Map<Bank>(input);
                await _bankRepository.InsertAsync(bank);
                return MapToEntityDto(bank);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<BankDto> UpdateAsync(UpdateBankDto input)
        {
            try
            {
                //Check if Bank exists
                int existingCount = await _bankRepository.CountAsync(at => at.Name == input.Name && at.Id != input.Id);
                if (existingCount > 0)
                    throw new UserFriendlyException(L("Pages.Banks.Error.AlreadyExist"));
                var bank = await _bankRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, bank);
                await _bankRepository.UpdateAsync(bank);
                return MapToEntityDto(bank);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<BankDto>> GetAllAsync(GetAllBanks input)
        {
            try
            {
                var query = _bankRepository.GetAll();

                query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Iban), at => at.Iban.Contains(input.Iban));
                query = query.WhereIf(!string.IsNullOrEmpty(input.AccountNumber), at => at.AccountNumber.Contains(input.AccountNumber));

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var banks = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<BankDto>(
                   query.Count(), ObjectMapper.Map<List<BankDto>>(banks)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
