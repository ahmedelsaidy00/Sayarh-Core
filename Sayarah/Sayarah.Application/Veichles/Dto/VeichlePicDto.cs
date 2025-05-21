using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Veichles;
using System.Collections.Generic;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(VeichlePic)), AutoMapTo(typeof(VeichlePic))]
    public class VeichlePicDto : EntityDto<long>
    {
        public string FilePath { get; set; }
        public long VeichleId { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(5, "800x600_" + FilePath))
                    return FilesPath.Veichles.ServerImagePath + "800x600_" + FilePath;
                else
                    return FilesPath.Veichles.DefaultImagePath;
            }
        }

        public string LargeFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(5, FilePath))
                    return FilesPath.Veichles.ServerImagePath + FilePath;
                else
                    return FilesPath.Veichles.DefaultImagePath;
            }
        }
    }
    [AutoMapFrom(typeof(VeichlePic))]
    public class ApiVeichlePic:EntityDto<long>
    {
        public long VeichleId { get; set; }
        public string FullFilePath { get; set; }
    }

    [AutoMapTo(typeof(VeichlePic))]
    public class CreateVeichlePicDto
    {
        public string Code { get; set; }
        public string FilePath { get; set; }
        public long VeichleId { get; set; }
    }

    [AutoMapTo(typeof(VeichlePic))]
    public class SaveVeichlePicDto
    {
        public List<VeichlePic> VeichlePicList { get; set; }
    }

    public class DeleteVeichlePicInput : EntityDto<long>
    {
        public int StorageLocation { get; set; }
    }



    [AutoMapTo(typeof(VeichlePic))]
    public class UpdateVeichlePicDto : EntityDto<long>
    {
        public string FilePath { get; set; }
        public long VeichleId { get; set; }
        public virtual VeichleDto Veichle { get; set; }

    }
    public class GetVeichlePicInput : DataTableInputDto
    {
        public long? VeichleId { get; set; }

    }
    public class GetAllVeichlePic : PagedResultRequestDto
    {
        public long? VeichleId { get; set; }
    }
}
