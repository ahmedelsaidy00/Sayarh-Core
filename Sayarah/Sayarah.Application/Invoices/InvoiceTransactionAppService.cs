using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Invoices.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Invoices;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Invoices
{
    public class InvoiceTransactionAppService : AsyncCrudAppService<InvoiceTransaction, InvoiceTransactionDto, long, GetAllInvoiceTransactions, CreateInvoiceTransactionDto, UpdateInvoiceTransactionDto>, IInvoiceTransactionAppService
    {
        private readonly IRepository<InvoiceTransaction, long> _invoiceTransactionRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        public ICommonAppService _commonAppService { get; set; }
        public InvoiceTransactionAppService(IRepository<InvoiceTransaction, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _invoiceTransactionRepository = repository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<InvoiceTransactionDto>> GetPaged(GetInvoiceTransactionsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int invoiceTransactionId = Convert.ToInt32(input.ids[i]);
                            InvoiceTransaction invoiceTransaction = await _invoiceTransactionRepository.GetAsync(invoiceTransactionId);
                            if (invoiceTransaction != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                   
                                   

                                    //if (centersCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.InvoiceTransactions.Error.HasCenters"));

                                    await _invoiceTransactionRepository.DeleteAsync(invoiceTransaction);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int invoiceTransactionId = Convert.ToInt32(input.ids[0]);
                            InvoiceTransaction invoiceTransaction = await _invoiceTransactionRepository.GetAsync(invoiceTransactionId);
                            if (invoiceTransaction != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    
                                    await _invoiceTransactionRepository.DeleteAsync(invoiceTransaction);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _invoiceTransactionRepository.CountAsync();
                    var query = _invoiceTransactionRepository.GetAll()
                        .Include(a => a.Invoice.Journal)
                        .Include(a => a.Invoice.Provider.MainProvider)
                        .Include(a => a.Invoice.MainProvider).AsQueryable();


                    query = query.WhereIf(input.ProviderId.HasValue, m => m.Invoice.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, m => m.Invoice.MainProviderId == input.MainProviderId);
                   
                    count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr) );
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.NameEn), at => at.NameEn.Contains(input.NameEn));


                    query = query.WhereIf(input.JournalId.HasValue, m => m.Invoice.JournalId == input.JournalId);
                    query = query.WhereIf(input.InvoiceId.HasValue, m => m.InvoiceId == input.InvoiceId);
                    query = query.WhereIf(input.TransId.HasValue, m => m.TransId == input.TransId);
                    query = query.WhereIf(input.TransType.HasValue, m => m.TransType == input.TransType);
                    
                    int filteredCount = await query.CountAsync();
                    var invoiceTransactions =
                          await query/*.Include(q => q.CreatorUser)*/
                           .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                              .ToListAsync();
                    return new DataTableOutputDto<InvoiceTransactionDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<InvoiceTransactionDto>>(invoiceTransactions)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<InvoiceTransactionDto> GetAsync(EntityDto<long> input)
        {
            var invoiceTransaction = _invoiceTransactionRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<InvoiceTransactionDto>(invoiceTransaction));
        }

        [AbpAuthorize]
        public override async Task<InvoiceTransactionDto> CreateAsync(CreateInvoiceTransactionDto input)
        {
            try
            {
                //int existingCount = await _invoiceTransactionRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.InvoiceTransactions.Error.AlreadyExist"));

                //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "InvoiceTransactions", CodeField = "Code" });

                var invoiceTransaction = ObjectMapper.Map<InvoiceTransaction>(input);
                await _invoiceTransactionRepository.InsertAsync(invoiceTransaction);
                return MapToEntityDto(invoiceTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<InvoiceTransactionDto> UpdateAsync(UpdateInvoiceTransactionDto input)
        {
            try
            {
                ////Check if InvoiceTransaction exists
                //int existingCount = await _invoiceTransactionRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.InvoiceTransactions.Error.AlreadyExist"));
                var invoiceTransaction = await _invoiceTransactionRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, invoiceTransaction);
                await _invoiceTransactionRepository.UpdateAsync(invoiceTransaction);
                return MapToEntityDto(invoiceTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<InvoiceTransactionDto>> GetAllAsync(GetAllInvoiceTransactions input)
        {
            try
            {
                var query = _invoiceTransactionRepository.GetAll()
                    .Include(a => a.Invoice.Journal)
                    .Include(a => a.Invoice.Provider.MainProvider)
                    .Include(a => a.Invoice.MainProvider).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.Invoice.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.Invoice.MainProviderId == input.MainProviderId);

                query = query.WhereIf(input.JournalId.HasValue, m => m.Invoice.JournalId == input.JournalId);
                query = query.WhereIf(input.InvoiceId.HasValue, m => m.InvoiceId  == input.InvoiceId);
                query = query.WhereIf(input.TransId.HasValue, m => m.TransId == input.TransId);
                query = query.WhereIf(input.TransType.HasValue, m => m.TransType == input.TransType);

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var invoiceTransactions = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<InvoiceTransactionDto>(
                   query.Count(), ObjectMapper.Map<List<InvoiceTransactionDto>>(invoiceTransactions)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
