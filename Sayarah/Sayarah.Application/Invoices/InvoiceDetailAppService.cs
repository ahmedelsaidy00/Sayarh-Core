using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Invoices.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Invoices;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Sayarah.Application.Invoices
{
    public class InvoiceDetailAppService : AsyncCrudAppService<InvoiceDetail, InvoiceDetailDto, long, GetAllInvoiceDetails, CreateInvoiceDetailDto, UpdateInvoiceDetailDto>, IInvoiceDetailAppService
    {
        private readonly IRepository<InvoiceDetail, long> _invoiceDetailRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        public ICommonAppService _commonAppService { get; set; }
        public InvoiceDetailAppService(IRepository<InvoiceDetail, long> repository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _invoiceDetailRepository = repository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<InvoiceDetailDto>> GetPaged(GetInvoiceDetailsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int invoiceDetailId = Convert.ToInt32(input.ids[i]);
                            InvoiceDetail invoiceDetail = await _invoiceDetailRepository.GetAsync(invoiceDetailId);
                            if (invoiceDetail != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                   
                                   

                                    //if (centersCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.InvoiceDetails.Error.HasCenters"));

                                    await _invoiceDetailRepository.DeleteAsync(invoiceDetail);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int invoiceDetailId = Convert.ToInt32(input.ids[0]);
                            InvoiceDetail invoiceDetail = await _invoiceDetailRepository.GetAsync(invoiceDetailId);
                            if (invoiceDetail != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    
                                    await _invoiceDetailRepository.DeleteAsync(invoiceDetail);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _invoiceDetailRepository.CountAsync();
                    var query = _invoiceDetailRepository.GetAll()
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

                    int filteredCount = await query.CountAsync();
                    var invoiceDetails =
                          await query/*.Include(q => q.CreatorUser)*/
                           .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                              .ToListAsync();
                    return new DataTableOutputDto<InvoiceDetailDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<InvoiceDetailDto>>(invoiceDetails)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<InvoiceDetailDto> GetAsync(EntityDto<long> input)
        {
            var invoiceDetail = _invoiceDetailRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<InvoiceDetailDto>(invoiceDetail));
        }

        [AbpAuthorize]
        public override async Task<InvoiceDetailDto> CreateAsync(CreateInvoiceDetailDto input)
        {
            try
            {
                //int existingCount = await _invoiceDetailRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.InvoiceDetails.Error.AlreadyExist"));

                //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "InvoiceDetails", CodeField = "Code" });

                var invoiceDetail = ObjectMapper.Map<InvoiceDetail>(input);
                await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                return MapToEntityDto(invoiceDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<InvoiceDetailDto> UpdateAsync(UpdateInvoiceDetailDto input)
        {
            try
            {
                ////Check if InvoiceDetail exists
                //int existingCount = await _invoiceDetailRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.InvoiceDetails.Error.AlreadyExist"));
                var invoiceDetail = await _invoiceDetailRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, invoiceDetail);
                await _invoiceDetailRepository.UpdateAsync(invoiceDetail);
                return MapToEntityDto(invoiceDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<InvoiceDetailDto>> GetAllAsync(GetAllInvoiceDetails input)
        {
            try
            {
                var query = _invoiceDetailRepository.GetAll()
                    .Include(a => a.Invoice.Journal)
                    .Include(a => a.Invoice.Provider.MainProvider)
                    .Include(a => a.Invoice.MainProvider).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.Invoice.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.Invoice.MainProviderId == input.MainProviderId);

                query = query.WhereIf(input.JournalId.HasValue, m => m.Invoice.JournalId == input.JournalId);
                query = query.WhereIf(input.InvoiceId.HasValue, m => m.InvoiceId  == input.InvoiceId);

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var invoiceDetails = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<InvoiceDetailDto>(
                   query.Count(), ObjectMapper.Map<List<InvoiceDetailDto>>(invoiceDetails)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
