using FitLog.Models;

namespace FitLog.Data;

/// <summary>
/// Seeds a fresh database with a starter exercise library, two routines, and a
/// couple weeks of logged sets so the dashboard and charts have something to show
/// on first run. Runs only when the Exercises table is empty.
/// </summary>
public static class DbSeeder
{
    public static void Seed(FitLogDbContext db)
    {
        if (db.Exercises.Any())
            return;

        var bench = new Exercise { Name = "Bench Press", MuscleGroup = MuscleGroup.Chest };
        var squat = new Exercise { Name = "Back Squat", MuscleGroup = MuscleGroup.Legs };
        var deadlift = new Exercise { Name = "Deadlift", MuscleGroup = MuscleGroup.Back };
        var ohp = new Exercise { Name = "Overhead Press", MuscleGroup = MuscleGroup.Shoulders };
        var row = new Exercise { Name = "Barbell Row", MuscleGroup = MuscleGroup.Back };
        var curl = new Exercise { Name = "Bicep Curl", MuscleGroup = MuscleGroup.Arms };
        var plank = new Exercise { Name = "Plank", MuscleGroup = MuscleGroup.Core };

        db.Exercises.AddRange(bench, squat, deadlift, ohp, row, curl, plank);
        db.SaveChanges();

        db.Routines.AddRange(
            new Routine
            {
                Name = "Push Day",
                Description = "Chest, shoulders, and triceps.",
                RoutineExercises = new()
                {
                    new RoutineExercise { ExerciseId = bench.Id, Order = 1, TargetSets = 4, TargetReps = 8 },
                    new RoutineExercise { ExerciseId = ohp.Id,   Order = 2, TargetSets = 3, TargetReps = 10 },
                }
            },
            new Routine
            {
                Name = "Pull Day",
                Description = "Back and biceps.",
                RoutineExercises = new()
                {
                    new RoutineExercise { ExerciseId = deadlift.Id, Order = 1, TargetSets = 3, TargetReps = 5 },
                    new RoutineExercise { ExerciseId = row.Id,      Order = 2, TargetSets = 4, TargetReps = 8 },
                    new RoutineExercise { ExerciseId = curl.Id,     Order = 3, TargetSets = 3, TargetReps = 12 },
                }
            });
        db.SaveChanges();

        // ~2 weeks of progressive-overload logs for two lifts.
        var start = DateTime.Today.AddDays(-14);
        for (var d = 0; d <= 14; d += 2)
        {
            var day = start.AddDays(d);
            db.LogEntries.Add(new LogEntry { ExerciseId = bench.Id, Date = day, Sets = 4, Reps = 8, Weight = 135 + d });
            db.LogEntries.Add(new LogEntry { ExerciseId = squat.Id, Date = day, Sets = 4, Reps = 6, Weight = 185 + d * 2 });
        }
        db.SaveChanges();
    }
}
