namespace SIGREF.API.Utils;

public static class ConsoleBanner
{
    // ANSI escape codes for colors
    private const string Reset = "\u001b[0m";
    private const string Blue = "\u001b[34m";
    private const string Green = "\u001b[32m";
    const string Gray = "\x1b[90m";

    public static void Print()
    {
        // Enable ANSI escape codes on Windows (if needed — .NET 6+ usually handles this automatically)
        // But just in case:
        if (OperatingSystem.IsWindows())
        {
            try
            {
                _ = Console.OutputEncoding; // Ensure output encoding supports ANSI
            }
            catch
            {
                // Fallback: just skip colors on problematic systems
            }
        }

        Console.WriteLine($"{Blue}");
        Console.WriteLine("_____     _____      _____     _____      ______     ______ ");
        Console.WriteLine("/ ____|   |_   _|    / ____|   |  __ \\    |  ____|   |  ____|");
        Console.WriteLine("| (___       | |     | |  __    | |__) |   | |__      | |__");
        Console.WriteLine("\\___ \\      | |     | | |_ |   |  _  /    |  __|     |  __|");
        Console.WriteLine("____) |    _| |_    | |__| |   | | \\ \\    | |____    | |");
        Console.WriteLine("|_____/    |_____|    \\_____|   |_|  \\_\\   |______|   |_|");
        Console.WriteLine("==== SIGREF ====");
        Console.WriteLine($"{Reset}");
        
        Console.WriteLine($"{Blue}┌───────────────────────────────────────────┐{Reset}");
        Console.WriteLine($"{Blue}│          {Green}SIGREF API{Blue}                       │{Reset}");
        Console.WriteLine($"{Blue}│           {Gray}v1.0.0 (stable){Blue}                  │{Reset}");
        Console.WriteLine($"{Blue}├───────────────────────────────────────────┤{Reset}");
        Console.WriteLine($"{Gray}│ GitHub: https://github.com/SIGREF-UNAH/SIGREF{Reset}");
        Console.WriteLine($"{Gray}│                                           {Reset}");
        Console.WriteLine($"{Gray}│ Dedicado a:                               {Reset}");
        Console.WriteLine($"{Gray}│ UNAH-COPAN • Hospital de Occidente        {Reset}");
        Console.WriteLine($"{Gray}│ IS-COPAN, Honduras C.A.                   {Reset}");
        Console.WriteLine($"{Blue}└───────────────────────────────────────────┘{Reset}");
        Console.WriteLine();
        Console.WriteLine($"{Reset}");
    }
}