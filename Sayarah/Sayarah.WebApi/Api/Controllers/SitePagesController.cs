using Abp.Application.Services.Dto;
using Abp.WebApi.Controllers;
using Sayarah.Api.Models;
using Sayarah.Contact;
using Sayarah.Contact.Dto;
using Sayarah.Helpers;
using Sayarah.SitePages;
using Sayarah.SitePages.Dto;
using Sayarah.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Sayarah.Helpers.Enums;
using Sayarah.Security;


namespace Sayarah.Api.Controllers
{
    public class SitePagesController : AbpApiController
    {
        public AppSession AppSession { get; set; }

        private readonly IUserAppService _userAppService;

        private readonly ISitePageAppService _sitePageAppService;
        private readonly IContactMessageAppService _contactMessageAppService;

        public SitePagesController(
                  IUserAppService userAppService,
                  ISitePageAppService sitePageAppService,
                  IContactMessageAppService contactMessageAppService

                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userAppService = userAppService;
            _sitePageAppService = sitePageAppService;
            _contactMessageAppService = contactMessageAppService;
        }

        CultureInfo new_lang = new CultureInfo("ar");


        #region SitePages
        //////////////////////////////////////////////SitePages/////////////////////////////////////////////////
        public LanguageEnum GetLang()
        {
            var Lang = HttpContext.Current.Request.Headers["Lang"];

            LanguageEnum lang = Lang == "ar" ? LanguageEnum.Ar : LanguageEnum.En;
            switch (Lang)
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
                var Lang = HttpContext.Current.Request.Headers["Lang"];
                LanguageEnum lang = Lang == "ar" ? LanguageEnum.Ar : LanguageEnum.En;
                var _images = new List<string>();
                AboutOutput output = new AboutOutput { };
                var about = await _sitePageAppService.GetAllAsync(new GetAllSitePages
                {
                    PageEnum = PageEnum.Terms,
                    Language = GetLang()
                });
                foreach (var item in about.Items)
                {
                    switch (item.Section)
                    {
                        case "Terms":
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
        public async Task<AboutOutput> GetPrivacy()
        {
            try
            {
                var Lang = HttpContext.Current.Request.Headers["Lang"];
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