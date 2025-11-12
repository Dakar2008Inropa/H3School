using GudumholmIF.Models.Application;

namespace GudumholmIF.Interfaces
{
    public interface IMembershipService
    {
        Task<MembershipActivityState> RecalculateAsync(int personId, string reason, CancellationToken ct);
    }
}