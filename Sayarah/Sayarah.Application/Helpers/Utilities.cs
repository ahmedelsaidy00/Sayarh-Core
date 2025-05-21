using Abp.Localization;
using Newtonsoft.Json;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Core.Helpers;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Text;
using System.Data;
using ClosedXML.Excel;

namespace Sayarah.Application.Helpers
{
    public static class Utilities
    {
        public static string StorageRoot(string path)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
        public static string APIKey = "AIzaSyA3Qs10RckYmnNww";
        public static string SENDER_ID = "456456555555555555555555555";

        public static Dictionary<int, string> filePathes = new Dictionary<int, string>()
            {
                {0,"files"},
                {1,"files\\Users"},
                {2,"files\\SitePages\\About"},
                {3,"files\\SitePages\\Index"},
                {4,"files\\Companies"},
                {5,"files\\Veichles"},
                {6,"files\\Drivers"},
                {7,"files\\Providers"},
                {8,"files\\Workers"},
                {9,"files\\FuelTransOut"},
                {10,"files\\FuelPriceChangeRequests"},
                {11,"files\\MaintainTransOut"},
                {12,"files\\WashTransOut"},
                {13,"files\\OilTransOut"},
                {14,"files\\Banks"},
                {15,"files\\Wallet"},
                {16,"files\\AnnouncmentBanners"},
                {17,"files\\Tickets"},
                {18,"files\\Vouchers"},
                {19,"files\\Invoices"}
            };

        public static Dictionary<int, string> servicesDictionary = new Dictionary<int, string>
            {
                { 1, "سوبر ماركت" },
                { 2, "صراف آلي" }
            };

        public static string GenerateImageName(string imageBase64String, string oldName)
        {
            string imageName = oldName;
            try
            {
                if (string.IsNullOrEmpty(imageName))
                    imageName = Guid.NewGuid().ToString() + ".jpeg";
                byte[] bytes = Convert.FromBase64String(imageBase64String);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image imgOriginal = Image.FromStream(ms, true);
                    if (imgOriginal.RawFormat.Equals(ImageFormat.Gif))
                        imageName = imageName.Replace(".jpeg", ".gif");
                    else if (imgOriginal.RawFormat.Equals(ImageFormat.Png))
                        imageName = imageName.Replace(".jpeg", ".png");
                }
                return imageName;
            }
            catch (Exception)
            {
                return imageName;
            }
        }

