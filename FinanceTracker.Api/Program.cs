using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using FinanceTracker.Api.Infrastructure;
using FinanceTracker.Api.Application;
using FinanceTracker.Api.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddDbContext<FinanceTrackerContext>(options => 
    options.UseSqlite("Data Source=../FinanceTracker/finance.db"));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();

app.Run();