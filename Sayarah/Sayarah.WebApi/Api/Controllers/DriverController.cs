using Abp.AspNetCore.Mvc.Controllers;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Sayarah.Api.Controllers;
using Sayarah.Api.Models;
using Sayarah.Application.Drivers;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Providers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Transactions.FuelTransactions;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Veichles;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Drivers;
using Sayarah.Security;
using System.Globalization;

namespace Sayarah.WebApi.Api.Controllers
{
    [ApiController]

    public class DriverController : AbpController
    {
        public AppSession AppSession { get; set; }
        private readonly AbpNotificationHelper _abpNotificationHelper;
        public UploadWebPController uploadController { get; set; }
        public readonly IDriverVeichleAppService _driverVeichleAppService;
        public readonly IProviderAppService _providerAppService;
        public readonly IRepository<Driver, long> _driverRepository;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly IVeichleAppService _veichleAppService;
        private readonly IFuelTransOutAppService _fuelTransOutAppService;

        public DriverController(
        AbpNotificationHelper abpNotificationHelper,
        IDriverVeichleAppService driverVeichleAppService,
        IProviderAppService providerVeichleAppService,
        IRepository<Driver, long> driverRepository,
        IStoredProcedureAppService storedProcedureAppService,
        IVeichleAppService veichleAppService,
        IFuelTransOutAppService fuelTransOutAppService

                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _abpNotificationHelper = abpNotificationHelper;
            _driverVeichleAppService = driverVeichleAppService;
            _providerAppService = providerVeichleAppService;
            _driverRepository = driverRepository;
            _storedProcedureAppService = storedProcedureAppService;
            _veichleAppService = veichleAppService;
            _fuelTransOutAppService = fuelTransOutAppService;
        }

        CultureInfo new_lang = new CultureInfo("ar");

