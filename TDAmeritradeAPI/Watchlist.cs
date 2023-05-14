using System.Text.Json;

namespace TDAmeritradeAPI
{
    public class Watchlist
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public class Enums
        {
            public enum Status { UNCHANGED, CREATED, UPDATE, DELETED }
        }
        public Watchlist()
        {
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Represents a watchlist in the format required by the TD Ameritrade API. More details at
        /// https://developer.tdameritrade.com/watchlist/apis/post/accounts/%7BaccountId%7D/watchlists-0.
        /// </summary>
        /// <param name="name">Name of the watchlist.</param>
        /// <param name="itemList">List watchlist items.</param>
        public Watchlist(string name, WatchlistItem[] itemList) : this()
        {
            Name = name;
            WatchlistItems = (WatchlistItem[])itemList.Clone();
        }

        public string? Name { get; set; }
        public string? WatchlistId { get; set; }
        public Enums.Status? Status { get; set; }
        public WatchlistItem[]? WatchlistItems { get; set; }
        public string AsJson()
        {
            return JsonSerializer.Serialize(this, _serializerOptions);
        }
    }

    /// <summary>
    /// Represents an item in a watchlist.
    /// </summary>
    public class WatchlistItem
    {
        public WatchlistItem() { }
        public WatchlistItem(string symbol, OrderInstrument.AssetTypes assetType)
        {
            Instrument = new OrderInstrument(symbol, assetType);
        }
        public int? SequenceId { get; set; }
        public double? Quantity { get; set; }
        public double? AveragePrice { get; set; }
        public double? Commission { get; set; }
        public string? Date { get; set; }
        public OrderInstrument? Instrument { get; set; }
        public Watchlist.Enums.Status? Status { get; set; }
    }
}
