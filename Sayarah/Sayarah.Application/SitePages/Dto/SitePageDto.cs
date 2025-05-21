using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Core.Helpers;
using Sayarah.SitePages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.SitePages.Dto
{
    [AutoMapFrom(typeof(SitePage)), AutoMapTo(typeof(SitePage))]
    public class SitePageDto : EntityDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Section { get; set; }
        public PageEnum PageEnum { get; set; }
        public LanguageEnum Language { get; set; }
        public string FilePath
        {
            get
            {
                if (Key == "Photo" || Key == "Photo2" || Key == "Photo_1" || Key == "Photo_2")
                {
                    switch (PageEnum)
                    {
                        case PageEnum.About:
                            if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage(2, "800x800_" + Value))
                                return FilesPath.SitePages.About.ServerImagePath + "800x800_" + Value;
                            else
                                return FilesPath.SitePages.About.DefaultImagePath;

                        //case PageEnum.OutDoorParties:
                        //    if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage(8, "800x800_" + Value))
                        //        return FilesPath.SitePages.OutDoorParties.ServerImagePath + "800x800_" + Value;
                        //    else
                        //        return FilesPath.SitePages.OutDoorParties.DefaultImagePath;
                            
                        //case PageEnum.EventHall:
                        //    if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage(9, "800x800_" + Value))
                        //        return FilesPath.SitePages.EventHalls.ServerImagePath + "800x800_" + Value;
                        //    else
                        //        return FilesPath.SitePages.EventHalls.DefaultImagePath;
                            
                        //case PageEnum.Gourmet:
                        //    if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage(10, "600x600_" + Value))
                        //        return FilesPath.SitePages.Gourmet.ServerImagePath + "600x600_" + Value;
                        //    else
                        //        return FilesPath.SitePages.Gourmet.DefaultImagePath;

                        case PageEnum.Intro:
                            if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage(3, "1920x1080_" + Value))
                                return FilesPath.SitePages.Index.ServerImagePath + "1920x1080_" + Value;
                            else
                                return FilesPath.SitePages.Index.DefaultImagePath;

                        default:
                            return string.Empty;
                    }
                }
                else
                    return string.Empty;
            }
        }



        //public List<string> FilePaths
        //{
        //    get
        //    {
        //        List<string> lst = new List<string>();
        //        switch (PageEnum)
        //        {
        //            case PageEnum.Intro:
        //                if (Key == "Photo")
        //                {
        //                         if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage((int)FileType.Intro, "400x175_"+Value))
        //                                lst.Add(FilesPath.SitePages.Intro.ServerImagePath + "400x175_" + Value);
        //                            else
        //                                lst.Add(FilesPath.SitePages.Intro.DefaultImagePath);
        //                        }                           
        //                return lst;                    
        //            case PageEnum.About:
        //                if (Key == "Photo")
        //                {
        //                    if (!string.IsNullOrEmpty(Value) && Utilities.CheckExistImage((int)FileType.About, "600x450_" + Value))
        //                        lst.Add(FilesPath.SitePages.About.ServerImagePath + "600x450_" + Value);
        //                    else
        //                        lst.Add(FilesPath.SitePages.About.DefaultImagePath);
        //                }
        //                return lst;
        //            //break;
        //            //case PageEnum.Index:
        //            //    if (Key == "Photo" && Section == "IndexSliderDescriptionOne")
        //            //    {
        //            //        if (!string.IsNullOrEmpty(Value))
        //            //        {
        //            //            string[] photos = Value.Split(';');
        //            //            string[] photosSizes = new string[2] { "600x600_", "520x580_" };

        //            //            for (var i = 0; i < photos.Length; i++)
        //            //            {
        //            //                if (!string.IsNullOrEmpty(photos[i]) && Utilities.CheckExistImage(4, photosSizes[i] + photos[i]))
        //            //                    lst.Add(FilesPath.SitePagesIndex.ImagesServerPath + photosSizes[i] + photos[i]);
        //            //                else
        //            //                {
        //            //                    if (photosSizes[i] == "600x600_")
        //            //                        lst.Add(FilesPath.SitePagesIndex.DefaultTopSlider1);
        //            //                    else if (photosSizes[i] == "520x580_")
        //            //                        lst.Add(FilesPath.SitePagesIndex.DefaultBackSlider1);

        //            //                }
        //            //            }
        //            //        }
        //            //    }

        //            //    else if (Key == "Photo" && Section == "IndexSliderDescriptionTwo")
        //            //    {
        //            //        if (!string.IsNullOrEmpty(Value))
        //            //        {
        //            //            string[] photos = Value.Split(';');
        //            //            string[] photosSizes = new string[2] { "787x450_", "483x879_" };

        //            //            for (var i = 0; i < photos.Length; i++)
        //            //            {
        //            //                if (!string.IsNullOrEmpty(photos[i]) && Utilities.CheckExistImage(4, photosSizes[i] + photos[i]))
        //            //                    lst.Add(FilesPath.SitePagesIndex.ImagesServerPath + photosSizes[i] + photos[i]);
        //            //                else
        //            //                {
        //            //                    if (photosSizes[i] == "787x450_")
        //            //                        lst.Add(FilesPath.SitePagesIndex.DefaultTopSlider2);
        //            //                    else if (photosSizes[i] == "483x879_")
        //            //                        lst.Add(FilesPath.SitePagesIndex.DefaultBackSlider2);
        //            //                }
        //            //            }
        //            //        }
        //            //    }
        //            //    return lst;
        //            default:
        //                return new List<string>();
        //        }

        //    }
        //}
        public int? Sort { get; set; }
        public bool? IsHidden { get; set; }
    }


    [AutoMapFrom(typeof(SitePage))]
    public class SitePagePlainDto : EntityDto
    {
        public string Value { get; set; }
        public string Section { get; set; }
        public string FilePath { get; set; }
        public int? Sort { get; set; }
        public bool? IsHidden { get; set; }
    }



    [AutoMapTo(typeof(SitePage))]
    public class CreateSitePageDto : BaseInputDto
    {
        public int? Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Section { get; set; }
        public PageEnum PageEnum { get; set; }
        public LanguageEnum Language { get; set; }
        public int? Sort { get; set; }
        public bool? IsHidden { get; set; }

    }
    [AutoMapTo(typeof(SitePage))]
    public class ManageSitePageDto
    {
        public List<CreateSitePageDto> SitePage { get; set; }

    }
    [AutoMapTo(typeof(SitePage))]
    public class UpdateSitePageDto : EntityDto
    {
        public List<SitePageDto> SitePage { get; set; }

    }
    public class GetAllSitePages : PagedResultRequestDto
    {
        public PageEnum? PageEnum { get; set; }
        public LanguageEnum? Language { get; set; }
        public bool? IsHidden { get; set; }
        public string Key { get; set; }
        public string Section { get; set; }
    }
    public class GetSitePagesOutput
    {
        public List<SitePageDto> SitePages { get; set; }
    }
    public class UpdateSortInput : EntityDto<int>
    {
        public bool ShiftUp { get; set; }
    }
}
