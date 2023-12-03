using Azure;
using Azure.AI.OpenAI;
using ConsoleGPT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyExpensesGPT.Extensions;
using MyExpensesGPT.Models;
using MyExpensesGPT.Repository;
using System.Reflection.Metadata;


namespace MyExpensesGPT;

internal class MyWalletGPTService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly OpenAIClient _openAIClient;
    private readonly IOptions<Settings> _settings;

    public MyWalletGPTService(IHostApplicationLifetime lifetime, OpenAIClient openAIClient, IOptions<Settings> settings)
    {
        this._lifetime = lifetime;
        this._openAIClient = openAIClient;
        this._settings = settings;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Settings s = _settings.Value;
        bool goodbye = false;
        ChatCompletionsOptions completionsOptions = new()
        {
            MaxTokens = s.MaxTokens,
            Temperature = s.Temperature,
            FrequencyPenalty = s.FrequencyPenalty,
            PresencePenalty = s.PresencePenalty,
            DeploymentName = s.Model,
            Messages = {
                new(ChatRole.System, s.SystemPrompt.Replace("{today}",DateTime.Today.ToString("yyyy-MM-dd")))
            },
            Functions = GetFunctionDefinitions()

        };
        // pre-load categories
        completionsOptions.Messages.Add(new(ChatRole.System, "call GetCategories to pre-load category"));
        await WaitResponse(completionsOptions, cancellationToken);

        await WriteAssistantMessage("I am your personal financial assistant.");
        while (!goodbye)
        {
            Console.Write("You > ");
            string? input = await Console.In.ReadLineAsync();
            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            if (input.Equals("goodbye", StringComparison.OrdinalIgnoreCase))
            {
                goodbye = true;
                continue;
            }
            try
            {
                completionsOptions.Messages.Add(new(ChatRole.User, input));
                await WaitResponse(completionsOptions, cancellationToken);
            }catch(Exception ex)
            {
               await Console.Out.WriteLineAsync("system error, try again. "  + ex.Message);
            }
            finally
            {
                completionsOptions.Messages.Clear();
            }
         


        }
    }

    /// <summary>
    /// Retrieves a list of all categories from the data store.
    /// </summary>
    /// <returns>A list of category objects.</returns>
    private static List<FunctionDefinition> GetFunctionDefinitions()
    {
        return new List<FunctionDefinition>() {

              new FunctionDefinition
             {
                 Name=nameof(GetCategories),
                 Description="Get all categories from data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{},
                        Required = Array.Empty<object>(),
                     }, JsonExtension.JsonSerializerOption)

            },
              new FunctionDefinition
             {
                 Name=nameof(AddCategory),
                 Description="Add a new category to data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{
                            id= new { Type="integer", Description="Unique key"},
                            name=new { Type="string", Description="Category name"}
                        },
                        Required = new string[] {"id", "name" },
                     }, JsonExtension.JsonSerializerOption)

            }, new FunctionDefinition
             {
                 Name=nameof(UpdateCategory),
                 Description="Update existing category to data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{
                            id= new { Type="integer", Description="Unique key"},
                            name=new { Type="string", Description="Category name"}
                        },
                        Required = new string[] {"id", "name" },
                     }, JsonExtension.JsonSerializerOption)

            },
              new FunctionDefinition
             {
                 Name=nameof(DeleteCategory),
                 Description="Delete a category from data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{
                            id= new { Type="integer", Description="Unique key"}
                        },
                        Required = new string[] {"id" },
                     }, JsonExtension.JsonSerializerOption)

            },
            new FunctionDefinition
             {
                 Name=nameof(FilterExpenses),
                 Description="Get list of expenses in detail, include id, description, date, amount, must filter filter by category, date start and start end, date range cannot more than 31 days",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{
                            startDate=new { Type="string", Description="Start date of expense"},
                            endDate=new { Type="string", Description="End date of expense"}
                        },
                        Required = new string[] {"startDate", "endDate"},
                     }, JsonExtension.JsonSerializerOption)

            },
            new FunctionDefinition
             {
                 Name=nameof(GetTotalExpensesByCategory),
                 Description="Retrieves summary of the total expenses by category",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{},
                        Required = new string[] {},
                     }, JsonExtension.JsonSerializerOption)

            },
             new FunctionDefinition
             {
                 Name=nameof(GetTotalExpensesByCategoryAndDateRange),
                 Description="Retrieves summary of the total expenses by category and within a specified date range",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{
                            startDate=new { Type="string", Description="Start date of expense"},
                            endDate=new { Type="string", Description="End date of expense"}
                        },
                        Required = new string[] {"startDate", "endDate"},
                     }, JsonExtension.JsonSerializerOption)

            },
             new FunctionDefinition
             {
                 Name=nameof(AddExpense),
                 Description="Add a new expense to data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties=new{           
                            categoryId=new { Type="integer", Description="Category id, foreign key of categories data store"},
                            amount=new { Type="number", Description="Amount of expense"},
                            date=new { Type="string", Description="Date of expense"},
                            description=new { Type="string", Description="Description of expense"}
                        },
                        Required = new string[] {"name", "amount","date","description" },
                     }, JsonExtension.JsonSerializerOption)

            },
             new FunctionDefinition
             {
                 Name=nameof(UpdateExpense),
                 Description="Update existing expense to data store",
                 Parameters=BinaryData.FromObjectAsJson(
                      new{
                        Type="object",
                        Properties = new {
                            categoryId=new { Type="integer", Description="Category id, foreign key of categories data store"},
                            amount=new { Type="number", Description="Amount of expense"},
                            date=new { Type="string", Description="Date of expense"},
                            description=new { Type="string", Description="Description of expense"}
                        },
                        Required = new string[] {"name", "amount","date","description" },
                     }, JsonExtension.JsonSerializerOption)

            },
             new FunctionDefinition
             {
                 Name=nameof(DeleteExpense),
                 Description="Delete an expense from data store",
                 Parameters=BinaryData.FromObjectAsJson(
                     new {
                        Type="object",
                        Properties = new {
                            id= new { Type="string", Description="Unique key"}
                        },
                        Required = new string[] {"id" },
                     }, JsonExtension.JsonSerializerOption)

            },

        };
    }

    /// <summary>
    /// Waits for a response from the chatbot.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="completionsOptions">The chat completions options.</param>
    /// <param name="role">The role of the chat message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task WaitResponse(ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {

        Response<ChatCompletions> completions = await _openAIClient.GetChatCompletionsAsync(completionsOptions, cancellationToken);
        if (completions.Value.Choices.Count == 0)
        {
            await Console.Out.WriteLineAsync("I'm sorry, I don't know how to respond to that.");
            return;
        }
        foreach (ChatChoice choice in completions.Value.Choices)
        {
            if (!string.IsNullOrEmpty(choice.Message.Content))
            {

                string content = choice.Message.Content;
                await WriteAssistantMessage(content);
                completionsOptions.Messages.Add(new(ChatRole.Assistant, content));


            }
            else if (choice.FinishReason == "function_call")
            {
                // function call
                var arg = choice.Message.FunctionCall.Arguments;
                var functionName = choice.Message.FunctionCall.Name;
                switch (functionName)
                {
                    case nameof(GetCategories):
                        await GetCategories(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(AddCategory):
                        await AddCategory(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(UpdateCategory):
                        await UpdateCategory(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(DeleteCategory):
                        await DeleteCategory(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(GetTotalExpensesByCategory):
                        await GetTotalExpensesByCategory(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(FilterExpenses):
                        await FilterExpenses(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(GetTotalExpensesByCategoryAndDateRange):
                        await GetTotalExpensesByCategoryAndDateRange(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(AddExpense):
                        await AddExpense(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(UpdateExpense):
                        await UpdateExpense(arg, completionsOptions, cancellationToken);
                        break;
                    case nameof(DeleteExpense):
                        await DeleteExpense(arg, completionsOptions, cancellationToken);
                        break;
                    default:
                        break;
                }

            }

        }
    }






    #region Expense
    private async Task DeleteExpense(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var expense = arg.DeserializeJson<Expense>();
        var content = "Failed to delete expense";
        if (expense is not null)
        {
            var result = ExpenseRepository.Delete(expense!.Id);
            content = result ? "expense deleted successfully" : "Failed to delete expense";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(DeleteExpense),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }

    private async Task UpdateExpense(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var expense = arg.DeserializeJson<Expense>();
        var content = "Failed to update expense";
        if (expense is not null)
        {
            var result = ExpenseRepository.Update(expense);
            content = result ? "expense updated successfully" : "Failed to update expense";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(UpdateExpense),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }

    private async Task AddExpense(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var expense = arg.DeserializeJson<Expense>();
        var content = "Failed to add expense";
        if (expense is not null)
        {
            var result = ExpenseRepository.Add(expense);
            content = result ? "expense added successfully" : "Failed to add expense";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(AddExpense),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }
    private async Task GetTotalExpensesByCategory(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
 
        var result = ExpenseRepository.GetTotalExpensesByCategoryAsJson();
        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(GetTotalExpensesByCategory),
            Content = result
        });

        // if the previous message is not system message, wait for response
        if (completionsOptions.Messages[completionsOptions.Messages.Count - 2].Role != ChatRole.System)
        {
            await WaitResponse(completionsOptions, cancellationToken);
        }

    }


    private async Task GetTotalExpensesByCategoryAndDateRange(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var dateRange = arg.DeserializeJson<DateRange>();
        var result = ExpenseRepository.GetTotalExpensesByCategoryAndDateRangeAsJson(dateRange!);
        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(GetTotalExpensesByCategoryAndDateRange),
            Content = result
        });

        // if the previous message is not system message, wait for response
        if (completionsOptions.Messages[completionsOptions.Messages.Count - 2].Role != ChatRole.System)
        {
            await WaitResponse(completionsOptions, cancellationToken);
        }
    }
    private async Task FilterExpenses(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var filter = arg.DeserializeJson<DateRange>();
        var result = ExpenseRepository.FilterExpenses(filter!);
        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(FilterExpenses),
            Content = result
        });

        // if the previous message is not system message, wait for response
        if (completionsOptions.Messages[completionsOptions.Messages.Count - 2].Role != ChatRole.System)
        {
            await WaitResponse(completionsOptions, cancellationToken);
        }
    }

    #endregion

    #region Category
    private async Task DeleteCategory(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var category = arg.DeserializeJson<Category>();
        var content = "Failed to delete category";
        if (category is not null)
        {
            var result = CategoryRepository.Delete(category!.Id);
            content = result ? "Category deleted successfully" : "Failed to delete category";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(DeleteCategory),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }

    private async Task UpdateCategory(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var category = arg.DeserializeJson<Category>();
        var content = "Failed to update category";
        if (category is not null)
        {
            var result = CategoryRepository.Update(category);
            content = result ? "Category updated successfully" : "Failed to update category";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(UpdateCategory),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }

    private async Task AddCategory(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var category = arg.DeserializeJson<Category>();
        var content = "Failed to add category";
        if (category is not null)
        {
            var result = CategoryRepository.Add(category);
            content = result ? "Category added successfully" : "Failed to add category";
        }

        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(AddCategory),
            Content = content
        });
        await WaitResponse(completionsOptions, cancellationToken);
    }

    private async Task GetCategories(string arg, ChatCompletionsOptions completionsOptions, CancellationToken cancellationToken)
    {
        var result = CategoryRepository.LoadAsJson();
        completionsOptions.Messages.Add(new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = nameof(GetCategories),
            Content = result
        });
        if (completionsOptions.Messages[completionsOptions.Messages.Count - 2].Role != ChatRole.System)
        {
            await WaitResponse(completionsOptions, cancellationToken);
        }

    }
    #endregion


    private static async Task WriteAssistantMessage(string content)
    {
        ConsoleColor textColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        await Console.Out.WriteLineAsync("AI > " + content);
        Console.ForegroundColor = textColor;
    }


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
