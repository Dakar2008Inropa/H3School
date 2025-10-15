using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Services
{
    public sealed class ParentRefreshService : BackgroundService
    {
        private readonly IServiceScopeFactory _sf;

        public ParentRefreshService(IServiceScopeFactory sf) => _sf = sf;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                var next = now.Date.AddDays(1).AddHours(3);
                await Task.Delay(next - now, ct);

                using var scope = _sf.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ClubContext>();
                var today = DateOnly.FromDateTime(DateTime.Today);

                var parents = await db.ParentRoles
                    .Include(pr => pr.Person)
                        .ThenInclude(p => p.HouseHold)
                            .ThenInclude(h => h.Members)
                    .Include(pr => pr.Person)
                        .ThenInclude(p => p.Sports)
                    .Include(pr => pr.Person.State)
                    .ToListAsync(ct);

                foreach (var pr in parents)
                {
                    var members = pr.Person.HouseHold.Members;
                    var activeChildren = members.Count(m =>
                        today < m.DateOfBirth.AddYears(18) &&
                        (m.State.State == MembershipActivityState.Active ||
                         m.Sports.Any(ps => ps.Left == null)));

                    pr.ActiveChildrenCount = activeChildren;

                    if (activeChildren == 0 && pr.Person.State.State != MembershipActivityState.Passive)
                    {
                        pr.Person.State.State = MembershipActivityState.Passive;
                        pr.Person.State.PassiveSince = today;
                        pr.Person.State.ActiveSince = null;
                    }
                }

                await db.SaveChangesAsync(ct);
            }
        }
    }
}