using Abp.Authorization;
using Abp.Configuration;
using Abp.Net.Mail;
using Abp.UI;
using Microsoft.Data.SqlClient;
using Sayarah.Application;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Configuration;
using Sayarah.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace Sayarah.Application.Helpers
{
    public class CommonAppService : SayarahAppServiceBase, ICommonAppService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ISettingManager _settingManager;

        private readonly List<string> blockedKeys = new List<string>() { "quoted_identifier", "exec ", "truncate", "alter",
            "tablehasidentity", "constraint", "delete ", "sp_msforeachtable", "checkident", "sp_", "nocheck",
            "sys.", "disable", "enable", "dbcc", "objectproperty", "identity_columns", "object_id", "table" };
        public CommonAppService(ICommonRepository commonRepository, ISettingManager settingManager)
        {
            _commonRepository = commonRepository;
            _settingManager = settingManager;
        }
        private bool CheckAddWhere(string key)
        {
            if (string.IsNullOrEmpty(key))
                return true;
            foreach (string item in blockedKeys)
            {
                if (key.ToLower().Contains(item))
                    return false;
            }
            return true;
        }
        public async Task<string> GetNextCode(GetNextCodeInputDto input)
        {
            if (!CheckAddWhere(input.AddWhere))
                return null;
            return await _commonRepository.GetNextCode(input.TableName, input.CodeField, input.AddWhere);
        }
        public async Task<int> ExecuteSqlAsync(string _apikey, string _addApikey, string sql)
        {
            if (_apikey == await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey) && _addApikey == await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey))
                return await _commonRepository.ExecuteSqlAsync(sql);
            else
                return 0;
        }

        public async Task<List<T>> GetListAsync<T>(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class
        {
            if (_apikey == await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey) && _addApikey == await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey))
                return await _commonRepository.GetListAsync<T>(storedProcedureName, storedProcedureParameter);
            else
                return null;
        }

        public async Task<(List<T> Items, int TotalCount)> GetListWithCountAsync<T>(
            string _apikey,
            string _addApikey,
            string storedProcedureName,
            List<SqlParameter> storedProcedureParameter) where T : class, new()
        {
            try
            {
                // Validate API keys
                var webApiKey = await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey);
                var addApiKey = await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey);

                if (_apikey == webApiKey && _addApikey == addApiKey)
                {
                    // Call the common repository method to execute the stored procedure
                    return await _commonRepository.GetListWithCountAsync<T>(storedProcedureName, storedProcedureParameter);
                }
                else
                {
                    // Return empty result if validation fails
                    return (new List<T>(), 0);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                throw new ApplicationException("An error occurred while fetching the data.", ex);
            }
        }

        public async Task<T> GetAsync<T>(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class
        {
            if (_apikey == await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey) && _addApikey == await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey))
                return await _commonRepository.GetAsync<T>(storedProcedureName, storedProcedureParameter);
            else
                return null;
        }

        public async Task ExecuteStoredProcedureAsync(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter)
        {
            if (_apikey == await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey) && _addApikey == await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey))
                await _commonRepository.ExecuteStoredProcedureAsync(storedProcedureName, storedProcedureParameter);
        }

        ////SitePages
        public async Task<string> GetNextKey(int pageSectionId, int pageCategoryId)
        {
            return await _commonRepository.GetNextKey(pageSectionId, pageCategoryId);
        }

        //[UnitOfWork(false)]
        //public async Task DBBackup()
        //{
        //    string dbName = "HRMSDB";
        //    string path = System.AppDomain.CurrentDomain.BaseDirectory +
        //        @"files\" + dbName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
        //    List<SqlParameter> sqlParameters = new List<SqlParameter>();
        //    sqlParameters.Add(new SqlParameter("@Path", path));
        //    sqlParameters.Add(new SqlParameter("@DBName", dbName));

        //    await ExecuteStoredProcedureAsync("SaveDBBackUp", sqlParameters);
        //}
        public async Task<bool> CheckRelatedTable(string TableName, long ID, string ExceptTableName)
        {
            if (ExceptTableName == null || ExceptTableName == string.Empty)
                ExceptTableName = "'''0'''";
            else
            {
                string[] _ExceptTableName = ExceptTableName.Split(',');
                ExceptTableName = "";
                for (int i = 0; i < _ExceptTableName.Length; i++)
                {
                    if (ExceptTableName != string.Empty)
                        ExceptTableName += ",";
                    ExceptTableName += "'" + _ExceptTableName[i] + "'";
                    //ExceptTableName += _ExceptTableName[i];
                }
            }

            return await _commonRepository.CheckRelatedTable(TableName, ID, ExceptTableName);
        }


        //public async Task<GetDashboardStatisticsOutput> GetNumbers(GetDashboardStatisticsInput input)
        //{
        //    try
        //    {
        //        GetDashboardStatisticsOutput result = new GetDashboardStatisticsOutput();
        //        //result.BestActives = await _commonRepository.ExecuteSql2Async<BestActiveDto>("Select * from TopWanted");

        //        //    List<SqlParameter> sqlParams = new List<SqlParameter>
        //        //{
        //        //    new SqlParameter("@FiltrationTime", (byte)input.FiltrationTime)
        //        //};
        //        //    result.MostActiveItems = await _commonRepository.GetListAsync<MostActiveDto>("MostActiveUsers", sqlParams);

        //        List<SqlParameter> sqlParams1 = new List<SqlParameter>
        //    {
        //        new SqlParameter("@FiltrationTime", (byte)input.ProjectType)
        //    };
        //        result.HomePageData = await _commonRepository.GetAsync<IndexStatisticsDto>("PopulateHomePageData", sqlParams1);

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<BestActives> GetBestSellerUsers()
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
            };
            var homePageData = await _commonRepository.ExecuteSql2Async<BestActiveDto>("Select * from TopWantedUsers");
            return new BestActives { BestActivesUsers = homePageData };
        }

        public async Task<BestActiveProducts> GetBestSellerProducts()
        {
            /* HomePageData */
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
            };
            //var homePageData = await _commonRepository.GetAsync<HomePageDataDto>("PopulateHomePageData", sqlParameters);
            var homePageData = await _commonRepository.ExecuteSql2Async<BestActiveProductDto>("Select * from TopWantedProducts");
            return new BestActiveProducts { BestActivesProducts = homePageData };
        }

        public async Task<bool> SendEmail(SendEmailRequest input)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                #region Header&Logo
                string webRootPath = AppDomain.CurrentDomain.BaseDirectory;
                string logoPath = Path.Combine(webRootPath, SayarahConsts.MailLogoPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                LinkedResource inlineLogo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                inlineLogo.ContentId = Guid.NewGuid().ToString();
                content.Append("<div style=\"text-align:center;width:100%;background-color:#f5f5f5;padding:50px 0\">");
                content.Append("<div style=\"width:100%;max-width: 600px;display:inline-block;background: #fff;\">");
                content.Append(string.Format("<div style=\"width:100%;background:#350d53;padding:15px 0\"><img style=\"width:80px\" src=\"cid:{0}\" id=\"img\" /></div>", inlineLogo.ContentId));
                #endregion

                #region body
                content.Append(string.Format("<p style=\"font-size:16px;margin:30px 0 0;color:#333;\"><strong>{0}</strong></p>", input.Subject));
                if (input.datalst != null && input.datalst.Length > 0)
                {
                    for (int i = 0; i < input.datalst.Length; i++)
                    {
                        content.Append(string.Format("<p style=\"font-size:30px;margin:5px 0 30px;color:#333;\"><strong>{0}</strong></p>", input.datalst[i]));
                    }
                }
                content.Append("<div style=\"margin-top:30px\">");
                if (!string.IsNullOrEmpty(input.ParamsHeader))
                    content.Append(string.Format("<p style=\"font-size:14px;color:#333;\">{0}</p>", input.ParamsHeader));
                if (input.Paramslst != null && input.Paramslst.Count > 0)
                {
                    foreach (var item in input.Paramslst)
                    {
                        content.Append(string.Format("<div style=\"display:inline-block;width:95%;\"><p style=\"width:35%;float:right;background:#0e74bc;color:#fff;padding:5px;margin:0 0 0 5px;\"><b>{0}</b></p><p style=\"color:#333;width:57%;float:right;background: whitesmoke;padding:5px 20px 5px 5px;margin: 0 0 5px 0;text-align:right;\">{1}</p></div>", item.Key, item.Value));
                    }
                }
                if (!string.IsNullOrEmpty(input.Url))
                    content.Append(string.Format("<a href=\"{0}\" style=\"display:inline-block;text-decoration:none;border:0;padding:15px 20px;border-radius:6px;background-color:#e4bc76;font-size:20px;color:#ffffff;width:300px;margin-top:20px;\" class=\"button_link\">{1}</a>", input.Url, L(input.UrlTitle)));
                #endregion

                #region Footer
                content.Append(string.Format("</div><div><p style=\"padding:20px;background: #efefe9;margin: 30px 0 0;color:#333;\"><strong>{0} {1}</strong></p></div></div></div>", L("Common.CopyRights", DateTime.Now.Year), L("Common.Al7osam")));

                #endregion

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content.ToString(), null, "text/html");
                htmlView.LinkedResources.Add(inlineLogo);
                MailMessage _mail = new MailMessage();
                _mail.Body = content.ToString();
                _mail.Subject = L("Common.SystemTitle");
                _mail.IsBodyHtml = input.IsBodyHtml;
                foreach (var mail in input.Emails)
                {
                    _mail.To.Add(mail);

                }

                _mail.AlternateViews.Add(htmlView);
                await Mailer.SendEmailAsync(_mail, await GetSettings(), true);
                return true;
            }


            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        async Task<MailData> GetSettings()
        {
            var result = new MailData()
            {
                Host = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host),
                Password = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password),
                Port = await SettingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port),
                Sender = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName),
                DefaultFromDisplayName = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName)
            };
            return result;
        }

        public async Task<bool> HandleSiteMap()
        {
            try
            {
                List<string> _urlListStr = new List<string>();

                _urlListStr.Add("https://alshurafa.com.sa/");
                _urlListStr.Add("https://alshurafa.com.sa/عن-الشرافة");
                _urlListStr.Add("https://alshurafa.com.sa/page-404/ar.html");
                _urlListStr.Add("https://alshurafa.com.sa/الشرافة-جورمية");
                _urlListStr.Add("https://alshurafa.com.sa/معرض-الصور");
                _urlListStr.Add("https://alshurafa.com.sa/عملائنا");
                _urlListStr.Add("https://alshurafa.com.sa/قاعة-المناسبات");
                _urlListStr.Add("https://alshurafa.com.sa/الحفلات-الخارجية");
                _urlListStr.Add("https://alshurafa.com.sa/اتصل-بنا");


                List<Url> _urlList = new List<Url>();

                foreach (var item in _urlListStr)
                {
                    Url _url = new Url
                    {
                        Lastmod = DateTime.Now,
                        Loc = item,
                        Priority = 0.8
                    };

                    _urlList.Add(_url);

                }

                Urlset _urlSet = new Urlset
                {
                    Xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9",
                    Url = _urlList
                };

                XmlSerializer writer =
                     new XmlSerializer(typeof(Urlset), "http://www.sitemaps.org/schemas/sitemap/0.9");


                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sitemap.xml");

                FileStream file = File.Create(path);

                writer.Serialize(file, _urlSet);
                file.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<FrequencyOutput>> GetFrequencyStations(GetFrequencyStationInput input)
        {
            try
            {
                List<FrequencyOutput> result = new List<FrequencyOutput>();

                List<SqlParameter> sqlParams = new List<SqlParameter>();


                if (!string.IsNullOrEmpty(input.PeriodFromString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodFromString) ?
                                        DateTime.ParseExact(input.PeriodFromString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    input.PeriodFrom = _reservationDate;

                    sqlParams.Add(new SqlParameter("@PeriodFrom", input.PeriodFrom.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.PeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodToString) ?
                                        DateTime.ParseExact(input.PeriodToString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;
                    input.PeriodTo = _reservationDate;
                    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.VeichleType.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleType", input.VeichleType.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleType", DBNull.Value));

                if (input.DriverId.HasValue)
                    sqlParams.Add(new SqlParameter("@DriverId", input.DriverId.Value));
                else
                    sqlParams.Add(new SqlParameter("@DriverId", DBNull.Value));

                if (input.VeichleId.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleId", input.VeichleId.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleId", DBNull.Value));

                if (input.FuelType.HasValue)
                    sqlParams.Add(new SqlParameter("@FuelType", input.FuelType.Value));
                else
                    sqlParams.Add(new SqlParameter("@FuelType", DBNull.Value));

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

                result = await _commonRepository.GetListAsync<FrequencyOutput>("FrequencyOfStations", sqlParams);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<string> FrequencyExportExcelAdmin(GetFrequencyStationInput input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;


                var outPut = await GetFrequencyStations(input);

                var frequencyOutputDto = ObjectMapper.Map<List<FrequencyOutputDto>>(outPut);

                var cBList = outPut.ToList();

                if (cBList != null && cBList.Count > 0)
                {
                    var excelData = frequencyOutputDto;

                    RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();

                    List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "نوع الملف",
                        Value = L("Pages.Reports.VisitingStations")
                    });

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "تاريخ الإصدار",
                        Value = DateTime.Now.ToString()
                    });

                    _options = new RequestFuelExcelOptionsDto
                    {
                        ExcelDate = DateTime.Now.ToString(),
                        //ProviderName = _mainProvider.NameAr,
                        ExcelType = L("Permission.CompanyFrequencyReport"),
                        KeyValues = _keyValues
                    };

                    List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                    optionsList.Add(_options);

                    var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                    if (dataSet == null)
                        return string.Empty;

                    ExcelSource source = ExcelSource.FuelTransactions;
                    return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.FrequencyReport.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                    // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                }
                else
                    throw new UserFriendlyException(L("Common.EmptyListToBeExported"));

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ConsumptionOutput>> GetConsumptions(GetConsumptionInput input)
        {
            try
            {
                List<ConsumptionOutput> result = new List<ConsumptionOutput>();

                List<SqlParameter> sqlParams = new List<SqlParameter>();


                if (!string.IsNullOrEmpty(input.PeriodFromString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodFromString) ?
                                        DateTime.ParseExact(input.PeriodFromString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    input.PeriodFrom = _reservationDate;

                    sqlParams.Add(new SqlParameter("@PeriodFrom", input.PeriodFrom.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.PeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodToString) ?
                                        DateTime.ParseExact(input.PeriodToString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;
                    input.PeriodTo = _reservationDate;
                    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.VeichleType.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleType", input.VeichleType.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleType", DBNull.Value));

                if (input.DriverId.HasValue)
                    sqlParams.Add(new SqlParameter("@DriverId", input.DriverId.Value));
                else
                    sqlParams.Add(new SqlParameter("@DriverId", DBNull.Value));

                if (input.VeichleId.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleId", input.VeichleId.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleId", DBNull.Value));

                if (input.FuelType.HasValue)
                    sqlParams.Add(new SqlParameter("@FuelType", input.FuelType.Value));
                else
                    sqlParams.Add(new SqlParameter("@FuelType", DBNull.Value));

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                result = await _commonRepository.GetListAsync<ConsumptionOutput>("Consumptions", sqlParams);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<string> ConsumptionExportExcelAdmin(GetConsumptionInput input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                var outPut = await GetConsumptions(input);

                var consumptionDto = ObjectMapper.Map<List<ConsumptionOutputDto>>(outPut);

                var cBList = outPut.ToList();

                if (cBList != null && cBList.Count > 0)
                {
                    var excelData = consumptionDto;

                    RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();

                    List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "نوع الملف",
                        Value = L("Pages.Reports.VeichlesConsumption")
                        //(currentCulture.Name == "ar") ? "معدل الاستهلاك المركبة" : "Vehicle Depreciation Rate"
                    });

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "تاريخ الإصدار",
                        Value = DateTime.Now.ToString()
                    });

                    _options = new RequestFuelExcelOptionsDto
                    {
                        ExcelDate = DateTime.Now.ToString(),
                        //ProviderName = _mainProvider.NameAr,
                        ExcelType = L("Permission.CompanyConsumptionReport"),
                        KeyValues = _keyValues
                    };

                    List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                    optionsList.Add(_options);

                    var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                    if (dataSet == null)
                        return string.Empty;

                    ExcelSource source = ExcelSource.FuelTransactions;
                    return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.Reports.ConsumptionReport") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                    // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                }
                else
                    throw new UserFriendlyException(L("Common.EmptyListToBeExported"));

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<VeichleConsumptionOutput>> GetVeichleConsumptions(GetVeichleConsumptionInput input)
        {
            try
            {
                List<VeichleConsumptionOutput> result = new List<VeichleConsumptionOutput>();

                List<SqlParameter> sqlParams = new List<SqlParameter>();


                if (!string.IsNullOrEmpty(input.PeriodFromString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodFromString) ?
                                        DateTime.ParseExact(input.PeriodFromString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    input.PeriodFrom = _reservationDate;

                    sqlParams.Add(new SqlParameter("@PeriodFrom", input.PeriodFrom.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.PeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodToString) ?
                                        DateTime.ParseExact(input.PeriodToString, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;
                    input.PeriodTo = _reservationDate;
                    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                }
                else
                    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));


                if (input.VeichleId.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleId", input.VeichleId.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleId", DBNull.Value));


                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                result = await _commonRepository.GetListAsync<VeichleConsumptionOutput>("VeichleConsumptions", sqlParams);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<string> VeichleConsumptionExportExcelAdmin(GetVeichleConsumptionInput input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                var outPut = await GetVeichleConsumptions(input);

                var consumptionDto = ObjectMapper.Map<List<VeichleConsumptionOutputDto>>(outPut);

                var cBList = outPut.ToList();

                if (cBList != null && cBList.Count > 0)
                {
                    var excelData = consumptionDto;

                    RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();

                    List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "نوع الملف",
                        //Value = (currentCulture.Name == "ar") ? "الاستهلاك للمركبة" : "Vehicle Depreciation"
                        Value = L("Pages.Reports.VeichleConsumptionReport")

                    });

                    _keyValues.Add(new RequestFuelExcelOptionKeyValue
                    {
                        Key = "تاريخ الإصدار",
                        Value = DateTime.Now.ToString()
                    });

                    _options = new RequestFuelExcelOptionsDto
                    {
                        ExcelDate = DateTime.Now.ToString(),
                        //ProviderName = _mainProvider.NameAr,
                        ExcelType = L("Permission.CompanyVeichleConsumptionReport"),
                        KeyValues = _keyValues
                    };

                    List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                    optionsList.Add(_options);

                    var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                    if (dataSet == null)
                        return string.Empty;

                    ExcelSource source = ExcelSource.FuelTransactions;
                    return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.Reports.ConsumptionReport") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                    // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                }
                else
                    throw new UserFriendlyException(L("Common.EmptyListToBeExported"));

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
