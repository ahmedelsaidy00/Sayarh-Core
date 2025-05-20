using Abp.Application.Services.Dto;

namespace Sayarah.AbpZeroTemplate.Dto
{
    public class PagedAndSortedInputDto : PagedInputDto, ISortedResultRequest
    {
        public string Sorting { get; set; }

        public PagedAndSortedInputDto()
        {
            MaxResultCount = SayarahConsts.DefaultPageSize;
        }
    }
}