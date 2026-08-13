using FitLog.Components;
using FitLog.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render (and most container hosts) terminate TLS at a reverse proxy and forward
// plain HTTP to the app. Without honouring the X-Forwarded-* headers the app sees
// scheme "http", refuses to issue the Secure auth cookie, and redirect-loops on
// sign-in. KnownNetworks/KnownProxies are cleared because the proxy's address is
// assigned dynamically by the platform.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Razor Components with interactive server-side rendering.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Makes the signed-in user's AuthenticationState available to every component
// via <CascadingAuthenticationState>, which <AuthorizeView> and [Authorize] read.
builder.Services.AddCascadingAuthenticationState();

// EF Core (SQLite). A DbContext *factory* is the recommended pattern for Blazor
// Server: components create a short-lived context per operation, which avoids
// the "second operation started on this context" threading errors you get when
// a single scoped context is shared across overlapping UI events.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=fitlog.db";

builder.Services.AddDbContextFactory<FitLogDbContext>(options =>
    options.UseSqlite(connectionString));

// ASP.NET Core Identity's EF stores need a *scoped* FitLogDbContext, but the app
// registers a factory. Resolving one from the factory keeps a single options
// registration instead of configuring the context twice with different lifetimes.
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<FitLogDbContext>>().CreateDbContext());

// --- Authentication and authorization ------------------------------------
// Cookie-based Identity. AddIdentityCore plus AddSignInManager gives the pieces
// needed for username/password sign-in without the MVC/Razor Pages UI, since the
// login and registration screens in this app are Blazor components.
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        // No email server in this project, so accounts are usable immediately.
        options.SignIn.RequireConfirmedAccount = false;

        // Relaxed for a course project. Production would keep stricter defaults.
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<FitLogDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Must run before any middleware that inspects the request scheme or host.
app.UseForwardedHeaders();

// Create the database (if needed) and seed sample data on startup.
// NOTE: EnsureCreated() gets the project running with zero setup. To switch to
// EF migrations, see the README ("Switching to migrations") - delete fitlog.db,
// add an InitialCreate migration, and swap the call below to db.Database.Migrate().
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FitLogDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();
        DbSeeder.Seed(db);
        logger.LogInformation("Database ready and seed check complete.");
    }
    catch (Exception ex)
    {
        // Log and continue rather than crashing at startup. The pages surface a
        // readable error if the database really is unavailable.
        logger.LogError(ex, "Database initialization failed. The app will start, but data pages will show an error.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// No UseHttpsRedirection or UseHsts here: the hosting proxy already terminates
// TLS and serves the public site over HTTPS. Redirecting again inside the
// container loops, because the forwarded request always arrives as plain HTTP.

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Sign-out has to clear the auth cookie on a real HTTP response, which a
// SignalR-driven component cannot do, so it is a plain POST endpoint.
app.MapPost("/Account/Logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/Account/Login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();