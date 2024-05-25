using System.Formats.Asn1;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerPeriodicApp
{
    internal class StockData
    {
        // Root class for the entire JSON response
        public class StockDataResponse
        {
            [JsonPropertyName("Meta Data")]
            public MetaData MetaData { get; set; }
            [JsonPropertyName("Time Series (Daily)")]
            public Dictionary<DateTime, TimeSeriesData> TimeSeriesDaily { get; set; }
        }

        // Class for "Meta Data" section
        public class MetaData
        {
            public string Information { get; set; }
            public string Symbol { get; set; }

            private DateTime lastRefreshed;
            [JsonPropertyName("Last Refreshed")]
            public DateTime LastRefreshed
            {
                get
                {
                    return this.lastRefreshed;
                }
                //Convert from ISO time
                set
                {
                    this.lastRefreshed = Convert.ToDateTime(value);
                }
            }
            [JsonPropertyName("Output Size")]
            public string OutputSize { get; set; }
            [JsonPropertyName("Time Zone")]
            public string TimeZone { get; set; }
        }

        // Class for daily time series data

        public class TimeSeriesData
        {
            [JsonPropertyName("1. open")]
            //Convert sting to decimal
            public string Open { get; set; }

            [JsonPropertyName("2. high")]
            public decimal High { get; set; }

            [JsonPropertyName("3. low")]
            public decimal Low { get; set; }

            [JsonPropertyName("4. close")]
            public decimal Close { get; set; }

            [JsonPropertyName("5. volume")]
            public long Volume { get; set; }
        }

        public class StockDataParser
        {
            // Method to deserialize with error handling
            public static StockDataResponse ParseStockData(string json)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Handle case-insensitive property names
                    NumberHandling = JsonNumberHandling.AllowReadingFromString |
     JsonNumberHandling.WriteAsString
                };

                try
                {
                    var stockData = JsonSerializer.Deserialize<StockDataResponse>(json, options);

                    ValidateStockData(stockData); // Add validation
                    return stockData;
                }
                catch (JsonException ex)
                {
                    // Handle JSON parsing errors (e.g., invalid format)
                    throw new ArgumentException("Invalid JSON format: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Handle any other unexpected errors during deserialization
                    throw new Exception("Error parsing stock data: " + ex.Message);
                }
            }

            // Validation method
            private static void ValidateStockData(StockDataResponse stockData)
            {
                if (stockData == null)
                    throw new ArgumentNullException("Stock data is null");

                if (stockData.MetaData == null)
                    throw new ArgumentException("Meta data is missing");

                if (stockData.TimeSeriesDaily == null || !stockData.TimeSeriesDaily.Any())
                    throw new ArgumentException("Time series data is missing or empty");

                // Add more specific validation rules if needed
            }
        }

        public class AlphaVantageClient
        {
            private readonly string _apiKey;
            private readonly HttpClient _httpClient;

            public AlphaVantageClient(string apiKey)
            {
                _apiKey = apiKey;
                _httpClient = new HttpClient();
            }

            public async Task<StockDataResponse> GetDailyTimeSeries(string symbol)
            {
                string url = BuildUrl(symbol); // Build the URL
                string responseJson = await FetchData(url); // Fetch the data
                return StockDataParser.ParseStockData(responseJson); // Parse the data
            }

            private string BuildUrl(string symbol)
            {
                return $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";
            }

            private async Task<string> FetchData(string url)
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw if not successful
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
