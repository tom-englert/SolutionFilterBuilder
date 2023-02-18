internal static class Output
{
    private static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

    public static void WriteLine(string? value = null)
    {
        Console.WriteLine(value);
    }

    public static void WriteError(string value)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Error: ");
        Console.ForegroundColor = DefaultForegroundColor;
        Console.WriteLine(value);
    }

    public static void WriteWarning(string value)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Warning: ");
        Console.ForegroundColor = DefaultForegroundColor;
        Console.WriteLine(value);
    }
}
