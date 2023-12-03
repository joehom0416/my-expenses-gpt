
using MyExpensesGPT.Extensions;
using MyExpensesGPT.Models;
using System.Text.Json;

namespace MyExpensesGPT.Repository;

internal static class CategoryRepository
{
    private static readonly string filePath= "./Data/categories.json";


    public static string LoadAsJson()
    {
        string jsonContent = File.ReadAllText(filePath);
        return jsonContent;
    }

    public static List<Category>? Load()
    {

        string jsonContent = LoadAsJson();
        var result = jsonContent.DeserializeJson<List<Category>>();
        return result;
    }

    public static bool Update(Category category)
    {
        var categories = Load();
        if (categories is null)
        {
            return false;
        }
        var categoryToUpdate = categories.FirstOrDefault(c => c.Id == category.Id);
        if (categoryToUpdate is null)
        {
            return false;
        }
        categoryToUpdate.Name = category.Name;
        Save(categories);
        return true;
    }

    public static bool Delete(int id)
    {
        var categories = Load();
        if (categories is null)
        {
            return false;
        }
        var categoryToDelete = categories.FirstOrDefault(c => c.Id == id);
        if (categoryToDelete is null)
        {
            return false;
        }
        categories.Remove(categoryToDelete);
        Save(categories);
        return true;
    }

    public static bool Add(Category category)
    {
        var categories = Load();
        if (categories is null)
        {
            return false;
        }
        categories.Add(category);
        Save(categories);
        return true;
    }



    public static void Save(List<Category> categories)
    {
        string? jsonContent = categories.SerializeJson();
        File.WriteAllText(filePath, jsonContent);
    }

}
