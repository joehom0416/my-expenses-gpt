﻿using Azure;
using Azure.AI.OpenAI;
using ConsoleGPT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyExpensesGPT;
using MyExpensesGPT.Repository;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile($"appsettings.json", true, true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

hostBuilder.ConfigureServices((context, services) =>
{
    
    services.Configure<Settings>(context.Configuration.GetSection("settings"));
    services.AddSingleton(provider =>
    {
        Settings settings = provider.GetRequiredService<IOptions<Settings>>().Value;

        OpenAIClient client = settings.Type == OpenAIType.Azure
            ? new OpenAIClient(new Uri(settings.Endpoint!), new AzureKeyCredential(settings.Key))
            : new OpenAIClient(settings.Key!);

        return client;
    });   
    services.AddHostedService<MyWalletGPTService>();
});

await hostBuilder.Build().RunAsync();