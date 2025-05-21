using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Sayarah.Application.Configuration;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Lookups;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Sayarah.Application.Lookups
{
    [DisableAuditing]
    public class AnnouncementAppService : AsyncCrudAppService<Announcement, AnnouncementDto, long, PagedResultRequestDto, CreateAnnouncementDto, UpdateAnnouncementDto>, IAnnouncementAppService
    {
        private readonly IRepository<Announcement, long> _announcementRepository;
        private readonly ISettingAppService _settingAppService;
        public AnnouncementAppService(IRepository<Announcement, long> announcementRepository, ISettingAppService settingAppService)
            : base(announcementRepository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _announcementRepository = announcementRepository;
            _settingAppService = settingAppService;
        }

        public async Task<List<AnnouncementDto>> GetAllAnnouncement(GetAnnouncementApiInput input)
        {
            var query = _announcementRepository.GetAll().Where(a => a.IsVisible == true);
            query = query.WhereIf(input.AnnouncementType.HasValue, x => x.AnnouncementType == input.AnnouncementType.Value);
            query = query.WhereIf(input.AnnouncementUserType.HasValue, x => x.AnnouncementUserType == input.AnnouncementUserType.Value);

            return ObjectMapper.Map<List<AnnouncementDto>>(await query.ToListAsync());
        }


        [AbpAuthorize]
        public async Task<bool> SetDefaultMedia(SetDefaultMediaInput input)
        {
            Announcement _announcement = await _announcementRepository.GetAsync(input.AnnouncementId);
            if (_announcement == null)
                return false;
            await _announcementRepository.GetAll().Where(x => x.Id != _announcement.Id).ForEachAsync(a => a.IsDefault = false);
            _announcement.IsDefault = true;
            await _announcementRepository.UpdateAsync(_announcement);
            return true;
        }
        [AbpAuthorize]
        public override async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto input)
        {
            try
            {
                var menuImg = ObjectMapper.Map<Announcement>(input);
                await _announcementRepository.InsertAsync(menuImg);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                //await _settingAppService.UpdateSettingVersion(new Configuration.Dtos.UpdateSettingVersionInput { SettingName = AppSettingNames.AnnouncementsVersion });

                return MapToEntityDto(menuImg);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        [AbpAuthorize]
        public async Task<DataTableOutputDto<AnnouncementDto>> GetPaged(GetAnnouncementInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    long id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            id = Convert.ToInt64(input.ids[i]);
                            Announcement announcement = await Repository.FirstOrDefaultAsync(id);
                            if (announcement != null)
                            {
                                if (input.action == "Delete")
                                {
                                    if (announcement != null && !string.IsNullOrEmpty(announcement.FilePath)) Utilities.DeleteImage(16, announcement.FilePath, new string[] { "1600x300_" });

                                    await Repository.DeleteAsync(announcement);

                                    await UnitOfWorkManager.Current.SaveChangesAsync();
                                }
                                if (input.action == "Visible")
                                {
                                    announcement.IsVisible = !announcement.IsVisible;
                                    await Repository.UpdateAsync(announcement);
                                }
                            }
                            await UnitOfWorkManager.Current.SaveChangesAsync();
                        }

                        //await _settingAppService.UpdateSettingVersion(new Configuration.Dtos.UpdateSettingVersionInput { SettingName = AppSettingNames.AnnouncementsVersion });

                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt64(input.ids[0]);
                            Announcement announcement = await Repository.FirstOrDefaultAsync(id);
                            if (announcement != null)
                            {
                                if (input.action == "Delete")
                                {

                                    if (announcement != null && !string.IsNullOrEmpty(announcement.FilePath)) Utilities.DeleteImage(16, announcement.FilePath, new string[] { "1600x300_" });


                                    await Repository.DeleteAsync(announcement);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                if (input.action == "Visible")
                                {
                                    announcement.IsVisible = !announcement.IsVisible;
                                    await Repository.UpdateAsync(announcement);
                                }
                                //else if (input.action == "NotVisible")
                                //{
                                //    announcement.BillBoardStatus = BillBoardStatus.Refused;
                                //}

                            }
                            await UnitOfWorkManager.Current.SaveChangesAsync();

                            //await _settingAppService.UpdateSettingVersion(new Configuration.Dtos.UpdateSettingVersionInput { SettingName = AppSettingNames.AnnouncementsVersion });
                        }
                    }

                    var query = Repository.GetAll();
                    int count = await query.CountAsync();

                    query = query.FilterDataTable(input);
                    count = await query.CountAsync();

                    query = query.WhereIf(input.AnnouncementType.HasValue, at => at.AnnouncementType == input.AnnouncementType);
                    query = query.WhereIf(input.IsVisible.HasValue, at => at.IsVisible == input.IsVisible);

                    query = query.WhereIf(input.AnnouncementUserType.HasValue, at => at.AnnouncementUserType == input.AnnouncementUserType);
                    int filteredCount = await query.CountAsync();

                    var announcements = await query
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();
                    return new DataTableOutputDto<AnnouncementDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<AnnouncementDto>>(announcements)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<AnnouncementDto> UpdateAsync(UpdateAnnouncementDto input)
        {
            var announcement = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

            if (announcement != null && !string.IsNullOrEmpty(announcement.FilePath) && announcement.FilePath != input.FilePath)
            {
                Utilities.DeleteImage(16, announcement.FilePath, new string[] { "1600x300_" });
            }


            ObjectMapper.Map(input, announcement);
            await Repository.UpdateAsync(announcement);

            return MapToEntityDto(announcement);
        }

        [AbpAuthorize]
        public override async Task<AnnouncementDto> GetAsync(EntityDto<long> input)
        {
            var announcement = Repository.GetAll().FirstOrDefault(x => x.Id == input.Id);
            return await Task.FromResult(ObjectMapper.Map<AnnouncementDto>(announcement));
        }
    }

}
