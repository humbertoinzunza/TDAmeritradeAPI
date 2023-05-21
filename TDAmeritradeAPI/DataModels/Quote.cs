using System.Text.Json.Serialization;

namespace TDAmeritradeAPI.DataModels
{
    public class Quote
    {
        public Instrument.Enums.AssetType? AssetType { get; set; }
        public Instrument.Enums.AssetType? AssetMainType { get; set; }
        public string? Cusip { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
        public long? TradeTimeInLong { get; set; }
        public bool? Delayed { get; set; }
        public bool? RealtimeEntitled { get; set; }

        public static Instrument.Enums.AssetType GetAssetType(string jsonQuote)
        {
            // Look for the 'assetMainType' property in the JSON string
            int startIndex = jsonQuote.IndexOf("assetMainType");
            // If 'assetMainType' was not found return UNKNOWN asset type
            if (startIndex == -1) return Instrument.Enums.AssetType.UNKNOWN;
            // Skip the 'assetMainType' part
            startIndex += 13;
            // Skip non-letter characters
            while (!char.IsLetter(jsonQuote[startIndex])) startIndex++;
            int endIndex = startIndex;
            // Find the end of the 'assetMainType' value
            while (jsonQuote[endIndex] != '"') endIndex++;
            // Parse the value to enum and return it
            return Enum.Parse<Instrument.Enums.AssetType>(jsonQuote[startIndex..endIndex], true);
        }
    }

    public class MutualFundQuote : Quote
    {
        public Instrument.Enums.AssetType? AssetSubType { get; set; }
        public double? ClosePrice { get; set; }
        public double? NetChange { get; set; }
        public double? TotalVolume { get; set; }
        public string? Exchange { get; set; }
        public string? ExchangeName { get; set; }
        public byte? Digits { get; set; }

        [JsonPropertyName("52WKHigh")]
        public double? FiftyTwoWeekHigh { get; set; }

        [JsonPropertyName("52WkLow")]
        public double? FiftyTwoWeekLow { get; set; }
        public double? NAV { get; set; }
        public double? PeRatio { get; set; }
        public double? DivAmount { get; set; }
        public double? DivYield { get; set; }
        public string? DivDate { get; set; }
        public string? SecurityStatus { get; set; }
        public double? NetPercentChangeInDouble { get; set; }
    }

