using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Lab1.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("uk")
};

// Налаштування Serilog для запису логів у папку Logs
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

// Логування всіх HTTP-запитів
app.Use(async (context, next) =>
{
    var requestTime = DateTime.Now;

    var fullUrl =
        $"{context.Request.Scheme}://" +
        $"{context.Request.Host}" +
        $"{context.Request.PathBase}" +
        $"{context.Request.Path}" +
        $"{context.Request.QueryString}";

    var ipAddress =
        context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    Log.Information(
        "{Time} | URL: {Url} | IP: {IP}",
        requestTime,
        fullUrl,
        ipAddress);

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Дозволяємо віддавати файли з wwwroot,
// включно з файлами, завантаженими під час роботи програми.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}