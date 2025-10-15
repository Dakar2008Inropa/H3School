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

        public PersonsController(ClubContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDto>>> GetAll(CancellationToken ct)
        {
            List<Person> entities = await db.Persons.Include(p => p.State).Include(p => p.ParentRole).ToListAsync(ct);

            List<PersonDto> dtos = entities.Adapt<List<PersonDto>>();

            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PersonDto>> Get(int id, CancellationToken ct)
        {
            Person entity = await db.Persons.Include(p => p.State).Include(p => p.ParentRole).FirstOrDefaultAsync(p => p.Id == id, ct);

            if (entity == null) return NotFound();

            PersonDto dto = entity.Adapt<PersonDto>();

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<PersonDto>> Create([FromBody] PersonCreateDto dto, CancellationToken ct)
        {
            Person entity = dto.Adapt<Person>();

            MembershipActivityState state = dto.MembershipState == "Active" ? MembershipActivityState.Active : MembershipActivityState.Passive;

            MembershipState ms = new MembershipState
            {
                Person = entity,
                State = state,
                ActiveSince = state == MembershipActivityState.Active ? DateOnly.FromDateTime(DateTime.Today) : null,
                PassiveSince = state == MembershipActivityState.Passive ? DateOnly.FromDateTime(DateTime.Today) : null
            };

            db.Persons.Add(entity);
            db.MembershipStates.Add(ms);

            await db.SaveChangesAsync(ct);

            PersonDto result = (await db.Persons.Include(p => p.State).Include(p => p.ParentRole).FirstAsync(p => p.Id == entity.Id, ct)).Adapt<PersonDto>();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PersonUpdateDto dto, CancellationToken ct)
        {
            Person entity = await db.Persons.Include(p => p.State).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null) return NotFound();

            dto.Adapt(entity);

            MembershipActivityState newState = dto.MembershipState == "Active" ? MembershipActivityState.Active : MembershipActivityState.Passive;

            if (entity.State.State != newState)
            {
                entity.State.State = newState;

                if (newState == MembershipActivityState.Active)
                {
                    entity.State.ActiveSince = DateOnly.FromDateTime(DateTime.Today);
                    entity.State.PassiveSince = null;
                }
                else
                {
                    entity.State.PassiveSince = DateOnly.FromDateTime(DateTime.Today);
                    entity.State.ActiveSince = null;
                }
            }

            await db.SaveChangesAsync(ct);

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
            int children = entity.HouseHold.Members.Count(m => today < m.DateOfBirth.AddYears(18));
            if (children < 1) return BadRequest("Cannot create parent role. No children under 18 in the household.");

            ParentRole pr = new ParentRole { PersonId = id, ActiveChildrenCount = children };
            db.ParentRoles.Add(pr);

            if (entity.State.State != MembershipActivityState.Active)
            {
                entity.State.State = MembershipActivityState.Active;
                entity.State.ActiveSince = today;
                entity.State.PassiveSince = null;
            }

            await db.SaveChangesAsync(ct);

            PersonDto result = (await db.Persons.Include(p => p.State).Include(p => p.ParentRole).FirstAsync(p => p.Id == id, ct)).Adapt<PersonDto>();
            return Ok(result);
        }
    }
}