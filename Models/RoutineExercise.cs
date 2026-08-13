using System.ComponentModel.DataAnnotations;

namespace FitLog.Models;

/// <summary>
/// Join entity linking a <see cref="Routine"/> to an <see cref="Exercise"/>.
/// It carries payload of its own — the position in the routine and the target
/// sets/reps — which is why it's a full entity rather than a plain many-to-many.
/// </summary>
public class RoutineExercise
{
    public int Id { get; set; }

    public int RoutineId { get; set; }
    public Routine? Routine { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    /// <summary>1-based position of this exercise within its routine.</summary>
    public int Order { get; set; }

    [Range(1, 20), Display(Name = "Target sets")]
    public int TargetSets { get; set; } = 3;

    [Range(1, 100), Display(Name = "Target reps")]
    public int TargetReps { get; set; } = 10;
}
