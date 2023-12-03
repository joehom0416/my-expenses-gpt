
using MyExpensesGPT.Models;
using System.Text.Json;

namespace MyExpensesGPT.Repository;



internal static class ExpenseRepository
{
    private static readonly string filePath = "./Data/expenses.json";

    private static JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };



    public static List<Expense>? Load()
    {

        string jsonContent = LoadAsJson();
        var result = JsonSerializer.Deserialize<List<Expense>>(jsonContent, JsonSerializerOption);
        return result;
    }

    public static string FilterExpenses(DateRange filter)
    {
        List<Expense>? expenses = Load();
  
        expenses = expenses?.Where(c => c.Date >= filter.StartDate && c.Date <= filter.EndDate).ToList();
        
        return JsonSerializer.Serialize(expenses, JsonSerializerOption);
    }

    /// <summary>
    /// Retrieves the total expenses by category as a JSON string.
    /// </summary>
    /// <returns>The JSON string representing the total expenses by category.</returns>
    public static string GetTotalExpensesByCategoryAsJson()
    {
        List<Expense>? expenses = Load();
        var result = expenses?.GroupBy(c => c.CategoryId).Select(c => new TotalExpensesByCategory
        {
            CategoryId = c.Key,
            TotalAmount = c.Sum(d => d.Amount)
        }).ToList();
        return JsonSerializer.Serialize(result, JsonSerializerOption);
    }

    /// <summary>
    /// Retrieves the total expenses by category and date range as a JSON string.
    /// </summary>
    /// <param name="dateRange">The date range to filter expenses.</param>
    /// <returns>The JSON string representing the total expenses by category and date range.</returns>
    public static string GetTotalExpensesByCategoryAndDateRangeAsJson(DateRange dateRange)
    {
        List<Expense>? expenses = Load();
        var result = expenses?.Where(c => c.Date >= dateRange.StartDate && c.Date <= dateRange.EndDate)
            .GroupBy(c => c.CategoryId).Select(c => new TotalExpensesByCategory
            {
                CategoryId = c.Key,
                TotalAmount = c.Sum(d => d.Amount)
            }).ToList();
        return JsonSerializer.Serialize(result, JsonSerializerOption);
    }

    public static string LoadAsJson()
    {
        string jsonContent = File.ReadAllText(filePath);
        return jsonContent;
    }   

    public static bool Update(Expense expense)
    {
        var expenses = Load();
        if (expenses is null)
        {
            return false;
        }
        var expenseToUpdate = expenses.FirstOrDefault(c => c.Id == expense.Id);
        if (expenseToUpdate is null)
        {
            return false;
        }
        expenseToUpdate.CategoryId = expense.CategoryId;
        expenseToUpdate.Amount = expense.Amount;
        expenseToUpdate.Date = expense.Date;
        expenseToUpdate.Description = expense.Description;

        Save(expenses);
        return true;
    }

    public static bool Delete(string id)
    {
        var expenses = Load();
        if (expenses is null)
        {
            return false;
        }
        var expenseToDelete = expenses.FirstOrDefault(c => c.Id == id);
        if (expenseToDelete is null)
        {
            return false;
        }
        expenses.Remove(expenseToDelete);
        Save(expenses);
        return true;
    }

    public static bool Add(Expense expense)
    {
        var expenses = Load();
        if (expenses is null)
        {
            return false;
        }
        expense.Id = Guid.NewGuid().ToString(); // assign new id
        expenses.Add(expense);
        Save(expenses);
        return true;
    }



    public static void Save(List<Expense> expenses)
    {
        string jsonContent = JsonSerializer.Serialize(expenses, JsonSerializerOption);
        File.WriteAllText(filePath, jsonContent);
    }




}
