namespace SIGREF.API.Utils;

public static class ConsoleBanner
{
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

        Console.WriteLine($"{AnsiColors.Blue}");
        Console.WriteLine("  _____   _____    _____   _____    ______   ______ ");
        Console.WriteLine(" / ____| |_   _|  / ____| |  __ \\  |  ____| |  ____|");
        Console.WriteLine("| (___     | |   | |  __  | |__) | | |__    | |__   ");
        Console.WriteLine(" \\___ \\    | |   | | |_ | |  _  /  |  __|   |  __|  ");
        Console.WriteLine(" ____) |  _| |_  | |__| | | | \\ \\  | |____  | |     ");
        Console.WriteLine("|_____/  |_____|  \\_____| |_|  \\_\\ |______| |_|     ");
        Console.WriteLine($"{AnsiColors.Reset}");
        
        Console.WriteLine($"{AnsiColors.Blue}┌───────────────────────────────────────────┐{AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Blue}│          {AnsiColors.Green}SIGREF API{AnsiColors.Blue}                       │{AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Blue}│           {AnsiColors.Gray}v1.0.0 (stable){AnsiColors.Blue}                  │{AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Blue}├───────────────────────────────────────────┤{AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Gray}│ GitHub: https://github.com/SIGREF-UNAH/SIGREF{AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Gray}│                                           {AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Gray}│ Dedicado a:                               {AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Gray}│ UNAH-COPAN • Hospital de Occidente        {AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Gray}│ IS-COPAN, Honduras C.A.                   {AnsiColors.Reset}");
        Console.WriteLine($"{AnsiColors.Blue}└───────────────────────────────────────────┘{AnsiColors.Reset}");
        Console.WriteLine();
        Console.WriteLine($"{AnsiColors.Reset}");
    }
}