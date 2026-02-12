using System.Text.Json;
using System.Text.Json.Serialization;

public class Account : IAccount
{
    public string Name { get; set; } = string.Empty;
    [JsonInclude]
    public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public event EventHandler? LowBalanceAlert;
    public int Id { get; set; }
    
    public Account(string name)
    {
        Name = name;
    }

    public void AddTransaction(decimal amount, string description, DateTime dateTime)
    {
        Transactions.Add(new Transaction(amount, description, dateTime));
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }

    public decimal GetBalance()
    {
        return Transactions.Sum(t => t.Amount);
    }

    public List<Transaction> GetExpenses()
    {
        return Transactions.Where(t => t.Amount < 0).ToList();
    }

    public List<Transaction> GetIncome()
    {
        return Transactions.Where(t => t.Amount > 0).ToList();
    }

    public int GetTransactionCount()
    {
        return Transactions.Count();
    }

    public List<Transaction> GetTransactionsSortedByAmount()
    {
        return Transactions.OrderByDescending(t => t.Amount).ToList();
    }

    public Transaction? GetMostRecentTransaction()
    {
        return Transactions.OrderByDescending(t => t.Date).FirstOrDefault();
    }

    public bool HasExpenses()
    {
        return Transactions.Any(t => t.Amount < 0);
    }

    public List<String> GetAllDescriptions()
    {
        return Transactions.Select(t => t.Description).ToList();
    }

    public bool SaveToFile(string filePath)
    {
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, jsonString);
            return true;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
        
    }

    public static Account? LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            Account? account = JsonSerializer.Deserialize<Account>(json);
            return account;
        }
        catch (System.Exception ex)
        {            
            System.Console.WriteLine($"Error reading file: {ex.Message}");
            return null;
        }

    }

    public void CheckLowBalance(decimal threshold)
    {
        if (GetBalance() < threshold)
        {
            LowBalanceAlert?.Invoke(this, EventArgs.Empty);
        }
    }

    public Transaction this[int index]
    {
        get { return Transactions[index]; }
    }
}