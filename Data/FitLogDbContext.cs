using FitLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitLog.Data;

/// <summary>
/// Application database context. Inherits from <see cref="IdentityDbContext{TUser}"/>
/// so that the ASP.NET Core Identity tables (users, roles, claims, logins) live
/// in the same SQLite database as the workout data.
/// </summary>
public class FitLogDbContext : IdentityDbContext<IdentityUser>
{
    public FitLogDbContext(DbContextOptions<FitLogDbContext> options) : base(options) { }

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineExercise> RoutineExercises => Set<RoutineExercise>();
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity needs its own model configuration applied first.
        base.OnModelCreating(modelBuilder);

        // Removing a routine cleans up its join rows automatically.
        modelBuilder.Entity<RoutineExercise>()
            .HasOne(re => re.Routine)
            .WithMany(r => r.RoutineExercises)
            .HasForeignKey(re => re.RoutineId)
            .OnDelete(DeleteBehavior.Cascade);

        // But deleting an exercise must NOT silently wipe routines/history that
        // reference it. Restrict forces the UI to archive instead.
        modelBuilder.Entity<RoutineExercise>()
            .HasOne(re => re.Exercise)
            .WithMany(e => e.RoutineExercises)
            .HasForeignKey(re => re.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LogEntry>()
            .HasOne(l => l.Exercise)
            .WithMany(e => e.LogEntries)
            .HasForeignKey(l => l.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // A log entry can outlive the routine it was done under.
        modelBuilder.Entity<LogEntry>()
            .HasOne(l => l.Routine)
            .WithMany()
            .HasForeignKey(l => l.RoutineId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
