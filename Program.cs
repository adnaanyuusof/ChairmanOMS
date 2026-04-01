using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using ChairmanOMS.Models;

// Enable legacy timestamp behavior for PostgreSQL to avoid DateTime Unspecified errors
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Bind to PORT env var for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5151";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Database configuration: Use PostgreSQL on Render, or detect provider locally
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parse Render's DATABASE_URL (postgres://user:pass@host:port/dbname)
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var dbPort = uri.Port > 0 ? uri.Port : 5432;
    var npgsqlConn = $"Host={uri.Host};Port={dbPort};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(npgsqlConn));
}
else if (connectionString.Contains("Host="))
{
    // Use PostgreSQL locally if connected to Render's external URL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use SQL Server locally
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); // Required to serve runtime uploads
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed roles and admin user, and auto-migrate on Render
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Raw SQL for missing columns to be 100% sure - Run BEFORE Migrate
        var connection = context.Database.GetDbConnection();
        connection.Open();
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""Purpose"" text;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""ReceiverName"" text;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""UnderProcessBy"" text;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""HandedTo"" text;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""Other"" text;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""DepartureDate"" timestamp without time zone;
                ALTER TABLE ""IncomingDocuments"" ADD COLUMN IF NOT EXISTS ""DateOfReturn"" timestamp without time zone;

                ALTER TABLE ""OutgoingDocuments"" ADD COLUMN IF NOT EXISTS ""ConveyerName"" text;
                ALTER TABLE ""OutgoingDocuments"" ADD COLUMN IF NOT EXISTS ""PhoneNumber"" text;
                ALTER TABLE ""OutgoingDocuments"" ADD COLUMN IF NOT EXISTS ""ReceiverName"" text;
                ALTER TABLE ""OutgoingDocuments"" ADD COLUMN IF NOT EXISTS ""LinkedIncomingDocumentId"" integer;

                ALTER TABLE ""Appointments"" ADD COLUMN IF NOT EXISTS ""Masuulka"" text;
                ALTER TABLE ""Appointments"" ADD COLUMN IF NOT EXISTS ""VisitorStatus"" text;
                ALTER TABLE ""Appointments"" ADD COLUMN IF NOT EXISTS ""CheckInTime"" timestamp without time zone;
                ALTER TABLE ""Appointments"" ADD COLUMN IF NOT EXISTS ""CheckOutTime"" timestamp without time zone;
                ALTER TABLE ""Appointments"" ADD COLUMN IF NOT EXISTS ""CreatedById"" text;
            ";
            cmd.ExecuteNonQuery();
        }

        // Auto-apply migrations
        context.Database.Migrate(); 

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await DbInitializer.InitializeAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during DB migration/seeding.");
    }
}

app.Run();
