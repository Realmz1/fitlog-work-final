using System.ComponentModel.DataAnnotations;

namespace FitLog.Models;

/// <summary>
/// A named, ordered collection of exercises the user performs together
/// (e.g. "Push Day"). The ordered list lives in <see cref="RoutineExercise"/>.
/// </summary>
public class Routine
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Display(Name = "Created")]
    public DateTime CreatedDate { get; set; } = DateTime.Today;

    // Navigation
    public List<RoutineExercise> RoutineExercises { get; set; } = new();
}
