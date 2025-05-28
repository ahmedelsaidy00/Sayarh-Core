using ImageMagick;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sayarah.WebApi.Api.Controllers
{
    public class UploadWebPController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadWebPController(IWebHostEnvironment env)
        {
            _env = env;
        }

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
                10 => Path.Combine(basePath, "MaintainTransOut"),
                11 => Path.Combine(basePath, "WashTransOut"),
                12 => Path.Combine(basePath, "OilTransOut"),
                _ => Path.Combine(basePath),
            };
        }

        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadPhotoWebP(IFormFile file, NewUploadFilesDto input)
        {
            var statuses = new List<NewViewDataUploadFilesResult>();
            if (file == null || file.Length <= 0)
                return BadRequest("No file uploaded.");

            string[] allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp", "image/*" };
            string fileName = Path.GetFileName(file.FileName);
            string ext = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid() + ext;
            string uniqueFileNameWebp = Guid.NewGuid() + ".webp";

            string storageRoot = GetStorageRoot(input.StorageLocation);
            Directory.CreateDirectory(storageRoot);

            string fullOriginalPath = Path.Combine(storageRoot, uniqueFileName);
            string fullWebpPath = Path.Combine(storageRoot, uniqueFileNameWebp);

            if (!allowedImageTypes.Contains(file.ContentType.ToLower()))
            {
                using (var stream = new FileStream(fullOriginalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                statuses.Add(new NewViewDataUploadFilesResult
                {
                    name = fileName,
                    uniqueName = uniqueFileName,
                    physicalPath = fullOriginalPath,
                    type = file.ContentType,
                    url = "/Upload/Download/" + uniqueFileName,
                    delete_url = "/Upload/Delete/" + uniqueFileName,
                    delete_type = "GET"
                });
            }
            else
            {
                byte[] photoBytes;
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    photoBytes = ms.ToArray();
                }

                if (input.UploadStyle == NewUploadStyle.OriginalOnly || input.UploadStyle == NewUploadStyle.BothOfThem)
                {
                    using (var image = new MagickImage(photoBytes))
                    {
                        image.Format = MagickFormat.WebP;
                        image.Quality = 100;
                        image.Write(fullWebpPath);
                    }
                }

                if (input.UploadStyle == NewUploadStyle.CopyOnly || input.UploadStyle == NewUploadStyle.BothOfThem)
                {
                    foreach (var size in input.FileSizes)
                    {
                        string resizedName = $"{size.Width}x{size.Height}_{uniqueFileNameWebp}";
                        string resizedPath = Path.Combine(storageRoot, resizedName);

                        using (var image = new MagickImage(photoBytes))
                        {
                            image.Format = MagickFormat.WebP;
                            image.Quality = 100;

                            var geometry = new MagickGeometry(Convert.ToUInt32(size.Width), Convert.ToUInt32(size.Height))
                            {
                                IgnoreAspectRatio = true // Stretch
                            };
                            image.Resize(geometry);

                            image.Write(resizedPath);
                        }
                    }
                }

                statuses.Add(new NewViewDataUploadFilesResult
                {
                    name = fileName,
                    uniqueName = uniqueFileNameWebp,
                    physicalPath = fullWebpPath,
                    type = "image/webp",
                    url = "/Upload/Download/" + uniqueFileNameWebp,
                    delete_url = "/Upload/Delete/" + uniqueFileNameWebp,
                    delete_type = "GET"
                });
            }

            return Ok(uniqueFileNameWebp);
        }
        private void ConvertToWebP(Stream input, string outputPath)
        {
            using var image = new MagickImage(input);
            image.Format = MagickFormat.WebP;
            image.Quality = 100;
            image.Write(outputPath);
        }
    }


    public class NewUploadFilesDto
    {
        public int StorageLocation { get; set; }
        public string Sizes { get; set; }
        public NewAllowedTypes AllowedTypes { get; set; }
        public NewUploadStyle UploadStyle { get; set; }

        public List<NewFileSize> FileSizes
        {
            get
            {
                var sizes = new List<NewFileSize>();
                if (!string.IsNullOrEmpty(Sizes))
                {
                    foreach (var size in Sizes.Split(';'))
                    {
                        try
                        {
                            var dimensions = size.Split('&');
                            sizes.Add(new NewFileSize
                            {
                                Width = int.Parse(dimensions[0]),
                                Height = int.Parse(dimensions[1])
                            });
                        }
                        catch { }
                    }
                }
                return sizes;
            }
        }
    }

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
