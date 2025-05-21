using Sayarah.Application.Authorization.Accounts.Dto;

namespace Sayarah.Application.authorization.Accounts.Dto;

public class IsTenantAvailableOutput
{
    public TenantAvailabilityState State { get; set; }
    public int? TenantId { get; set; }
    public IsTenantAvailableOutput(TenantAvailabilityState state, int? tenantId = null)
    {
        State = state;
        TenantId = tenantId;
    }
}