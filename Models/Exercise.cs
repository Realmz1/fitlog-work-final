using System.ComponentModel.DataAnnotations;

namespace FitLog.Models;

/// <summary>
/// A single exercise in the user's library (e.g. "Bench Press"). Exercises are
/// referenced by routines and by logged sets, so they are archived rather than
/// hard-deleted once they have history.
/// </summary>
public class Exercise
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Muscle group")]
    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.FullBody;

    [StringLength(300)]
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }

    // Navigation properties
    public List<RoutineExercise> RoutineExercises { get; set; } = new();
    public List<LogEntry> LogEntries { get; set; } = new();
}
