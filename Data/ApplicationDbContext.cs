using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Models;

namespace RecruitmentAndCandidateScreeningSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<CV> CVs => Set<CV>();
    public DbSet<JobCircular> JobCirculars => Set<JobCircular>();
    public DbSet<JobRequirement> JobRequirements => Set<JobRequirement>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RecruitmentSource> RecruitmentSources => Set<RecruitmentSource>();
    public DbSet<AIMatchingResult> AIMatchingResults => Set<AIMatchingResult>();
    public DbSet<InterviewRanking> InterviewRankings => Set<InterviewRanking>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<JoiningSchedule> JoiningSchedules => Set<JoiningSchedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.CandidateProfile)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<CandidateProfile>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CandidateProfile>()
            .HasIndex(p => p.ApplicationUserId)
            .IsUnique();

        builder.Entity<JobCircular>()
            .HasOne(j => j.JobRequirement)
            .WithOne(r => r.JobCircular)
            .HasForeignKey<JobRequirement>(r => r.JobCircularId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobRequirement>()
            .HasIndex(r => r.JobCircularId)
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasIndex(a => a.ApplicationReferenceId)
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Payment)
            .WithOne(p => p.JobApplication)
            .HasForeignKey<Payment>(p => p.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .HasIndex(p => p.JobApplicationId)
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasOne(a => a.AIMatchingResult)
            .WithOne(r => r.JobApplication)
            .HasForeignKey<AIMatchingResult>(r => r.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AIMatchingResult>()
            .HasIndex(r => r.JobApplicationId)
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasOne(a => a.InterviewRanking)
            .WithOne(r => r.JobApplication)
            .HasForeignKey<InterviewRanking>(r => r.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InterviewRanking>()
            .HasIndex(r => r.JobApplicationId)
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Offer)
            .WithOne(o => o.JobApplication)
            .HasForeignKey<Offer>(o => o.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Offer>()
            .HasIndex(o => o.JobApplicationId)
            .IsUnique();

        builder.Entity<Offer>()
            .HasOne(o => o.JoiningSchedule)
            .WithOne(j => j.Offer)
            .HasForeignKey<JoiningSchedule>(j => j.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JoiningSchedule>()
            .HasIndex(j => j.OfferId)
            .IsUnique();

        builder.Entity<CandidateProfile>()
            .HasMany(p => p.CVs)
            .WithOne(c => c.CandidateProfile)
            .HasForeignKey(c => c.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobCircular>()
            .Property(j => j.ApplicationFee)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Entity<AIMatchingResult>()
            .Property(r => r.MatchingScore)
            .HasPrecision(5, 2);

        builder.Entity<InterviewRanking>()
            .Property(r => r.InterviewScore)
            .HasPrecision(5, 2);
    }
}