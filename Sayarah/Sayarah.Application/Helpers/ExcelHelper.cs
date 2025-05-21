using Sayarah.Application.DataTables.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers
{
    public interface IExcel<T> where T : ExcelBaseInput
    {
        Task<string> Export(T input);
    }
    public class ExcelBaseInput
    {
        public Column[] columns { get; set; }
        public Order[] order { get; set; }
        public List<DisplayedColumn> DisplayedColumns { get; set; }
        public bool? isDeleted { get; set; }
        public string creatorUserName { get; set; }
        public DateTime? creationTimeFrom { get; set; }
        public DateTime? creationTimeTo { get; set; }
        public string modifierUserName { get; set; }
        public DateTime? lastModificationTimeFrom { get; set; }
        public DateTime? lastModificationTimeTo { get; set; }
        public string deleterUserName { get; set; }
        public DateTime? deletionTimeFrom { get; set; }
        public DateTime? deletionTimeTo { get; set; }
        public string lang { get; set; }
    }
    public class DisplayedColumn
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }

}
