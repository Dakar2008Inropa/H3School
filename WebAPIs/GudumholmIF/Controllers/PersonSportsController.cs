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

        public PersonSportsController(ClubContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonSportDto>>> GetAll(int personId, CancellationToken ct)
        {
            bool exists = await db.Persons.AnyAsync(p => p.Id == personId, ct);
            if (!exists) return NotFound();

            List<PersonSport> list = await db.PersonSports
                .Where(ps => ps.PersonId == personId)
                .AsNoTracking()
                .ToListAsync(ct);

            return Ok(list.Adapt<List<PersonSportDto>>());
        }

        [HttpPost("join")]
        public async Task<ActionResult<PersonSportDto>> Join(int personId, [FromBody] PersonSportJoinDto dto, CancellationToken ct)
        {
            Person person = await db.Persons.Include(p => p.State).FirstOrDefaultAsync(p => p.Id == personId, ct);
            if (person == null) return NotFound();

            if (person.State.State != MembershipActivityState.Active) return BadRequest("Person must be active to join a sport.");

            DateOnly joined = dto.Joined ?? DateOnly.FromDateTime(DateTime.Today);

            bool alreadyActive = await db.PersonSports.AnyAsync(ps => ps.PersonId == personId && ps.SportId == dto.SportId && ps.Left == null, ct);
            if (alreadyActive) return Conflict("Already a member of this sport.");

            PersonSport link = new PersonSport
            {
                PersonId = personId,
                SportId = dto.SportId,
                Joined = joined,
                Left = null
            };
            db.PersonSports.Add(link);
            await db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetAll), new { personId = personId }, link.Adapt<PersonSportDto>());
        }

        [HttpPost("{sportId:int}/leave")]
        public async Task<IActionResult> Leave(int personId, int sportId, [FromBody] PersonSportLeaveDto dto, CancellationToken ct)
        {
            PersonSport active = await db.PersonSports
                .Where(ps => ps.PersonId == personId && ps.SportId == sportId && ps.Left == null)
                .OrderByDescending(ps => ps.Joined)
                .FirstOrDefaultAsync(ct);

            if (active == null) return NotFound();

            active.Left = dto.Left;
            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}