using Abp.Application.Services.Dto;
using Abp.EntityHistory;
using Abp.Events.Bus.Entities;
using System;
using System.Collections.Generic;

namespace Sayarah.AbpZeroTemplate.Auditing.Dto
{
    public class EntityChangeListDto : EntityDto<long>
    {
        public long? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ChangeTime { get; set; }

        public string EntityTypeFullName { get; set; }

        public EntityChangeType ChangeType { get; set; }

        public string ChangeTypeName => ChangeType.ToString();

        public long EntityChangeSetId { get; set; }
        public string EntityId { get; set; }


    }



    public class EntityChangeWithPropertiesListDto : EntityDto<long>
    {
        public long? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ChangeTime { get; set; }

        public string EntityTypeFullName { get; set; }

        public EntityChangeType ChangeType { get; set; }

        public string ChangeTypeName => ChangeType.ToString();

        public long EntityChangeSetId { get; set; }
        public string EntityId { get; set; }
        public List<EntityPropertyChange> PropertyChanges { get; set; }
    }


}