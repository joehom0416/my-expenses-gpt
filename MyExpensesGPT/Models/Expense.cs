
namespace MyExpensesGPT.Models;

internal class Expense
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int CategoryId { get; set; } = 0;
    public double Amount { get; set; } = 0;
    public DateTime Date { get; set; } = DateTime.Now;
    public string Description { get; set; } = string.Empty;
}
