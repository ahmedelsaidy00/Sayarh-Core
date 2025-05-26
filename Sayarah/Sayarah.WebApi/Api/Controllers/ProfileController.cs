using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sayarah.Api.Models;
using Sayarah.Application.Drivers;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Providers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Security;
using System.Globalization;
using static Sayarah.SayarahConsts;

namespace Sayarah.Api.Controllers
{
    [ApiController]

    public class ProfileController : AbpController
    {
        public AppSession AppSession { get; set; }

        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IDriverAppService _driverAppService;
        private readonly IWorkerAppService _workerAppService;
        public UploadWebPController uploadController { get; set; }
        private readonly IHttpContextAccessor _HttpContextAccessor;

        public ProfileController(
            IHttpContextAccessor httpContextAccessor,
                   AbpNotificationHelper abpNotificationHelper,
                  IDriverAppService driverAppService,
                  IWorkerAppService workerAppService
                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            this._HttpContextAccessor = httpContextAccessor;
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
                var env = HttpContext?.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                var contentRootPath = env?.ContentRootPath ?? Directory.GetCurrentDirectory();

                switch (storageLocation)
                {
                    case 1:
                        return Path.Combine(contentRootPath, "Files/Users");
                    case 2:
                        return Path.Combine(contentRootPath, "files/SitePages/About");
                    case 3:
                        return Path.Combine(contentRootPath, "files/SitePages/Index");
                    case 4:
                        return Path.Combine(contentRootPath, "files/Companies");
                    case 5:
                        return Path.Combine(contentRootPath, "files/Veichles");
                    case 6:
                        return Path.Combine(contentRootPath, "files/Drivers");
                    case 7:
                        return Path.Combine(contentRootPath, "files/Providers");
                    case 8:
                        return Path.Combine(contentRootPath, "files/Workers");
                    case 9:
                        return Path.Combine(contentRootPath, "files/FuelTransOut");
                    default:
                        return Path.Combine(contentRootPath, "Files");
                }
            }
        }
        CultureInfo new_lang = new CultureInfo("ar");



        #region DriverProfile

        //////////////////////////////////////////////Profile/////////////////////////////////////////////////

        [HttpPost]
        [Language("Lang")]
        public async Task<IActionResult> GetDriverProfile()
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
        public async Task<IActionResult> UpdateDriverProfile(UpdateDriverProfileInput input)
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
        public async Task<IActionResult> UpdateDriverPhoto()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();

                var httpContext = HttpContext;
                if (httpContext == null || httpContext.Request == null || httpContext.Request.Form == null)
                    return BadRequest(new UpdateProfilePicOutput { Message = "No file uploaded", Success = false });

                var files = httpContext.Request.Form.Files;
                if (files == null || files.Count == 0)
                    return BadRequest(new UpdateProfilePicOutput { Message = "No file uploaded", Success = false });

                var itemFile = files[0];
                if (itemFile == null || string.IsNullOrEmpty(itemFile.FileName))
                    return BadRequest(new UpdateProfilePicOutput { Message = "Invalid file", Success = false });

                var input = new UpdateDriverDto();

                var newUploadFilesDto = new NewUploadFilesDto
                {
                    StorageLocation = 6,
                    AllowedTypes = 0,
                    Sizes = "600&600",
                    UploadStyle = NewUploadStyle.CopyOnly
                };

                // UploadPhotoWebP is async in .NET Core

                var uniqueFileName = await uploadController.UploadPhotoWebP(itemFile, newUploadFilesDto);
                input.Avatar = uniqueFileName.Value;

                var result = await _driverAppService.UpdateDriverPhotoAsync(input);

                string avatarPath;
                if (!string.IsNullOrEmpty(input.Avatar) && Utilities.CheckExistImage(6, "600x600_" + input.Avatar))
                    avatarPath = FilesPath.Drivers.ServerImagePath + "600x600_" + input.Avatar;
                else
                    avatarPath = FilesPath.Drivers.DefaultImagePath;

                return Ok(new UpdateProfilePicOutput
                {
                    Message = L("MobileApi.Messages.ProfileImageUpdated"),
                    Success = true,
                    AvatarPath = avatarPath
                });
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
        public async Task<IActionResult> GetWorkerProfile()
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
        public async Task<IActionResult> UpdateWorkerProfile(UpdateWorkerProfileInput input)
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
        public async Task<IActionResult> UpdateWorkerPhoto()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return Unauthorized();

                var httpContext = HttpContext;
                if (httpContext == null || httpContext.Request == null || httpContext.Request.Form == null)
                    return BadRequest(new UpdateWorkerProfileOutput { Message = "No file uploaded", Success = false });

                var files = httpContext.Request.Form.Files;
                if (files == null || files.Count == 0)
                    return BadRequest(new UpdateWorkerProfileOutput { Message = "No file uploaded", Success = false });

                var itemFile = files[0];
                if (itemFile == null || string.IsNullOrEmpty(itemFile.FileName))
                    return BadRequest(new UpdateWorkerProfileOutput { Message = "Invalid file", Success = false });

                var input = new UpdateWorkerDto
                {
                    UserId = AbpSession.UserId.Value
                };

                var newUploadFilesDto = new NewUploadFilesDto
                {
                    StorageLocation = 8,
                    AllowedTypes = 0,
                    Sizes = "400&400",
                    UploadStyle = NewUploadStyle.CopyOnly
                };

                var uniqueFileName = await uploadController.UploadPhotoWebP(itemFile, newUploadFilesDto);
                input.Avatar = uniqueFileName.Value;

                var result = await _workerAppService.UpdateWorkerPhotoAsync(input);

                return Ok(new UpdateWorkerProfileOutput
                {
                    Message = L("MobileApi.Messages.ProfileImageUpdated"),
                    Success = true,
                    Worker = ObjectMapper.Map<ApiWorkerDto>(result)
                });
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
                var httpContext = _HttpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.Headers.Append("UnReadCount", result.UnReadCount.ToString());
                }
            }
        }

    }
}