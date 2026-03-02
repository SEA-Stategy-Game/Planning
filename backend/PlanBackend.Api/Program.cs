using Microsoft.EntityFrameworkCore;
using PlanBackend.Application.Interfaces;
using PlanBackend.Application.Services;
using PlanBackend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PlanDbContext>(options =>
    options.UseSqlite("Data Source=plans.db"));

builder.Services.AddHttpClient<CoreNotifier>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ICoreNotifier, CoreNotifier>();
builder.Services.AddScoped<PlanService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
