using System.Data;
using System.IO;

namespace Sayarah.Application.Helpers
{
    //public class ExcelReader
    //{
    //    public DataSet ImportExcel(string filePath)
    //    {

    //        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
    //        {

    //            // Auto-detect format, supports:
    //            //  - Binary Excel files (2.0-2003 format; *.xls)
    //            //  - OpenXml Excel files (2007 format; *.xlsx)
    //            using (var reader = ExcelReaderFactory.CreateReader(stream))
    //            {

    //                // Choose one of either 1 or 2:

    //                // 1. Use the reader methods
    //                do
    //                {
    //                    while (reader.Read())
    //                    {
    //                        // reader.GetDouble(0);
    //                    }
    //                } while (reader.NextResult());

    //                // 2. Use the AsDataSet extension method
    //                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
    //                {
    //                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
    //                    {
    //                        UseHeaderRow = true
    //                    }
    //                });
    //                return result;
    //                // The result of each spreadsheet is in result.Tables
    //            }
    //        }

    //        //FileStream stream;
    //        //DataSet result = null;
    //        //IExcelDataReader excelReader = null;

    //        //stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
    //        //if (filePath.Contains(".xlsx"))
    //        //    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
    //        //else if (filePath.Contains(".xls"))
    //        //    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
    //        ////excelReader. = true;
    //        //if (excelReader != null)
    //        //    result = excelReader.AsDataSet();

    //        //return result;
    //    }
    //}
}

//if (System.IO.File.Exists(filePath))
//           {
//                stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
//               //1. Reading from a binary Excel file ('97-2003 format; *.xls)
//                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
//               //...
//               //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
//                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
//               //...
//               //3. DataSet - The result of each spreadsheet will be created in the result.Tables
//                result = excelReader.AsDataSet();

//           }

