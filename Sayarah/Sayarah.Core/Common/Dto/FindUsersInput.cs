using Sayarah.AbpZeroTemplate.Dto;

namespace Sayarah.AbpZeroTemplate.Common.Dto
{
    public class FindUsersInput : PagedAndFilteredInputDto
    {
        public int? TenantId { get; set; }

        public bool ExcludeCurrentUser { get; set; }
    }
}