        [HttpPost]
        [Language("Lang")]
        public async Task<IActionResult> GetAllDriverVeichles(GetDriverVeichlesInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();

                input.DriverUserId = AbpSession.UserId.Value;
                var result = await _driverVeichleAppService.GetAllAsync(input);

                if (result != null && result.TotalCount > 0)
                    return Ok(new GetAllDriverVechilesOutput { Success = true, TotalCount = result.TotalCount, DriverVechiles = ObjectMapper.Map<List<SmallDriverVeichleDto>>(result.Items) });
                else
                    return Ok(new GetAllDriverVechilesOutput { Message = L("MobileApi.DriverVeichles.Messages.NoRecords"), DriverVechiles = new List<SmallDriverVeichleDto>() });


            }
            catch (Exception ex)
            {
                return Ok(new GetAllDriverVechilesOutput { Message = ex.Message, DriverVechiles = new List<SmallDriverVeichleDto>() });
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<GetProvidersOutput> GetProvidersByDistance(GetProvidersInputApi input)
        {
            try
            {
                input.VisibleInMap = true;

                var result = await _providerAppService.GetAllProvidersMobile(input);



                if (result != null && result.TotalCount > 0)
                {
                    var _mappedCenters = ObjectMapper.Map<List<ApiProviderDto>>(result.Items);
                    foreach (var item in _mappedCenters)
                    {

                        double distance = 0;
                        if (item.Latitude.HasValue && item.Longitude.HasValue && input.Latitude.HasValue && input.Longitude.HasValue)
                            distance = GeoCoordinateHepler.GetDistance(item.Latitude.Value,item.Longitude.Value, input.Latitude.Value, input.Longitude.Value);

                        item.Distance = distance;
                        if (!string.IsNullOrEmpty(item.FuelTypes))
                            item.FinalFuelTypes = item.FuelTypes.Split(';');

                        List<string> list = new List<string>();
                        if (!string.IsNullOrEmpty(item.Services))
                        {
                            list = item.Services.Split(';').ToList();
                        }

                        if (!string.IsNullOrEmpty(item.Services))
                            item.ServicesList = list.Select(x => !string.IsNullOrEmpty(Utilities.servicesDictionary[Convert.ToInt32(x)]) ? Utilities.servicesDictionary[Convert.ToInt32(x)] : x.ToString()).ToList();

                    }
                    return new GetProvidersOutput { Providers = _mappedCenters, TotalCount = result.TotalCount, Success = true };
                }
                else
                    return new GetProvidersOutput { Message = L("MobileApi.Messages.NoRecords") };
            }
            catch (Exception ex)
            {
                return new GetProvidersOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<GetProvidersOutput> GetProvidersByDistanceAndMainProvider(GetProvidersByMainProviderIdInputApi input)
        {
            try
            {
                input.VisibleInMap = true;
                if (!AbpSession.UserId.HasValue)
                    throw new Abp.Authorization.AbpAuthorizationException("Unauthorized access. User is not authenticated.");

                input.UserId = AbpSession.UserId.Value;
              
                var result = await _providerAppService.GetAllProvidersByMainProviderIdMobile(input);



                if (result != null && result.TotalCount > 0)
                {
                    var _mappedCenters = ObjectMapper.Map<List<ApiProviderDto>>(result.Items);
                    foreach (var item in _mappedCenters)
                    {

                        double distance = 0;
                        if (item.Latitude.HasValue && item.Longitude.HasValue && input.Latitude.HasValue && input.Longitude.HasValue)
                            distance = GeoCoordinateHepler.GetDistance(item.Latitude.Value, item.Longitude.Value, input.Latitude.Value, input.Longitude.Value);

                        item.Distance = distance;
                        if (!string.IsNullOrEmpty(item.FuelTypes))
                            item.FinalFuelTypes = item.FuelTypes.Split(';');

                        List<string> list = new List<string>();
                        if (!string.IsNullOrEmpty(item.Services))
                        {
                            list = item.Services.Split(';').ToList();
                        }

                        if (!string.IsNullOrEmpty(item.Services))
                            item.ServicesList = list.Select(x => !string.IsNullOrEmpty(Utilities.servicesDictionary[Convert.ToInt32(x)]) ? Utilities.servicesDictionary[Convert.ToInt32(x)] : x.ToString()).ToList();

                    }
                    return new GetProvidersOutput { Providers = _mappedCenters, TotalCount = result.TotalCount, Success = true };
                }
                else
                    return new GetProvidersOutput { Message = L("MobileApi.Messages.NoRecords") };
            }
            catch (Exception ex)
            {
                return new GetProvidersOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<GetTransactionsOutput> GetDriverTransactions(GetTransactionsReportInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetTransactionsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var driver = await _driverRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (driver == null)
                    return new GetTransactionsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                input.DriverId = driver.Id;
                var transactions = await _storedProcedureAppService.GetTransactionsReport(input);

                if (input.SkipCount.HasValue && input.MaxResultCount.HasValue)
                    transactions.Transactions = transactions.Transactions.Skip(input.SkipCount.Value).Take(input.MaxResultCount.Value).ToList();


                if (transactions != null)
                {
                    return new GetTransactionsOutput
                    {
                        Success = true,
                        Transactions = ObjectMapper.Map<List<TransactionDto>>(transactions.Transactions)
                    };
                }

                return new GetTransactionsOutput { Transactions = new List<TransactionDto>() };
            }
            catch (Exception ex)
            {
                return new GetTransactionsOutput { Message = ex.Message };
            }
        }




        [HttpPost]
        [Language("Lang")]
        public async Task<GetVeichleDetailsOutput> GetVeichleDetails(GetVeichlesInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var result = await _veichleAppService.GetVeichle(new GetVeichlesInput { Id = input.Id.Value });
                if (result != null)
                {
                    decimal fuelPrice = 0;
                    if (result.Veichle.FuelType.HasValue == true)
                        fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput { FuelType = result.Veichle.FuelType.Value, VeichleId = result.Veichle.Id });

                    return new GetVeichleDetailsOutput
                    {
                        Success = true,
                        Veichle = ObjectMapper.Map<ApiVeichleDto>(result.Veichle),
                        FuelPrice = fuelPrice,
                        VeichleBalanceInSar = result.VeichleBalanceInSar,
                        VeichleBalanceInLitre = result.VeichleBalanceInLitre,
                        GroupType = result.GroupType,
                        BranchWalletAmount = result.Veichle.Branch.WalletAmount,

                        BranchFuelAmount = result.Veichle.Branch.FuelAmount,
                        BranchCleanAmount = result.Veichle.Branch.CleanAmount,
                        BranchMaintainAmount = result.Veichle.Branch.MaintainAmount,
                        CounterPicIsRequired = result.CounterPicIsRequired

                    };
                }
                else
                {
                    // here => mobile will call the update vechile service
                    return new GetVeichleDetailsOutput
                    {
                        Success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetVeichleDetailsOutput { Message = ex.Message };
            }
        }
    }
}