using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Resolve SQLite connection string safely to ContentRootPath for IIS / MonsterASP.NET hosting.
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=RecruitmentAndCandidateScreeningSystem.db";

var sqliteBuilder = new SqliteConnectionStringBuilder(rawConnectionString);
if (!string.IsNullOrWhiteSpace(sqliteBuilder.DataSource) && !Path.IsPathRooted(sqliteBuilder.DataSource))
{
    sqliteBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource);
}
if (sqliteBuilder.DefaultTimeout < 30)
{
    sqliteBuilder.DefaultTimeout = 30;
}
string resolvedConnectionString = sqliteBuilder.ToString();

// Ensure the folder containing the SQLite database exists
if (!string.IsNullOrWhiteSpace(sqliteBuilder.DataSource))
{
    var dbDirectory = Path.GetDirectoryName(sqliteBuilder.DataSource);
    if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
    {
        Directory.CreateDirectory(dbDirectory);
    }
}

// Add Entity Framework Core with SQLite.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(resolvedConnectionString));

// Configure Stripe (safe fallback when key is absent in demo mode).
var stripeKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeKey))
{
    StripeConfiguration.ApiKey = stripeKey;
}

// Configure Forwarded Headers for MonsterASP.NET / IIS reverse proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add Stripe payment service.
builder.Services.AddScoped<StripePaymentService>();
// Add Gemini 
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<AIMatchingService>();
builder.Services.AddScoped<CVTextExtractionService>();
// Add ASP.NET Core Identity.
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC services.
builder.Services.AddControllersWithViews();

// Add Razor Pages required by ASP.NET Core Identity UI.
builder.Services.AddRazorPages();

var app = builder.Build();

// Ensure secure upload folders exist.
var secureUploadsRoot = Path.Combine(app.Environment.ContentRootPath, "SecureUploads");
Directory.CreateDirectory(Path.Combine(secureUploadsRoot, "Photos"));
Directory.CreateDirectory(Path.Combine(secureUploadsRoot, "Signatures"));
Directory.CreateDirectory(Path.Combine(secureUploadsRoot, "CVs"));

// Use forwarded headers before HTTPS redirection for reverse proxies.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map ASP.NET Core Identity Razor Pages.
app.MapRazorPages();

// Initialize database schema via migrations and seed Identity roles and accounts.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Verifying and applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations verified and applied.");

        logger.LogInformation("Seeding Identity roles and default accounts...");
        await IdentitySeeder.SeedRolesAsync(services);
        logger.LogInformation("Identity roles and accounts seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "A fatal error occurred during database migration or seeding at application startup.");
        throw;
    }
}

app.Run();