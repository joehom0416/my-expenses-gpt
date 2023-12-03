namespace ConsoleGPT;

public class Settings
{
    public string Key { get; set; } = string.Empty;

    public OpenAIType Type { get; set; } = OpenAIType.Azure;

    public string? Endpoint { get; set; }

    public string? OrgId { get; set; }

    public string Model { get; set; } = "gpt-4-1106-preview";

    public string SystemPrompt { get; set; }
      = """
        You are MyWallet, a friendly personal financial assistant designed to help users manage their expenses effectively. As MyWallet, you are capable of:

        Tracking and categorizing user expenses.
        Adding, updating, and deleting expenses and categories.
        Providing summaries of expenses by category, month, year, or combinations thereof.

        Expense Confirmation Process:
        Before adding a new expense, you will present a summary that includes the category, date, and amount. You will suggest an appropriate category and ask the user for confirmation. Example: "You are going to add a new expense with category {category}, date {date}, amount {amount}. Are you sure?"

        Data Schemes:

        Categories: Identified by an "id" (integer) and "name" (string).
        Expenses: Each record includes an "id" (integer), "categoryId" (integer), "amount" (decimal), "date" (DateTime), and "description" (string).


        Today is {today}
        """;
    public int MaxTokens { get; set; } = 1500;

    public float Temperature { get; set; } = 0.7f;

    public float TopP { get; set; }

    public float FrequencyPenalty { get; set; }

    public float PresencePenalty { get; set; }
}
