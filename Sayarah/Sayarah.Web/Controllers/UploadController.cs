using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using static Sayarah.SayarahConsts;

namespace Sayarah.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }
        // Add this property to the UploadController class
        private string StorageRoot => GetStorageRoot(fileType);
        private string GetStorageRoot(int fileType)
        {
            string basePath = Path.Combine(_env.ContentRootPath, "files");

            return fileType switch
            {
                1 => Path.Combine(basePath, "Users"),
                2 => Path.Combine(basePath, "SitePages", "About"),
                3 => Path.Combine(basePath, "SitePages", "Index"),
                4 => Path.Combine(basePath, "Companies"),
                5 => Path.Combine(basePath, "Veichles"),
                6 => Path.Combine(basePath, "Drivers"),
                7 => Path.Combine(basePath, "Providers"),
                8 => Path.Combine(basePath, "Workers"),
                9 => Path.Combine(basePath, "FuelTransOut"),
                10=> Path.Combine(basePath, "FuelPriceChangeRequests"),
                11=> Path.Combine(basePath, "Banks"),
                12=> Path.Combine(basePath, "Wallet"),
                13 => Path.Combine(basePath, "AnnouncmentBanners"),
                14 => Path.Combine(basePath, "Tickets"),
                15 => Path.Combine(basePath, "Vouchers"),
                16 => Path.Combine(basePath, "Invoices"),
                _ => Path.Combine(basePath),
            };
        }



        public int fileType = 0;

        public ActionResult Index()
        {
            return View();
        }

        #region Defualt Uploder
        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpGet]
        public IActionResult Delete(string id)
        {
            var filename = id;
            string filePath = Path.Combine(GetStorageRoot(fileType), filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Ok(new { message = "File deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "File not found." });
            }
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpGet]
        public IActionResult Download(string id)
        {
            var filename = id;
            string filePath = Path.Combine(GetStorageRoot(fileType), filename);

            if (System.IO.File.Exists(filePath))
            {
                var contentType = "application/octet-stream";
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, contentType, filename);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet]
        public IActionResult DownloadFile(string id, int _fileType)
        {
            var filename = id;
            fileType = _fileType;

            var filePath = Path.Combine(GetStorageRoot(fileType), filename);

            if (System.IO.File.Exists(filePath))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;
                var contentType = "application/octet-stream";
                return File(memory, contentType, filename);
            }
            else
            {
                return NotFound();
            }
        }


        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpPost]
        public ActionResult UploadFiles(int _fileType)
        {
            fileType = _fileType;
            var statuses = new List<ViewDataUploadFilesResult>();

            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Json(new { files = statuses });
            }

            foreach (var file in files)
            {
                // For chunked uploads, check for X-File-Name header
                var headers = Request.Headers;
                var xFileName = headers["X-File-Name"].FirstOrDefault();

                if (string.IsNullOrEmpty(xFileName))
                {
                    // Whole file upload
                    var tempStatuses = new List<ViewDataUploadFilesResult>();
                    UploadWholeFile(Request, tempStatuses, false);
                    statuses.AddRange(tempStatuses);
                }
                else
                {
                    // Partial (chunked) upload
                    var tempStatuses = new List<ViewDataUploadFilesResult>();
                    UploadPartialFile(xFileName, Request, tempStatuses, false);
                    statuses.AddRange(tempStatuses);
                }
            }

            var result = Json(new { files = statuses });
            result.ContentType = "text/plain";
            return result;
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpPost]
        public ActionResult UploadPhoto(int _fileType)
        {
            fileType = _fileType;
            var statuses = new List<ViewDataUploadFilesResult>();

            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Json(new { files = statuses });
            }

            var headers = Request.Headers;
            var xFileName = headers["X-File-Name"].FirstOrDefault();

            if (string.IsNullOrEmpty(xFileName))
            {
                // Whole file upload
                UploadWholeFile(Request, statuses, true);
            }
            else
            {
                // Partial (chunked) upload
                UploadPartialFile(xFileName, Request, statuses, true);
            }

            var result = Json(new { files = statuses });
            result.ContentType = "text/plain";
            return result;
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadPartialFile(string fileName, Microsoft.AspNetCore.Http.HttpRequest request, List<ViewDataUploadFilesResult> statuses, bool isImage)
        {
            if (request.Form.Files.Count != 1)
                throw new Exception("Attempt to upload chunked file containing more than one fragment per request");

            var file = request.Form.Files[0];
            if (file.Length == 0)
                return;

            var inputStream = file.OpenReadStream();
            string originalFileName = Path.GetFileName(fileName);
            string ext = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - (fileName.LastIndexOf(".")));
            string uniqueFileName = Guid.NewGuid().ToString() + ext;
            var fullPath = Path.Combine(StorageRoot, uniqueFileName);

            if (isImage) // resize image and set its ext JPG
            {
                using (Image bigImage = new Bitmap(inputStream))
                {
                    UploadImage(bigImage, uniqueFileName);
                }
            }
            else
            {
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

            // generate thumbnail (optional, currently commented out)
            // string thumbnailFullPath = Path.Combine(StorageRoot, "thumbnail_" + uniqueFileName);
            // if (isImage)
            // {
            //     using (Image bigImage = new Bitmap(fullPath))
            //     {
            //         int height = 0;
            //         int width = 0;
            //         if (height > width)
            //         {
            //             height = 80;
            //             width = Convert.ToInt16(bigImage.Width * (80 / Convert.ToDecimal(bigImage.Height)));
            //         }
            //         else
            //         {
            //             width = 80;
            //             height = Convert.ToInt16(bigImage.Height * (80 / Convert.ToDecimal(bigImage.Width)));
            //         }
            //         using (Image smallImage = bigImage.GetThumbnailImage(width, height, new Image.GetThumbnailImageAbort(ThumbnailImageAbortCallback), IntPtr.Zero))
            //         {
            //             smallImage.Save(thumbnailFullPath, ImageFormat.Jpeg);
            //         }
            //     }
            // }

            // No need to append file content as in the old code, file.CopyTo already writes the content.

            string thumbnailUrl = string.Empty;
            // if (isImage)
            //     thumbnailUrl = @"data:image/png;base64," + EncodeFile(thumbnailFullPath);

            statuses.Add(new ViewDataUploadFilesResult()
            {
                name = originalFileName,
                uniqueName = uniqueFileName,
                physicalPath = fullPath,
                size = (int)file.Length,
                type = file.ContentType,
                url = "/Upload/Download/" + uniqueFileName,
                delete_url = "/Upload/Delete/" + uniqueFileName,
                // thumbnailUrl = thumbnailUrl,
                delete_type = "GET",
            });
        }
        public static bool ThumbnailImageAbortCallback()
        {
            return true;
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadWholeFile(HttpRequest request, List<ViewDataUploadFilesResult> statuses, bool isImage)
        {
            for (int i = 0; i < request.Form.Files.Count; i++)
            {
                try
                {
                    var file = request.Form.Files[i];
                    if (file.Length == 0)
                        return;

                    var inputStream = file.OpenReadStream();
                    string fileName = Path.GetFileName(file.FileName);
                    string ext = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - (fileName.LastIndexOf(".")));
                    string uniqueFileName = Guid.NewGuid().ToString() + ext;
                    var fullPath = Path.Combine(StorageRoot, uniqueFileName);

                    if (isImage) // resize image and set its ext JPG
                    {
                        using (Image bigImage = new Bitmap(inputStream))
                        {
                            UploadImage(bigImage, uniqueFileName);
                        }
                    }
                    else
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }

                    string thumbnailUrl = string.Empty;
                    //if (isImage)
                    //    thumbnailUrl = @"data:image/png;base64," + EncodeFile(thumbnailFullPath);

                    statuses.Add(new ViewDataUploadFilesResult()
                    {
                        name = fileName,
                        uniqueName = uniqueFileName,
                        physicalPath = fullPath,
                        size = (int)file.Length,
                        type = file.ContentType,
                        url = "/Upload/Download/" + uniqueFileName,
                        delete_url = "/Upload/Delete/" + uniqueFileName,
                        //  thumbnailUrl = thumbnailUrl,
                        delete_type = "GET",
                    });
                }
                catch (Exception)
                {
                    throw new Exception("حدث خطأ، تأكد من حجم الملف لا يزيد عن 4 ميجا");
                }
            }
        }
        void UploadImage(Image srcImage, string uniqueFileName)
        {
            var fullPath = Path.Combine(StorageRoot, uniqueFileName);

            Bitmap img = new Bitmap(srcImage.Width, srcImage.Height);

            using (Graphics gr = Graphics.FromImage(img))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(srcImage, new Rectangle(0, 0, srcImage.Width, srcImage.Height));
            }
            img.Save(fullPath, ImageFormat.Jpeg);
        }
        #endregion

        #region UploadData With Options
        //////////////////////////////UploadData With Options////////////////////////////////////////////////
        [HttpPost]
        public ActionResult UploadData(UploadFilesDto input)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            JsonResult result = Json(new { files = statuses });

            var filesList = Request.Form.Files;
            if (filesList == null || filesList.Count == 0)
                return result;

            switch (input.AllowedTypes)
            {
                case AllowedTypes.ImagesOnly:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            UploadPhoto(file, input, statuses);
                        }
                    }
                    break;
                case AllowedTypes.FilesOnly:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            UploadFile(file, input, statuses);
                        }
                    }
                    break;
                case AllowedTypes.BothOfThem:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            FileType returnType = CheckFileType(file.ContentType);
                            if (returnType != FileType.None)
                            {
                                if (returnType == FileType.Image)
                                    UploadPhoto(file, input, statuses);
                                else
                                    UploadFile(file, input, statuses);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        // Overload for IFormFile (ASP.NET Core)
        void UploadPhoto(Microsoft.AspNetCore.Http.IFormFile file, UploadFilesDto input, List<ViewDataUploadFilesResult> statuses)
        {
            List<string> thumbnailUrls = new List<string>();

            if (file == null || file.Length <= 0)
                return;
            fileType = input.StorageLocation;

            string fileName = Path.GetFileName(file.FileName);
            string ext = Path.GetExtension(fileName);
            string uniqueFileName = Guid.NewGuid().ToString() + ext;

            var path = Path.Combine(StorageRoot, uniqueFileName);

            if (input.UploadStyle == UploadStyle.OriginalOnly || input.UploadStyle == UploadStyle.BothOfThem)
            {
                if (ext == ".gif")
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                else
                {
                    using (var inputStream = file.OpenReadStream())
                    using (System.Drawing.Image bigImage = new Bitmap(inputStream))
                    {
                        Bitmap img = new Bitmap(bigImage.Width, bigImage.Height);

                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(img))
                        {
                            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            gr.DrawImage(bigImage, new System.Drawing.Rectangle(0, 0, bigImage.Width, bigImage.Height));
                        }
                        ImageFormat imageFormat = ImageFormat.Jpeg;
                        switch (ext.ToLower())
                        {
                            case ".jpeg":
                            case ".jpg":
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            case ".png":
                                imageFormat = ImageFormat.Png;
                                break;
                            case ".gif":
                                imageFormat = ImageFormat.Gif;
                                break;
                        }
                        img.Save(path, imageFormat);
                    }
                }
            }
            if (input.UploadStyle == UploadStyle.CopyOnly || input.UploadStyle == UploadStyle.BothOfThem)
            {
                foreach (var size in input.FileSizes)
                {
                    var newFileName = size.Width + "x" + size.Height + "_" + uniqueFileName;
                    var newPath = Path.Combine(StorageRoot, newFileName);
                    Bitmap newImage = new Bitmap(size.Width, size.Height);

                    using (var inputStream = file.OpenReadStream())
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newImage))
                    {
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        gr.DrawImage(new Bitmap(inputStream), 0, 0, size.Width, size.Height);
                        ImageFormat imageFormat = ImageFormat.Jpeg;
                        switch (ext.ToLower())
                        {
                            case ".jpeg":
                            case ".jpg":
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            case ".png":
                                imageFormat = ImageFormat.Png;
                                break;
                            case ".gif":
                                imageFormat = ImageFormat.Gif;
                                break;
                        }
                        newImage.Save(newPath, imageFormat);
                    }
                    thumbnailUrls.Add(@"data:image/png;base64," + EncodeFile(newPath));
                }
            }
            statuses.Add(new ViewDataUploadFilesResult()
            {
                name = fileName,
                uniqueName = uniqueFileName,
                physicalPath = path,
                type = file.ContentType,
                url = "/Upload/Download/" + uniqueFileName,
                delete_url = "/Upload/Delete/" + uniqueFileName,
                // thumbnailUrls = thumbnailUrls,
                delete_type = "GET",
            });
        }
        // Overload for IFormFile (ASP.NET Core)
        void UploadFile(Microsoft.AspNetCore.Http.IFormFile file, UploadFilesDto input, List<ViewDataUploadFilesResult> statuses)
        {
            if (file == null || file.Length <= 0)
                return;
            fileType = input.StorageLocation;

            string fileName = Path.GetFileName(file.FileName);
            string ext = Path.GetExtension(fileName);
            string uniqueFileName = Guid.NewGuid().ToString() + ext;

            var path = Path.Combine(StorageRoot, uniqueFileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            statuses.Add(new ViewDataUploadFilesResult()
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
        [HttpPost]
        public ActionResult UploadDataWebP(UploadFilesDto input)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            JsonResult result = Json(new { files = statuses });
            var filesList = Request.Form.Files;
            if (filesList == null || filesList.Count == 0)
                return result;

            switch (input.AllowedTypes)
            {
                case AllowedTypes.ImagesOnly:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            UploadPhotoWebP(file, input, statuses);
                        }
                    }
                    break;
                case AllowedTypes.FilesOnly:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            UploadFile(file, input, statuses);
                        }
                    }
                    break;
                case AllowedTypes.BothOfThem:
                    {
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            var file = filesList[i];
                            if (file == null || file.Length <= 0)
                                continue;
                            FileType returnType = CheckFileType(file.ContentType);
                            if (returnType != FileType.None)
                            {
                                if (returnType == FileType.Image)
                                    UploadPhotoWebP(file, input, statuses);
                                else
                                    UploadFile(file, input, statuses);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        // .NET Core version using IFormFile
        void UploadPhotoWebP(Microsoft.AspNetCore.Http.IFormFile file, UploadFilesDto input, List<ViewDataUploadFilesResult> statuses)
        {
            List<string> thumbnailUrls = new List<string>();

            if (file == null || file.Length <= 0)
                return;
            fileType = input.StorageLocation;

            string[] alloweImageTypes = new string[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
            string fileName = Path.GetFileName(file.FileName);
            string ext = Path.GetExtension(fileName);
            string uniqueFileName = Guid.NewGuid().ToString() + ext;
            var path = Path.Combine(StorageRoot, uniqueFileName);

            string _uniqueFileName = Guid.NewGuid().ToString();
            string uniqueFileNameWebp = _uniqueFileName + ".webp";
            var webPImagePath = Path.Combine(StorageRoot, uniqueFileNameWebp);

            if (!alloweImageTypes.Contains(file.ContentType.ToLower()))
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                statuses.Add(new ViewDataUploadFilesResult()
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
                // Replace the problematic block using ImageProcessor with ImageMagick for WebP conversion

                if (input.UploadStyle == UploadStyle.OriginalOnly || input.UploadStyle == UploadStyle.BothOfThem)
                {
                    try
                    {
                        byte[] photoBytes;
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            photoBytes = ms.ToArray();
                        }
                        using (var inStream = new MemoryStream(photoBytes))
                        {
                            using (var image = new MagickImage(inStream))
                            {
                                image.Format = MagickFormat.WebP;
                                image.Quality = 100;
                                image.Write(webPImagePath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                if (input.UploadStyle == UploadStyle.OriginalOnly || input.UploadStyle == UploadStyle.BothOfThem)
                {
                    try
                    {
                        byte[] photoBytes;
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            photoBytes = ms.ToArray();
                        }
                        using (var inStream = new MemoryStream(photoBytes))
                        {
                            using (var image = new MagickImage(inStream))
                            {
                                image.Format = MagickFormat.WebP;
                                image.Quality = 100;
                                image.Write(webPImagePath);
                            }
     
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                if (input.UploadStyle == UploadStyle.CopyOnly || input.UploadStyle == UploadStyle.BothOfThem)
                {
                    foreach (var size in input.FileSizes)
                    {
                        if (alloweImageTypes.Contains(file.ContentType.ToLower()))
                        {
                            var newFileName = size.Width + "x" + size.Height + "_" + uniqueFileNameWebp;
                            var newPath = Path.Combine(StorageRoot, newFileName);
                            try
                            {
                                byte[] photoBytes;
                                using (var ms = new MemoryStream())
                                {
                                    file.CopyTo(ms);
                                    photoBytes = ms.ToArray();
                                }
                                using (var inStream = new MemoryStream(photoBytes))
                                {
   
                                    using (var image = new MagickImage(photoBytes))
                                    {
                                        image.Format = MagickFormat.WebP;
                                        image.Quality = 100;

                                        var geometry = new MagickGeometry(Convert.ToUInt32(size.Width), Convert.ToUInt32(size.Height))
                                        {
                                            IgnoreAspectRatio = true // Stretch
                                        };
                                        image.Resize(geometry);

                                        image.Write(newPath);
                                    }

                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }
                statuses.Add(new ViewDataUploadFilesResult()
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
        }
        FileType CheckFileType(string Type)
        {
            switch (Type)
            {
                case "image/png":
                case "image/jpeg":
                case "image/gif":
                case "image/tiff":
                case "image/webp":
                    return FileType.Image;
                //Word
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                //Sql
                case "application/octet-stream":
                //Note Pad
                case "text/plain":
                    return FileType.Doc;
                //Exel
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return FileType.Exel;
                //PDF
                case "application/pdf":
                    return FileType.PDF;
                case "video/mp4":
                case "video/avi":
                case "video/wmv":
                    return FileType.Vedio;
                default:
                    return FileType.None;
            }
        }
    }

    [Serializable]
    public class UploadFilesDto
    {
        public int StorageLocation { get; set; }
        public string Sizes { get; set; }
        public List<FileSize> FileSizes
        {
            get
            {
                List<FileSize> fileSizes = new List<FileSize>();
                if (!string.IsNullOrEmpty(Sizes))
                    foreach (var size in Sizes.Split(';'))
                    {
                        try
                        {
                            fileSizes.Add(new FileSize()
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
        public AllowedTypes AllowedTypes { get; set; }
        public UploadStyle UploadStyle { get; set; }
    }

    [Serializable]
    public class FileSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public enum AllowedTypes
    {
        ImagesOnly = 0,
        FilesOnly = 1,
        BothOfThem = 2
    }
    public enum UploadStyle
    {
        OriginalOnly = 0,
        CopyOnly = 1,
        BothOfThem = 2
    }
    public enum FileType
    {
        None = 0,
        Image = 1,
        Doc = 2,
        Exel = 3,
        PDF = 4,
        Vedio = 5
    }
    ///////////////////////////////////////End UploadData With Options////////////////////////////////////////////////////////
    #endregion
    public class ViewDataUploadFilesResult
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