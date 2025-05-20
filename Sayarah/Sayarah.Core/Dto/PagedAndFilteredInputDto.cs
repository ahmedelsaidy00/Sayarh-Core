using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Sayarah.AbpZeroTemplate.Dto
{
    public class PagedAndFilteredInputDto : IPagedResultRequest
    {
        [Range(1, SayarahConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        public string Filter { get; set; }

        public PagedAndFilteredInputDto()
        {
            MaxResultCount = SayarahConsts.DefaultPageSize;
        }
    }
}