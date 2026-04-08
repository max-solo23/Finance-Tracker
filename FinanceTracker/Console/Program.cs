using Microsoft.EntityFrameworkCore;

try
{
    using var context = new FinanceTrackerContext();

    context.Database.Migrate();

    var consoleUser = await context.Users.FirstOrDefaultAsync();
    if (consoleUser == null)
    {
        consoleUser = new FinanceTracker.Models.User
        {
            Email = "console@user",
            PasswordHash = "console-no-auth",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(consoleUser);
        await context.SaveChangesAsync();
    }

    IAccount? wallet = context.Accounts
        .Include(a => a.Transactions)
        .FirstOrDefault();

    if (wallet == null)
    {
        Account newAccount = new Account("Cash Wallet") { UserId = consoleUser.Id };
        wallet = newAccount;
        context.Accounts.Add(newAccount);
        await context.SaveChangesAsync();
    }

    var bankAccount = await context.Accounts
        .FirstOrDefaultAsync(a => a.Name == "Bank Account");
    if (bankAccount == null)
    {
        bankAccount = new Account("Bank Account") { UserId = consoleUser.Id };
        context.Accounts.Add(bankAccount);
        await context.SaveChangesAsync();
        Logger.Info("Created Bank Account for testing");
    }

    wallet.LowBalanceAlert += OnLowBalance;

    while (true)
    {
        System.Console.WriteLine("\n--- Finance Tracker ---");
        System.Console.WriteLine("1. Add transaction");
        System.Console.WriteLine("2. View all transactions");
        System.Console.WriteLine("3. Save to database");
        System.Console.WriteLine("4. View recent (last 7 days)");
        System.Console.WriteLine("5. Transfer between accounts");
        System.Console.WriteLine("6. Quit");
        System.Console.WriteLine("Choose option: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                System.Console.WriteLine("Amount: ");
                string? amountInput = System.Console.ReadLine();

                System.Console.WriteLine("Description: ");
                string? description = System.Console.ReadLine();

                if (decimal.TryParse(amountInput, out decimal amount))
                {
                    Transaction transaction;

                    if (amount > 0)
                    {
                        System.Console.WriteLine("Source (e.g., Salary, Freelance): ");
                        string? source = Console.ReadLine() ?? "";
                        transaction = new Income(
                            amount, description ?? "", DateTime.Now, source);
                    }
                    else
                    {
                        System.Console.WriteLine("Category (e.g., Food, Rent): ");
                        string? category = Console.ReadLine() ?? "";
                        transaction = new Expense(
                            amount, description ?? "", DateTime.Now, category);
                    }
                    try
                    {
                        wallet.AddTransaction(transaction);
                        await context.SaveChangesAsync();
                        Logger.Info($"Transaction added: {amount:C} - {description}");
                        wallet.CheckLowBalance(50);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error($"Failed to save transaction: {ex.Message}");
                    }

                }
                else
                {
                    System.Console.WriteLine("Invalid amount. Please enter a number.");
                }
                break;
            case "2":
                if (wallet.Transactions.Count == 0)
                {
                    System.Console.WriteLine("No transactions yet.");
                }
                else
                {
                    System.Console.WriteLine($"\n--- {wallet.Name} ---");
                    foreach (Transaction t in wallet.Transactions)
                    {
                        System.Console.WriteLine($"{t.Date:d}: {t.Description} = {t.Amount:C}");
                    }
                    System.Console.WriteLine($"Balance: {wallet.GetBalance():C}");
                    if (wallet.Transactions.Count > 0)
                    {
                        System.Console.WriteLine($"First transaction: {wallet[0].Description}");
                    }
                    System.Console.WriteLine($"Total expenses: {wallet.Transactions.GetTotalExpenses():C}");
                }
                break;
            case "3":
                await context.SaveChangesAsync();
                Logger.Info("Saved to database!");
                break;
            case "4":
                var recent = wallet.Transactions.GetLast7Days();
                if (recent.Count == 0)
                {
                    System.Console.WriteLine("No transactions in the last 7 days.");
                }
                else
                {
                    System.Console.WriteLine($"\n--- Last 7 Days ({recent.Count} transactions) ---");
                    foreach (Transaction t in recent)
                    {
                        System.Console.WriteLine($"{t.Date:d}: {t.Description} = {t.Amount:C}");
                    }
                }
                break;
            case "5":
                System.Console.WriteLine("Transfer from: ");
                string? fromName = Console.ReadLine();

                System.Console.WriteLine("Transfer to: ");
                string? toName = Console.ReadLine();

                System.Console.WriteLine("Amount: ");
                string? transferAmount = Console.ReadLine();

                if (decimal.TryParse(transferAmount, out decimal transferAmt))
                {
                    try
                    {
                        await TransferMoney(context, fromName ?? "", toName ?? "", transferAmt);
                        System.Console.WriteLine("Transfer complete!");
                        Logger.Info($"Transferred from {fromName} to {toName} amount: {transferAmt}");
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine($"Transfer failed: {ex.Message}");
                    }
                    
                }
                else
                {
                    System.Console.WriteLine("Invalid amount.");
                }
                break;
            case "6":
                System.Console.WriteLine("Goodbye!");
                return;
            default:
                System.Console.WriteLine("Invalid choice. Please enter 1-6.");
                break;
        }
    }
}
catch (Exception ex)
{
    Logger.Critical($"Database connection failed: {ex.Message}");
    System.Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}


static void OnLowBalance(object? sender, EventArgs e)
{
    Logger.Warning("Low balance detected!");
}

static async Task TransferMoney(
    FinanceTrackerContext context,
    string fromAccountName,
    string toAccountName,
    decimal amount)
{
    using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        var fromAccount = await context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Name == fromAccountName);

        var toAccount = await context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Name == toAccountName);

        if (fromAccount == null || toAccount == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        var withdrawal = new Expense(
            -amount,
            $"Transfer to {toAccountName}",
            DateTime.Now,
            "Transfer"
        );
        fromAccount.AddTransaction(withdrawal);
        await context.SaveChangesAsync();

        var deposit = new Income(
            amount,
            $"Transfer from {fromAccountName}",
            DateTime.Now,
            "Transfer"
        );
        toAccount.AddTransaction(deposit);
        await context.SaveChangesAsync();
        
        await transaction.CommitAsync();
        Logger.Info($"Transfer successful: {amount:C} from {fromAccountName} to {toAccountName}");
    }
    catch (System.Exception ex)
    {
        await transaction.RollbackAsync();
        Logger.Error($"Transfer failed: {ex.Message}");
        throw;
    }
}
