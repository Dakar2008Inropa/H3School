using GudumholmIF.Interfaces;
using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Person;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly ClubContext db;
        private readonly IMembershipService membership;

        public PersonsController(ClubContext db, IMembershipService membership)
        {
            this.db = db;
            this.membership = membership;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDto>>> GetAll(CancellationToken ct)
        {
            List<Person> entities = await db.Persons
                .Include(p => p.State)
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .ToListAsync(ct);

            List<PersonDto> dtos = entities.Adapt<List<PersonDto>>();

            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PersonDto>> Get(int id, CancellationToken ct)
        {
            Person entity = await db.Persons
                .Include(p => p.State)
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (entity == null) return NotFound();

            PersonDto dto = entity.Adapt<PersonDto>();

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<PersonDto>> Create([FromBody] PersonCreateDto dto, CancellationToken ct)
        {
            Person entity = dto.Adapt<Person>();

            MembershipState ms = new MembershipState
            {
                Person = entity,
                State = MembershipActivityState.Passive,
                ActiveSince = null,
                PassiveSince = DateOnly.FromDateTime(DateTime.Today)
            };

            db.Persons.Add(entity);
            db.MembershipStates.Add(ms);

            await db.SaveChangesAsync(ct);

            db.Set<MembershipHistory>().Add(new MembershipHistory
            {
                PersonId = entity.Id,
                State = MembershipActivityState.Passive,
                ChangedOn = DateOnly.FromDateTime(DateTime.Today),
                Reason = "Created"
            });

            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(entity.Id, "Auto state on create", ct);

            PersonDto result = (await db.Persons
                .Include(p => p.State)
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .FirstAsync(p => p.Id == entity.Id, ct)).Adapt<PersonDto>();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PersonUpdateDto dto, CancellationToken ct)
        {
            Person entity = await db.Persons.Include(p => p.State).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null) return NotFound();

            dto.Adapt(entity);

            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(id, "Auto state on person update", ct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            Person entity = await db.Persons.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null) return NotFound();

            db.Persons.Remove(entity);
            await db.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpPost("{id:int}/parent")]
        public async Task<ActionResult<PersonDto>> CreateParentRole(int id, [FromBody] ParentRoleCreateDto body, CancellationToken ct)
        {
            Person entity = await db.Persons
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .Include(p => p.State)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (entity == null) return NotFound();

            if (entity.ParentRole != null) return Conflict("Parent role already exists.");

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (today < entity.DateOfBirth.AddYears(18))
                return BadRequest("Cannot create parent role. Person must be 18 or older.");

            int children = entity.HouseHold.Members.Count(m => today < m.DateOfBirth.AddYears(18));
            if (children < 1) return BadRequest("Cannot create parent role. No children under 18 in the household.");

            ParentRole pr = new ParentRole { PersonId = id, ActiveChildrenCount = children };
            db.ParentRoles.Add(pr);

            await membership.RecalculateAsync(id, "Became parent", ct);

            await db.SaveChangesAsync(ct);

            PersonDto result = (await db.Persons
                .Include(p => p.State)
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .FirstAsync(p => p.Id == id, ct)).Adapt<PersonDto>();
            return Ok(result);
        }

        [HttpDelete("{id:int}/parent")]
        public async Task<ActionResult<PersonDto>> RemoveParentRole(int id, CancellationToken ct)
        {
            Person entity = await db.Persons
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .Include(p => p.State)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (entity == null) return NotFound();
            if (entity.ParentRole == null) return NotFound("Parent role deos not exist");

            db.ParentRoles.Remove(entity.ParentRole);

            await db.SaveChangesAsync(ct);

            PersonDto result = (await db.Persons
                .Include(p => p.State)
                .Include(p => p.ParentRole)
                .Include(p => p.HouseHold)
                    .ThenInclude(h => h.Members)
                .FirstAsync(p => p.Id == id, ct)).Adapt<PersonDto>();

            await membership.RecalculateAsync(id, "Remove parent role", ct);

            return Ok(result);
        }

        [HttpGet("{id:int}/membership-history")]
        public async Task<ActionResult<IEnumerable<MembershipHistoryDto>>> GetMembershipHistory(int id, CancellationToken ct)
        {
            bool exists = await db.Persons.AnyAsync(p => p.Id == id, ct);
            if (!exists) return NotFound();

            List<MembershipHistoryDto> dtos = await db.MembershipHistories
                .Where(h => h.PersonId == id)
                .OrderByDescending(h => h.ChangedOn)
                .ProjectToType<MembershipHistoryDto>()
                .ToListAsync(ct);

            return Ok(dtos);
        }
    }
}