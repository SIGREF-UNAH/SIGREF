using SIGREF.API.Services.FhirUtils;

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

        Console.WriteLine($"{FhirAnsiColors.Blue}");
        Console.WriteLine("_____     _____      _____     _____      ______     ______ ");
        Console.WriteLine("/ ____|   |_   _|    / ____|   |  __ \\    |  ____|   |  ____|");
        Console.WriteLine("| (___       | |     | |  __    | |__) |   | |__      | |__");
        Console.WriteLine("\\___ \\      | |     | | |_ |   |  _  /    |  __|     |  __|");
        Console.WriteLine("____) |    _| |_    | |__| |   | | \\ \\    | |____    | |");
        Console.WriteLine("|_____/    |_____|    \\_____|   |_|  \\_\\   |______|   |_|");
        Console.WriteLine("==== SIGREF ====");
        Console.WriteLine($"{FhirAnsiColors.Reset}");
        
        Console.WriteLine($"{FhirAnsiColors.Blue}┌───────────────────────────────────────────┐{FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Blue}│          {FhirAnsiColors.Green}SIGREF API{FhirAnsiColors.Blue}                       │{FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Blue}│           {FhirAnsiColors.Gray}v1.0.0 (stable){FhirAnsiColors.Blue}                  │{FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Blue}├───────────────────────────────────────────┤{FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Gray}│ GitHub: https://github.com/SIGREF-UNAH/SIGREF{FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Gray}│                                           {FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Gray}│ Dedicado a:                               {FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Gray}│ UNAH-COPAN • Hospital de Occidente        {FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Gray}│ IS-COPAN, Honduras C.A.                   {FhirAnsiColors.Reset}");
        Console.WriteLine($"{FhirAnsiColors.Blue}└───────────────────────────────────────────┘{FhirAnsiColors.Reset}");
        Console.WriteLine();
        Console.WriteLine($"{FhirAnsiColors.Reset}");
    }
}