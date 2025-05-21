using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.CompanyInvoices;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.CompanyInvoices;

public class CompanyInvoiceTransactionAppService : AsyncCrudAppService<CompanyInvoiceTransaction, CompanyInvoiceTransactionDto, long, GetAllCompanyInvoiceTransactions, CreateCompanyInvoiceTransactionDto, UpdateCompanyInvoiceTransactionDto>, ICompanyInvoiceTransactionAppService
{
    private readonly IRepository<CompanyInvoiceTransaction, long> _companyInvoiceTransactionRepository;
    public ICommonAppService _commonAppService { get; set; }
    public CompanyInvoiceTransactionAppService(IRepository<CompanyInvoiceTransaction, long> repository,
        ICommonAppService commonAppService)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _companyInvoiceTransactionRepository = repository;
        _commonAppService = commonAppService;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<CompanyInvoiceTransactionDto>> GetPaged(GetCompanyInvoiceTransactionsInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int companyInvoiceTransactionId = Convert.ToInt32(input.ids[i]);
                        CompanyInvoiceTransaction companyInvoiceTransaction = await _companyInvoiceTransactionRepository.GetAsync(companyInvoiceTransactionId);
                        if (companyInvoiceTransaction != null)
                        {
                            if (input.action == "Delete")//Delete
                            {
                                await _companyInvoiceTransactionRepository.DeleteAsync(companyInvoiceTransaction);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int companyInvoiceTransactionId = Convert.ToInt32(input.ids[0]);
                        CompanyInvoiceTransaction companyInvoiceTransaction = await _companyInvoiceTransactionRepository.GetAsync(companyInvoiceTransactionId);
                        if (companyInvoiceTransaction != null)
                        {
                            if (input.action == "Delete")//Delete
                            {
                                
                                await _companyInvoiceTransactionRepository.DeleteAsync(companyInvoiceTransaction);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                int count = await _companyInvoiceTransactionRepository.CountAsync();
                var query = _companyInvoiceTransactionRepository.GetAll()
                    .Include(a => a.CompanyInvoice.Company).AsQueryable();


                query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyInvoice.CompanyId == input.CompanyId);
               
                count = query.Count();
                query = query.FilterDataTable(input);
                 
                query = query.WhereIf(input.CompanyInvoiceId.HasValue, m => m.CompanyInvoiceId == input.CompanyInvoiceId);
                query = query.WhereIf(input.TransId.HasValue, m => m.TransId == input.TransId);
                query = query.WhereIf(input.TransType.HasValue, m => m.TransType == input.TransType);
                
                int filteredCount = await query.CountAsync();
                var companyInvoiceTransactions =
                      await query/*.Include(q => q.CreatorUser)*/
                       .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start)
                        .Take(input.length)
                          .ToListAsync();
                return new DataTableOutputDto<CompanyInvoiceTransactionDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<CompanyInvoiceTransactionDto>>(companyInvoiceTransactions)
                };
            }
        }
        catch (Exception ex)
        {
            throw ;
        }
    }
    public override async Task<CompanyInvoiceTransactionDto> GetAsync(EntityDto<long> input)
    {
        var companyInvoiceTransaction = _companyInvoiceTransactionRepository.FirstOrDefault(x => x.Id == input.Id);

        return await Task.FromResult(ObjectMapper.Map<CompanyInvoiceTransactionDto>(companyInvoiceTransaction));
    }
    [AbpAuthorize]
    public override async Task<CompanyInvoiceTransactionDto> CreateAsync(CreateCompanyInvoiceTransactionDto input)
    {
        try
        { 
            var companyInvoiceTransaction = ObjectMapper.Map<CompanyInvoiceTransaction>(input);
            await _companyInvoiceTransactionRepository.InsertAsync(companyInvoiceTransaction);
            return MapToEntityDto(companyInvoiceTransaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    [AbpAuthorize]
    public override async Task<CompanyInvoiceTransactionDto> UpdateAsync(UpdateCompanyInvoiceTransactionDto input)
    {
        try
        {
            var companyInvoiceTransaction = await _companyInvoiceTransactionRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, companyInvoiceTransaction);
            await _companyInvoiceTransactionRepository.UpdateAsync(companyInvoiceTransaction);
            return MapToEntityDto(companyInvoiceTransaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public override async Task<PagedResultDto<CompanyInvoiceTransactionDto>> GetAllAsync(GetAllCompanyInvoiceTransactions input)
    {
        try
        {
            var query = _companyInvoiceTransactionRepository.GetAll()
                .Include(a => a.CompanyInvoice.Company).AsQueryable();

            query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyInvoice.CompanyId == input.CompanyId);
            query = query.WhereIf(input.CompanyInvoiceId.HasValue, m => m.CompanyInvoiceId  == input.CompanyInvoiceId);
            query = query.WhereIf(input.TransId.HasValue, m => m.TransId == input.TransId);
            query = query.WhereIf(input.TransType.HasValue, m => m.TransType == input.TransType);

            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }
            var companyInvoiceTransactions = await query
                .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToListAsync();
            return new PagedResultDto<CompanyInvoiceTransactionDto>(query.Count(), ObjectMapper.Map<List<CompanyInvoiceTransactionDto>>(companyInvoiceTransactions));
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
