using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddDbContext<FinanceTrackerContext>(options => 
    options.UseSqlite("Data Source=../FinanceTracker/finance.db"));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();

app.Run();