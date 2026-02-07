using System;

public static class Logger
{
    public static void Info(string message)
    {
        Log("INFO", message);
    }

    public static void Warning(string message)
    {
        Log("WARNING", message);
    }

    public static void Error(string message)
    {
        Log("ERROR", message);
    }

    public static void Critical(string message)
    {
        Log("CRITICAL", message);
    }

    private static void Log(string level, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        System.Console.WriteLine($"[{level}] {timestamp} - {message}");
    }
}