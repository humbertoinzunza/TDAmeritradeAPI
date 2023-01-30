using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TDAmeritradeAPI
{
    public class Watchlist
    {
        private static readonly JsonSerializerSettings _serializerSettings = new();
        public Watchlist()
        {
            _serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        /// <summary>
        /// Represents a watchlist in the format required by the TD Ameritrade API. More details at
        /// https://developer.tdameritrade.com/watchlist/apis/post/accounts/%7BaccountId%7D/watchlists-0.
        /// </summary>
        /// <param name="name">Name of the watchlist.</param>
        /// <param name="itemList">List watchlist items.</param>
        public Watchlist(string name, List<WatchlistItem> itemList) : this()
        {
            Name = name;
            WatchlistItems = itemList;
        }

        public string? Name { get; set; }
        public List<WatchlistItem>? WatchlistItems { get; set; }

        public string? WatchlistId { get; set; }

        public string AsJson()
        {
            string jsonString = JsonConvert.SerializeObject(this, _serializerSettings);
            return jsonString;
        }
    }

    /// <summary>
    /// Represents an item in a watchlist.
    /// </summary>
    public class WatchlistItem
    {
        public class Enums
        {
            public struct Instrument
            {
                public Instrument(string symbol, string assetType)
                {
                    Symbol = symbol;
                    AssetType = assetType;
                }
                public string? Symbol { get; set; }
                public string? AssetType { get; set; }
            }
        }
        public WatchlistItem() { }
        public WatchlistItem(string symbol, string assetType)
        {
            Instrument = new Enums.Instrument(symbol, assetType);
        }
            
        public static int Quantity { get { return 0; } set { Quantity = value; } }
        public static int AveragePrice { get { return 0; } set { AveragePrice = value; } }
        public static int Commission { get { return 0; } set { Commission = value; }  }
        public Enums.Instrument? Instrument { get; set; }
        public int? SequenceId { get; set; }
    }
}
