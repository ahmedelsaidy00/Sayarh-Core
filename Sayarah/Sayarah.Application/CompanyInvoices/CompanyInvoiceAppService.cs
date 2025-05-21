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
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.CompanyInvoices;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.CompanyInvoices;

[AbpAuthorize]
public class CompanyInvoiceAppService : AsyncCrudAppService<CompanyInvoice, CompanyInvoiceDto, long, GetAllCompanyInvoices, CreateCompanyInvoiceDto, UpdateCompanyInvoiceDto>, ICompanyInvoiceAppService
{
    private readonly IRepository<CompanyInvoice, long> _companyInvoiceRepository;
    private readonly IRepository<CompanyInvoiceTransaction, long> _companyInvoiceTransactionRepository;
    private ICommonAppService _commonAppService { get; set; }
    private readonly IStoredProcedureAppService _storedProcedureAppService;
    public CompanyInvoiceAppService(IRepository<CompanyInvoice, long> repository,
        IRepository<CompanyInvoiceTransaction, long> companyInvoiceTransactionRepository,
        ICommonAppService commonAppService,
        IStoredProcedureAppService storedProcedureAppService)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _companyInvoiceRepository = repository;
        _companyInvoiceTransactionRepository = companyInvoiceTransactionRepository;
        _commonAppService = commonAppService;
        _storedProcedureAppService = storedProcedureAppService;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<CompanyInvoiceDto>> GetPaged(GetCompanyInvoicesInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[i]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {
                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[0]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }



                int count = await _companyInvoiceRepository.CountAsync();
                var query = _companyInvoiceRepository.GetAll()
                    .Include(a => a.Company)
                    .GroupBy(a => new { a.Month  , a.Year , a.CompanyId }).Select(a => a.FirstOrDefault());

                count = query.Count();

                query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyId == input.CompanyId);

                query = query.FilterDataTable(input);

