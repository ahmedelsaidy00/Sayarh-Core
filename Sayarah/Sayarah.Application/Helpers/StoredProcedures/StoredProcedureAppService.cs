using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Companies;
using Sayarah.Core.Repositories;
using Sayarah.Invoices;
using Sayarah.Providers;
using Sayarah.Transactions;
using System.Globalization;


namespace Sayarah.Application.Helpers.StoredProcedures
{
    public class StoredProcedureAppService : SayarahAppServiceBase, IStoredProcedureAppService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Voucher, long> _voucherRepository;
        private readonly IRepository<FuelTransOut, long> _fuelTransOutRepository;
        public StoredProcedureAppService(
            ICommonRepository commonRepository,
            IRepository<Branch, long> branchRepository,
            IRepository<Company, long> companyRepository,
            IRepository<Provider, long> providerRepository,
            IRepository<MainProvider, long> mainProviderRepository,
            IRepository<Voucher, long> voucherRepository,
            IRepository<FuelTransOut, long> fuelTransOutRepository
            )
        {
            _commonRepository = commonRepository;
            _branchRepository = branchRepository;
            _companyRepository = companyRepository;
            _providerRepository = providerRepository;
            _mainProviderRepository = mainProviderRepository;
            _voucherRepository = voucherRepository;
            _fuelTransOutRepository = fuelTransOutRepository;
        }


        public async Task<GetTransactionsReportOutput> GetTransactionsReport(GetTransactionsReportInput input)
        {
            try
            {
                GetTransactionsReportOutput result = new GetTransactionsReportOutput();

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


                if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                {
                    DateTime _reservationDate = DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);

                    DateTime _utc = _reservationDate.ToUniversalTime();

                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.FullPeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                        DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    DateTime _utc = _reservationDate.ToUniversalTime();

                    sqlParams.Add(new SqlParameter("@FullPeriodTo", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodTo", DBNull.Value));


                if (input.TransactionType.HasValue)
                    sqlParams.Add(new SqlParameter("@TransactionType", (byte)input.TransactionType.Value));
                else
                    sqlParams.Add(new SqlParameter("@TransactionType", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));


                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));


                if (input.WorkerId.HasValue)
                    sqlParams.Add(new SqlParameter("@WorkerId", input.WorkerId.Value));
                else
                    sqlParams.Add(new SqlParameter("@WorkerId", DBNull.Value));

                if (input.VeichleId.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleId", input.VeichleId.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleId", DBNull.Value));

                if (input.DriverId.HasValue)
                    sqlParams.Add(new SqlParameter("@DriverId", input.DriverId.Value));
                else
                    sqlParams.Add(new SqlParameter("@DriverId", DBNull.Value));


                if (input.FuelType.HasValue)
                    sqlParams.Add(new SqlParameter("@FuelType", input.FuelType.Value));
                else
                    sqlParams.Add(new SqlParameter("@FuelType", DBNull.Value));



                if (!string.IsNullOrEmpty(input.Date))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.Date) ?
                                        DateTime.ParseExact(input.Date, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    DateTime _date = _reservationDate;

                    sqlParams.Add(new SqlParameter("@Date", _date));
                }
                else
                    sqlParams.Add(new SqlParameter("@Date", DBNull.Value));


                if (input.NewTransactions.HasValue)
                    sqlParams.Add(new SqlParameter("@NewTransactions", input.NewTransactions.Value == true ? '1' : '0'));
                else
                    sqlParams.Add(new SqlParameter("@NewTransactions", '0'));




                result.Transactions = await _commonRepository.GetListAsync<TransactionDto>("TransactionsReports", sqlParams);

