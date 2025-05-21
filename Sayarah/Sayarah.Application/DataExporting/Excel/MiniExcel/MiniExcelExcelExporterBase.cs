using Abp.Dependency;
using Sayarah.AbpZeroTemplate.Dto;
using Sayarah.AbpZeroTemplate.Storage;

namespace Sayarah.Application.DataExporting.Excel.MiniExcel;

public abstract class MiniExcelExcelExporterBase(ITempFileCacheManager tempFileCacheManager) : SayarahAppServiceBase, ITransientDependency
{
    private readonly ITempFileCacheManager _tempFileCacheManager = tempFileCacheManager;

    protected FileDto CreateExcelPackage(string fileName, List<Dictionary<string, object>> items)
    {
        //var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
        var file = new FileDto();
        
        Save(items, file);

        return file;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    /// <param name="file"></param>
    protected virtual void Save(List<Dictionary<string, object>> items, FileDto file)
    {
        using var stream = new MemoryStream();
        // stream.SaveAs(items);
        _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
    }
}
