using Microsoft.ApplicationInsights.Extensibility;

using MX.GeoLocation.GeoLocationApi.Client;
using MX.GeoLocation.PublicWebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllersWithViews();
builder.Services.AddGeoLocationApiClient(options =>
{
    options.BaseUrl = builder.Configuration["apim_base_url"];
    options.ApiKey = builder.Configuration["apim_subscription_key"];
    options.ApiPathPrefix = builder.Configuration["apim_geolocation_path_prefix"] ?? "geolocation";
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
