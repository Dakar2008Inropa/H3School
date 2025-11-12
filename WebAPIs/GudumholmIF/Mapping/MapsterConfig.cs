using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.BoardAndPersonSport;
using GudumholmIF.Models.DTOs.Household;
using GudumholmIF.Models.DTOs.Person;
using GudumholmIF.Models.DTOs.Settings;
using GudumholmIF.Models.DTOs.Sport;
using Mapster;

namespace GudumholmIF.Mapping
{
    public static class MapsterConfig
    {
        public static void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Household, HouseholdDto>()
                .Map(d => d.MemberCount, s => s.Members != null ? s.Members.Count : 0);

            config.NewConfig<HouseholdCreateDto, Household>();

            config.NewConfig<HouseholdUpdateDto, Household>();

            config.NewConfig<Person, PersonDto>()
                .Map(d => d.MembershipState, s => s.State.State.ToString())
                .Map(d => d.ActiveChildrenCount, s => s.ParentRole != null ? s.ParentRole.ActiveChildrenCount : 0)
                .Map(d => d.HasParentRole, s => s.ParentRole != null)
                .Map(d => d.ChildrenUnder18Count,
                s => s.HouseHold != null && s.HouseHold.Members != null
                ? s.HouseHold.Members.Count(m => DateOnly.FromDateTime(DateTime.Today) < m.DateOfBirth.AddYears(18))
                : 0);

            config.NewConfig<PersonCreateDto, Person>()
                .Ignore(d => d.Id)
                .Ignore(d => d.State)
                .Ignore(d => d.ParentRole)
                .Ignore(d => d.BoardRoles)
                .Ignore(d => d.Sports);

            config.NewConfig<PersonUpdateDto, Person>()
                .Ignore(d => d.Id)
                .Ignore(d => d.State)
                .Ignore(d => d.ParentRole)
                .Ignore(d => d.BoardRoles)
                .Ignore(d => d.Sports);

            config.NewConfig<Sport, SportDto>();
            config.NewConfig<SportFeeHistory, SportFeeHistoryDto>();

            config.NewConfig<Sport, SportDto>();

            config.NewConfig<SportCreateDto, Sport>()
                .Ignore(d => d.Id)
                .Ignore(d => d.FeeHistory)
                .Ignore(d => d.Members);

            config.NewConfig<SportUpdateDto, Sport>()
                .Ignore(d => d.Id)
                .Ignore(d => d.FeeHistory)
                .Ignore(d => d.Members);

            config.NewConfig<SportFeeHistory, SportFeeHistoryDto>();

            config.NewConfig<SportFeeChangeDto, SportFeeHistory>()
                .Ignore(d => d.Id)
                .Ignore(d => d.Sport);

            config.NewConfig<BoardRole, BoardRoleDto>();

            config.NewConfig<BoardRoleCreateDto, BoardRole>()
                .Ignore(d => d.Id)
                .Ignore(d => d.Person)
                .Ignore(d => d.Sport)
                .Ignore(d => d.To);

            config.NewConfig<PersonSport, PersonSportDto>();

            config.NewConfig<MembershipHistory, MembershipHistoryDto>()
                .Map(d => d.State, s => s.State.ToString());

            config.NewConfig<ApplicationSetting, SettingsDto>();
        }
    }
}