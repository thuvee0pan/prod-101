using System.Text;
using ExecutionOS.API.Data;
using ExecutionOS.API.Middleware;
using ExecutionOS.API.Services;
using ExecutionOS.API.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExecutionOS API",
        Version = "v1",
        Description = "Personal Execution OS — Track goals, projects, daily logs, streaks, and weekly reviews.\n\n" +
                      "**Authentication:** Sign in with Google to obtain a JWT token, then click \"Authorize\" and enter: `Bearer <your-token>`"
    });

    // JWT Bearer auth in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token obtained from the Google OAuth login.\n\n" +
                      "**How to get a token:**\n" +
                      "1. Sign in via the frontend or call `POST /api/auth/google` with your Google ID token\n" +
                      "2. Copy the `token` from the response\n" +
                      "3. Paste it here (no need to add 'Bearer ' prefix)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSecret = builder.Configuration["Auth:JwtSecret"] ?? "dev-secret-key-change-in-production-min-32-chars!!";
var jwtIssuer = builder.Configuration["Auth:JwtIssuer"] ?? "ExecutionOS";
var jwtAudience = builder.Configuration["Auth:JwtAudience"] ?? "ExecutionOS";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GoalService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<DailyLogService>();
builder.Services.AddScoped<StreakService>();
builder.Services.AddScoped<WeeklyReviewService>();
builder.Services.AddScoped<WarningService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<TodoService>();
builder.Services.AddHostedService<InactivityDetectionJob>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowCrossUrl"])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ExecutionOS API v1");
    options.DocumentTitle = "ExecutionOS API — Swagger";
});

app.UseCors("AllowFrontend");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
