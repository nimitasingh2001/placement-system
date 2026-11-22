using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlacementApi.Auth;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Infrastructure;
using PlacementApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// fail fast, with a helpful message, if the signing key is missing
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. Set it with: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<long random text>\" --project src/PlacementApi");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddControllers();
builder.Services.AddDbContext<PlacementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PlacementDb")));

builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStudentPortalService, StudentPortalService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // keep the claim names exactly as we wrote them: sub, email, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = "role"          // tells [Authorize(Roles = ...)] which claim holds the role
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();   // 1st: read the token and work out who the caller is
app.UseAuthorization();    // 2nd: decide whether that caller may do this
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();