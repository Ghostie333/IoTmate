using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, Microsoft.AspNetCore.Components.Server.ServerAuthenticationStateProvider>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
    {
        options.DetailedErrors = true;
    });
}

builder.Services.AddMudServices();

Directory.CreateDirectory("/app/data/dpkeys");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data/dpkeys"))
    .SetApplicationName("IoTmate");

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<IoTmate.Authentication.AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Auth") ?? "Data Source=auth.db"));

builder.Services.AddIdentity<IoTmate.Authentication.AppUser, IdentityRole>()
    .AddEntityFrameworkStores<IoTmate.Authentication.AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IoTmate.Authentication.ICurrentUser, IoTmate.Authentication.CurrentUser>();

builder.Services.Configure<IoTmate.Data.Mongo.MongoOptions>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IoTmate.Data.Mongo.IMongoDb, IoTmate.Data.Mongo.MongoDb>();

builder.Services.AddSingleton<IoTmate.Services.IDeviceStore, IoTmate.Services.MongoDeviceStore>();
builder.Services.AddSingleton<IoTmate.Services.ITelemetryBus, IoTmate.Services.InMemoryTelemetryBus>();
builder.Services.Configure<IoTmate.Infrastructure.Mqtt.MqttOptions>(builder.Configuration.GetSection("Mqtt"));
builder.Services.AddSingleton<IoTmate.Infrastructure.Mqtt.IMqttPublisher, IoTmate.Infrastructure.Mqtt.MqttPublisher>();
builder.Services.AddHostedService<IoTmate.BackgroundServices.MqttListenerService>();
builder.Services.AddHostedService<IoTmate.BackgroundServices.MqttTestPublisherService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    Directory.CreateDirectory("/app/data");
    var db = scope.ServiceProvider.GetRequiredService<IoTmate.Authentication.AuthDbContext>();
    db.Database.EnsureCreated();

    var mongo = scope.ServiceProvider.GetRequiredService<IoTmate.Data.Mongo.IMongoDb>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MongoIndexSetup");
    IoTmate.Data.Devices.DeviceCollectionSetup.EnsureIndexes(mongo, logger);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/antiforgery/token", (Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, HttpContext http) =>
{
    var tokens = antiforgery.GetAndStoreTokens(http);
    return Results.Ok(new { token = tokens.RequestToken });
});

app.MapPost("/logout", async ([Microsoft.AspNetCore.Mvc.FromServices] Microsoft.AspNetCore.Identity.SignInManager<IoTmate.Authentication.AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});

app.MapControllers();
app.MapRazorPages();

app.MapRazorComponents<IoTmate.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Auth forms are implemented using Razor Pages in `Pages/`.
