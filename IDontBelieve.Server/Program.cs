using IDontBelieve.Core.Services;
using IDontBelieve.Infrastructure.Data;
using IDontBelieve.Infrastructure.Hubs;
using IDontBelieve.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var clientUrl = "http://localhost:5073";

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorOrigin",
        policy =>
        {
            policy.WithOrigins(clientUrl)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb"));
});

builder.Services.AddScoped<IGameRoomService, GameRoomService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazorOrigin");

app.MapHub<GameRoomHub>("/gameroomhub");

app.Run();