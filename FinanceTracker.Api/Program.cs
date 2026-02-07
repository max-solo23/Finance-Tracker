using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<FinanceTrackerContext>(options => 
    options.UseSqlite("Data Source=../FinanceTracker/finance.db"));

var app = builder.Build();

app.MapControllers();

app.Run();