                if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                {

                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        result.Transactions = result.Transactions.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId)).ToList();
                    }
                    else
                        result.Transactions = new List<TransactionDto>();
                }

                if (input.Paginated == true)
                {
                    result.filterCount = result.Transactions.Count();
                    result.Transactions = result.Transactions.Skip(input.SkipCount.Value).Take(input.MaxResultCount.Value).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<GetTransactionsReportPagedOutput> GetTransactionsReportPaged(GetTransactionsReportInput input)
        {
            try
            {
                GetTransactionsReportPagedOutput result = new GetTransactionsReportPagedOutput();

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


                if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                        DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.FullPeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                        DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;


                    sqlParams.Add(new SqlParameter("@FullPeriodTo", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodTo", DBNull.Value));




                //if (input.PeriodTo.HasValue)
                //    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                if (input.TransactionType.HasValue)
                    sqlParams.Add(new SqlParameter("@TransactionType", (byte)input.TransactionType.Value));
                else
                    sqlParams.Add(new SqlParameter("@TransactionType", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));


                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));


                if (input.WorkerId.HasValue)
                    sqlParams.Add(new SqlParameter("@WorkerId", input.WorkerId.Value));
                else
                    sqlParams.Add(new SqlParameter("@WorkerId", DBNull.Value));

                if (input.VeichleId.HasValue)
                    sqlParams.Add(new SqlParameter("@VeichleId", input.VeichleId.Value));
                else
                    sqlParams.Add(new SqlParameter("@VeichleId", DBNull.Value));

                if (input.DriverId.HasValue)
                    sqlParams.Add(new SqlParameter("@DriverId", input.DriverId.Value));
                else
                    sqlParams.Add(new SqlParameter("@DriverId", DBNull.Value));


                if (input.FuelType.HasValue)
                    sqlParams.Add(new SqlParameter("@FuelType", input.FuelType.Value));
                else
                    sqlParams.Add(new SqlParameter("@FuelType", DBNull.Value));



                if (!string.IsNullOrEmpty(input.Date))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.Date) ?
                                        DateTime.ParseExact(input.Date, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    DateTime _date = _reservationDate;

                    sqlParams.Add(new SqlParameter("@Date", _date));
                }
                else
                    sqlParams.Add(new SqlParameter("@Date", DBNull.Value));


                if (input.MaxCount.HasValue)
                    sqlParams.Add(new SqlParameter("@MaxCount", input.MaxCount.Value ? 1 : 0)); // Use integer values for boolean flags
                else
                    sqlParams.Add(new SqlParameter("@MaxCount", 0)); // Default to 0

                if (input.SkipCount.HasValue)
                    sqlParams.Add(new SqlParameter("@SkipCount", input.SkipCount.Value)); // Directly pass the integer value
                else
                    sqlParams.Add(new SqlParameter("@SkipCount", 0)); // Default to 0

                if (input.MaxResultCount.HasValue)
                    sqlParams.Add(new SqlParameter("@MaxResultCount", input.MaxResultCount.Value)); // Directly pass the integer value
                else
                    sqlParams.Add(new SqlParameter("@MaxResultCount", 10)); // Default to 10



                var (transactions, totalCount) = await _commonRepository.GetListWithCountAsync<TransactionDto>(
          "GetTransactionsReportsPaged",
          sqlParams);

                result.Transactions = transactions;
                result.TotalCount = totalCount;

                return result;


                //result.Transactions = await _commonRepository.GetListAsync<TransactionDto>("GetTransactionsReportsPaged", sqlParams);

                //if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                //{

                //    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                //    {
                //        result.Transactions = result.Transactions.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId)).ToList();
                //    }
                //    else
                //        result.Transactions = new List<TransactionDto>();
                //}

                //if(input.MaxCount == false)
                //{
                //    result.filterCount = result.Transactions.Count();
                //    result.Transactions = result.Transactions.Skip(input.SkipCount.Value).Take(input.MaxResultCount.Value).ToList();
                //}

                //return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetInvoicesReportOutput> GetInvoicesReport(GetTransactionsReportInput input)
        {
            try
            {
                GetInvoicesReportOutput result = new GetInvoicesReportOutput();

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


                //if (input.PeriodTo.HasValue)
                //    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                //if (input.TransactionType.HasValue)
                //    sqlParams.Add(new SqlParameter("@TransactionType", (byte)input.TransactionType.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@TransactionType", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));


                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                result.Invoices = await _commonRepository.GetListAsync<InvoiceReportDto>("InvoicesReports", sqlParams);


                if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                {
                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        result.Invoices = result.Invoices.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId)).ToList();
                    }
                    else
                        result.Invoices = new List<InvoiceReportDto>();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetVouchersReportOutput> GetVouchersReport(GetTransactionsReportInput input)
        {
            try
            {
                GetVouchersReportOutput result = new GetVouchersReportOutput();

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


                //if (input.PeriodTo.HasValue)
                //    sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));

                //if (input.TransactionType.HasValue)
                //    sqlParams.Add(new SqlParameter("@TransactionType", (byte)input.TransactionType.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@TransactionType", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));


                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                result.Vouchers = await _commonRepository.GetListAsync<VoucherReportDto>("VouchersReports", sqlParams);


                if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                {

                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        result.Vouchers = result.Vouchers.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId)).ToList();
                    }
                    else
                        result.Vouchers = new List<VoucherReportDto>();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<GetAdminDashboardStatisticsOutput> GetAdminDashboardStatistics(GetDashboardStatisticsInput input)
        {
            try
            {


                GetAdminDashboardStatisticsOutput result = new GetAdminDashboardStatisticsOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FiltrationTime", (byte)input.FiltrationTime)
            };

                //if (input.ClientId.HasValue)
                //    sqlParams.Add(new SqlParameter("@ClientId", input.ClientId.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@ClientId", DBNull.Value));


                if (input.DateFrom.HasValue)
                    sqlParams.Add(new SqlParameter("@DateFrom", input.DateFrom.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateFrom", DBNull.Value));

                if (input.DateTo.HasValue)
                    sqlParams.Add(new SqlParameter("@DateTo", input.DateTo.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateTo", DBNull.Value));


                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                result = await _commonRepository.GetAsync<GetAdminDashboardStatisticsOutput>("AdminDashboardStatistics", sqlParams);

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetBranchDashboardStatisticsOutput> GetBranchDashboardStatistics(GetDashboardStatisticsInput input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                GetBranchDashboardStatisticsOutput result = new GetBranchDashboardStatisticsOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FiltrationTime", (byte)input.FiltrationTime)
            };

                //if (input.ClientId.HasValue)
                //    sqlParams.Add(new SqlParameter("@ClientId", input.ClientId.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@ClientId", DBNull.Value));

                if (input.DateFrom.HasValue)
                    sqlParams.Add(new SqlParameter("@DateFrom", input.DateFrom.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateFrom", DBNull.Value));

                if (input.DateTo.HasValue)
                    sqlParams.Add(new SqlParameter("@DateTo", input.DateTo.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateTo", DBNull.Value));


                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (currentCulture.Name.Contains("ar"))
                    sqlParams.Add(new SqlParameter("@Lang", 1));
                else
                    sqlParams.Add(new SqlParameter("@Lang", 2));

                result = await _commonRepository.GetAsync<GetBranchDashboardStatisticsOutput>("TestBranchDashboardStatistics", sqlParams);

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetCompanyDashboardStatisticsOutput> GetCompanyDashboardStatistics(GetDashboardStatisticsInput input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                GetCompanyDashboardStatisticsOutput result = new GetCompanyDashboardStatisticsOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FiltrationTime", (byte)input.FiltrationTime)
            };

                //if (input.ClientId.HasValue)
                //    sqlParams.Add(new SqlParameter("@ClientId", input.ClientId.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@ClientId", DBNull.Value));


                if (input.DateFrom.HasValue)
                    sqlParams.Add(new SqlParameter("@DateFrom", input.DateFrom.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateFrom", DBNull.Value));

                if (input.DateTo.HasValue)
                    sqlParams.Add(new SqlParameter("@DateTo", input.DateTo.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateTo", DBNull.Value));


                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

                if (currentCulture.Name.Contains("ar"))
                    sqlParams.Add(new SqlParameter("@Lang", 1));
                else
                    sqlParams.Add(new SqlParameter("@Lang", 2));

                result.CompanyStatistics = await _commonRepository.GetAsync<GetCompanyDashboardStatisticsObject>("CompanyDashboardStatistics", sqlParams);


                //var branches = await _branchRepository.GetAll().Where(a => a.CompanyId == input.CompanyId).ToListAsync();
                //result.Branches = ObjectMapper.Map<List<StatisticsBranchDto>>(branches);

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetProviderDashboardStatisticsOutput> GetProviderDashboardStatistics(GetDashboardStatisticsInput input)
        {
            try
            {


                GetProviderDashboardStatisticsOutput result = new GetProviderDashboardStatisticsOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FiltrationTime", (byte)input.FiltrationTime)
            };

                //if (input.ClientId.HasValue)
                //    sqlParams.Add(new SqlParameter("@ClientId", input.ClientId.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@ClientId", DBNull.Value));


                if (input.DateFrom.HasValue)
                    sqlParams.Add(new SqlParameter("@DateFrom", input.DateFrom.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateFrom", DBNull.Value));

                if (input.DateTo.HasValue)
                    sqlParams.Add(new SqlParameter("@DateTo", input.DateTo.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateTo", DBNull.Value));


                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));

                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                if (input.IsEmployee.HasValue)
                    sqlParams.Add(new SqlParameter("@IsEmployee", input.IsEmployee.Value == true ? "1" : "0"));
                else
                    sqlParams.Add(new SqlParameter("@IsEmployee", "0"));


                if (!string.IsNullOrEmpty(input.ProviderIds))
                    sqlParams.Add(new SqlParameter("@ProviderIds", input.ProviderIds));
                else
                    sqlParams.Add(new SqlParameter("@ProviderIds", DBNull.Value));

                result = await _commonRepository.GetAsync<GetProviderDashboardStatisticsOutput>("ProviderDashboardStatistics", sqlParams);

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task UpdateInvoiceCodeInFuelTransOuts(UpdateInvoiceCodeInFuelTransOutsInput input)
        {
            try
            {
                GetTransactionsReportOutput result = new GetTransactionsReportOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>();

                sqlParams.Add(new SqlParameter("@InvoiceCode", input.InvoiceCode));
                sqlParams.Add(new SqlParameter("@Ids", input.Ids));

                await _commonRepository.ExecuteStoredProcedureAsync("UpdateInvoiceCodeInFuelTransOuts", sqlParams);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task UpdateCompanyInvoiceCodeInFuelTransOuts(UpdateInvoiceCodeInFuelTransOutsInput input)
        {
            try
            {
                GetTransactionsReportOutput result = new GetTransactionsReportOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>();

                sqlParams.Add(new SqlParameter("@CompanyInvoiceCode", input.InvoiceCode));
                sqlParams.Add(new SqlParameter("@Ids", input.Ids));

                await _commonRepository.ExecuteStoredProcedureAsync("UpdateCompanyInvoiceCodeInFuelTransOuts", sqlParams);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetProviderStatmentReportOutput> GetProviderStatmentReport(GetTransactionsReportInput input)
        {
            try
            {
                GetProviderStatmentReportOutput result = new GetProviderStatmentReportOutput();

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




                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));

                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                result.Invoices = await _commonRepository.GetListAsync<ProviderStatmentDto>("ProviderStatmentFromTransactions", sqlParams);


                if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                {

                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        result.Invoices = result.Invoices.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId)).ToList();
                    }
                    else
                        result.Invoices = new List<ProviderStatmentDto>();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<string> ExportProviderStatment(ExportProviderStatmentInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        var courseBookingRequestsList = await GetProviderStatmentReport(new GetTransactionsReportInput
                        {
                            ProviderId = input.ProviderId,
                            MainProviderId = input.MainProviderId,
                            PeriodFrom = input.PeriodFrom,
                            PeriodFromString = input.PeriodFromString,
                            PeriodTo = input.PeriodTo,
                            PeriodToString = input.PeriodToString,
                        });


                        string providerName = string.Empty;

                        if (input.ProviderId.HasValue)
                        {
                            var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
                            providerName = provider.NameAr;
                        }

                        else if (input.MainProviderId.HasValue)
                        {
                            var mainProvider = await _mainProviderRepository.FirstOrDefaultAsync(input.MainProviderId.Value);
                            providerName = mainProvider.NameAr;
                        }


                        if (courseBookingRequestsList.Invoices != null && courseBookingRequestsList.Invoices.Count > 0)
                        {

                            decimal rowBalance = 0;

                            for (var i = 0; i < courseBookingRequestsList.Invoices.Count; i++)
                            {
                                //if (i == 0)
                                //    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == StatmentTypes.Invoice ? courseBookingRequestsList.Invoices[i].RecordPrice : -courseBookingRequestsList.Invoices[i].RecordPrice;
                                //else
                                //    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == StatmentTypes.Invoice ? (rowBalance + courseBookingRequestsList.Invoices[i].RecordPrice) : (rowBalance - courseBookingRequestsList.Invoices[i].RecordPrice);


                                if (i == 0)
                                    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == ProviderStatmentTypes.Voucher ? courseBookingRequestsList.Invoices[i].RecordPrice : -courseBookingRequestsList.Invoices[i].RecordPrice;
                                else
                                    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == ProviderStatmentTypes.Voucher ? rowBalance + courseBookingRequestsList.Invoices[i].RecordPrice : rowBalance - courseBookingRequestsList.Invoices[i].RecordPrice;


                                rowBalance = courseBookingRequestsList.Invoices[i].Balance;
                            }
                            var excelData = ObjectMapper.Map<List<ProviderStatmentExcelDto>>(courseBookingRequestsList.Invoices.Select(m => new ProviderStatmentExcelDto
                            {
                                //Code = m.Code,
                                CreationTime = m.TransactionDate.HasValue ? m.TransactionDate.Value.ToString("dd/MM/yyyy") : "",
                                StatmentType = m.StatmentType == ProviderStatmentTypes.Invoice ? L("Pages.Vouchers.TotalFuelTransouts") : m.Note,
                                DepitPrice = m.StatmentType == ProviderStatmentTypes.Voucher ? m.RecordPrice.ToString() : "0.00",
                                CreditPrice = m.StatmentType == ProviderStatmentTypes.Invoice ? m.RecordPrice.ToString() : "0.00",
                                Balance = m.Balance.ToString()
                            }));

                            decimal _creditPrice = courseBookingRequestsList.Invoices.Where(a => a.StatmentType == ProviderStatmentTypes.Invoice).Sum(a => a.RecordPrice);
                            decimal _debitPrice = courseBookingRequestsList.Invoices.Where(a => a.StatmentType == ProviderStatmentTypes.Voucher).Sum(a => a.RecordPrice);

                            //decimal balance = _creditPrice - _debitPrice;
                            decimal balance = _debitPrice - _creditPrice;

                            excelData.Add(new ProviderStatmentExcelDto
                            {
                                //Code = "المبلغ الإجمالي",
                                CreationTime = L("Pages.Invoices.TotalAmount"),
                                CreditPrice = _creditPrice.ToString(),
                                DepitPrice = _debitPrice.ToString(),
                                Balance = balance.ToString()
                            });


                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();
                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "نوع الملف",
                                Value = L("Pages.Reports.AccountStatement")
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "تاريخ الإصدار",
                                Value = DateTime.Now.ToString()
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = input.ProviderId.HasValue ? L("Pages.Branches.ModalTitle") : L("Pages.Reports.ServiceProvider"),
                                Value = providerName
                            });

                            _options = new RequestFuelExcelOptionsDto
                            {
                                KeyValues = _keyValues
                            };

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            dataSet.Tables[1].TableName = L("Pages.AccountStatement.Title");

                            ExcelSource source = ExcelSource.AccountStatment;

                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.AccountStatement.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);
                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetMonthlyFuelStatsByTypeOutput> GetMonthlyFuelStatsByType(GetDashboardStatisticsInput input)
        {
            try
            {


                GetMonthlyFuelStatsByTypeOutput result = new GetMonthlyFuelStatsByTypeOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                };

                //if (input.ClientId.HasValue)
                //    sqlParams.Add(new SqlParameter("@ClientId", input.ClientId.Value));
                //else
                //    sqlParams.Add(new SqlParameter("@ClientId", DBNull.Value));


                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));

                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

                if (input.Year.HasValue)
                    sqlParams.Add(new SqlParameter("@Year", input.Year.Value));
                else
                    sqlParams.Add(new SqlParameter("@Year", DBNull.Value));

                if (input.IsProviderEmployee.HasValue)
                    sqlParams.Add(new SqlParameter("@IsProviderEmployee", input.IsProviderEmployee.Value == true ? "1" : "0"));
                else
                    sqlParams.Add(new SqlParameter("@IsProviderEmployee", "0"));


                if (!string.IsNullOrEmpty(input.ProviderIds))
                    sqlParams.Add(new SqlParameter("@ProviderIds", input.ProviderIds));
                else
                    sqlParams.Add(new SqlParameter("@ProviderIds", DBNull.Value));


                result.Months = await _commonRepository.GetListAsync<MonthlyFuelStats>("GetMonthlyFuelStatsByType", sqlParams);

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GetFuelTypesStatisticsOutput> GetFuelTypesStatistics(GetDashboardStatisticsInput input)
        {
            try
            {


                GetFuelTypesStatisticsOutput result = new GetFuelTypesStatisticsOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {

                };

                if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                        DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodFrom", DBNull.Value));

                if (!string.IsNullOrEmpty(input.FullPeriodToString))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                        DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;


                    sqlParams.Add(new SqlParameter("@FullPeriodTo", _reservationDate));
                }
                else
                    sqlParams.Add(new SqlParameter("@FullPeriodTo", DBNull.Value));



                if (input.DateFrom.HasValue)
                    sqlParams.Add(new SqlParameter("@DateFrom", input.DateFrom.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateFrom", DBNull.Value));

                if (input.DateTo.HasValue)
                    sqlParams.Add(new SqlParameter("@DateTo", input.DateTo.Value));
                else
                    sqlParams.Add(new SqlParameter("@DateTo", DBNull.Value));

                if (!string.IsNullOrEmpty(input.Date))
                {
                    DateTime _reservationDate = !string.IsNullOrEmpty(input.Date) ?
                                        DateTime.ParseExact(input.Date, "MM/dd/yyyy", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                       : DateTime.UtcNow;

                    DateTime _date = _reservationDate;

                    sqlParams.Add(new SqlParameter("@Date", _date));
                }
                else
                    sqlParams.Add(new SqlParameter("@Date", DBNull.Value));



                if (input.BranchId.HasValue)
                    sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
                else
                    sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));


                if (input.WorkerId.HasValue)
                    sqlParams.Add(new SqlParameter("@WorkerId", input.WorkerId.Value));
                else
                    sqlParams.Add(new SqlParameter("@WorkerId", DBNull.Value));

                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));

                if (input.ProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@ProviderId", input.ProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@ProviderId", DBNull.Value));


                if (input.IsProviderEmployee.HasValue)
                    sqlParams.Add(new SqlParameter("@IsProviderEmployee", input.IsProviderEmployee.Value == true ? "1" : "0"));
                else
                    sqlParams.Add(new SqlParameter("@IsProviderEmployee", "0"));


                if (!string.IsNullOrEmpty(input.ProviderIds))
                    sqlParams.Add(new SqlParameter("@ProviderIds", input.ProviderIds));
                else
                    sqlParams.Add(new SqlParameter("@ProviderIds", DBNull.Value));

                result.FuelTypesStatistics = await _commonRepository.GetAsync<FuelTypesStatisticsObject>("FuelTypesStatistics", sqlParams);
                result.Success = true;
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task CopySubscriptionToTransactions(CopySubscriptionToTransactionsInput input)
        {
            try
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>();

                sqlParams.Add(new SqlParameter("@SubscriptionId", input.SubscriptionId));
                sqlParams.Add(new SqlParameter("@TransactionType", input.TransactionType));

                await _commonRepository.ExecuteStoredProcedureAsync("CopySubscriptionToTransactions", sqlParams);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<GetProviderRevenueOutput> GetProviderRevenue(GetDashboardStatisticsInput input)
        {
            try
            {


                GetProviderRevenueOutput result = new GetProviderRevenueOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {

                };

                if (input.MainProviderId.HasValue)
                    sqlParams.Add(new SqlParameter("@MainProviderId", input.MainProviderId.Value));
                else
                    sqlParams.Add(new SqlParameter("@MainProviderId", DBNull.Value));


                var totalCount = await _mainProviderRepository.CountAsync(a => a.IsDeleted == false);
                if (input.MaxCount.HasValue && input.MaxCount.Value == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = totalCount;
                }

                if (input.SkipCount > 0)
                    sqlParams.Add(new SqlParameter("@SkipCount", input.SkipCount));
                else
                    sqlParams.Add(new SqlParameter("@SkipCount", "0"));

                if (input.MaxResultCount > 0)
                    sqlParams.Add(new SqlParameter("@MaxResultCount", input.MaxResultCount));
                else
                    sqlParams.Add(new SqlParameter("@MaxResultCount", "10"));

                result.List = await _commonRepository.GetListAsync<ProviderRevenue>("ProviderRevenue", sqlParams);
                result.Success = true;

                result.TotalCount = totalCount;
                decimal _voucherAmount = await _voucherRepository.GetAll().SumAsync(a => a.Amount);
                decimal _transAmount = await _fuelTransOutRepository.GetAll()
                    .Where(a => a.Completed == true && a.IsDeleted == false && a.Provider.IsDeleted == false && a.Provider.MainProvider.IsDeleted == false)
                    .SumAsync(a => a.Price);

                result.TotalVouchers = _voucherAmount;
                result.TotalTransactions = _transAmount;

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[AbpAuthorize]
        public async Task<GetMonthlyFuelConsumptionReportOutput> GetMonthlyFuelConsumptionReport(GetMonthlyFuelConsumptionReportInput input)
        {
            try
            {
                string[] parts = null;
                int month = 0;
                int year = 0;
                DateTime _periodFrom = new DateTime();
                DateTime _periodTo = new DateTime();
                GetMonthlyFuelConsumptionReportOutput result = new GetMonthlyFuelConsumptionReportOutput();

                List<SqlParameter> sqlParams = new List<SqlParameter>();

                if (input.Month != null)
                {
                    parts = input.Month.Split('/'); // Split into month and year
                    month = int.Parse(parts[0]);
                    year = int.Parse(parts[1]);
                    _periodFrom = new DateTime(year, month, 1, 0, 0, 0);
                    _periodTo = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);



                    sqlParams.Add(new SqlParameter("@PeriodFrom", _periodFrom));
                    sqlParams.Add(new SqlParameter("@PeriodTo", _periodTo));
                }
                else
                {

                    sqlParams.Add(new SqlParameter("@PeriodFrom", DBNull.Value));
                    sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));
                }
                var utcTimeFrom = _periodFrom.ToUniversalTime();
                var utcTimeTo = _periodTo.ToUniversalTime();


                if (input.CompanyId.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                }

                result.Transactions = await _commonRepository.GetListAsync<GetTransactionsListOutput>("GetFuelConsumptionReport", sqlParams);
                foreach (var item in result.Transactions)
                {
                    result.TotalRowsVatValue += item.VatValue;
                    result.TotalRowsWithoutVat += item.TotalWithoutVat;
                    result.TotalRowsWithVat += item.TotalWithVat;
                    result.PeriodFrom = utcTimeFrom;
                    if (utcTimeTo.AddHours(-6) > DateTime.UtcNow)
                    {
                        result.PeriodTo = DateTime.UtcNow.AddHours(-3);
                    }
                    else
                    {
                        result.PeriodTo = utcTimeTo.AddHours(-6);
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<string> ExportProviderRevenue(GetProviderRevenueExcelDtoInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        var result = await GetProviderRevenue(new GetDashboardStatisticsInput { MainProviderId = input.MainProviderId, MaxCount = true });



                        if (result != null && result.List.Count > 0)
                        {

                            var excelData = new List<AdminRevenueFuelExcelDto>();


                            excelData = ObjectMapper.Map<List<AdminRevenueFuelExcelDto>>(result.List.Select(m => new AdminRevenueFuelExcelDto
                            {
                                Code = m.Code,
                                NameAr = m.NameAr,
                                VouchersAmount = m.VouchersAmount,
                                TransactionsAmount = m.TransactionsAmount,
                                //ClaimBalance = (m.TransactionsAmount - m.VouchersAmount),
                                ClaimBalance = m.VouchersAmount - m.TransactionsAmount
                            }));

                            decimal _transactionsAmount = excelData.Sum(a => a.TransactionsAmount);
                            decimal _vouchersAmount = excelData.Sum(a => a.VouchersAmount);
                            decimal _claimBalanceAmount = excelData.Sum(a => a.ClaimBalance);

                            excelData.Add(new AdminRevenueFuelExcelDto
                            {
                                Code = L("Pages.Report.TotalPrice"),
                                VouchersAmount = _vouchersAmount,
                                TransactionsAmount = _transactionsAmount,
                                ClaimBalance = _claimBalanceAmount
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();



                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "نوع الملف",
                                Value = L("Pages.Reports.ServiceProviderRevenue")
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "تاريخ الإصدار",
                                Value = DateTime.Now.ToString()
                            });



                            _options = new RequestFuelExcelOptionsDto
                            {
                                ExcelDate = DateTime.Now.ToString(),
                                ExcelType = L("Pages.Reports.ServiceProviderRevenue"),
                                KeyValues = _keyValues
                            };

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            ExcelSource source = ExcelSource.Revenue;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.ProviderRevenue.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<GetCompanyStatmentReportOutput> GetCompanyStatmentReport(GetTransactionsReportInput input)
        {
            try
            {
                GetCompanyStatmentReportOutput result = new GetCompanyStatmentReportOutput();

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

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

                result.Invoices = await _commonRepository.GetListAsync<CompanyStatmentDto>("CompanyStatmentFromTransactions", sqlParams);


                if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                {
                    if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                    {
                        result.Invoices = result.Invoices.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId)).ToList();
                    }
                    else
                        result.Invoices = new List<CompanyStatmentDto>();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<string> ExportCompanyStatment(ExportProviderStatmentInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        var courseBookingRequestsList = await GetCompanyStatmentReport(new GetTransactionsReportInput
                        {
                            BranchId = input.BranchId,
                            CompanyId = input.CompanyId,
                            PeriodFrom = input.PeriodFrom,
                            PeriodFromString = input.PeriodFromString,
                            PeriodTo = input.PeriodTo,
                            PeriodToString = input.PeriodToString,
                        });


                        string branchName = string.Empty;

                        if (input.BranchId.HasValue)
                        {
                            var branch = await _branchRepository.FirstOrDefaultAsync(input.BranchId.Value);
                            branchName = branch.NameAr;
                        }

                        else if (input.CompanyId.HasValue)
                        {
                            var company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);
                            branchName = company.NameAr;
                        }


                        if (courseBookingRequestsList.Invoices != null && courseBookingRequestsList.Invoices.Count > 0)
                        {

                            decimal rowBalance = 0;

                            for (var i = 0; i < courseBookingRequestsList.Invoices.Count; i++)
                            {
                                //if (i == 0)
                                //    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == StatmentTypes.Invoice ? courseBookingRequestsList.Invoices[i].RecordPrice : -courseBookingRequestsList.Invoices[i].RecordPrice;
                                //else
                                //    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == StatmentTypes.Invoice ? (rowBalance + courseBookingRequestsList.Invoices[i].RecordPrice) : (rowBalance - courseBookingRequestsList.Invoices[i].RecordPrice);


                                if (i == 0)
                                    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == CompanyStatmentTypes.Wallet ? courseBookingRequestsList.Invoices[i].RecordPrice : -courseBookingRequestsList.Invoices[i].RecordPrice;
                                else
                                    courseBookingRequestsList.Invoices[i].Balance = courseBookingRequestsList.Invoices[i].StatmentType == CompanyStatmentTypes.Wallet ? rowBalance + courseBookingRequestsList.Invoices[i].RecordPrice : rowBalance - courseBookingRequestsList.Invoices[i].RecordPrice;


                                rowBalance = courseBookingRequestsList.Invoices[i].Balance;
                            }
                            var excelData = ObjectMapper.Map<List<ProviderStatmentExcelDto>>(courseBookingRequestsList.Invoices.Select(m => new ProviderStatmentExcelDto
                            {
                                //Code = m.Code,
                                CreationTime = m.TransactionDate.HasValue ? m.TransactionDate.Value.ToString("dd/MM/yyyy") : "",
                                StatmentType = m.StatmentType == CompanyStatmentTypes.Invoice ? L("Pages.Vouchers.TotalFuelTransouts") : m.StatmentType == CompanyStatmentTypes.Wallet ? L("Pages.AccountStatement.WalletCharge") : L("Pages.AccountStatement.Package"),
                                DepitPrice = m.StatmentType == CompanyStatmentTypes.Invoice || m.StatmentType == CompanyStatmentTypes.Package ? m.RecordPrice.ToString() : "0.00",
                                CreditPrice = m.StatmentType == CompanyStatmentTypes.Wallet ? m.RecordPrice.ToString() : "0.00",
                                Balance = m.Balance.ToString()
                            }));

                            decimal _debitPrice = courseBookingRequestsList.Invoices.Where(a => a.StatmentType == CompanyStatmentTypes.Invoice || a.StatmentType == CompanyStatmentTypes.Package).Sum(a => a.RecordPrice);
                            decimal _creditPrice = courseBookingRequestsList.Invoices.Where(a => a.StatmentType == CompanyStatmentTypes.Wallet).Sum(a => a.RecordPrice);

                            decimal balance = _creditPrice - _debitPrice;
                            //decimal balance = _debitPrice - _creditPrice;

                            excelData.Add(new ProviderStatmentExcelDto
                            {
                                //Code = "المبلغ الإجمالي",
                                CreationTime = L("Pages.Invoices.TotalAmount"),
                                CreditPrice = _creditPrice.ToString(),
                                DepitPrice = _debitPrice.ToString(),
                                Balance = balance.ToString()
                            });


                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();
                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "نوع الملف",
                                Value = L("Permission.ProviderAccountStatement")
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "تاريخ الإصدار",
                                Value = DateTime.Now.ToString()
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = input.CompanyId.HasValue ? L("Pages.Branches.ModalTitle") : L("Pages.Companies.ModalTitle"),
                                Value = branchName
                            });

                            _options = new RequestFuelExcelOptionsDto
                            {
                                KeyValues = _keyValues
                            };

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            dataSet.Tables[1].TableName = L("Pages.AccountStatement.Title");

                            ExcelSource source = ExcelSource.AccountStatment;

                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.AccountStatement.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);
                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[AbpAuthorize]
        //public async Task<GetBranchConsumptionReportOutput> GetBranchConsumptionReport(GetTransactionsReportInput input)
        //{
        //    try
        //    {


        //        GetBranchConsumptionReportOutput result = new GetBranchConsumptionReportOutput();

        //        List<SqlParameter> sqlParams = new List<SqlParameter>
        //        {

        //        };

        //        if (!string.IsNullOrEmpty(input.PeriodFromString))
        //        {
        //            DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodFromString) ?
        //                                DateTime.ParseExact(input.PeriodFromString, "MM/dd/yyyy", (new CultureInfo("en-US")).DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
        //                               : DateTime.UtcNow;

        //            input.PeriodFrom = _reservationDate;

        //            sqlParams.Add(new SqlParameter("@PeriodFrom", input.PeriodFrom.Value));
        //        }
        //        else
        //            sqlParams.Add(new SqlParameter("@PeriodFrom", DBNull.Value));

        //        if (!string.IsNullOrEmpty(input.PeriodToString))
        //        {
        //            DateTime _reservationDate = !string.IsNullOrEmpty(input.PeriodToString) ?
        //                                DateTime.ParseExact(input.PeriodToString, "MM/dd/yyyy", (new CultureInfo("en-US")).DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
        //                               : DateTime.UtcNow;

        //            input.PeriodTo = _reservationDate;

        //            sqlParams.Add(new SqlParameter("@PeriodTo", input.PeriodTo.Value));
        //        }
        //        else
        //            sqlParams.Add(new SqlParameter("@PeriodTo", DBNull.Value));


        //        if (input.BranchId.HasValue)
        //            sqlParams.Add(new SqlParameter("@BranchId", input.BranchId.Value));
        //        else
        //            sqlParams.Add(new SqlParameter("@BranchId", DBNull.Value));

        //        if (input.CompanyId.HasValue)
        //            sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
        //        else
        //            sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

        //        if (input.FuelType.HasValue)
        //            sqlParams.Add(new SqlParameter("@FuelType", input.FuelType.Value));
        //        else
        //            sqlParams.Add(new SqlParameter("@FuelType", DBNull.Value));




        //        var totalCount = await _mainProviderRepository.CountAsync(a => a.IsDeleted == false);
        //        if (input.MaxCount.HasValue && input.MaxCount.Value == true)
        //        {
        //            input.SkipCount = 0;
        //            input.MaxResultCount = totalCount;
        //        }


        //        if (input.SkipCount > 0)
        //            sqlParams.Add(new SqlParameter("@SkipCount", input.SkipCount));
        //        else
        //            sqlParams.Add(new SqlParameter("@SkipCount", "0"));

        //        if (input.MaxResultCount > 0)
        //            sqlParams.Add(new SqlParameter("@MaxResultCount", input.MaxResultCount));
        //        else
        //            sqlParams.Add(new SqlParameter("@MaxResultCount", "10"));


        //        result.List = await _commonRepository.GetListAsync<BranchConsumption>("BranchConsumptionReport", sqlParams);
        //        result.Success = true;

        //        result.TotalCount = totalCount;

        //        #region total quantiy and total price

        //        var query = _fuelTransOutRepository.GetAll().Where(a => a.Completed == true && a.Branch.CompanyId == input.CompanyId);
        //        query = query.WhereIf(input.BranchId.HasValue, a => a.BranchId == input.BranchId.Value);
        //        query = query.WhereIf(input.FuelType.HasValue, a => a.FuelType == (FuelType)input.FuelType);
        //        query = query.WhereIf(input.PeriodFrom.HasValue, a => a.CreationTime >= input.PeriodFrom);
        //        query = query.WhereIf(input.PeriodTo.HasValue, a => a.CreationTime <= input.PeriodTo);


        //        decimal _totalQuantity = await query.SumAsync(a=>a.Quantity);
        //        decimal _totalPrice = await query.SumAsync(a=>a.Price);

        //        result.TotalQuantity = _totalQuantity;
        //        result.TotalPrice = _totalPrice;
        //        #endregion

        //        return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        [AbpAuthorize]
        public async Task<List<WalletsPendingTransOutput>> WalletsPendingTrans(WalletsPendingTrans input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                List<WalletsPendingTransOutput> result = new List<WalletsPendingTransOutput>();

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

                if (input.CompanyId.HasValue)
                    sqlParams.Add(new SqlParameter("@CompanyId", input.CompanyId.Value));
                else
                    sqlParams.Add(new SqlParameter("@CompanyId", DBNull.Value));

                if (currentCulture.Name.Contains("ar"))
                    sqlParams.Add(new SqlParameter("@Lang", 1));
                else
                    sqlParams.Add(new SqlParameter("@Lang", 2));

                result = await _commonRepository.GetListAsync<WalletsPendingTransOutput>("WalletsPendingTrans", sqlParams);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}