using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sayarah.Api.Models;
using Sayarah.Application.Contact;
using Sayarah.Application.Contact.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.SitePages;
using Sayarah.Application.SitePages.Dto;
using Sayarah.Application.Users;
using Sayarah.Contact;
using Sayarah.Core.Helpers;
using Sayarah.Security;
using System.Globalization;


namespace Sayarah.Api.Controllers
{
    [ApiController]

    public class SitePagesController : AbpController
    {
        public AppSession AppSession { get; set; }

        private readonly IUserAppService _userAppService;

        private readonly ISitePageAppService _sitePageAppService;
        private readonly IContactMessageAppService _contactMessageAppService;
        private readonly IHttpContextAccessor _HttpContextAccessor;

        public SitePagesController(
                  IUserAppService userAppService,
                  ISitePageAppService sitePageAppService,
                  IContactMessageAppService contactMessageAppService,
                  IHttpContextAccessor httpContextAccessor

                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userAppService = userAppService;
            _sitePageAppService = sitePageAppService;
            _contactMessageAppService = contactMessageAppService;
            _HttpContextAccessor = httpContextAccessor;

        }

        CultureInfo new_lang = new CultureInfo("ar");


        #region SitePages
        //////////////////////////////////////////////SitePages//////////////////////     
        public LanguageEnum GetLang()
        {
            var langHeader = _HttpContextAccessor?.HttpContext?.Request?.Headers["Lang"].ToString();

            LanguageEnum lang = langHeader == "ar" ? LanguageEnum.Ar : LanguageEnum.En;
            switch (langHeader)
            {
                case "ar":
                    lang = LanguageEnum.Ar;
                    break;
                case "en":
                    lang = LanguageEnum.En;
                    break;
                default:
                    lang = LanguageEnum.Ar;
                    break;
            }
            return lang;
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<AboutOutput> GetAbout()
        {

            try
            {  
                var _images = new List<string>();
                AboutOutput output = new AboutOutput { };
                var about = await _sitePageAppService.GetAllAsync(new GetAllSitePages
                {
                    PageEnum = PageEnum.About,
                    Language = GetLang()
                });
                foreach (var item in about.Items)
                {
                    switch (item.Key)
                    {

                        case "Description":
                            output.Description = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                return new AboutOutput
                {
                    Message = ex.Message,
                    Success = false
                };
            }
        }
        [HttpPost]
        [Language("Lang")]
        public async Task<AboutOutput> GetTerms()
        {
            try
            {
                // Read the "Lang" header from the request
                var langHeader = _HttpContextAccessor?.HttpContext?.Request?.Headers["Lang"].ToString();
                LanguageEnum lang = langHeader == "ar" ? LanguageEnum.Ar : LanguageEnum.En;

                var output = new AboutOutput();
                var about = await _sitePageAppService.GetAllAsync(new GetAllSitePages
                {
                    PageEnum = PageEnum.Terms,
                    Language = lang
                });

                foreach (var item in about.Items)
                {
                    if (item.Section == "Terms")
                    {
                        output.Description = item.Value;
                    }
                }

                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                return new AboutOutput
                {
                    Message = ex.Message,
                    Success = false
                };
            }
        }



        [HttpPost]
        [Language("Lang")]
        public async Task<AboutOutput> GetPrivacy()
        {
            try
            {
                var Lang = _HttpContextAccessor?.HttpContext?.Request?.Headers["Lang"].ToString();
                LanguageEnum lang = Lang == "ar" ? LanguageEnum.Ar : LanguageEnum.En;
                var _images = new List<string>();
                AboutOutput output = new AboutOutput { };
                var about = await _sitePageAppService.GetAllAsync(new GetAllSitePages
                {
                    PageEnum = PageEnum.Privacy,
                    Language = GetLang()
                });
                foreach (var item in about.Items)
                {
                    switch (item.Section)
                    {
                        case "Privacy":
                            output.Description = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                return new AboutOutput
                {
                    Message = ex.Message,
                    Success = false
                };
            }
        }

        #endregion

        // create ContactMessage
        [HttpPost]
        [Language("Lang")]
        public async Task<CreateContactMessageOutput> CreateContactMessage(CreateContactMessageDto input)
        {
            try
            {
                input.ContactType = ContactsType.Contact; 

                var result = await _contactMessageAppService.CreateAsync(input);
                if (result != null)
                {
                    if (result.ErrorMsg != null)
                    {
                        return new CreateContactMessageOutput { Success = false, Message = result.ErrorMsg };
                    }
                    return new CreateContactMessageOutput { Success = true, Message = L("MobileApi.Messages.SentSuccessfully") };
                }
                else
                {
                    return new CreateContactMessageOutput { Success = false, Message = L("MobileApi.Message.Error") };
                }
            }
            catch (Exception ex)
            {
                return new CreateContactMessageOutput { Success = false, Message = ex.Message };
            }
        }
    }
}