using GudumholmIF.Interfaces;
using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Utilites
{
    public sealed class FeeCalculator : IFeeCalculator
    {
        private readonly ClubContext _db;

        public FeeCalculator(ClubContext db) => _db = db;

        public async Task<decimal> PersonAnnualAsync(int personId, CancellationToken ct)
        {
            var isActive = await _db.MembershipStates
                .Where(s => s.PersonId == personId)
                .Select(s => s.State == MembershipActivityState.Active)
                .SingleAsync(ct);

            if (!isActive) return 0m;

            var q = from ps in _db.PersonSports
                    where ps.PersonId == personId && ps.Left == null
                    select ps.Sport.AnnualFee;
            return await q.SumAsync(ct);
        }

        public async Task<decimal> HouseholdAnnualAsync(int householdId, CancellationToken ct)
        {
            var q = from p in _db.Persons
                    where p.HouseholdId == householdId && p.State.State == MembershipActivityState.Active
                    from ps in p.Sports
                    where ps.Left == null
                    select ps.Sport.AnnualFee;
            return await q.SumAsync(ct);
        }

        public async Task<decimal> SportAnnualAsync(int sportId, CancellationToken ct)
        {
            var q = from ps in _db.PersonSports
                    where ps.SportId == sportId
                          && ps.Left == null
                          && ps.Person.State.State == MembershipActivityState.Active
                    select ps.Sport.AnnualFee;
            return await q.SumAsync(ct);
        }

        public async Task<decimal> AllSportsAnnualAsync(CancellationToken ct)
        {
            var q = from ps in _db.PersonSports
                    where ps.Left == null && ps.Person.State.State == MembershipActivityState.Active
                    select ps.Sport.AnnualFee;
            return await q.SumAsync(ct);
        }
    }
}