                query = query.WhereIf(input.Id.HasValue, m => m.Id == input.Id);
                 
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Month), at => (at.Month + "/" + at.Year).Contains(input.Month));

                query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);

                query = query.WhereIf(input.DiscountFrom.HasValue, at => at.Discount >= input.DiscountFrom);
                query = query.WhereIf(input.DiscountTo.HasValue, at => at.Discount <= input.DiscountTo);

                query = query.WhereIf(input.TaxesFrom.HasValue, at => at.Taxes >= input.TaxesFrom);
                query = query.WhereIf(input.TaxesTo.HasValue, at => at.Taxes <= input.TaxesTo);

                query = query.WhereIf(input.NetFrom.HasValue, at => at.Net >= input.NetFrom);
                query = query.WhereIf(input.NetTo.HasValue, at => at.Net <= input.NetTo);



                if (input.PeriodFrom.HasValue || input.PeriodTo.HasValue)
                {
                    DateTime periodFrom = new DateTime();
                    DateTime periodTo = new DateTime();

                    if (input.PeriodTo.HasValue)
                    {
                        periodTo = input.PeriodTo.Value.Date.AddSeconds(86399).AddDays(1);
                    }
                    query = query.WhereIf(input.PeriodFrom.HasValue, at => at.PeriodFrom >= periodFrom);
                    query = query.WhereIf(input.PeriodTo.HasValue, at => at.PeriodTo <= periodTo);
                }


                int filteredCount = await query.CountAsync();
                var companyInvoices =
                      await query
                    .Include(a => a.Company)
                       .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start)
                        .Take(input.length)
                          .ToListAsync();
                 

                return new DataTableOutputDto<CompanyInvoiceDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<CompanyInvoiceDto>>(companyInvoices)
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }
    [AbpAuthorize]
    public async Task<DataTableOutputDto<CompanyInvoiceDto>> GetPagedMainProvides(GetCompanyInvoicesInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[i]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                //if (centersCount > 0)
                                //    throw new UserFriendlyException(L("Pages.CompanyInvoices.Error.HasCenters"));

                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[0]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                int count = await _companyInvoiceRepository.CountAsync();
                var query = _companyInvoiceRepository.GetAll()
                    .Include(a => a.Company)
                    .GroupBy(a => new { a.Month, a.Year, a.MainProviderId }).Select(a => a.FirstOrDefault());

                count = query.Count();

                query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyId == input.CompanyId);

                query = query.FilterDataTable(input);

                query = query.WhereIf(input.Id.HasValue, m => m.Id == input.Id);

                query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Month), at => (at.Month + "/" + at.Year).Contains(input.Month));

                query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);

                query = query.WhereIf(input.DiscountFrom.HasValue, at => at.Discount >= input.DiscountFrom);
                query = query.WhereIf(input.DiscountTo.HasValue, at => at.Discount <= input.DiscountTo);

                query = query.WhereIf(input.TaxesFrom.HasValue, at => at.Taxes >= input.TaxesFrom);
                query = query.WhereIf(input.TaxesTo.HasValue, at => at.Taxes <= input.TaxesTo);

                query = query.WhereIf(input.NetFrom.HasValue, at => at.Net >= input.NetFrom);
                query = query.WhereIf(input.NetTo.HasValue, at => at.Net <= input.NetTo);



                if (input.PeriodFrom.HasValue || input.PeriodTo.HasValue)
                {
                    DateTime periodFrom = new DateTime();
                    DateTime periodTo = new DateTime();

                    if (input.PeriodTo.HasValue)
                    {
                        periodTo = input.PeriodTo.Value.Date.AddSeconds(86399).AddDays(1);
                    }
                    query = query.WhereIf(input.PeriodFrom.HasValue, at => at.PeriodFrom >= periodFrom);
                    query = query.WhereIf(input.PeriodTo.HasValue, at => at.PeriodTo <= periodTo);
                }


                int filteredCount = await query.CountAsync();
                var companyInvoices =
                      await query
                    .Include(a => a.Company)
                       .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start)
                        .Take(input.length)
                          .ToListAsync();


                return new DataTableOutputDto<CompanyInvoiceDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<CompanyInvoiceDto>>(companyInvoices)
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }
    [AbpAuthorize]
    public async Task<DataTableOutputDto<CompanyInvoiceDto>> GetPagedAdmins(GetCompanyInvoicesInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[i]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                //if (centersCount > 0)
                                //    throw new UserFriendlyException(L("Pages.CompanyInvoices.Error.HasCenters"));

                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int companyInvoiceId = Convert.ToInt32(input.ids[0]);
                        CompanyInvoice companyInvoice = await _companyInvoiceRepository.GetAsync(companyInvoiceId);
                        if (companyInvoice != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await _companyInvoiceRepository.DeleteAsync(companyInvoice);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
                if (input.Month == null && input.CompanyId == null && input.MainProviderId == null)
                {
                    return new DataTableOutputDto<CompanyInvoiceDto> { };
                }

                int count = await _companyInvoiceRepository.CountAsync();
                var query = _companyInvoiceRepository.GetAll()
                    .Include(a => a.Company)
                    .WhereIf(input.CompanyId.HasValue,a=> a.CompanyId == input.CompanyId)
                    .WhereIf(input.MainProviderId.HasValue,a=> a.MainProviderId == input.MainProviderId)
                    .GroupBy(a => new { a.Month, a.Year}).Select(a => a.FirstOrDefault());

                count = query.Count();

                query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyId == input.CompanyId);

                query = query.FilterDataTable(input);

                query = query.WhereIf(input.Id.HasValue, m => m.Id == input.Id);

                query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Month), at => (at.Month + "/" + at.Year).Contains(input.Month));

                query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);

                query = query.WhereIf(input.DiscountFrom.HasValue, at => at.Discount >= input.DiscountFrom);
                query = query.WhereIf(input.DiscountTo.HasValue, at => at.Discount <= input.DiscountTo);

                query = query.WhereIf(input.TaxesFrom.HasValue, at => at.Taxes >= input.TaxesFrom);
                query = query.WhereIf(input.TaxesTo.HasValue, at => at.Taxes <= input.TaxesTo);

                query = query.WhereIf(input.NetFrom.HasValue, at => at.Net >= input.NetFrom);
                query = query.WhereIf(input.NetTo.HasValue, at => at.Net <= input.NetTo);



                if (input.PeriodFrom.HasValue || input.PeriodTo.HasValue)
                {
                    DateTime periodFrom = new DateTime();
                    DateTime periodTo = new DateTime();

                    if (input.PeriodTo.HasValue)
                    {
                        periodTo = input.PeriodTo.Value.Date.AddSeconds(86399).AddDays(1);
                    }
                    query = query.WhereIf(input.PeriodFrom.HasValue, at => at.PeriodFrom >= periodFrom);
                    query = query.WhereIf(input.PeriodTo.HasValue, at => at.PeriodTo <= periodTo);
                }


                int filteredCount = await query.CountAsync();
                var companyInvoices =
                      await query
                    .Include(a => a.Company)
                       .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start)
                        .Take(input.length)
                          .ToListAsync();


                return new DataTableOutputDto<CompanyInvoiceDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<CompanyInvoiceDto>>(companyInvoices)
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }
    public override async Task<CompanyInvoiceDto> GetAsync(EntityDto<long> input)
    {
        var companyInvoice = _companyInvoiceRepository.FirstOrDefault(x => x.Id == input.Id);

        return await Task.FromResult(ObjectMapper.Map<CompanyInvoiceDto>(companyInvoice));
    }
    public async Task<CompanyInvoiceDto> GetInVoiceDetails(GetAllCompanyInvoices input)
    {
        try
        {

            var query = _companyInvoiceRepository
                .GetAll()
                .Include(a => a.Company)
                .Include(a => a.CompanyInvoiceTransactions.Select(aa => aa.Veichle.Branch.Company)).AsQueryable();


            query = query.WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId);
            var companyInvoice = await query.FirstOrDefaultAsync(a => a.Id == input.Id);

            var mappedCompanyInvoice = ObjectMapper.Map<CompanyInvoiceDto>(companyInvoice);

            //if (mappedCompanyInvoice != null && mappedCompanyInvoice.CompanyInvoiceTransactions != null)
            //{
            //    mappedCompanyInvoice.BranchCompanyInvoiceDetails = mappedCompanyInvoice.CompanyInvoiceTransactions.GroupBy(a => a.Veichle.BranchId)
            //        .Select(a => new BranchCompanyInvoiceDetailDto
            //        {
            //            Price = a.ToList().Sum(aa => aa.Price),
            //            BranchName = a.ToList().FirstOrDefault().Veichle.Branch.Name,
            //            CompanyId = a.ToList().FirstOrDefault().Veichle.Branch.CompanyId,
            //            CompanyName = a.ToList().FirstOrDefault().Veichle.Branch.Company.Name,
            //        }).ToList();
            //}

            return mappedCompanyInvoice;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    [AbpAuthorize]
    public override async Task<CompanyInvoiceDto> CreateAsync(CreateCompanyInvoiceDto input)
    {
        try
        {
            //int existingCount = await _companyInvoiceRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn));
            //if (existingCount > 0)
            //    throw new UserFriendlyException(L("Pages.CompanyInvoices.Error.AlreadyExist"));

            //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyInvoices", CodeField = "Code" });

            var companyInvoice = ObjectMapper.Map<CompanyInvoice>(input);
            await _companyInvoiceRepository.InsertAsync(companyInvoice);
            return MapToEntityDto(companyInvoice);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    [AbpAuthorize]
    public override async Task<CompanyInvoiceDto> UpdateAsync(UpdateCompanyInvoiceDto input)
    {
        try
        {
            ////Check if CompanyInvoice exists
            //int existingCount = await _companyInvoiceRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
            //if (existingCount > 0)
            //    throw new UserFriendlyException(L("Pages.CompanyInvoices.Error.AlreadyExist"));
            var companyInvoice = await _companyInvoiceRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, companyInvoice);
            await _companyInvoiceRepository.UpdateAsync(companyInvoice);
            return MapToEntityDto(companyInvoice);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public override async Task<PagedResultDto<CompanyInvoiceDto>> GetAllAsync(GetAllCompanyInvoices input)
    {
        try
        {
            var query = _companyInvoiceRepository.GetAll()
                .Include(a => a.Company).AsQueryable();

            //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
            query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyId == input.CompanyId);

            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }
            var companyInvoices = await query
                .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToListAsync();
            return new PagedResultDto<CompanyInvoiceDto>(
               query.Count(), ObjectMapper.Map<List<CompanyInvoiceDto>>(companyInvoices)
                );
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    [AbpAuthorize]
    public async Task<CompanyInvoiceDto> CreateCompanyInvoice(CreateCompanyInvoiceInput input)
    {
        try
        {
            input.CompanyInvoice.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyInvoices", CodeField = "Code" });

            var companyInvoice = ObjectMapper.Map<CompanyInvoice>(input.CompanyInvoice);
            await _companyInvoiceRepository.InsertAsync(companyInvoice);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            // create companyInvoice trans

            if (input.Transactions != null)
            {
                int i = 1;
                foreach (var transaction in input.Transactions)
                {
                    await _companyInvoiceTransactionRepository.InsertAsync(new CompanyInvoiceTransaction
                    {
                        CompanyInvoiceId = companyInvoice.Id,
                        TransId = transaction.Id,
                        TransType = transaction.TransactionType,
                        VeichleId = transaction.VeichleId,
                        Price = transaction.Price ?? 0,
                        FuelPrice = transaction.FuelPrice ?? 0,
                        TransFuelType = transaction.TransFuelType,
                        Quantity = transaction.Quantity ?? 0
                    });
                    i++;
                }

                await UnitOfWorkManager.Current.SaveChangesAsync();
            }

            await _storedProcedureAppService.UpdateCompanyInvoiceCodeInFuelTransOuts(new UpdateInvoiceCodeInFuelTransOutsInput
            {
                InvoiceCode = companyInvoice.Code,
                Ids = input.TransactionIds
            });

            // send notification to admin 
            //await SendNotificationToEmployees(new GetAllCompanyInvoices
            //{
            //    CompanyId = companyInvoice.CompanyId, 
            //    CompanyInvoiceId = companyInvoice.Id
            //});

            return MapToEntityDto(companyInvoice);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
}