    public class EquityQuote : Quote
    {
        public string? AssetSubType { get; set; }
        public double? BidPrice { get; set; }
        public int? BidSize { get; set; }
        public string? BidId { get; set; }
        public double? AskPrice { get; set; }
        public int? AskSize { get; set; }
        public string? AskId { get; set; }
        public double? LastPrice { get; set; }
        public int? LastSize { get; set; }
        public string? LastId { get; set; }
        public double? OpenPrice { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public string? BidTick { get; set; }
        public double? ClosePrice { get; set; }
        public double? NetChange { get; set; }
        public long? TotalVolume { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public double? Mark { get; set; }
        public string? Exchange { get; set; }
        public string? ExchangeName { get; set; }
        public bool? Marginable { get; set; }
        public bool? Shortable { get; set; }
        public double? Volatility { get; set; }
        public byte? Digits { get; set; }

        [JsonPropertyName("52WkHigh")]
        public double? FiftyTwoWeekHigh { get; set; }

        [JsonPropertyName("52WkLow")]
        public double? FiftyTwoWeekLow { get; set; }
        public double? NAV { get; set; }
        public double? PeRatio { get; set; }
        public double? DivAmount { get; set; }
        public double? DivYield { get; set; }
        public string? DivDate { get; set; }
        public string? SecurityStatus { get; set; }
        public double? RegularMarketLastPrice { get; set; }
        public int? RegularMarketLastSize { get; set; }
        public double? RegularMarketNetChange { get; set; }
        public long? RegularMarketTradeTimeInLong { get; set; }
        public double? NetPercentChangeInDouble { get; set; }
        public double? MarkChangeInDouble { get; set; }
        public double? MarkPercentChangeInDouble { get; set; }
        public double? RegularMarketPercentChangeInDouble { get; set; }
    }

    public class FutureQuote : Quote
    {
        public double? BidPriceInDouble { get; set; }
        public double? AskPriceInDouble { get; set; }
        public double? LastPriceInDouble { get; set; }
        public long? BidSizeInLong { get; set; }
        public long? AskSizeInLong { get; set; }
        public string? BidId { get; set; }
        public string? AskId { get; set; }
        public long? TotalVolume { get; set; }
        public long? LastSizeInLong { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public double? HighPriceInDouble { get; set; }
        public double? LowPriceInDouble { get; set; }
        public double? ClosePriceInDouble { get; set; }
        public string? Exchange { get; set; }
        public string? LastId { get; set; }
        public double? OpenPriceInDouble { get; set; }
        public double? ChangeInDouble { get; set; }
        public double? FuturePercentChange { get; set; }
        public string? ExchangeName { get; set; }
        public string? SecurityStatus { get; set; }
        public double? OpenInterest { get; set; }
        public double? Mark { get; set; }
        public double? Tick { get; set; }
        public double? TickAmount { get; set; }
        public string? Product { get; set; }
        public string? FuturePriceFormat { get; set; }
        public string? FutureTradingHours { get; set; }
        public bool? FutureIsTradable { get; set; }
        public double? FutureMultiplier { get; set; }
        public bool? FutureIsActive { get; set; }
        public double? FutureSettlementPrice { get; set; }
        public string? FutureActiveSymbol { get; set; }
        public long? FutureExpirationDate { get; set; }
    }

    public class FutureOptionQuote : Quote
    {
        public double? MarkChange { get; set; }
        public double? NetChange { get; set; }
        public double? PercentChange { get; set; }
        public double? BidPriceInDouble { get; set; }
        public double? AskPriceInDouble { get; set; }
        public double? LastPriceInDouble { get; set; }
        public long? BidSizeInLong { get; set; }
        public long? AskSizeInLong { get; set; }
        public byte? BidIdInByte { get; set; }
        public byte? AskIdInByte { get; set; }
        public long? TotalVolumeInLong { get; set; }
        public long? LastSizeInLong { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public double? HighPriceInDouble { get; set; }
        public double? LowPriceInDouble { get; set; }
        public double? ClosePriceInDouble { get; set; }
        public byte? LastIdInByte { get; set; }
        public double? OpenPriceInDouble { get; set; }
        public long? OpenInterestInLong { get; set; }
        public double? Mark { get; set; }
        public double? Tick { get; set; }
        public double? TickAmount { get; set; }
        public double? FutureMultiplier { get; set; }
        public double? SettlementPriceInDouble { get; set; }
        public string? UnderlyingSymbol { get; set; }
        public double? StrikePriceInDouble { get; set; }
        public long? FutureExpirationDate { get; set; }
        public string? ExpirationStyle { get; set; }
    }

    public class OptionQuote : Quote
    {
        public double? BidPrice { get; set; }
        public int? BidSize { get; set; }
        public double? AskPrice { get; set; }
        public int? AskSize { get; set; }
        public double? LastPrice { get; set; }
        public int? LastSize { get; set; }
        public double? OpenPrice { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? ClosePrice { get; set; }
        public double? NetChange { get; set; }
        public int? TotalVolume { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public double? Mark { get; set; }
        public double? OpenInterest { get; set; }
        public double? Volatility { get; set; }
        public double? MoneyIntrinsicValue { get; set; }
        public double? Multiplier { get; set; }
        public byte? Digits { get; set; }
        public double? StrikePrice { get; set; }
        public string? ContractType { get; set; }
        public string? Underlying { get; set; }
        public byte? ExpirationDay { get; set; }
        public byte? ExpirationMonth { get; set; }
        public short? ExpirationYear { get; set; }
        public short? DaysToExpiration { get; set; }
        public double? TimeValue { get; set; }
        public string? Deliverables { get; set; }
        public double? Delta { get; set; }
        public double? Gamma { get; set; }
        public double? Theta { get; set; }
        public double? Vega { get; set; }
        public double? Rho { get; set; }
        public string? SecurityStatus { get; set; }
        public double? TheoreticalOptionValue { get; set; }
        public double? UnderlyingPrice { get; set; }
        public string? UvExpirationType { get; set; }
        public string? Exchange { get; set; }
        public string? ExchangeName { get; set; }
        public long? LastTradingDay { get; set; }
        public string? SettlementType { get; set; }
        public double? NetPercentChangeInDouble { get; set; }
        public double? MarkChangeInDouble { get; set; }
        public double? MarkPercentChangeInDouble { get; set; }
        public double? ImpliedYield { get; set; }
        public bool? IsPennyPilot { get; set; }
    }

    public class ForexQuote : Quote
    {
        public double? BidPriceInDouble { get; set; }
        public double? AskPriceInDouble { get; set; }
        public double? LastPriceInDouble { get; set; }
        public long? BidSizeInLong { get; set; }
        public long? AskSizeInLong { get; set; }
        public long? TotalVolume { get; set; }
        public long? LastSizeInLong { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public double? HighPriceInDouble { get; set; }
        public double? LowPriceInDouble { get; set; }
        public double? ClosePriceInDouble { get; set; }
        public string? Exchange { get; set; }
        public double? OpenPriceInDouble { get; set; }
        public double? ChangeInDouble { get; set; }
        public double? PercentChange { get; set; }
        public string? ExchangeName { get; set; }
        public byte? Digits { get; set; }
        public string? SecurityStatus { get; set; }
        public double? Tick { get; set; }
        public double? TickAmount { get; set; }
        public string? Product { get; set; }
        public string? TradingHours { get; set; }
        public bool? IsTradable { get; set; }
        public string? MarketMaker { get; set; }

        [JsonPropertyName("52WkHighInDouble")]
        public double? FiftyTwoWeekHighInDouble { get; set; }

        [JsonPropertyName("52WkLowInDouble")]
        public double? FiftyTwoWeekLowInDouble { get; set; }
        public double? Mark { get; set; }
    }

    public class IndexQuote : Quote
    {
        public double? LastPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? ClosePrice { get; set; }
        public double? NetChange { get; set; }
        public int? TotalVolume { get; set; }
        public string? Exchange { get; set; }
        public string? ExchangeName { get; set; }
        public byte? Digits { get; set; }

        [JsonPropertyName("52WKHigh")]
        public double? FiftyTwoWeekHigh { get; set; }

        [JsonPropertyName("52WkLow")]
        public double? FiftyTwoWeekLow { get; set; }
        public string? SecurityStatus { get; set; }
        public double? NetPercentChangeInDouble { get; set; }
    }
}
