# FitLog — Workout & Habit Tracker

CSE 325 group project (single-member group, instructor approved). A Blazor web
app for building routines, logging workouts, and tracking progress (streaks,
personal records, and training volume over time).

## Tech stack

- **.NET 8 / Blazor Web App** (interactive server rendering)
- **Entity Framework Core 8** with **SQLite**
- No external UI/chart libraries — the dashboard chart is plain CSS, so `dotnet run` is all anyone needs.

## Running it

Requires the **.NET 8 SDK**.

```
cd FitLog
dotnet run
```

Then open the URL it prints (e.g. `http://localhost:5090`). On first launch the
app creates `fitlog.db` and seeds a starter exercise library, two routines, and
~2 weeks of sample logs so the dashboard isn't empty.

To start over, stop the app and delete `fitlog.db`.

## Project structure

```
FitLog/
  Program.cs              app startup, DI, EF Core registration, seed-on-start
  Models/                 Exercise, Routine, RoutineExercise, LogEntry, MuscleGroup
  Data/
    FitLogDbContext.cs    DbSets + relationships / delete behavior
    DbSeeder.cs           first-run sample data
  Components/
    App.razor             root document + render mode
    Routes.razor          router
    Layout/               MainLayout, NavMenu
    Pages/
      Home.razor          dashboard: stats, PRs, 14-day volume chart
      LogWorkout.razor    log a set
      Exercises.razor     exercise library CRUD
      Routines.razor      create routines, add/remove exercises
      History.razor       past sets grouped by day
  wwwroot/app.css         styles
```

## Data model

Four entities, matching the Week 02 proposal:

- **Exercise** — the library (name, muscle group, notes). Archived rather than
  deleted once it has history.
- **Routine** — a named group of exercises.
- **RoutineExercise** — explicit join between Routine and Exercise, carrying
  display order plus target sets/reps. Modeled explicitly rather than as an
  implicit EF Core join table because the relationship itself carries data.
- **LogEntry** — one logged set (exercise, date, sets, reps, weight). Powers the
  dashboard.

## Switching to EF migrations

The app currently uses `EnsureCreated()` so it runs with zero setup. Converting
to migrations is a tracked task on the project board:

1. Install the tool once: `dotnet tool install --global dotnet-ef`
2. Delete `fitlog.db` if it exists.
3. `dotnet ef migrations add InitialCreate`
4. In `Program.cs`, change `db.Database.EnsureCreated();` to
   `db.Database.Migrate();`

## Work breakdown

The project is organized into four areas, each tracked on the Azure DevOps
board and each isolated to its own set of files so work can proceed area by
area without merge conflicts:

- **Data / EF** — models, `DbContext`, relationships, seeding, migrations
- **Exercises + Routines** — library CRUD and the routine builder
- **Logging + History** — the log form and the grouped history view
- **Dashboard** — summary stats, personal records, and the volume chart

## Authentication

The app uses **ASP.NET Core Identity** with cookie authentication and an EF Core
user store, so the Identity tables live in the same SQLite database as the
workout data.

- Visit any page while signed out and you are redirected to `/Account/Login`,
  with the address you wanted preserved so you land there after signing in.
- Create an account at `/Account/Register`. No email confirmation is required.
- Sign out from the button at the bottom of the sidebar.

Workout data is shared across accounts rather than partitioned per user — the
requirement here is authenticated access, and keeping one shared library keeps
the seeded demo data visible to any account.

**Implementation note:** the login and registration pages render statically (no
`@rendermode`), while the five data pages opt into `InteractiveServer`
individually. This matters: signing in writes an authentication cookie, which
requires a real HTTP response. A component running over a SignalR circuit has
already sent its headers and cannot set one. Sign-out posts to a minimal API
endpoint for the same reason.

## Using the app

**Dashboard** (`/`) — Your training at a glance: total sessions, current day
streak, sessions this week, and 7-day volume. Below that, a 14-day volume chart
(rest days show as empty bars), volume by muscle group over the last 30 days,
and your personal record for every exercise.

**Log Workout** (`/log`) — Record one working set: pick an exercise, set the
date, and enter sets, reps, and weight. Optionally tag the routine you were
following and add a note. Live volume for the set is shown next to the save
button, and each save appears in a "Just logged" list so you can log several
sets in a row without losing your place.

**Account** (`/Account/Login`, `/Account/Register`) — Create an account or sign
in. All other pages require a signed-in user.

**Exercises** (`/exercises`) — Your exercise library. Add, edit, search, and
filter by muscle group. Deleting an exercise that already has logged history
archives it instead, so past workouts keep their data. Archived exercises are
hidden from the logging dropdown but stay visible here.

**Routines** (`/routines`) — Build named routines like "Push Day". Select a
routine, then add exercises with target sets and reps. Exercises are ordered,
and can be removed without affecting logged history.

**History** (`/history`) — Every logged set, grouped by day, newest first.
Filter by exercise, muscle group, and date range; a summary line shows the set
count and total volume for whatever is currently filtered. Sets can be deleted
individually.

## Error handling

Every database operation is wrapped in exception handling that surfaces a
readable message rather than an unhandled exception:

- **Page level** — each page catches load and save failures and displays a
  dismissible `StatusBanner` (an ARIA live region, so screen readers announce
  it). Success and error states are styled distinctly.
- **Validation** — data annotations on the models drive both `DataAnnotationsValidator`
  in the log form and explicit pre-save checks (duplicate exercise names, future
  dates, duplicate exercises within a routine, missing selections).
- **Concurrency** — deletes re-fetch the record first and report cleanly if
  another action already removed it, instead of throwing.
- **Referential integrity** — `DbUpdateException` is caught where a restricted
  foreign key could block a delete, and explained in plain language.
- **App level** — an `ErrorBoundary` in `MainLayout` catches anything that slips
  through and offers a "Try again" recovery button.
- **Startup** — database initialization failures are logged and do not crash the
  app; the affected pages show an error instead.

## Accessibility

Built against WCAG 2.1 Level AA:

- Skip-to-content link as the first focusable element
- Semantic landmarks (`nav`, `main`, `header`, `section`) with a labelled nav
- Visible `:focus-visible` outlines on all interactive elements
- Status messages as ARIA live regions with appropriate priority
- Decorative emoji marked `aria-hidden`; the chart carries a text alternative
- `prefers-reduced-motion` respected
- Colour is never the only signal — banners pair colour with an icon and text

## Project status

**Week 05 checkpoint.** Feature-complete for the checkpoint milestone:

- Four-entity data model with EF Core relationships and delete behavior
- First-run seeding so a fresh clone has data to display
- Exercise library CRUD, routine builder, workout logging, history
- Dashboard: session stats, current streak, personal records, 14-day volume
  chart, and volume by muscle group over the last 30 days
- Form validation via data annotations and `DataAnnotationsValidator`
- History filtering by exercise, muscle group, and date range
- Error handling on every data operation with user-facing feedback
- Accessibility pass against WCAG 2.1 AA
- ASP.NET Core Identity authentication protecting all data pages
- Responsive layout for phones and tablets

Remaining: convert `EnsureCreated()` to EF Core migrations (see above), and
deploy to a cloud host.
