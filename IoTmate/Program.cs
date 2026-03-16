using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data/dpkeys"))
    .SetApplicationName("IoTmate");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

    // W Blazor Server HttpClient działa na serwerze, ale żeby przeglądarka dostała cookie
    // dla poprawnego origin, requesty muszą iść pod ten sam host co UI (Host header).
    // W Dockerze aplikacja słucha na 8080, ale Host z przeglądarki to np. localhost:7070.
    var host = httpContext?.Request.Headers.Host.ToString();
    var scheme = httpContext?.Request.Headers["X-Forwarded-Proto"].ToString();
    if (string.IsNullOrWhiteSpace(scheme))
    {
        scheme = httpContext?.Request.Scheme;
    }

    if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(scheme))
    {
        return new HttpClient { BaseAddress = new Uri($"{scheme}://{host}/") };
    }

    // fallback
    return new HttpClient { BaseAddress = new Uri("http://localhost:7070/") };
});

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    Directory.CreateDirectory("/app/data");
    var db = scope.ServiceProvider.GetRequiredService<IoTmate.Authentication.AuthDbContext>();
    db.Database.EnsureCreated();
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

app.MapPost("/login", async (
    [Microsoft.AspNetCore.Mvc.FromServices] Microsoft.AspNetCore.Identity.SignInManager<IoTmate.Authentication.AppUser> signInManager,
    [Microsoft.AspNetCore.Mvc.FromServices] Microsoft.AspNetCore.Identity.UserManager<IoTmate.Authentication.AppUser> userManager,
    [Microsoft.AspNetCore.Mvc.FromForm] LoginForm request) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
        return Results.Redirect("/login");
    }

    var rememberMe = string.Equals(request.RememberMe, "on", StringComparison.OrdinalIgnoreCase)
        || string.Equals(request.RememberMe, "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(request.RememberMe, "1", StringComparison.OrdinalIgnoreCase);

    var result = await signInManager.PasswordSignInAsync(user, request.Password, rememberMe, lockoutOnFailure: false);
    return result.Succeeded ? Results.Redirect("/devices") : Results.Redirect("/login");
}).DisableAntiforgery();

app.MapPost("/logout", async ([Microsoft.AspNetCore.Mvc.FromServices] Microsoft.AspNetCore.Identity.SignInManager<IoTmate.Authentication.AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/register", async (
    [Microsoft.AspNetCore.Mvc.FromServices] Microsoft.AspNetCore.Identity.UserManager<IoTmate.Authentication.AppUser> userManager,
    [Microsoft.AspNetCore.Mvc.FromForm] RegisterForm request) =>
{
    if (request.Password != request.ConfirmPassword)
    {
        return Results.Redirect("/register");
    }

    var user = new IoTmate.Authentication.AppUser
    {
        UserName = request.Email,
        Email = request.Email
    };

    var result = await userManager.CreateAsync(user, request.Password);
    return result.Succeeded ? Results.Redirect("/login") : Results.Redirect("/register");
}).DisableAntiforgery();

app.MapControllers();

app.MapRazorComponents<IoTmate.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

sealed record LoginRequest(string Email, string Password, bool RememberMe);
sealed record RegisterRequest(string Email, string Password, string ConfirmPassword);

sealed class LoginForm
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RememberMe { get; set; }
}

sealed class RegisterForm
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
