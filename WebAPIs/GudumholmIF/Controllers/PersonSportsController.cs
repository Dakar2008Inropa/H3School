using GudumholmIF.Interfaces;
using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.BoardAndPersonSport;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/persons/{personId:int}/sports")]
    public sealed class PersonSportsController : ControllerBase
    {
        private readonly ClubContext db;
        private readonly IMembershipService membership;

        public PersonSportsController(ClubContext db, IMembershipService membership)
        {
            this.db = db;
            this.membership = membership;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonSportDto>>> GetAll(int personId, CancellationToken ct)
        {
            bool exists = await db.Persons.AnyAsync(p => p.Id == personId, ct);
            if (!exists) return NotFound();

            List<PersonSport> list = await db.PersonSports
                .Where(ps => ps.PersonId == personId)
                .AsNoTracking()
                .OrderByDescending(m => m.Left == null)
                .ThenByDescending(m => m.FullLeftDate ?? m.FullJoinedDate)
                .ToListAsync(ct);

            return Ok(list.Adapt<List<PersonSportDto>>());
        }

        [HttpPost("join")]
        public async Task<ActionResult<PersonSportDto>> Join(int personId, [FromBody] PersonSportJoinDto dto, CancellationToken ct)
        {
            Person person = await db.Persons.Include(p => p.State).FirstOrDefaultAsync(p => p.Id == personId, ct);
            if (person == null) return NotFound();

            var dateTimeToday = DateTime.Now;

            DateOnly joined = dto.Joined ?? DateOnly.FromDateTime(dateTimeToday);
            DateTime joinedFully = dateTimeToday;

            bool alreadyActive = await db.PersonSports.AnyAsync(ps => ps.PersonId == personId && ps.SportId == dto.SportId && ps.Left == null, ct);
            if (alreadyActive) return Conflict("Already a member of this sport.");

            PersonSport link = new PersonSport
            {
                PersonId = personId,
                SportId = dto.SportId,
                Joined = joined,
                FullJoinedDate = joinedFully,
                FullLeftDate = null,
                Active = true,
                Left = null
            };
            db.PersonSports.Add(link);
            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(personId, "Joined sport", ct);

            return CreatedAtAction(nameof(GetAll), new { personId }, link.Adapt<PersonSportDto>());
        }

        [HttpPost("{sportId:int}/leave")]
        public async Task<IActionResult> Leave(int personId, int sportId, [FromBody] PersonSportLeaveDto dto, CancellationToken ct)
        {
            PersonSport active = await db.PersonSports
                .Where(ps => ps.PersonId == personId && ps.SportId == sportId && ps.Left == null)
                .OrderByDescending(ps => ps.Joined)
                .FirstOrDefaultAsync(ct);

            if (active == null) return NotFound();

            DateTime now = DateTime.Now;
            DateTime fullLeft = dto.FullLeftDate != default(DateTime) ? dto.FullLeftDate : now;
            DateOnly leftDate = dto.Left != default(DateOnly) ? dto.Left : DateOnly.FromDateTime(fullLeft);

            active.Left = leftDate;
            active.FullLeftDate = fullLeft;
            active.Active = false;
            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(personId, "Left sport", ct);

            return NoContent();
        }
    }
}