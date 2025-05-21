using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Zatca;
using Sayarah.Application.Subscriptions.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Configuration;
using Sayarah.Core.Helpers;
using Sayarah.Core.Repositories;
using Sayarah.Packages;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Subscriptions;

public class SubscriptionTransactionAppService : AsyncCrudAppService<SubscriptionTransaction, SubscriptionTransactionDto, long, GetAllSubscriptionTransactions, CreateSubscriptionTransactionDto, UpdateSubscriptionTransactionDto>, ISubscriptionTransactionAppService
{
    private readonly ICommonRepository _commonRepository;
    private readonly IRepository<User, long> _userRepository;
    public SubscriptionTransactionAppService(
        IRepository<SubscriptionTransaction, long> repository,
        ICommonRepository commonRepository, 
        IRepository<User, long> userRepository)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _commonRepository = commonRepository;
        _userRepository = userRepository;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<SubscriptionTransactionDto>> GetPaged(GetSubscriptionTransactionsInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {


                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int subscriptionTransactionId = Convert.ToInt32(input.ids[i]);
                        SubscriptionTransaction subscriptionTransaction = await Repository.GetAll()
                            .FirstOrDefaultAsync(m => m.Id == subscriptionTransactionId);
                        if (subscriptionTransaction != null)
                        {
                            if (input.action == "Delete")//Delete
                            {
                                await Repository.DeleteAsync(subscriptionTransaction);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int subscriptionTransactionId = Convert.ToInt32(input.ids[0]);

                        SubscriptionTransaction subscriptionTransaction = await Repository.GetAll()
                            .FirstOrDefaultAsync(m => m.Id == subscriptionTransactionId);
                        if (subscriptionTransaction != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await Repository.DeleteAsync(subscriptionTransaction);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
                int count = await Repository.CountAsync();
                var query = Repository.GetAll()
                    .Include(x => x.Company)
                    .Include(x => x.Bank)
                    .Include(x => x.Package).AsQueryable();
                count = query.Count();
                query = query.FilterDataTable((DataTableInputDto)input);
                query = query.WhereIf(input.PackageId.HasValue, a => a.PackageId == input.PackageId);
                query = query.WhereIf(input.Price.HasValue, a => a.Price == input.Price);
                query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), a => a.Company.NameAr.Contains(input.UserName) || a.Company.NameEn.Contains(input.UserName));
                query = query.WhereIf(input.EndDateFrom.HasValue, a => a.EndDate >= input.EndDateFrom);
                query = query.WhereIf(input.IsSpecialId.HasValue, a => a.Id == input.IsSpecialId);
                query = query.WhereIf(!string.IsNullOrEmpty(input.PackageName), a => a.Package.NameAr.Contains(input.PackageName) || a.Package.NameEn.Contains(input.PackageName));
                query = query.WhereIf(input.PayMethod.HasValue, a => a.PayMethod == input.PayMethod);
                query = query.WhereIf(input.PackageType.HasValue, a => a.PackageType == input.PackageType);
                query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                if (input.EndDateTo.HasValue)
                {
                    input.EndDateTo = input.EndDateTo.Value.Add(TimeSpan.FromSeconds(86399));
                    query = query.WhereIf(input.EndDateTo.HasValue, a => a.EndDate <= input.EndDateTo);

                }
                int filteredCount = await query.CountAsync();


                var subscriptionTransactions = await query.Include(x => x.CreatorUser)
                     .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length)
                     .ToListAsync();

                return new DataTableOutputDto<SubscriptionTransactionDto>
                {
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = count,
                    aaData = ObjectMapper.Map<List<SubscriptionTransactionDto>>(subscriptionTransactions)
                };
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public override async Task<SubscriptionTransactionDto> GetAsync(EntityDto<long> input)
    {
        var subscriptionTransaction = await Repository.GetAll()
            .Include(m => m.CreatorUser)
            .Include(m => m.Company)
            .Include(x => x.Bank)
            .Include(m => m.Package)
            .FirstOrDefaultAsync(x => x.Id == input.Id);
        var _subscriptionTransaction = ObjectMapper.Map<SubscriptionTransactionDto>(subscriptionTransaction);
        return _subscriptionTransaction;
    }
    public async Task<SubscriptionTransactionDto> PrintSubscriptionDetails(GetAllSubscriptionTransactions input)
    {
        try
        {

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {

                var companyName = await SettingManager.GetSettingValueAsync(AppSettingNames.CompanyName);
                var taxNumber = await SettingManager.GetSettingValueAsync(AppSettingNames.TaxNumber);

                var query = Repository.GetAll()
                                      .Include(a => a.Company)
                                      .Include(a => a.Package)
                                      .Include(a => a.Subscription).AsQueryable();

                query = query.WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId);

                var _subscription = await query.FirstOrDefaultAsync(a => a.Id == input.Id);


                var subscription = ObjectMapper.Map<SubscriptionTransactionDto>(_subscription);


                if (subscription != null)
                {


                    TLVCls tlv = new TLVCls(
                        companyName,
                        taxNumber, // tax number
                        subscription.CreationTime,
                        (double)subscription.NetPrice,
                        (double)subscription.TaxAmount);

                    var taxQRString = tlv.ToBase64();

                    //From here on, you can implement your platform-dependent byte[]-to-image code 
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(taxQRString, QRCodeGenerator.ECCLevel.M);

                    // Create base64-encoded QR code image
                    var qrCodeImage = new Base64QRCode(qrCodeData);
                    var qrCodeImageSrc = qrCodeImage.GetGraphic(20);

                    // Get QR code image source as string
                    var qrCodeImageSrcString = "data:image/png;base64," + qrCodeImageSrc;
                    subscription.QrCode = qrCodeImageSrcString;
                }
                return subscription;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    [AbpAuthorize]
    public async Task<SubscriptionTransactionDto> GetSubscriptionTransaction(GetAllSubscriptionTransactions input)
    {

        // get current user 
        var user = await _userRepository.GetAll().Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
        if (user == null)
            return null;

        long companyUserId = AbpSession.UserId.Value;
        if (user.UserType == UserTypes.Company)
            companyUserId = user.Id;

        else if (user.UserType == UserTypes.Employee && user.CompanyId.HasValue == true)
            companyUserId = user.Company.UserId.Value;
        else
            return null;


        var subscriptionTransaction = await Repository.GetAll()
            .Include(m => m.CreatorUser)
            .Include(m => m.Company)
            .Include(x => x.Bank)
            .Include(m => m.Package)
            .FirstOrDefaultAsync(x => x.Id == input.Id.Value && x.Company.UserId == companyUserId);
        var _subscriptionTransaction = ObjectMapper.Map<SubscriptionTransactionDto>(subscriptionTransaction);
        return _subscriptionTransaction;
    }
    public override async Task<SubscriptionTransactionDto> CreateAsync(CreateSubscriptionTransactionDto input)
    {
        try
        {
            //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "SubscriptionTransactions", CodeField = "Code" });
            // create company invoice
            InvoiceLatestCode code = await _commonRepository.GetAsync<InvoiceLatestCode>("SELECT * FROM CompanyInvoice_LatestCode");
            input.Code = code.LatestCode;
            var subscriptionTransaction = ObjectMapper.Map<SubscriptionTransaction>(input);
            await Repository.InsertAsync(subscriptionTransaction);
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapToEntityDto(subscriptionTransaction);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    [AbpAuthorize]
    public override async Task<SubscriptionTransactionDto> UpdateAsync(UpdateSubscriptionTransactionDto input)
    {
        try
        {
            var subscriptionTransactionDetails = await Repository.GetAsync(input.Id);
            subscriptionTransactionDetails.EndDate = input.EndDate.Value;
            await Repository.UpdateAsync(subscriptionTransactionDetails);
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapToEntityDto(subscriptionTransactionDetails);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public override async Task<PagedResultDto<SubscriptionTransactionDto>> GetAllAsync(GetAllSubscriptionTransactions input)
    {
        try
        {
            var query = Repository.GetAll();
            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }


            query = query.WhereIf(input.CreatorUserId.HasValue && input.CreatorUserId.Value > 0, m => m.CreatorUserId == input.CreatorUserId.Value);
            var subscriptionTransactions = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToListAsync();
            return new PagedResultDto<SubscriptionTransactionDto>(query.Count(), ObjectMapper.Map<List<SubscriptionTransactionDto>>(subscriptionTransactions));
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    public async Task<PagedResultDto<SubscriptionTransactionDto>> GetCompanySubscriptionTransactions(GetAllSubscriptionTransactions input)
    {
        try
        {

            // get current user 
            var user = await _userRepository.GetAll().Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            if (user == null)
                return null;

            long companyUserId = AbpSession.UserId.Value;
            if (user.UserType == UserTypes.Company)
                companyUserId = user.Id;

            else if (user.UserType == UserTypes.Employee && user.CompanyId.HasValue == true)
                companyUserId = user.Company.UserId.Value;
            else
                return null;


            var query = Repository.GetAll()
                .Include(a => a.Package)
                .Include(a => a.Company)
                .Include(a => a.Subscription)
                .Where(a => a.Company.UserId == companyUserId);

            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            query = query.WhereIf(input.CreatorUserId.HasValue && input.CreatorUserId.Value > 0, m => m.CreatorUserId == input.CreatorUserId.Value);
            var subscriptionTransactions = await query.OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<SubscriptionTransactionDto>(query.Count(), ObjectMapper.Map<List<SubscriptionTransactionDto>>(subscriptionTransactions));
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
