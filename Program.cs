using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework Core with SQLite.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Stripe.
StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];

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

// Seed Identity roles and HR account.
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.Run();