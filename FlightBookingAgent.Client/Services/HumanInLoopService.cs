using Microsoft.Extensions.Logging;

namespace FlightBookingAgent.Client.Services;

public class HumanInLoopService
{
    private readonly ILogger<HumanInLoopService> _logger;

    public HumanInLoopService(ILogger<HumanInLoopService> logger)
    {
        _logger = logger;
    }

    public bool ShouldProceedWithActionAsync(string action, string details)
    {
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("🤖 AI AGENT REQUEST FOR HUMAN APPROVAL");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"Action: {action}");
        Console.WriteLine($"Details: {details}");
        Console.WriteLine(new string('-', 50));

        while (true)
        {
            Console.Write("Do you want to proceed? (y/n/details): ");
            var input = Console.ReadLine()?.ToLower().Trim();

            switch (input)
            {
                case "y":
                case "yes":
                    _logger.LogInformation("Human approved action: {Action}", action);
                    Console.WriteLine("✅ Approved. Proceeding with action...\n");
                    return true;

                case "n":
                case "no":
                    _logger.LogInformation("Human rejected action: {Action}", action);
                    Console.WriteLine("❌ Rejected. Action cancelled.\n");
                    return false;

                case "details":
                case "d":
                    Console.WriteLine($"\nDetailed information:\n{details}\n");
                    break;

                default:
                    Console.WriteLine("Please enter 'y' for yes, 'n' for no, or 'details' for more information.");
                    break;
            }
        }
    }

    public string GetUserInputAsync(string prompt)
    {
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("🤖 AI AGENT NEEDS HUMAN INPUT");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine(prompt);
        Console.WriteLine(new string('-', 50));
        Console.Write("Your input: ");

        var input = Console.ReadLine() ?? string.Empty;
        _logger.LogInformation("Human provided input for prompt: {Prompt}", prompt);

        return input;
    }

    public (string name, string email, string phone) CollectPassengerDetailsAsync()
    {
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("🤖 COLLECTING PASSENGER INFORMATION");
        Console.WriteLine(new string('=', 50));

        Console.Write("Full Name: ");
        var name = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.Write("Name is required. Please enter full name: ");
            name = Console.ReadLine();
        }

        Console.Write("Email Address: ");
        var email = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            Console.Write("Valid email is required. Please enter email: ");
            email = Console.ReadLine();
        }

        Console.Write("Phone Number: ");
        var phone = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(phone))
        {
            Console.Write("Phone number is required. Please enter phone: ");
            phone = Console.ReadLine();
        }

        _logger.LogInformation("Collected passenger details for {Name}", name);
        return (name!, email!, phone!);
    }
    
    public static bool ContainsBookingIntent(string input)
    {
        input = input.ToLower().Trim();
        
        // Quick negation check
        if (input.Contains("don't") || input.Contains("not") || input.Contains("won't"))
            return false;
        
        // More specific patterns instead of broad keyword matching
        var intentPatterns = new[]
        {
            "book a flight", "book flight", "buy ticket", "buy a ticket",
            "purchase ticket", "reserve flight", "get ticket", "need flight",
            "want to book", "want to buy", "looking to book"
        };
        
        return intentPatterns.Any(pattern => input.Contains(pattern));
    }
}