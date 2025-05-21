using Abp.Auditing;
using System;
using System.Collections.Generic;
namespace Sayarah.Application.DataTables.Dto
{
    public class DataTableInputDto
    {
        [DisableAuditing]
        public Column[] columns { get; set; }
        [DisableAuditing]
        public Order[] order { get; set; }
        [DisableAuditing]
        public int start { get; set; }
        [DisableAuditing]
        public int length { get; set; }
        [DisableAuditing]
        public int draw { get; set; }
        [DisableAuditing]
        public bool? isDeleted { get; set; }
        [DisableAuditing]
        public string creatorUserName { get; set; }
        [DisableAuditing]
        public DateTime? creationTimeFrom { get; set; }
        [DisableAuditing]
        public DateTime? creationTimeTo { get; set; }
        [DisableAuditing]
        public string modifierUserName { get; set; }
        [DisableAuditing]
        public DateTime? lastModificationTimeFrom { get; set; }
        [DisableAuditing]
        public DateTime? lastModificationTimeTo { get; set; }
        [DisableAuditing]
        public string deleterUserName { get; set; }
        [DisableAuditing]
        public DateTime? deletionTimeFrom { get; set; }
        [DisableAuditing]
        public DateTime? deletionTimeTo { get; set; }
         
        public string actionType { get; set; }
        public string action { get; set; }
        public string[] ids { get; set; }
        [DisableAuditing]
        public string lang { get; set; }

    }

    public class DataTableOutputDto<T>
    {
        public int draw { get; set; }
        public string actionResult { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public IReadOnlyList<T> aaData { get; set; }
        public DataTableOutputDto()
        {
        }
        public DataTableOutputDto(int _iTotalRecords,int _iTotalDisplayRecords, IReadOnlyList<T> _items)
        {
            iTotalRecords = _iTotalRecords;
            iTotalDisplayRecords = _iTotalDisplayRecords;
            aaData = _items;
        }
    }

    public class Column
    {
        public string name { get; set; }
        public bool orderable { get; set; }
        public bool searchable { get; set; }
        public Search search{ get; set; }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class Search
    {
        public bool regex { get; set; }
        public string value { get; set; }
    }
}
