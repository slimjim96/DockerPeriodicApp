using DockerPeriodicApp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Get the API key from secrets.json


        // Add JSON configuration provider for secrets.json
        builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
        builder.Services.Configure<StockService>(
            builder.Configuration.GetSection("API_KEY"));

        if (string.IsNullOrEmpty(apiKey) )
        {
            Console.WriteLine("API key is missing");
            return;
        }

        // Set the API key (optional
        builder.Services.AddHostedService<StockService>(
            //set api key
            (serviceProvider) =>
            {
                var service = serviceProvider.GetRequiredService<StockService>();
                service.setApiKey(apiKey);
                return service;
            }
           );
        //builder.Logging.ClearProviders();
        builder.Logging.AddConsole(); // Add a logging provider here

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.MapGet("/quote/{date}", async context =>
        {
            var date = context.Request.RouteValues["date"] as string;
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid date format");
                return;
            }

            string filePath = $"StockData_{parsedDate.Year}{parsedDate.Month:D2}{parsedDate.Day:D2}.txt";

            if (File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);
                context.Response.Headers.Append("Content-Type", "text/plain");
                await context.Response.WriteAsync(content);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("No quotes found for this date");
            }
        });
        await app.RunAsync();
    }
}
