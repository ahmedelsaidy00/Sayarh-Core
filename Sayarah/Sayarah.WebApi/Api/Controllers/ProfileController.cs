using Abp.Application.Services.Dto;
using Abp.WebApi.Controllers;
using Sayarah.Api.Models;
using Sayarah.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Sayarah.Security;
using Sayarah.Drivers;
using Sayarah.Drivers.Dto;
using Sayarah.Workers.Dto;
using Sayarah.Workers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Api.Controllers
{
    public class ProfileController : AbpApiController
    {
        public AppSession AppSession { get; set; }
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IDriverAppService _driverAppService;
        private readonly IWorkerAppService _workerAppService;
        public UploadWebPController uploadController { get; set; }

        public ProfileController(
                   AbpNotificationHelper abpNotificationHelper,
                  IDriverAppService driverAppService,
                  IWorkerAppService workerAppService
                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _abpNotificationHelper = abpNotificationHelper;
            _driverAppService = driverAppService;
            _workerAppService = workerAppService;

        }

        private int storageLocation = 0;
        //  readonly string storageRoot = Path.Combine(HttpRuntime.AppDomainAppPath, "Files/Users");
        private string StorageRoot
        {
            get
            {
                switch (storageLocation)
                {
                    case 1:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "Files/Users");
                    case 2:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/SitePages/About");
                    case 3:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/SitePages/Index");
                    case 4:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/Companies");
                    case 5:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/Veichles");
                    case 6:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/Drivers");
                    case 7:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/Providers");
                    case 8:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/Workers");
                    case 9:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/FuelTransOut");
                    default:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "Files");
                }
            }
        }
        CultureInfo new_lang = new CultureInfo("ar");



        #region DriverProfile

        //////////////////////////////////////////////Profile/////////////////////////////////////////////////

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetDriverProfile()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();

                var result = await _driverAppService.GetByUserId(new EntityDto<long> { Id = AbpSession.UserId.Value });
                var driver = ObjectMapper.Map<ApiDriverDto>(result);
                driver.Name = result.User.UserName;


                return Ok(new GetDriverProfileOutput { Driver = driver, Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new GetDriverProfileOutput { Message = ex.Message });
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> UpdateDriverProfile(UpdateDriverProfileInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();


                var result = await _driverAppService.UpdateMobile(input);
                await CurrentUnitOfWork.SaveChangesAsync();

                var driver = ObjectMapper.Map<ApiDriverDto>(result);

                driver.Name = input.Name;

                return Ok(new UpdateDriverProfileOutput { Driver = driver, Success = true, Message = L("MobileApi.Messages.UpdatedSuccessfully") });
            }
            catch (Exception ex)
            {
                return Ok(new UpdateDriverProfileOutput { Message = ex.Message });
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> UpdateDriverPhoto()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();


                UpdateDriverDto input = new UpdateDriverDto();


                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                var files = cntx.Request.Files;
                if (files != null && files.Count > 0)
                {
                    var itemFile = cntx.Request.Files[0];
                    if (!string.IsNullOrEmpty(itemFile.FileName))
                    {

                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 6;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.Sizes = "600&600";
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.CopyOnly;

                        string _uniqueFileName = uploadController.UploadPhotoWebP(itemFile, _newUploadFilesDto);

                        input.Avatar = _uniqueFileName;
                    }
                }


                var result = await _driverAppService.UpdateDriverPhotoAsync(input);

                string _avatarPath = string.Empty;

                if (!string.IsNullOrEmpty(input.Avatar) && Utilities.CheckExistImage(6, "600x600_" + input.Avatar))
                    _avatarPath = FilesPath.Drivers.ServerImagePath + "600x600_" + input.Avatar;
                else
                    _avatarPath = FilesPath.Drivers.DefaultImagePath;



                return Ok(new UpdateProfilePicOutput { Message = L("MobileApi.Messages.ProfileImageUpdated"), Success = true, AvatarPath = _avatarPath });

            }
            catch (Exception ex)
            {
                return Ok(new UpdateProfilePicOutput { Message = ex.Message, Success = false });
            }
        }

        #endregion

        #region WorkerProfile

        //////////////////////////////////////////////WorkerProfile/////////////////////////////////////////////////

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetWorkerProfile()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();

                var result = await _workerAppService.GetByUserId(new EntityDto<long> { Id = AbpSession.UserId.Value });
                var worker = ObjectMapper.Map<ApiWorkerDto>(result);
                worker.Name = result.User.UserName;


                return Ok(new GetWorkerProfileOutput { Worker = worker, Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new GetWorkerProfileOutput { Message = ex.Message });
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> UpdateWorkerProfile(UpdateWorkerProfileInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();


                var result = await _workerAppService.UpdateMobile(input);
                await CurrentUnitOfWork.SaveChangesAsync();

                var worker = ObjectMapper.Map<ApiWorkerDto>(result);

                worker.Name = input.Name;

                return Ok(new UpdateWorkerProfileOutput { Worker = worker, Success = true, Message = L("MobileApi.Messages.UpdatedSuccessfully") });
            }
            catch (Exception ex)
            {
                return Ok(new UpdateWorkerProfileOutput { Message = ex.Message });
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> UpdateWorkerPhoto()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();


                UpdateWorkerDto input = new UpdateWorkerDto();


                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                var files = cntx.Request.Files;
                if (files != null && files.Count > 0)
                {
                    var itemFile = cntx.Request.Files[0];
                    if (!string.IsNullOrEmpty(itemFile.FileName))
                    {

                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 8;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.Sizes = "400&400";
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.CopyOnly;

                        string _uniqueFileName = uploadController.UploadPhotoWebP(itemFile, _newUploadFilesDto);

                        input.Avatar = _uniqueFileName;
                    }
                }


                var result = await _workerAppService.UpdateWorkerPhotoAsync(input);

                return Ok(new UpdateWorkerProfileOutput { Message = L("MobileApi.Messages.ProfileImageUpdated"), Success = true, Worker = ObjectMapper.Map<ApiWorkerDto>(result) });

            }
            catch (Exception ex)
            {
                return Ok(new UpdateWorkerProfileOutput { Message = ex.Message, Success = false });
            }
        }

        #endregion
        public async Task AddToHeader(EntityDto<long> input)
        {

            if (input.Id > 0)
            {
                var result = await _abpNotificationHelper.GetNotificationsCount(new GetNotificationsInput { UserId = input.Id });
                HttpContext.Current.Response.Headers.Add("UnReadCount", result.UnReadCount.ToString());
            }

        }

    }
}