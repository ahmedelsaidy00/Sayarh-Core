using System;
using System.Linq;
using System.Collections.Generic;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Helpers
{
    /// <summary>
    /// Some useful extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="filterInput">DataTable filter input</param>
        /// <returns>Filtered or not filtered query based on <see cref="condition"/></returns>
        public static IQueryable<T> FilterDataTable<T>(this IQueryable<T> query, DataTableInputDto filterInput)
        {
            List<Filter> filter = new List<Filter>();
            //Filter by creation params
            if (!string.IsNullOrEmpty(filterInput.creatorUserName))
                
                    filter.Add(new Filter() { PropertyName = "CreatorUser.Name", Operation = Op.Contains, Value = filterInput.creatorUserName });

            if (filterInput.creationTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "CreationTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.creationTimeFrom });
            if (filterInput.creationTimeTo.HasValue)
            {
                filterInput.creationTimeTo = filterInput.creationTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "CreationTime", Operation = Op.LessThanOrEqual, Value = filterInput.creationTimeTo });
            }
            //Filter by modification params
            if (!string.IsNullOrEmpty(filterInput.modifierUserName))
                
                    filter.Add(new Filter() { PropertyName = "LastModifierUser.Name", Operation = Op.Contains, Value = filterInput.modifierUserName });
            if (filterInput.lastModificationTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "LastModificationTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.lastModificationTimeFrom });
            if (filterInput.lastModificationTimeTo.HasValue)
            {
                filterInput.lastModificationTimeTo = filterInput.lastModificationTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "LastModificationTime", Operation = Op.LessThanOrEqual, Value = filterInput.lastModificationTimeTo });
            }
            //Filter by deletion params
            // if delete filter applied respect it otherwise get the not deleted
            if (typeof(T).GetProperty("IsDeleted") != null)
            {
                if (filterInput.isDeleted.HasValue)
                    filter.Add(new Filter() { PropertyName = "IsDeleted", Operation = Op.Equals, Value = filterInput.isDeleted });
                else
                    filter.Add(new Filter() { PropertyName = "IsDeleted", Operation = Op.Equals, Value = false });
            }
            if (!string.IsNullOrEmpty(filterInput.deleterUserName))
                
                    filter.Add(new Filter() { PropertyName = "DeleterUser.Name", Operation = Op.Contains, Value = filterInput.deleterUserName });
            if (filterInput.deletionTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "DeletionTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.deletionTimeFrom });
            if (filterInput.deletionTimeTo.HasValue)
            {
                filterInput.deletionTimeTo = filterInput.deletionTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "DeletionTime", Operation = Op.LessThanOrEqual, Value = filterInput.deletionTimeTo });
            }

            var deleg = ExpressionBuilder.GetExpression<T>(filter);
            if (deleg != null)
                return query.Where(deleg);
            else
                return query;
        }

        public static IQueryable<T> FilterDataTable<T>(this IQueryable<T> query, ExcelBaseInput filterInput)
        {
            List<Filter> filter = new List<Filter>();
            //Filter by creation params
            if (!string.IsNullOrEmpty(filterInput.creatorUserName))
                if (filterInput.lang.Equals("ar"))
                    filter.Add(new Filter() { PropertyName = "CreatorUser.Name", Operation = Op.Contains, Value = filterInput.creatorUserName });

            if (filterInput.creationTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "CreationTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.creationTimeFrom });
            if (filterInput.creationTimeTo.HasValue)
            {
                filterInput.creationTimeTo = filterInput.creationTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "CreationTime", Operation = Op.LessThanOrEqual, Value = filterInput.creationTimeTo });
            }
            //Filter by modification params
            if (!string.IsNullOrEmpty(filterInput.modifierUserName))
                filter.Add(new Filter() { PropertyName = "LastModifierUser.Name", Operation = Op.Contains, Value = filterInput.modifierUserName });
            if (filterInput.lastModificationTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "LastModificationTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.lastModificationTimeFrom });
            if (filterInput.lastModificationTimeTo.HasValue)
            {
                filterInput.lastModificationTimeTo = filterInput.lastModificationTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "LastModificationTime", Operation = Op.LessThanOrEqual, Value = filterInput.lastModificationTimeTo });
            }
            //Filter by deletion params
            // if delete filter applied respect it otherwise get the not deleted
            if (typeof(T).GetProperty("IsDeleted") != null)
            {
                if (filterInput.isDeleted.HasValue)
                    filter.Add(new Filter() { PropertyName = "IsDeleted", Operation = Op.Equals, Value = filterInput.isDeleted });
                else
                    filter.Add(new Filter() { PropertyName = "IsDeleted", Operation = Op.Equals, Value = false });
            }
            if (!string.IsNullOrEmpty(filterInput.deleterUserName))
                if (filterInput.lang.Equals("ar"))
                    filter.Add(new Filter() { PropertyName = "DeleterUser.Name", Operation = Op.Contains, Value = filterInput.deleterUserName });
            if (filterInput.deletionTimeFrom.HasValue)
                filter.Add(new Filter() { PropertyName = "DeletionTime", Operation = Op.GreaterThanOrEqual, Value = filterInput.deletionTimeFrom });
            if (filterInput.deletionTimeTo.HasValue)
            {
                filterInput.deletionTimeTo = filterInput.deletionTimeTo.Value.Add(TimeSpan.FromSeconds(86399)); // add 1 day minus 1 second
                filter.Add(new Filter() { PropertyName = "DeletionTime", Operation = Op.LessThanOrEqual, Value = filterInput.deletionTimeTo });
            }

            var deleg = ExpressionBuilder.GetExpression<T>(filter);
            if (deleg != null)
                return query.Where(deleg);
            else
                return query;
        }
    }

}