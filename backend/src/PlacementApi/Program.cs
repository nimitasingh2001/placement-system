using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Infrastructure;
using PlacementApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<PlacementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PlacementDb")));
builder.Services.AddScoped<ICompanyService, CompanyService>();
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
app.UseAuthorization();
app.MapControllers();

app.Run();