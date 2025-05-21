using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
namespace Sayarah.Api.Controllers
{
    public class UploadWebPController : Controller
    {
        private string StorageRoot
        {
            get
            {
                switch (fileType)
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
                    case 10:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/MaintainTransOut");
                    case 11:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/WashTransOut");
                    case 12:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "files/OilTransOut");
                    default:
                        return Path.Combine(HttpRuntime.AppDomainAppPath, "Files");
                }
            }
        }

        public int fileType = 0;
        #region UploadData With Options
        //////////////////////////////UploadData With Options////////////////////////////////////////////////
        //tested
        public string UploadPhotoWebP(HttpPostedFileBase file, NewUploadFilesDto input)
        {
            List<string> thumbnailUrls = new List<string>();

            if (file == null || file.ContentLength <= 0)
            {
                return "";
            }
            fileType = input.StorageLocation;
            string[] alloweImageTypes = new string[] { "image/jpeg", "image/png", "image/jpg", "image/webp", "image/*" };
            string fileName = Path.GetFileName(file.FileName);
            string ext = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - (fileName.LastIndexOf(".")));
            string uniqueFileName = Guid.NewGuid().ToString() + ext;
            var path = Path.Combine(StorageRoot, uniqueFileName);

            string _uniqueFileName = Guid.NewGuid().ToString();
            string uniqueFileNameWebp = _uniqueFileName + ".webp";
            var webPImagePath = Path.Combine(StorageRoot, uniqueFileNameWebp);
            var statuses = new List<NewViewDataUploadFilesResult>();
            JsonResult result = Json(new { files = statuses });
            if (!alloweImageTypes.Contains(file.ContentType.ToLower()))
            {
                file.SaveAs(path);
                statuses.Add(new NewViewDataUploadFilesResult()
                {
                    name = fileName,
                    uniqueName = uniqueFileName,
                    physicalPath = path,
                    type = file.ContentType,
                    url = "/Upload/Download/" + uniqueFileName,
                    delete_url = "/Upload/Delete/" + uniqueFileName,
                    thumbnailUrl = string.Empty,
                    thumbnailUrls = new List<string>(),
                    delete_type = "GET",
                });
            }
            else
            {
                if (input.UploadStyle == NewUploadStyle.OriginalOnly || input.UploadStyle == NewUploadStyle.BothOfThem)
                {
                    try
                    {
                        file.InputStream.Position = 0;
                        byte[] photoBytes;
                        using (var ms = new MemoryStream())
                        {
                            int length = System.Convert.ToInt32(file.InputStream.Length);
                            file.InputStream.CopyTo(ms, length);
                            photoBytes = ms.ToArray();
                        }
                        using (var inStream = new MemoryStream(photoBytes))
                        {
                            using (var imageFactory = new ImageFactory(preserveExifData: true))
                            {
                                imageFactory.Load(inStream);
                                imageFactory.Format(new WebPFormat { Quality = 100 });
                                imageFactory.Save(webPImagePath);
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                if (input.UploadStyle == NewUploadStyle.CopyOnly || input.UploadStyle == NewUploadStyle.BothOfThem)
                {
                    foreach (var size in input.FileSizes)
                    {
                        if (alloweImageTypes.Contains(file.ContentType.ToLower()))
                        {
                            var newFileName = size.Width + "x" + size.Height + "_" + uniqueFileNameWebp;
                            var newPath = Path.Combine(StorageRoot, newFileName);
                            try
                            {
                                file.InputStream.Position = 0;
                                byte[] photoBytes;
                                using (var ms = new MemoryStream())
                                {
                                    int length = System.Convert.ToInt32(file.InputStream.Length);
                                    file.InputStream.CopyTo(ms, length);
                                    photoBytes = ms.ToArray();
                                }
                                using (var inStream = new MemoryStream(photoBytes))
                                {
                                    using (var imageFactory2 = new ImageFactory(preserveExifData: true))
                                    {
                                        System.Drawing.Size _size = new System.Drawing.Size { Height = size.Height, Width = size.Width };
                                        imageFactory2.Load(inStream);
                                        imageFactory2.Format(new WebPFormat { Quality = 100 });
                                        imageFactory2.Resize(new ResizeLayer(_size, ResizeMode.Stretch));
                                        imageFactory2.Save(newPath);
                                    }

                                }

                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                statuses.Add(new NewViewDataUploadFilesResult()
                {
                    name = fileName,
                    uniqueName = uniqueFileNameWebp,
                    physicalPath = webPImagePath,
                    type = "image/webp",
                    url = "/Upload/Download/" + uniqueFileNameWebp,
                    delete_url = "/Upload/Delete/" + uniqueFileNameWebp,
                    delete_type = "GET",
                });
            }
            return uniqueFileNameWebp;
        }
    }

    [Serializable]
    public class NewUploadFilesDto
    {
        public int StorageLocation { get; set; }
        public string Sizes { get; set; }
        public List<NewFileSize> FileSizes
        {
            get
            {
                List<NewFileSize> fileSizes = new List<NewFileSize>();
                if (!string.IsNullOrEmpty(Sizes))
                    foreach (var size in Sizes.Split(';'))
                    {
                        try
                        {
                            fileSizes.Add(new NewFileSize()
                            {
                                Width = Convert.ToInt32(size.Split('&')[0]),
                                Height = Convert.ToInt32(size.Split('&')[1]),
                            });
                        }
                        catch { }
                    }
                return fileSizes;
            }
        }
        public NewAllowedTypes AllowedTypes { get; set; }
        public NewUploadStyle UploadStyle { get; set; }
    }

    [Serializable]
    public class NewFileSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public enum NewAllowedTypes
    {
        ImagesOnly = 0,
        FilesOnly = 1,
        BothOfThem = 2
    }
    public enum NewUploadStyle
    {
        OriginalOnly = 0,
        CopyOnly = 1,
        BothOfThem = 2
    }
    ///////////////////////////////////////End UploadData With Options////////////////////////////////////////////////////////
    #endregion
    public class NewViewDataUploadFilesResult
    {
        public string name { get; set; }
        public string uniqueName { get; set; }
        public string physicalPath { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string delete_url { get; set; }
        public string thumbnailUrl { get; set; }
        public List<string> thumbnailUrls { get; set; }
        public string delete_type { get; set; }

    }
}