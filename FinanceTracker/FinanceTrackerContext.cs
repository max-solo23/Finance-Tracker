using Microsoft.EntityFrameworkCore;

public class FinanceTrackerContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    public FinanceTrackerContext()
    {
    }

    public FinanceTrackerContext(DbContextOptions<FinanceTrackerContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=finance.db");
        }        
    }
}