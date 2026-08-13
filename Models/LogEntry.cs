using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitLog.Models;

/// <summary>
/// One logged working set for an exercise on a given day. This is the record
/// that powers the dashboard: streaks, personal records, and volume over time.
/// </summary>
public class LogEntry
{
    public int Id { get; set; }

    [Required, Display(Name = "Exercise")]
    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    /// <summary>Optional: the routine this set was performed under.</summary>
    [Display(Name = "Routine")]
    public int? RoutineId { get; set; }
    public Routine? Routine { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Range(1, 100)]
    public int Sets { get; set; } = 3;

    [Range(1, 500)]
    public int Reps { get; set; } = 10;

    [Range(0, 2000)]
    public double Weight { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    /// <summary>Training volume for this entry (sets x reps x weight). Not stored.</summary>
    [NotMapped]
    public double Volume => Sets * Reps * Weight;
}
