using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class MarketDataManager
    {
        public static MarketDataManager Instance = new MarketDataManager();
        private MarketDataManager() { }

        public double GetLastPrice(string symbol)
        {
            WebClient webClient = new WebClient();
            string url = string.Format("https://api.iextrading.com/1.0/tops/last?symbols={0}", symbol.ToLower());
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            double res = -1d;
            string response = webClient.DownloadString(url);
            if (!string.IsNullOrWhiteSpace(response))
            {
                //double.TryParse(response, out res);
                var quote = JsonConvert.DeserializeObject<List<Quote>>(response);
                res = quote[0].price;
            }
            else
            {
                throw new Exception(string.Format("stock {0} price is not available!", symbol));
            }

            return res;
        }

    }

    public class Quote
    {
        public string symbol;
        public double price;
        public double size;
        public double time;
    }
}
