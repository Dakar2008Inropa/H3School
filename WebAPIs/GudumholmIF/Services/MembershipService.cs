using GudumholmIF.Interfaces;
using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Services
{
    public sealed class MembershipService : IMembershipService
    {
        private readonly ClubContext _db;

        public MembershipService(ClubContext db)
        {
            _db = db;
        }

        public async Task<MembershipActivityState> RecalculateAsync(int personId, string reason, CancellationToken ct)
        {
            Person person = await _db.Persons
                .Include(p => p.State)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .Include(p => p.BoardRoles.Where(br => br.To == null))
                .Include(p => p.Sports.Where(ps => ps.Left == null))
                .FirstOrDefaultAsync(p => p.Id == personId, ct);

            if (person == null || person.State == null) return MembershipActivityState.Passive;

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            bool hasOpenBoardRole = person.BoardRoles != null && person.BoardRoles.Any();
            bool hasActiveSport = person.Sports != null && person.Sports.Any(x => x.Active);

            bool shouldBeActive = hasOpenBoardRole || hasActiveSport;

            MembershipActivityState target = shouldBeActive ? MembershipActivityState.Active : MembershipActivityState.Passive;

            if (person.State.State == target) return target;

            person.State.State = target;

            if (target == MembershipActivityState.Active)
            {
                person.State.ActiveSince = today;
                person.State.PassiveSince = null;
            }
            else
            {
                person.State.PassiveSince = today;
                person.State.ActiveSince = null;
            }

            _db.Set<MembershipHistory>().Add(new MembershipHistory
            {
                PersonId = person.Id,
                State = target,
                ChangedOn = today,
                Reason = reason
            });

            await _db.SaveChangesAsync(ct);

            return target;
        }
    }
}