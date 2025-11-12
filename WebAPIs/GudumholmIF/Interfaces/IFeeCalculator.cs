namespace GudumholmIF.Interfaces
{
    public interface IFeeCalculator
    {
        Task<decimal> PersonAnnualAsync(int personId, CancellationToken ct);

        Task<decimal> HouseholdAnnualAsync(int householdId, CancellationToken ct);

        Task<decimal> SportAnnualAsync(int sportId, CancellationToken ct);

        Task<decimal> AllSportsAnnualAsync(CancellationToken ct);

        Task<decimal> AllPersonsAnnualAsync(CancellationToken ct);
    }
}