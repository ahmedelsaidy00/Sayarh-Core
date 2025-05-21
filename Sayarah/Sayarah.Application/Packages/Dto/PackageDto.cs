using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Packages;

namespace Sayarah.Application.Packages.Dto
{
    [AutoMapFrom(typeof(Package)), AutoMapTo(typeof(Package))]
    public class PackageDto : AuditedEntityDto<long>
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string Desc { get; set; }

        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }
        public int VeichlesCount { get { return VeichlesFrom; } }
        public int NfcCount { get { return VeichlesFrom; } }
        public decimal NetPrice {
            get { 
                return VeichlesFrom * MonthlyPrice; 
            } 
        }

        public decimal TaxAmount
        {
            get
            {
                return NetPrice * 15/100;
            }
        }
         


        public decimal Price
        {
            get
            {
                return NetPrice + TaxAmount;
            }
        }


        public decimal AttachNfcPrice { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public decimal SimPrice { get; set; }
        public bool Free { get; set; }
        public bool Visible { get; set; }
    }

    [AutoMapTo(typeof(Package))]
    public class CreatePackageDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }

        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }

        public decimal AttachNfcPrice { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public decimal SimPrice { get; set; }
        public bool Free { get; set; }
        public bool Visible { get; set; }
    }

    [AutoMapTo(typeof(Package))]
    public class UpdatePackageDto : EntityDto<long>
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }

        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }

        public decimal AttachNfcPrice { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public decimal SimPrice { get; set; }
        public bool Free { get; set; }
        public bool Visible { get; set; }
    }
    public class GetAllPackages : PagedResultRequestDto
    {
        public string Name { get; set; }
        public bool MaxCount { get; set; }
        public int? VeichlesFrom { get; set; }
        public int? VeichlesTo { get; set; }
        public int? VeichlesCount { get; set; }

        public decimal? AttachNfcPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public decimal? SimPrice { get; set; }

    }
    public class GetPackagePagedInput : DataTableInputDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }

        public int? VeichlesFrom { get; set; }
        public int? VeichlesTo { get; set; }

        public decimal? AttachNfcPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }

        public decimal? SimPrice { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public int? DurationFrom { get; set; }
        public int? DurationTo { get; set; }
        public long? PackageId { get; set; }
        public bool? Visible { get; set; }
        public bool? Free { get; set; }

    }

}
