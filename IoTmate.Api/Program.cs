using IoTmate.Api.Exceptions;
using IoTmate.Model;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Repository.Rooms;
using IoTmate.Repository.Rules;
using IoTmate.Services.Auth;
using IoTmate.Services.Devices;
using IoTmate.Services.Devices.StateHandlers;
using IoTmate.Services.Hubs;
using IoTmate.Services.Mqtt;
using IoTmate.Services.Rooms;
using IoTmate.Services.Rules;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

// Repositories & Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

// MQTT Services registration
builder.Services.AddSingleton<MqttBrokerListenerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttBrokerListenerService>());
builder.Services.AddScoped<IMqttMessageRouterService, MqttMessageRouterService>();

// Device services
builder.Services.AddSingleton<IDeviceStateService, DeviceStateService>();
builder.Services.AddHostedService<DevicePresenceSweepService>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

// Device state handlers
builder.Services.AddKeyedScoped<IDeviceStateHandler, PowerStripStateHandler>(DeviceType.PowerStrip);
builder.Services.AddKeyedScoped<IDeviceStateHandler, SensorStateHandler>(DeviceType.Sensor);

// Room services
builder.Services.AddScoped<IRoomService, RoomService>();

// SignalR hubs
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Rules
builder.Services.AddScoped<IRuleRepository, RuleRepository>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IRuleEngineService, RuleEngineService>();
builder.Services.AddHostedService<TimeRuleScheduler>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Global exception handling (maps domain exceptions to HTTP status codes)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<DeviceHub>("/hubs/devices");
app.MapControllers();

// Auto migration on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error while migrating database");
    }
}

app.Run();