using Microsoft.EntityFrameworkCore;
using PlanBackend.Application.Interfaces;
using PlanBackend.Application.Services;
using PlanBackend.Application.Validation;
using PlanBackend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PlanDbContext>(options =>
    options.UseSqlite("Data Source=plans.db"));

builder.Services.AddHttpClient<ICoreNotifier, CoreNotifier>(client =>
    client.BaseAddress = new Uri(builder.Configuration["CoreBaseUrl"] ?? "http://localhost:8085"));

builder.Services.AddHttpClient<CoreSenseClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["CoreBaseUrl"] ?? "http://localhost:8085"));
builder.Services.AddScoped<ISenseQueryClient>(sp => sp.GetRequiredService<CoreSenseClient>());

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<PlanValidator>();
builder.Services.AddScoped<PlanService>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PlanDbContext>().Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
