using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application;
using HelpDeskBiDemo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "HelpDeskBiDemo.Session";
    options.IdleTimeout = TimeSpan.FromHours(6);
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDemoDataInitializer>();
        await initializer.InitializeAsync();
    }
    catch (Exception exception)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");

        logger.LogWarning(exception, "Database initialization skipped or failed during startup.");
    }
}

app.Run();
