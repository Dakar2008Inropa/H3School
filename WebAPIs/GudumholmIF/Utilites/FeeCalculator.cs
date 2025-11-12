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

        private static bool IsAdult(DateOnly dob)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            return today >= dob.AddYears(18);
        }

        private async Task<ApplicationSetting> GetSettingsOrDefaultAsync(CancellationToken ct)
        {
            ApplicationSetting settings = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            if (settings != null) return settings;

            return new ApplicationSetting
            {
                PassiveAdultAnnualFee = 400m,
                PassiveChildAnnualFee = 200m
            };
        }

        public async Task<decimal> PersonAnnualAsync(int personId, CancellationToken ct)
        {
            Person person = await _db.Persons
                .Include(p => p.State)
                .Include(p => p.Sports.Where(ps => ps.Left == null))
                    .ThenInclude(ps => ps.Sport)
                .Include(p => p.BoardRoles.Where(br => br.To == null))
                .FirstOrDefaultAsync(p => p.Id == personId, ct);

            if (person == null) return 0m;
            if (person.State == null) return 0m;

            bool isAdult = IsAdult(person.DateOfBirth);

            if (person.State.State != MembershipActivityState.Active)
            {
                ApplicationSetting s = await GetSettingsOrDefaultAsync(ct);
                return isAdult ? s.PassiveAdultAnnualFee : s.PassiveChildAnnualFee;
            }

            List<int> boardRoleSportIds = person.BoardRoles
                .Where(br => br.To == null)
                .Select(br => br.SportId)
                .Distinct()
                .ToList();

            decimal sum = person.Sports
                .Where(ps => ps.Sport != null && !boardRoleSportIds.Contains(ps.SportId))
                .Select(ps => isAdult ? ps.Sport.AnnualFeeAdult : ps.Sport.AnnualFeeChild)
                .Sum();

            return sum;
        }

        public async Task<decimal> HouseholdAnnualAsync(int householdId, CancellationToken ct)
        {
            List<Person> persons = await _db.Persons
                .Where(p => p.HouseholdId == householdId)
                .Include(p => p.State)
                .Include(p => p.Sports.Where(ps => ps.Left == null))
                    .ThenInclude(ps => ps.Sport)
                .Include(p => p.BoardRoles.Where(br => br.To == null))
                .ToListAsync(ct);

            if (persons.Count == 0) return 0m;

            ApplicationSetting settings = await GetSettingsOrDefaultAsync(ct);
            decimal total = 0m;

            foreach (Person p in persons)
            {
                if (p.State == null) continue;
                bool isAdult = IsAdult(p.DateOfBirth);

                if (p.State.State == MembershipActivityState.Active)
                {
                    List<int> boardRoleSportIds = p.BoardRoles
                        .Where(br => br.To == null)
                        .Select(br => br.SportId)
                        .Distinct()
                        .ToList();

                    total += p.Sports
                        .Where(ps => ps.Sport != null && !boardRoleSportIds.Contains(ps.SportId))
                        .Select(ps => isAdult ? ps.Sport.AnnualFeeAdult : ps.Sport.AnnualFeeChild)
                        .Sum();
                }
                else
                {
                    total += isAdult ? settings.PassiveAdultAnnualFee : settings.PassiveChildAnnualFee;
                }
            }

            return total;
        }

        public async Task<decimal> SportAnnualAsync(int sportId, CancellationToken ct)
        {
            var rows = await _db.PersonSports
                .Where(ps => ps.SportId == sportId &&
                             ps.Left == null &&
                             ps.Person.State.State == MembershipActivityState.Active)
                .Select(ps => new
                {
                    Dob = ps.Person.DateOfBirth,
                    AdultFee = ps.Sport.AnnualFeeAdult,
                    ChildFee = ps.Sport.AnnualFeeChild,
                    HasBoardRole = ps.Person.BoardRoles.Any(br => br.To == null && br.SportId == sportId)
                })
                .AsNoTracking()
                .ToListAsync(ct);

            decimal sum = 0m;
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            foreach (var r in rows)
            {
                if (r.HasBoardRole) continue;

                bool isAdult = today >= r.Dob.AddYears(18);
                sum += isAdult ? r.AdultFee : r.ChildFee;
            }
            return sum;
        }

        public async Task<decimal> AllSportsAnnualAsync(CancellationToken ct)
        {
            var rows = await _db.PersonSports
                .Where(ps => ps.Left == null &&
                             ps.Person.State.State == MembershipActivityState.Active)
                .Select(ps => new
                {
                    Dob = ps.Person.DateOfBirth,
                    AdultFee = ps.Sport.AnnualFeeAdult,
                    ChildFee = ps.Sport.AnnualFeeChild,
                    HasBoardRole = ps.Person.BoardRoles.Any(br => br.To == null && br.SportId == ps.SportId)
                })
                .AsNoTracking()
                .ToListAsync(ct);

            decimal sum = 0m;
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            foreach (var r in rows)
            {
                if (r.HasBoardRole) continue;
                bool isAdult = today >= r.Dob.AddYears(18);
                sum += isAdult ? r.AdultFee : r.ChildFee;
            }
            return sum;
        }

        public async Task<decimal> AllPersonsAnnualAsync(CancellationToken ct)
        {
            ApplicationSetting settings = await GetSettingsOrDefaultAsync(ct);

            List<Person> persons = await _db.Persons
                .Include(p => p.State)
                .Include(p => p.Sports.Where(ps => ps.Left == null))
                    .ThenInclude(ps => ps.Sport)
                .Include(p => p.BoardRoles.Where(br => br.To == null))
                .AsNoTracking()
                .ToListAsync(ct);

            if (persons.Count == 0) return 0m;

            decimal total = 0m;

            foreach (Person p in persons)
            {
                if (p.State == null) continue;

                bool isAdult = IsAdult(p.DateOfBirth);

                if (p.State.State == MembershipActivityState.Active)
                {
                    List<int> boardRoleSportIds = p.BoardRoles
                        .Where(br => br.To == null)
                        .Select(br => br.SportId)
                        .Distinct()
                        .ToList();

                    total += p.Sports
                        .Where(ps => ps.Sport != null && !boardRoleSportIds.Contains(ps.SportId))
                        .Select(ps => isAdult ? ps.Sport.AnnualFeeAdult : ps.Sport.AnnualFeeChild)
                        .Sum();
                }
                else
                {
                    total += isAdult ? settings.PassiveAdultAnnualFee : settings.PassiveChildAnnualFee;
                }
            }

            return total;
        }
    }
}