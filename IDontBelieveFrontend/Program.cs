using IDontBelieve.Infrastructure.Data;
using IDontBelieve.Core.Services;
using IDontBelieve.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameRoomService, GameRoomService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    if (builder.Environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully");
        
        await SeedDataAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
{
    try
    {
        if (!await context.Users.AnyAsync())
        {
            var testUser = new IDontBelieve.Core.Models.User
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Rating = 1000,
                Coins = 1000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Created test user: {Username}", testUser.UserName);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}