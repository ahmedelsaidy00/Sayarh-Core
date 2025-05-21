using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Lookups;
using System.ComponentModel.DataAnnotations;
using static Sayarah.SayarahConsts;


namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(Announcement)), AutoMapTo(typeof(Announcement))]
    public class AnnouncementDto : EntityDto<long>
    {
        public string FilePath { get; set; }
        public bool IsDefault { get; set; }
        public string FullFilePath
        {
            get
            {

                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(16, "1600x300_" + FilePath))
                    return FilesPath.Announcements.ServerImagePath + "1600x300_" + FilePath;
                else
                    return FilesPath.Announcements.DefaultImagePath;
            }

        }
      
         
        public bool IsVisible { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementUserType AnnouncementUserType { get; set; }
        public string Url { get; set; }
       
    }

    [AutoMapFrom(typeof(AnnouncementDto)), AutoMapTo(typeof(AnnouncementDto))]
    public class ApiAnnouncementDto : EntityDto<long>
    {
        public string FilePath { get; set; } // for mobile
        public string FullFilePath { get; set; } // for web
        public bool IsDefault { get; set; }
        public bool IsVisible { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementUserType AnnouncementUserType { get; set; }
        public string Url { get; set; }
    }
     

    [AutoMapTo(typeof(Announcement))]
    public class CreateAnnouncementDto
    {
        public string FilePath { get; set; } // for mobile 
        public bool IsDefault { get; set; }
        public bool IsVisible { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementUserType AnnouncementUserType { get; set; }
        public string Url { get; set; }
    }


    [AutoMapTo(typeof(Announcement))]
    public class UpdateAnnouncementDto : EntityDto<long>
    {
        public string FilePath { get; set; } // for mobile 
        public bool IsDefault { get; set; }
        public bool IsVisible { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementUserType AnnouncementUserType { get; set; }
        public string Url { get; set; }
    }

    public class GetAnnouncementInput : DataTableInputDto
    {
        public string FilePath { get; set; } // for mobile 
        public bool? IsDefault { get; set; }
        public bool? IsVisible { get; set; }
        public AnnouncementType? AnnouncementType { get; set; }
        public AnnouncementUserType? AnnouncementUserType { get; set; }
        public string Url { get; set; }
    }

    public class GetAnnouncementApiInput
    {
        public string FilePath { get; set; } // for mobile 
        public bool? IsDefault { get; set; }
        public bool? IsVisible { get; set; }
        public AnnouncementType? AnnouncementType { get; set; }
        public AnnouncementUserType? AnnouncementUserType { get; set; }
        public string Url { get; set; }
    }
    public class DeleteAnnouncementInput
    {
        [Required]
        public int MediaId { get; set; }
    }
    public class SetDefaultMediaInput
    {
        [Required, Range(1, long.MaxValue)]
        public int AnnouncementId { get; set; }
    }
}
