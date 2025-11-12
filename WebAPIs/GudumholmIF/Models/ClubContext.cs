using GudumholmIF.Models.Application;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Models
{
    public sealed class ClubContext : IdentityDbContext<ApplicationUser>
    {
        public ClubContext(DbContextOptions<ClubContext> options) : base(options)
        {
        }

        public DbSet<Household> HouseHolds => Set<Household>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<MembershipState> MembershipStates => Set<MembershipState>();
        public DbSet<ParentRole> ParentRoles => Set<ParentRole>();
        public DbSet<BoardRole> BoardRoles => Set<BoardRole>();
        public DbSet<Sport> Sports => Set<Sport>();
        public DbSet<SportFeeHistory> SportFeeHistories => Set<SportFeeHistory>();
        public DbSet<PersonSport> PersonSports => Set<PersonSport>();
        public DbSet<MembershipHistory> MembershipHistories => Set<MembershipHistory>();

        public DbSet<ApplicationSetting> AppSettings => Set<ApplicationSetting>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Household>(e =>
            {
                e.Property(p => p.Street).HasMaxLength(200).IsRequired();
                e.Property(p => p.City).HasMaxLength(120).IsRequired();
                e.Property(p => p.PostalCode).HasMaxLength(16).IsRequired();
            });

            b.Entity<Person>(e =>
            {
                e.HasIndex(p => p.CPR).IsUnique();
                e.Property(p => p.CPR).HasMaxLength(11).IsRequired();
                e.ToTable(t => t.HasCheckConstraint("CK_Person_CPR_Format",
                    "CPR LIKE '[0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]'"));

                e.HasOne(p => p.HouseHold)
                 .WithMany(h => h.Members)
                 .HasForeignKey(p => p.HouseholdId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.State)
                 .WithOne(s => s.Person)
                 .HasForeignKey<MembershipState>(s => s.PersonId)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.ParentRole)
                 .WithOne(r => r.Person)
                 .HasForeignKey<ParentRole>(r => r.PersonId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(p => p.BoardRoles)
                 .WithOne(br => br.Person)
                 .HasForeignKey(br => br.PersonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<MembershipState>(e =>
            {
                e.HasKey(s => s.PersonId);
                e.Property(s => s.State).IsRequired();

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MembershipState_Dates",
                        @"(State = 1 AND ActiveSince IS NOT NULL AND PassiveSince IS NULL)
                      OR (State = 2 AND PassiveSince IS NOT NULL AND ActiveSince IS NULL)");
                });
            });

            b.Entity<ParentRole>(e =>
            {
                e.HasKey(r => r.PersonId);
                e.Property(r => r.ActiveChildrenCount).IsRequired();
            });

            b.Entity<BoardRole>(e =>
            {
                e.Property(br => br.From).IsRequired();
                e.HasOne(br => br.Sport).WithMany().HasForeignKey(br => br.SportId).OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(br => new { br.PersonId, br.To })
                 .IsUnique()
                 .HasFilter("[To] IS NULL");
            });

            b.Entity<Sport>(e =>
            {
                e.HasIndex(s => s.Name).IsUnique();
                e.Property(s => s.Name).HasMaxLength(120).IsRequired();
                e.Property(s => s.AnnualFeeAdult).HasColumnType("decimal(10,2)");
                e.Property(s => s.AnnualFeeChild).HasColumnType("decimal(10,2)");
            });

            b.Entity<SportFeeHistory>(e =>
            {
                e.HasOne(h => h.Sport).WithMany(s => s.FeeHistory).HasForeignKey(h => h.SportId);
                e.Property(h => h.AnnualFeeAdult).HasColumnType("decimal(10,2)");
                e.Property(h => h.AnnualFeeChild).HasColumnType("decimal(10,2)");
                e.HasIndex(h => new { h.SportId, h.EffectiveFrom }).IsUnique();
            });

            b.Entity<PersonSport>(e =>
            {
                e.HasKey(ps => ps.Id);

                e.HasOne(ps => ps.Person).WithMany(p => p.Sports).HasForeignKey(ps => ps.PersonId);
                e.HasOne(ps => ps.Sport).WithMany(p => p.Members).HasForeignKey(ps => ps.SportId);

                e.HasIndex(ps => new { ps.PersonId, ps.SportId }).IsUnique().HasFilter("[Left] IS NULL");
            });

            b.Entity<MembershipHistory>(e =>
            {
                e.Property(h => h.State).IsRequired();
                e.Property(h => h.ChangedOn).IsRequired();

                e.HasOne(h => h.Person)
                .WithMany()
                .HasForeignKey(h => h.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(h => new { h.PersonId, h.ChangedOn });
            });

            b.Entity<ApplicationSetting>().HasData(new ApplicationSetting
            {
                Id = 1,
                PassiveAdultAnnualFee = 400m,
                PassiveChildAnnualFee = 200m
            });
        }
    }
}