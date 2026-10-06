using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpenseBot.Parsing;

public class ParsedSpending
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = "";
    public string? Note { get; set; }
}

public static class SpendingParser
{
    public static bool TryParse(string text, out ParsedSpending result, out string error)
    {
        result = new ParsedSpending();
        error = "";

        var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            error = "Write amount and category, e.g. 4.50 coffee";
            return false;
        }

        var amountText = parts[0].Replace(',', '.');
        if (!decimal.TryParse(amountText, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var amount))
        {
            error = "Amount must be the first word and a number";
            return false;
        }

        if (amount <= 0)
        {
            error = "Amount must be greater than 0";
            return false;
        }

        if (!Regex.IsMatch(parts[1], "^[a-zA-Z]+$"))
        {
            error = "Category must be one word with letters only";
            return false;
        }

        result.Amount = amount;
        result.Category = parts[1].ToLower();
        result.Note = parts.Length > 2 ? string.Join(' ', parts.Skip(2)) : null;
        return true;
    }
}