        public static bool SaveImageFromBase64String(string imageName, string folderName, string imageBase64String, int maxWidth, int maxHeight)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(imageBase64String);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image imgOriginal = Image.FromStream(ms, true);
                    string thumbnailFullPath = Path.Combine(StorageRoot(folderName), "thumbnail_" + imageName);
                    if (imgOriginal.RawFormat.Equals(ImageFormat.Gif))
                    {
                        imgOriginal.Save(thumbnailFullPath, ImageFormat.Gif);
                        imgOriginal.Dispose();
                    }
                    else if (imgOriginal.RawFormat.Equals(ImageFormat.Png))
                    {
                        Image imgActual = Scale(imgOriginal, maxWidth, maxHeight);
                        imgActual.Save(thumbnailFullPath, ImageFormat.Png);
                        imgOriginal.Dispose();
                        imgActual.Dispose();
                    }
                    else
                    {
                        Image imgActual = Scale(imgOriginal, maxWidth, maxHeight);
                        imgActual.Save(thumbnailFullPath, ImageFormat.Jpeg);
                        imgOriginal.Dispose();
                        imgActual.Dispose();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SaveImageFromBase64String(string imageName, string folderName, string imageBase64String, int maxWidth, int maxHeight, bool apply_gif)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(imageBase64String);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image imgOriginal = Image.FromStream(ms, true);
                    string thumbnailFullPath = Path.Combine(StorageRoot(folderName), "thumbnail_" + imageName);
                    if (imgOriginal.RawFormat.Equals(ImageFormat.Gif) && apply_gif)
                    {
                        imgOriginal.Save(thumbnailFullPath, ImageFormat.Gif);
                        imgOriginal.Dispose();
                    }
                    else if (imgOriginal.RawFormat.Equals(ImageFormat.Png))
                    {
                        Image imgActual = Scale(imgOriginal, maxWidth, maxHeight);
                        imgActual.Save(thumbnailFullPath, ImageFormat.Png);
                        imgOriginal.Dispose();
                        imgActual.Dispose();
                    }
                    else
                    {
                        Image imgActual = Scale(imgOriginal, maxWidth, maxHeight);
                        imgActual.Save(thumbnailFullPath, ImageFormat.Jpeg);
                        imgOriginal.Dispose();
                        imgActual.Dispose();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Image Scale(Image imgPhoto, int Width, int Height)
        {
            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;
            float destHeight = 0;
            float destWidth = 0;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;
            if (Width != 0 && Height != 0)
            {
                destWidth = Width;
                destHeight = Height;
            }
            else if (Height != 0)
            {
                destWidth = (float)(Height * sourceWidth) / sourceHeight;
                destHeight = Height;
            }
            else
            {
                destWidth = Width;
                destHeight = (float)(sourceHeight * Width / sourceWidth);
            }
            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight, PixelFormat.Format32bppPArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                    new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

        public static string GetCurrencies()
        {
            try
            {
                string sResponseFromServer = string.Empty;
                string url = @"https://api.coinmarketcap.com/v1/ticker/?limit=0";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    sResponseFromServer = reader.ReadToEnd();
                }
                return sResponseFromServer;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static void PushFcmMessage(string[] recieverDevicesId, long chatId, long swapId, long senderId, string senderName, string senderAvatar, string message, long messageId)
        {
            try
            {
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.UseDefaultCredentials = true;
                tRequest.PreAuthenticate = true;
                tRequest.Credentials = CredentialCache.DefaultCredentials;
                var data = new
                {
                    registration_ids = recieverDevicesId,
                    data = new
                    {
                        chatId,
                        swapId,
                        senderId,
                        senderName,
                        senderAvatar,
                        message,
                        messageId
                    },
                    priority = "high"
                };
                var json = JsonConvert.SerializeObject(data);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add($"Authorization: key={APIKey}");
                tRequest.Headers.Add($"Sender: id={SENDER_ID}");
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    {
                        string sResponseFromServer = tReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void PushFcmNotification(string[] recieverDevicesId, long swapId, long senderId, string senderName, string senderAvatar, string text, long notificationId)
        {
            try
            {
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.UseDefaultCredentials = true;
                tRequest.PreAuthenticate = true;
                tRequest.Credentials = CredentialCache.DefaultCredentials;
                var data = new
                {
                    registration_ids = recieverDevicesId,
                    notification = new
                    {
                        swapId,
                        senderId,
                        senderName,
                        senderAvatar,
                        message = text,
                        messageId = notificationId
                    }
                };
                var json = JsonConvert.SerializeObject(data);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add($"Authorization: key={APIKey}");
                tRequest.Headers.Add($"Sender: id={SENDER_ID}");
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    {
                        string sResponseFromServer = tReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static string GregToHijri(DateTime dt, string format)
        {
            try
            {
                CultureInfo arSA = CultureInfo.CreateSpecificCulture("ar-SA");
                string s = dt.ToString(format, arSA);
                return s;
            }
            catch (Exception) { return ""; }
        }

        public static bool DeleteImage(int pathKey, string fileName, string[] otherExtensions = null)
        {
            try
            {
                string filePath = StorageRoot(string.Empty) + filePathes[pathKey] + "\\" + fileName;
                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (otherExtensions != null && otherExtensions.Length > 0)
                    foreach (string str in otherExtensions)
                    {
                        filePath = StorageRoot(string.Empty) + filePathes[pathKey] + "\\" + str + fileName;
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckExistImage(int pathKey, string fileName)
        {
            string filePath = StorageRoot(filePathes[pathKey]) + "\\" + fileName;
            return File.Exists(filePath);
        }

        public static string GetYouTubeThumbnail(string YoutubeUrl, YouTubeThumbQuality thumbQuality)
        {
            string youTubeThumb = string.Empty;
            if (string.IsNullOrEmpty(YoutubeUrl))
                return "";

            if (YoutubeUrl.IndexOf("=") > 0)
            {
                youTubeThumb = YoutubeUrl.Split('=')[1];
            }
            else if (YoutubeUrl.IndexOf("/v/") > 0)
            {
                string strVideoCode = YoutubeUrl.Substring(YoutubeUrl.IndexOf("/v/") + 3);
                int ind = strVideoCode.IndexOf("?");
                youTubeThumb = strVideoCode.Substring(0, ind == -1 ? strVideoCode.Length : ind);
            }
            else if (YoutubeUrl.IndexOf('/') < 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[3];
            }
            else if (YoutubeUrl.IndexOf('/') > 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[1];
            }
            switch (thumbQuality)
            {
                case YouTubeThumbQuality.Mqdefault:
                    return "https://img.youtube.com/vi/" + youTubeThumb + "/mqdefault.jpg";
                case YouTubeThumbQuality.Hqdefault:
                    return "https://img.youtube.com/vi/" + youTubeThumb + "/hqdefault.jpg";
                case YouTubeThumbQuality.Hd720:
                    return "https://img.youtube.com/vi/" + youTubeThumb + "/hq720.jpg";
                default:
                    return "https://img.youtube.com/vi/" + youTubeThumb + "/hqdefault.jpg";
            }
        }

        public static string L(ILocalizationManager localizationManager, string name)
        {
            return localizationManager.GetString(SayarahConsts.LocalizationSourceName, name);
        }

        public static string AddSpaces(string text)
        {
            StringBuilder spacedText = new StringBuilder();
            foreach (char c in text)
            {
                spacedText.Append(c);
                spacedText.Append(" ");
            }
            return spacedText.ToString().Trim();
        }

        public static FileType CheckFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            switch (extension.ToLower())
            {
                case ".txt":
                    return FileType.None;
                case ".doc":
                case ".docx":
                    return FileType.Doc;
                case ".xls":
                case ".xlsx":
                    return FileType.Exel;
                case ".ppt":
                case ".pptx":
                    return FileType.Exel;
                case ".pdf":
                    return FileType.PDF;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    return FileType.Image;
                default:
                    return FileType.Image;
            }
        }

        public static string CreateExcelEpPlus(System.Data.DataTable dt, string mediaPath, string mediaName, string title)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var excel = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add(dt.TableName);
                worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                worksheet.TabColor = System.Drawing.Color.Black;
                worksheet.DefaultRowHeight = 12;
                worksheet.DefaultColWidth = 25;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                worksheet.Row(1).Height = 20;
                int colCount = worksheet.Dimension.End.Column;
                string headerRange = "A1:" + Char.ConvertFromUtf32(colCount + 64) + "1";
                worksheet.Cells[headerRange].AutoFilter = true;
                worksheet.Cells[headerRange].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[headerRange].Style.Font.Bold = true;
                worksheet.Cells[headerRange].Style.Font.Size = 14;
                worksheet.Cells[headerRange].Style.Font.Color.SetColor(System.Drawing.Color.White);
                worksheet.Cells[headerRange].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[headerRange].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);

                if (!string.IsNullOrEmpty(mediaName))
                {
                    foreach (string file in Directory.EnumerateFiles(StorageRoot(mediaPath), "*.xlsx"))
                    {
                        File.Delete(file);
                    }
                }

                mediaName = string.IsNullOrEmpty(mediaName) ? Guid.NewGuid().ToString() : mediaName;
                mediaName = mediaName + ".xlsx";

                FileInfo excelFile = new FileInfo(StorageRoot(mediaPath + mediaName));
                excel.SaveAs(excelFile);

                return mediaName;
            }
        }
        #region Excel
        public static string CreateExcelEpPlusV2(DataSet dataSet, string filePath, string fileName, string title, ExcelSource source)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Combined Data");
                int startRow = 1;
                int tableNum = 1;

                foreach (DataTable table in dataSet.Tables)
                {
                    if (tableNum < dataSet.Tables.Count)
                    {
                        if (source == ExcelSource.FuelTransactions)
                        {
                            worksheet.Cells["A1:G1"].Merge = true;
                            worksheet.Cells["A1"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName} ";
                            if (table.Columns.Count > 2)
                                worksheet.Cells["A1"].Value += $" - {table.Columns[2].ColumnName} ";
                            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        else if (source == ExcelSource.AccountStatment)
                        {
                            worksheet.Cells["A1:E1"].Merge = true;
                            worksheet.Cells["A1"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName} - {table.Columns[2].ColumnName}";
                            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        else if (source == ExcelSource.Revenue || source == ExcelSource.BankInfo)
                        {
                            worksheet.Cells["A1:D1"].Merge = true;
                            worksheet.Cells["A1"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName}";
                            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        else if (source == ExcelSource.BranchConsumption)
                        {
                            if (tableNum == 1)
                            {
                                worksheet.Cells["A1:F1"].Merge = true;
                                worksheet.Cells["A1"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName} ";
                                if (table.Columns.Count > 2)
                                    worksheet.Cells["A1"].Value += $" - {table.Columns[2].ColumnName} ";
                                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }
                            else if (tableNum == 2)
                            {
                                worksheet.Cells["A3:F3"].Merge = true;
                                worksheet.Cells["A3"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName} ";
                                if (table.Columns.Count > 2)
                                    worksheet.Cells["A3"].Value += $" - {table.Columns[2].ColumnName} ";
                                worksheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                worksheet.Cells["A3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }
                            else if (tableNum == 3)
                            {
                                worksheet.Cells["A5:F5"].Merge = true;
                                worksheet.Cells["A5"].Value = $"{table.Columns[0].ColumnName} - {table.Columns[1].ColumnName} ";
                                if (table.Columns.Count > 2)
                                    worksheet.Cells["A5"].Value += $" - {table.Columns[2].ColumnName} ";
                                worksheet.Cells["A5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                worksheet.Cells["A5"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }
                        }
                    }
                    else
                    {
                        worksheet.Cells[startRow, 1].LoadFromDataTable(table, true);
                        using (var headerRange = worksheet.Cells[startRow, 1, startRow, table.Columns.Count])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                    }
                    startRow += table.Rows.Count + 2;
                    tableNum++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                if (!string.IsNullOrEmpty(filePath))
                {
                    foreach (string file in Directory.EnumerateFiles(StorageRoot(filePath), "*.xlsx"))
                    {
                        File.Delete(file);
                    }
                }

                fileName = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName;
                fileName = fileName + ".xlsx";

                FileInfo excelFile = new FileInfo(StorageRoot(filePath + fileName));
                package.SaveAs(excelFile);

                return fileName;
            }
        }    
        public static string SaveExcel(DataTable dt, string mediaPath, string mediaName, string title)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                // Create the excel file and add worksheet
                //  XLWorkbook wb = new XLWorkbook();

                IXLWorksheet workSheet = wb.Worksheets.Add(dt.TableName);

                // alighn all text of cells in worksheet
                workSheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                workSheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Hardcode title and contents locations
                IXLCell titleCell = workSheet.Cell(1, 1);
                IXLCell contentsCell = workSheet.Cell(2, 1);

                //Pretty-up the title
                titleCell.Value = title;
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontColor = XLColor.White;
                titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(75, 141, 248);

                // Merge cells for title
                workSheet.Range(titleCell, workSheet.Cell(1, dt.Columns.Count + 6)).Merge();

                // change height for title
                var titleRow = workSheet.Row(1);
                titleRow.ClearHeight();
                titleRow.Height = 30;


                // Insert table contents, and adjust for content width
                contentsCell.InsertTable(dt);


                // Change the background color of the headers
                var rngColsHeaders = workSheet.Range(contentsCell, workSheet.Cell(2, dt.Columns.Count));
                rngColsHeaders.Style.Fill.BackgroundColor = XLColor.FromArgb(38, 52, 75);


                // change width for conent
                for (int i = 1; i <= dt.Columns.Count; i++)
                {
                    var col = workSheet.Column(i);
                    col.Width = 20;
                }

                if (!string.IsNullOrEmpty(mediaName))
                {
                    foreach (string file in Directory.EnumerateFiles(StorageRoot(mediaPath), "*.xlsx"))
                    {
                        System.IO.File.Delete(file);
                    }
                }

                mediaName = string.IsNullOrEmpty(mediaName) ? Guid.NewGuid().ToString() : mediaName;
                mediaName = mediaName + ".xlsx";

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    File.WriteAllBytes(StorageRoot(mediaPath + mediaName), ms.GetBuffer());

                    ms.Close();
                }
                return mediaName;

            }
        }
        public static DataTable ToDataTableForExcel<T>(IList<T> data, List<DisplayedColumn> displayedColumns, ILocalizationManager localizationManager)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                var displayedColumn = displayedColumns.Find(x => x.Name == prop.Name);
                if (displayedColumn != null)
                {
                    table.Columns.Add(displayedColumn.Title, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);


                }
            }

            if (table.Columns.Count <= 0)
                return null;
            foreach (T item in data)
            {
                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    var displayedColumn = displayedColumns.Find(x => x.Name == prop.Name);
                    if (displayedColumn != null)
                    {
                        if (prop.GetValue(item) != null)
                        {
                            try
                            {
                                if (prop.GetValue(item).ToString().StartsWith("L."))
                                {
                                    row[displayedColumn.Title] = L(localizationManager, prop.GetValue(item).ToString().Replace("L.", string.Empty));
                                }
                                else
                                    row[displayedColumn.Title] = prop.GetValue(item);
                            }
                            catch (Exception ex)
                            {
                                row[displayedColumn.Title] = prop.GetValue(item);
                            }
                        }
                        else
                            row[displayedColumn.Title] = DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }

            return table;
        }
        public static DataSet ToDataTableForExcelV2<T>(IList<T> data, List<DisplayedColumn> displayedColumns, ILocalizationManager localizationManager, List<RequestFuelExcelOptionsDto> options)
        {
            // Create a DataSet
            DataSet dataSet = new DataSet();


            // First DataTable for Column1, Column2, Column3
            DataTable headerTable = new DataTable("HeaderTable");

            DataTable dateTable_1 = new DataTable("dateTable_1");
            bool addTable1 = false;
            DataTable dateTable_2 = new DataTable("dateTable_2");
            bool addTable2 = false;

            if (options.Count > 0)
            {
                int i = 0;
                foreach (var option in options)
                {

                    if (i == 0)
                    {
                        foreach (var item in option.KeyValues.ToList())
                        {
                            headerTable.Columns.Add(item.Value);
                        }
                    }

                    if (i == 1)
                    {
                        foreach (var item in option.KeyValues.ToList())
                        {
                            dateTable_1.Columns.Add(item.Value);
                        }

                        addTable1 = true;
                    }

                    if (i == 2)
                    {
                        foreach (var item in option.KeyValues.ToList())
                        {
                            dateTable_2.Columns.Add(item.Value);
                        }
                        addTable2 = true;
                    }
                    i++;
                }
            }



            //// Add two rows after HeaderTable
            //DataRow additionalRow1 = headerTable.NewRow();
            //DataRow additionalRow2 = headerTable.NewRow();

            //// Populate the additional rows with desired values
            //foreach (var column in headerTable.Columns.Cast<DataColumn>())
            //{
            //    additionalRow1[column.ColumnName] = "Additional Row 1 Value"; // Replace with actual values
            //    additionalRow2[column.ColumnName] = "Additional Row 2 Value"; // Replace with actual values
            //}

            //headerTable.Rows.Add(additionalRow1);
            //headerTable.Rows.Add(additionalRow2);


            // Add the header table to the DataSet
            dataSet.Tables.Add(headerTable);

            if (addTable1 == true)
                dataSet.Tables.Add(dateTable_1);
            if (addTable2 == true)
                dataSet.Tables.Add(dateTable_2);

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            // Second DataTable for the actual data
            DataTable mainTable = new DataTable("MainTable");

            foreach (PropertyDescriptor prop in properties)
            {
                var displayedColumn = displayedColumns.Find(x => x.Name == prop.Name);
                if (displayedColumn != null)
                {
                    mainTable.Columns.Add(displayedColumn.Title, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            if (mainTable.Columns.Count <= 0)
                return null;
            foreach (T item in data)
            {
                DataRow row = mainTable.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    var displayedColumn = displayedColumns.Find(x => x.Name == prop.Name);
                    if (displayedColumn != null)
                    {
                        if (prop.GetValue(item) != null)
                        {
                            try
                            {
                                if (prop.GetValue(item).ToString().StartsWith("L."))
                                {
                                    row[displayedColumn.Title] = L(localizationManager, prop.GetValue(item).ToString().Replace("L.", string.Empty));
                                }
                                else
                                    row[displayedColumn.Title] = prop.GetValue(item);
                            }
                            catch (Exception ex)
                            {
                                row[displayedColumn.Title] = prop.GetValue(item);
                            }
                        }
                        else
                            row[displayedColumn.Title] = DBNull.Value;
                    }
                }
                mainTable.Rows.Add(row);
            }

            dataSet.Tables.Add(mainTable);

            return dataSet;
        }
        #endregion
    }
}
