using Abp;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Abp.EntityHistory;
using Abp.Modules;
using Sayarah.AbpZeroTemplate.Auditing.Dto;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Application.Configuration;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Journals.Dto;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Application.Packages.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.Subscriptions.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Journals;
using Sayarah.Lookups;
using Sayarah.Packages;
using Sayarah.Providers;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Sayarah.Application
{
    [DependsOn(typeof(SayarahCoreModule), typeof(AbpAutoMapperModule))]
    public class SayarahApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.EntityHistory.IsEnabled = true;
            Configuration.EntityHistory.IsEnabledForAnonymousUsers = true;

            Configuration.EntityHistory.Selectors.Add(
                new NamedTypeSelector(
                    "Abp.FullAuditedEntities",
                    type => typeof(IFullAudited).IsAssignableFrom(type)
                )
            );
        }

        public override void Initialize()
        {
            Configuration.Settings.Providers.Add<SayarahSettingProvider>();

            IocManager.RegisterAssemblyByConvention(typeof(SayarahApplicationModule).Assembly);

            // TODO: Is there somewhere else to store these, with the dto classes
            Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg =>
            {
                // Role and permission
                cfg.CreateMap<Permission, string>().ConvertUsing(r => r.Name);
                cfg.CreateMap<RolePermissionSetting, string>().ConvertUsing(r => r.Name);
                //
                cfg.CreateMap<CreateRoleDto, Role>();
                cfg.CreateMap<RoleDto, Role>();
                //
                cfg.CreateMap<UserDto, UserApiDto>();
                cfg.CreateMap<UserDto, ShortUserDto>();
                cfg.CreateMap<UserDto, SmallUserDto>();
                cfg.CreateMap<UserDto, UserPlainDto>();
                cfg.CreateMap<UserDto, ApiUserDto>();
                cfg.CreateMap<DriverDto, ApiDriverDto>();
                cfg.CreateMap<UserDto, CreateNewUserInput>();
                cfg.CreateMap<DriverVeichleDto, SmallDriverVeichleDto>();
                cfg.CreateMap<ProviderDto, ApiProviderDto>();
                cfg.CreateMap<UpdateVeichleSimPicDto, Veichle>();
                cfg.CreateMap<WorkerDto, ApiWorkerDto>();
                cfg.CreateMap<CompanyInvoiceDto, CompanyInvoiceOutput>();

                cfg.CreateMap<Company, CompanyDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.DescAr : src.DescEn));

                cfg.CreateMap<CreateCompanyDto, Company>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<Company, CompanyNameDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<CreateNewUserInput, User>()
                    .ForMember(x => x.UserDashboards, opt => opt.Ignore());

                cfg.CreateMap<Branch, BranchDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.DescAr : src.DescEn));

                cfg.CreateMap<Branch, ApiBranchDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Branch, StatisticsBranchDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<CreateBranchDto, Branch>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                //    cfg.CreateMap<UpdateBranchDto, Branch>()
                //.ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<City, CityDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Brand, BrandDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Model, ModelDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<CompanyType, CompanyTypeDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Package, PackageDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.DescAr : src.DescEn));

                cfg.CreateMap<Subscription, SubscriptionDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.DescAr : src.DescEn));

                cfg.CreateMap<SubscriptionTransaction, SubscriptionTransactionDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.DescAr : src.DescEn));

                cfg.CreateMap<Provider, ProviderDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Provider, ApiProviderDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<CreateProviderDto, Provider>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<MainProvider, MainProviderDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<MainProvider, MainProviderBankInfoDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<CreateMainProviderDto, MainProvider>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<CreateWorkerDto, Worker>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<Veichle, VeichleDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn))
                    .ForMember(dest => dest.MappedFullPlateNumber, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.FullPlateNumberAr : src.FullPlateNumber));

                cfg.CreateMap<Veichle, ApiVeichleDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Veichle, VeichleNumbersDto>()
                    .ForMember(dest => dest.FullPlateNumber, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.FullPlateNumberAr : src.FullPlateNumber));

                cfg.CreateMap<VeichleDto, ApiVeichleDto>();
                cfg.CreateMap<FuelTransOutDto, ApiFuelTransOutDto>();

                cfg.CreateMap<FuelTransOut, NewFuelTransOutDto>().AfterMap((src, dest) =>
                {
                    bool iAr = Thread.CurrentThread.CurrentUICulture.Name.Contains("ar");
                    dest.MappedFullPlateNumber = iAr ? AddSpaces(src.Veichle.FullPlateNumberAr) : AddSpaces(src.Veichle.FullPlateNumber);
                    dest.VeichleTypeString = src.Veichle.VeichleType.ToString();
                    if (dest.VeichleTypeString == "Bus" && iAr)
                    {
                        dest.VeichleTypeString = "حافلة";
                    }
                    if (dest.VeichleTypeString == "Private" && iAr)
                    {
                        dest.VeichleTypeString = "خصوصي";
                    }
                    if (dest.VeichleTypeString == "Transport" && iAr)
                    {
                        dest.VeichleTypeString = "نقل";
                    }
                    if (dest.VeichleTypeString == "Taxi" && iAr)
                    {
                        dest.VeichleTypeString = "أجرة";
                    }
                    if (dest.VeichleTypeString == "Truck" && iAr)
                    {
                        dest.VeichleTypeString = "شاحنة";
                    }
                    if (dest.VeichleTypeString == "Van" && iAr)
                    {
                        dest.VeichleTypeString = "فان";
                    }
                    if (dest.VeichleTypeString == "Motorcycle" && iAr)
                    {
                        dest.VeichleTypeString = "دراجة بخارية";
                    }
                    if (dest.VeichleTypeString == "HeavyEquipment" && iAr)
                    {
                        dest.VeichleTypeString = "معدات ثقيلة";
                    }
                    if (dest.VeichleTypeString == "PrivateTransfer" && iAr)
                    {
                        dest.VeichleTypeString = "نقل خاص";
                    }
                    if (dest.VeichleTypeString == "Diplomat" && iAr)
                    {
                        dest.VeichleTypeString = "دبلوماسي";
                    }
                });

                cfg.CreateMap<CreateDriverDto, Driver>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<UpdateDriverDto, Driver>()
                    .ForMember(x => x.User, opt => opt.Ignore());

                cfg.CreateMap<FuelGroup, FuelGroupDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Journal, ApiJournalDto>()
                    .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.Branch.NameAr : src.Branch.NameEn))
                    .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.Branch.Company.NameAr : src.Branch.Company.NameEn))
                    .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.Provider.NameAr : src.Provider.NameEn))
                    .ForMember(dest => dest.MainProviderName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.Provider.MainProvider.NameAr : src.Provider.MainProvider.NameEn));

                cfg.CreateMap<FuelPumpDto, ApiFuelPumpDto>();

                cfg.CreateMap<Provider, SmallProviderDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<MainProvider, SmallMainProviderDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Branch, SmallBranchDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Branch, BranchNameDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Company, SmallCompanyDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<Company, GetWalletDetailsDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.NameAr : src.NameEn));

                cfg.CreateMap<AuditLog, AuditLogListDto>();
                cfg.CreateMap<EntityChange, EntityChangeListDto>();
                cfg.CreateMap<EntityChange, EntityChangeWithPropertiesListDto>();
                cfg.CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();
                //EntityPropertyChange

                cfg.CreateMap<CreateVeichleDto, Veichle>()
                    .ForMember(x => x.Driver, opt => opt.Ignore());

                cfg.CreateMap<UpdateVeichleDto, Veichle>()
                    .ForMember(x => x.Driver, opt => opt.Ignore());

                cfg.CreateMap<FrequencyOutput, FrequencyOutputDto>()
                    .ForMember(dest => dest.BrancheName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.BrancheNameAr : src.BrancheNameEn))
                    .ForMember(dest => dest.FullPlateNumber, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.FullPlateNumberAr : src.FullPlateNumber))
                    .ForMember(dest => dest.VeichleTypeString, opt => opt.MapFrom(src => src.VeichleType.ToString()))
                    .ForMember(dest => dest.FuelTypeString, opt => opt.MapFrom(src => src.FuelType.ToString()))
                    .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.BrandNameAr : src.BrandNameEn))
                    .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime.ToString()))
                    .AfterMap((src, dest) =>
                    {
                        var cultureName = Thread.CurrentThread.CurrentUICulture.Name;
                        if (src.IsActive && !src.IsDeleted)
                        {
                            dest.IsActiveString = cultureName.Contains("ar")
                                ? "نشط"
                                : "Active";
                        }
                        else
                        {
                            dest.IsActiveString = cultureName.Contains("ar")
                                ? "غير نشط"
                                : "Passive";
                        }

                        if (dest.VeichleTypeString == "Bus" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "حافلة";
                        }
                        if (dest.VeichleTypeString == "Private" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "خصوصي";
                        }
                        if (dest.VeichleTypeString == "Transport" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "نقل";
                        }
                        if (dest.VeichleTypeString == "Taxi" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "أجرة";
                        }
                        if (dest.VeichleTypeString == "Truck" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "شاحنة";
                        }
                        if (dest.VeichleTypeString == "Van" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "فان";
                        }
                        if (dest.VeichleTypeString == "Motorcycle" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "دراجة بخارية";
                        }
                        if (dest.VeichleTypeString == "HeavyEquipment" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "معدات ثقيلة";
                        }
                        if (dest.VeichleTypeString == "PrivateTransfer" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "نقل خاص";
                        }
                        if (dest.VeichleTypeString == "Diplomat" && cultureName.Contains("ar"))
                        {
                            dest.VeichleTypeString = "دبلوماسي";
                        }
                        if (!string.IsNullOrEmpty(dest.FullPlateNumber))
                        {
                            dest.FullPlateNumber = string.Concat(dest.FullPlateNumber.Where(c => !char.IsWhiteSpace(c)));
                            // Add two spaces between each character
                            dest.FullPlateNumber = string.Join("  ", dest.FullPlateNumber.ToCharArray());
                        }
                    });

                cfg.CreateMap<ConsumptionOutput, ConsumptionOutputDto>()
                    .ForMember(dest => dest.BrancheName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.BrancheNameAr : src.BrancheNameEn))
                    .ForMember(dest => dest.FullPlateNumber, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.FullPlateNumberAr : src.FullPlateNumber))
                    .ForMember(dest => dest.FuelTypeString, opt => opt.MapFrom(src => src.FuelType.ToString()))
                    .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime.ToString()))
                    .AfterMap((src, dest) =>
                    {
                        var cultureName = Thread.CurrentThread.CurrentUICulture.Name;
                        if (src.IsActive && !src.IsDeleted)
                        {
                            dest.IsActiveString = cultureName.Contains("ar")
                                ? "نشط"
                                : "Active";
                        }
                        else
                        {
                            dest.IsActiveString = cultureName.Contains("ar")
                                ? "غير نشط"
                                : "Passive";
                        }

                        if (!string.IsNullOrEmpty(dest.FullPlateNumber))
                        {
                            dest.FullPlateNumber = string.Concat(dest.FullPlateNumber.Where(c => !char.IsWhiteSpace(c)));
                            // Add two spaces between each character
                            dest.FullPlateNumber = string.Join("  ", dest.FullPlateNumber.ToCharArray());
                        }
                        dest.ConsumptionRate = src.ConsumptionRate.Value.ToString("N2");
                    });

                cfg.CreateMap<VeichleConsumptionOutput, VeichleConsumptionOutputDto>()
                    .ForMember(dest => dest.BrancheName, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.BrancheNameAr : src.BrancheNameEn))
                    .ForMember(dest => dest.FullPlateNumber, opt => opt.MapFrom(src => Thread.CurrentThread.CurrentUICulture.Name.Contains("ar") ? src.FullPlateNumberAr : src.FullPlateNumber))
                    .ForMember(dest => dest.FuelTypeString, opt => opt.MapFrom(src => src.FuelType.ToString())).AfterMap((src, dest) =>
                    {
                        if (!string.IsNullOrEmpty(dest.FullPlateNumber))
                        {
                            dest.FullPlateNumber = string.Concat(dest.FullPlateNumber.Where(c => !char.IsWhiteSpace(c)));
                            // Add two spaces between each character
                            dest.FullPlateNumber = string.Join("  ", dest.FullPlateNumber.ToCharArray());
                        }
                    });
                cfg.CreateMap<GetFuelTypesStatisticsOutput, MappedGetFuelTypesStatisticsOutput>().AfterMap((src, dest) =>
                {
                    dest.FuelTypesStatistics.Fuel95Amount = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.Fuel95Amount, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                    dest.FuelTypesStatistics.Fuel91Amount = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.Fuel91Amount, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                    dest.FuelTypesStatistics.FuelDesielAmount = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.FuelDesielAmount, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                    dest.FuelTypesStatistics.Fuel91Quantity = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.Fuel91Quantity, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                    dest.FuelTypesStatistics.Fuel95Quantity = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.Fuel95Quantity, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                    dest.FuelTypesStatistics.FuelDesielQuantity = Convert.ToDecimal(Math.Round(src.FuelTypesStatistics.FuelDesielQuantity, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                });
                cfg.CreateMap<TransactionDto, MappedTransactionDto>().AfterMap((src, dest) =>
                {
                    dest.Quantity = src.Quantity.HasValue ? Convert.ToDecimal(Math.Round(src.Quantity.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                    dest.Price = src.Price.HasValue ? Convert.ToDecimal(Math.Round(src.Price.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                    dest.FuelPrice = src.FuelPrice.HasValue ? Convert.ToDecimal(Math.Round(src.FuelPrice.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                });

                cfg.CreateMap<TransactionDto, MappedTransactionDto>().AfterMap((src, dest) =>
                {
                    dest.Quantity = src.Quantity.HasValue ? Convert.ToDecimal(Math.Round(src.Quantity.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                    dest.Price = src.Price.HasValue ? Convert.ToDecimal(Math.Round(src.Price.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                    dest.FuelPrice = src.FuelPrice.HasValue ? Convert.ToDecimal(Math.Round(src.FuelPrice.Value, 3, MidpointRounding.AwayFromZero).ToString("F3")) : 0;
                });
            });
        }

        public static string AddSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Remove all existing spaces
            text = text.Replace(" ", string.Empty);

            // Add two spaces between each character
            return string.Join("  ", text.ToCharArray());
        }

        public static string ReverseArabicCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Reverse the entire string
            var charArray = input.ToCharArray();
            Array.Reverse(charArray);
            string reversed = new(charArray);

            // Match Arabic characters using a regex
            var arabicRegex = @"[\u0600-\u06FF\u0750-\u077F]+";
            return Regex.Replace(reversed, arabicRegex, match =>
            {
                var arabicCharArray = match.Value.ToCharArray();
                Array.Reverse(arabicCharArray);
                return new string(arabicCharArray);
            });
        }
    }
}
