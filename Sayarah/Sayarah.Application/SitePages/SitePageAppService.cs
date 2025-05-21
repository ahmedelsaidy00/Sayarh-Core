using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.SitePages.Dto;
using Sayarah.Core.Helpers;
using Sayarah.SitePages;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Sayarah.Application.SitePages
{

    [DisableAuditing]
    public class SitePageAppService : AsyncCrudAppService<SitePage, SitePageDto, int, GetAllSitePages, CreateSitePageDto, UpdateSitePageDto>, ISitePageAppService
    {
        private readonly IRepository<SitePage> _sitePageRepository;
        private readonly ICommonAppService _commonService;
        public SitePageAppService(IRepository<SitePage> sitePageRepository
            , ICommonAppService commonService)
            : base(sitePageRepository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _sitePageRepository = sitePageRepository;
            _commonService = commonService;
        }

        public async Task<bool> Manage(ManageSitePageDto input)
        {
            try
            {

                if (input.SitePage.Count > 0)
                {
                    foreach (var item in input.SitePage)
                    {
                        switch (item.EntityAction)
                        {
                            case EntityAction.Create:
                                if (item.PageEnum == PageEnum.Intro && item.Key == "Photo")
                                {
                                    var sitePage = ObjectMapper.Map<SitePage>(item);
                                    await _sitePageRepository.InsertAsync(sitePage);
                                }
                                else
                                {
                                    if (item.Key == "VideoUrl")
                                    {
                                        var exist = await _sitePageRepository.FirstOrDefaultAsync(x => x.Key == "VideoUrl" && x.PageEnum == PageEnum.Intro);
                                        if (exist != null)
                                        {
                                            ObjectMapper.Map(item, exist);
                                            await _sitePageRepository.UpdateAsync(exist);
                                        }
                                        else
                                        {
                                            var sitePage = ObjectMapper.Map<SitePage>(item);
                                            await _sitePageRepository.InsertAsync(sitePage);
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(item.Value) && item.Key != "VideoUrl")
                                    {
                                        var sitePage = ObjectMapper.Map<SitePage>(item);
                                        await _sitePageRepository.InsertAsync(sitePage);
                                    }
                                }
                                break;
                            case EntityAction.Update:
                                if (item.Id.HasValue && item.Id.Value > 0)
                                {
                                    var sitePage = await _sitePageRepository.GetAsync(item.Id.Value);
                                    if (!string.IsNullOrEmpty(item.Value))
                                    {
                                        if (item.Key == "Photo" || item.Key == "Photo_1" || item.Key == "Photo_2" || item.Key == "PdfFile")
                                        {
                                            switch (item.PageEnum)
                                            {
                                                case PageEnum.About:
                                                    if (!string.IsNullOrEmpty(sitePage.Value) && (string.IsNullOrEmpty(item.Value) || !sitePage.Value.Equals(item.Value)))
                                                    {
                                                        Utilities.DeleteImage(2, sitePage.Value, new string[] { "1920x900_" });
                                                    }
                                                    break;
                                                case PageEnum.Contact:
                                                    if (!string.IsNullOrEmpty(sitePage.Value) && (string.IsNullOrEmpty(item.Value) || !sitePage.Value.Equals(item.Value)))
                                                    {
                                                        Utilities.DeleteImage(3, sitePage.Value, new string[] { "800x900_" });
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }

                                        }
                                        ObjectMapper.Map(item, sitePage);
                                        await _sitePageRepository.UpdateAsync(sitePage);
                                    }
                                    else
                                        await _sitePageRepository.DeleteAsync(sitePage);
                                }
                                break;
                            case EntityAction.Delete:
                                await _sitePageRepository.DeleteAsync(item.Id.Value);
                                break;
                        }
                    }
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public override async Task<PagedResultDto<SitePageDto>> GetAllAsync(GetAllSitePages input)
        {
            var query = _sitePageRepository.GetAll();
            query = query.WhereIf(input.PageEnum.HasValue, m => m.PageEnum == input.PageEnum);
            query = query.WhereIf(input.IsHidden.HasValue, m => m.IsHidden == input.IsHidden);
            query = query.WhereIf(input.Language.HasValue, m => m.Language == input.Language || m.Language == LanguageEnum.None);
            query = query.WhereIf(!string.IsNullOrEmpty(input.Section), at => at.Section.Contains(input.Section));

            query = query.WhereIf(!string.IsNullOrEmpty(input.Key) && input.Key.Equals("SocialLinks"), m => m.Key.Equals("Facebook") || m.Key.Equals("Twitter")
            || m.Key.Equals("Instagram") || m.Key.Equals("Youtube") || m.Key.Equals("Email"));
            query = query.WhereIf(!string.IsNullOrEmpty(input.Key) && !input.Key.Equals("SocialLinks"), m => m.Key.Equals(input.Key));
            query = query.WhereIf(!string.IsNullOrEmpty(input.Key) && !input.Key.Equals("Photo"), m => m.Key.Equals(input.Key));

            var sitePages = await query.OrderBy(m => m.Id).ToListAsync();

            var mappedSitePages = ObjectMapper.Map<List<SitePageDto>>(sitePages);


            return new PagedResultDto<SitePageDto>(
               sitePages.Count, mappedSitePages);


        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            var sitePage = await _sitePageRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (sitePage != null)
            {
                if (!string.IsNullOrEmpty(sitePage.Value))
                    Utilities.DeleteImage(4, sitePage.Value, new string[] { "800x900_" });
                await _sitePageRepository.DeleteAsync(sitePage);
            }
        }
        public async Task<List<SitePageDto>> ApiGetAll(GetAllSitePages input)
        {
            try
            {
                var query = _sitePageRepository.GetAll();
                query = query.WhereIf(input.PageEnum.HasValue, m => m.PageEnum == input.PageEnum);
                query = query.WhereIf(input.Language.HasValue, m => m.Language == input.Language || m.Language == LanguageEnum.None);
                query = query.WhereIf(!string.IsNullOrEmpty(input.Section), at => at.Section.Contains(input.Section));
                query = query.WhereIf(!string.IsNullOrEmpty(input.Key), at => at.Key.Contains(input.Key));

                var sitePages = await query.OrderBy(m => m.Id).ToListAsync();
                return ObjectMapper.Map<List<SitePageDto>>(sitePages);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ChangeSort(UpdateSortInput input)
        {
            try
            {

                var _currentStory = await Repository.FirstOrDefaultAsync(input.Id);

                if (_currentStory != null)
                {
                    int _currentSort = _currentStory.Sort.Value;
                    int _secondTapeSort = input.ShiftUp == true ? _currentSort - 1 : _currentSort + 1;

                    // get 
                    var secondTape = await Repository.FirstOrDefaultAsync(a => a.Sort == _secondTapeSort && a.PageEnum == _currentStory.PageEnum);

                    secondTape.Sort = _currentSort;

                    _currentStory.Sort = _secondTapeSort;
                    await Repository.UpdateAsync(_currentStory);
                    await Repository.UpdateAsync(secondTape);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<SitePageDto> UpdateStatus(EntityDto<int> input)
        {
            try
            {
                var story = await Repository.GetAsync(input.Id);
                story.IsHidden = !story.IsHidden;
                await Repository.UpdateAsync(story);
                return MapToEntityDto(story);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SitePageDto> CreateSlider(CreateSitePageDto input)
        {
            try
            {
                string _sort = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "SitePages", CodeField = "Sort", AddWhere = " and PageEnum = " + 5 });
                input.Sort = Convert.ToInt32(_sort);
                var story = ObjectMapper.Map<SitePage>(input);
                await Repository.InsertAsync(story);
                return MapToEntityDto(story);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        //public async Task<bool> GetPaymentLive()
        //{
        //    var query = await _sitePageRepository.FirstOrDefaultAsync(x => x.PageEnum == PageEnum.PaymentLive);
        //    if (query != null)
        //        return Convert.ToBoolean(query.Value);
        //    else

        //        return false;
        //}

    }

}
