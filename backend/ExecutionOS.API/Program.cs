using ExecutionOS.API.Data;
using ExecutionOS.API.Services;
using ExecutionOS.API.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<GoalService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<DailyLogService>();
builder.Services.AddScoped<StreakService>();
builder.Services.AddScoped<WeeklyReviewService>();
builder.Services.AddScoped<WarningService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddHostedService<InactivityDetectionJob>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
