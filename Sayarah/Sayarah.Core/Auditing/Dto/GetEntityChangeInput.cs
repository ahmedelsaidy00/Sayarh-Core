using Abp.Events.Bus.Entities;
using Abp.Runtime.Validation;
using Sayarah.AbpZeroTemplate.Common;
using Sayarah.AbpZeroTemplate.Dto;
using System;

namespace Sayarah.AbpZeroTemplate.Auditing.Dto
{
    public class GetEntityChangeInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public long? Id { get; set; }
        public string UserName { get; set; }

        public string EntityTypeFullName { get; set; }
        public string EntityId { get; set; }
        public EntityChangeType? ChangeType { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "ChangeTime DESC";
            }

            Sorting = DtoSortingHelper.ReplaceSorting(Sorting, s =>
            {
                if (s.IndexOf("UserName", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    s = "User." + s;
                }
                else
                {
                    s = "EntityChange." + s;
                }

                return s;
            });
        }
    }

    public class GetEntityTypeChangeInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string EntityTypeFullName { get; set; }

        public string EntityId { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "ChangeTime DESC";
            }

            Sorting = DtoSortingHelper.ReplaceSorting(Sorting, s =>
            {
                if (s.IndexOf("UserName", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    s = "User." + s;
                }
                else
                {
                    s = "EntityChange." + s;
                }

                return s;
            });
        }
    }
}
