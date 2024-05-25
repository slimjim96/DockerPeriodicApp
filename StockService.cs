using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DockerPeriodicApp.StockData;

namespace DockerPeriodicApp
{
    internal class StockService : BackgroundService
    {
        private readonly AlphaVantageClient _stockDataReader;

        //set api key
        protected string ApiKey { get; set; }

        public void setApiKey(string apiKey  )
        {
            ApiKey = apiKey;
        }
        public StockService()
        {
           if(string.IsNullOrEmpty(ApiKey))
            {
                ApiKey = String.Empty;  
            }
            _stockDataReader = new AlphaVantageClient(ApiKey);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var quote = await _stockDataReader.GetDailyTimeSeries("PAYX");// Replace with actual symbol

                // calculate average of past close numbers for each day returned
                if(quote != null)
                {
                    Decimal averageClose = 0;
                    foreach(var item in quote.TimeSeriesDaily) 
                    {
                        averageClose += item.Value.Close;
                    }
                    averageClose = averageClose / quote.TimeSeriesDaily.Count;

                    using(StreamWriter file = new StreamWriter("StockData_" + DateTime.Now.Year + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + ".txt", true))
                    {
                        file.WriteLine("Average Close: " + averageClose + " - Date: " + DateTime.Now);
                    }
                    
                }
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}