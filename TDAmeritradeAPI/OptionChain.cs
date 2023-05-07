using Microsoft.VisualBasic;
using OpenQA.Selenium.DevTools;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TDAmeritradeAPI
{

   public class OptionChain
    {
        public class Enums
        {
            public enum Strategy : byte
            {
                SINGLE, ANALYTICAL, COVERED, VERTICAL, CALENDAR,
                STRANGLE, STRADDLE, BUTTERFLY, CONDOR, DIAGONAL, COLLAR, ROLL
            }
        }
        public string? Symbol { get; set; }
        public string? Status { get; set; }
        public Underlying? Underlying { get; set; }
        public Enums.Strategy? Strategy { get; set; }
        public double? Interval { get; set; }
        public bool? IsDelayed { get; set; }
        public bool? IsIndex { get; set; }
        public double? DaysToExpiration { get; set; }
        public double? InterestRate { get; set; }
        public double? UnderlyingPrice { get; set; }
        public double? Volatility { get; set; }
        public int? NumberOfContracts { get; set; }
        public Dictionary<string, Dictionary<string, OptionInfo[]>>? CallExpDateMap { get; set; }
        public Dictionary<string, Dictionary<string, OptionInfo[]>>? PutExpDateMap { get; set; }

        public string ToJson()
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
            return JsonSerializer.Serialize(this, jsonSerializerOptions);
        }
    }
    public class Underlying
    {
        public class Enums
        {
            public enum Exchange : byte { IND, ASE, NYS, NAS, NAP, PAC, OPR, BATS }
        }
        public double? Ask { get; set; }
        public int? AskSize { get; set; }
        public double? Bid { get; set; }
        public int? BidSize { get; set; }
        public double? Change { get; set; }
        public double? Close { get; set; }
        public bool? Delayed { get; set; }
        public string? Description { get; set; }
        public Enums.Exchange? ExchangeName { get; set; }
        public double? FiftyTwoWeekHigh { get; set; }
        public double? FiftyTwoWeekLow { get; set; }
        public double? HighPrice { get; set; }
        public double? Last { get; set; }
        public double? LowPrice { get; set; }
        public double? Mark { get; set; }
        public double? MarkChange { get; set; }
        public double? MarkPercentChange { get; set; }
        public double? OpenPrice { get; set; }
        public double? PercentChange { get; set; }
        public long? QuoteTime { get; set; }
        public string? Symbol { get; set; }
        public long? TotalVolume { get; set; }
        public long? TradeTime { get; set; }
    }
    public class OptionInfo
    {
        public Option.Enums.PutCall? PutCall { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
        public string? ExchangeName { get; set; }
        public double? Bid { get; set; }
        public double? Ask { get; set; }
        public double? Last { get; set; }
        public double? Mark { get; set; }
        public int? BidSize { get; set; }
        public int? AskSize { get; set; }
        public string? BidAskSize { get; set; }
        public int? LastSize { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? ClosePrice { get; set; }
        public long? TotalVolume { get; set; }
        public string? TradeDate { get; set; }
        public long? QuoteTimeInLong { get; set; }
        public long? TradeTimeInLong { get; set; }
        public double? NetChange { get; set; }
        public double? Volatility { get; set; }
        public double? Delta { get; set; }
        public double? Gamma { get; set; }
        public double? Theta { get; set; }
        public double? Vega { get; set; }
        public double? Rho { get; set; }
        public double? TimeValue { get; set; }
        public double? OpenInterest { get; set; }
        public bool? InTheMoney { get; set; }
        public double? TheoreticalOptionValue { get; set; }
        public double? TheoreticalVolatility { get; set; }
        public bool? Mini { get; set; }
        public bool? NonStandard { get; set; }
        public Optiondeliverable[]? OptionDeliverablesList { get; set; }
        public double? StrikePrice { get; set; }
        public long? ExpirationDate { get; set; }
        public short? DaysToExpiration { get; set; }
        public string? ExpirationType { get; set; }
        public long? LastTradingDay { get; set; }
        public double? Multiplier { get; set; }
        public string? SettlementType { get; set; }
        public string? DeliverableNote { get; set; }
        public bool? IsIndexOption { get; set; }
        public double? PercentChange { get; set; }
        public double? MarkChange { get; set; }
        public double? MarkPercentChange { get; set; }
        public double? IntrinsicValue { get; set; }
        public bool? PennyPilot { get; set; }
    }
    public class Optiondeliverable
    {
        public string? Symbol { get; set; }
        public string? AssetType { get; set; }
        public string? DeliverableUnits { get; set; }
        public string? CurrencyType { get; set; }
    }
    public class OptionChainSearchOptions
    {
        public class Enums
        {
            public enum Range : byte { ITM, NTM, OTM, SAK, SBK, SNK, ALL }
            public enum ExpirationMonth : byte { JAN, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, DEC, ALL }
            public enum OptionType : byte { S, NS, ALL }
        }
        public OptionChainSearchOptions(string symbol)
        {
            Symbol = symbol;
        }
        /// <summary>
        /// The symbol to search options for.
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Type of contracts to return in the chain. Can be CALL, PUT, or ALL. Default is ALL.
        /// </summary>
        public Option.Enums.PutCall? ContractType { get; set; }
        /// <summary>
        /// The number of strikes to return above and below the at-the-money price.
        /// </summary>
        public ushort? StrikeCount { get; set; }
        /// <summary>
        /// Include quotes for options in the option chain. Can be TRUE or FALSE. Default is FALSE.
        /// </summary>
        public bool? IncludeQuotes { get; set; }
        /// <summary>
        /// Passing a value returns a Strategy Chain. Possible values are SINGLE, ANALYTICAL 
        /// (allows use of the volatility, underlyingPrice, interestRate,
        /// and daysToExpiration params to calculate theoretical values), COVERED, VERTICAL,
        /// CALENDAR, STRANGLE, STRADDLE, BUTTERFLY, CONDOR, DIAGONAL, COLLAR, or ROLL. Default is SINGLE.
        /// </summary>
        public OptionChain.Enums.Strategy? Strategy { get; set; }
        /// <summary>
        /// Strike interval for spread strategy chains (see strategy param).
        /// </summary>
        public double? Interval { get; set; }
        /// <summary>
        /// Provide a strike price to return options only at that strike price.
        /// </summary>
        public double? Strike { get; set; }
        /// <summary>
        /// Returns options for the given range. Possible values are:
        /// ITM: In-the-money
        /// NTM: Near-the-money
        /// OTM: Out-of-the-money
        /// SAK: Strikes Above Market
        /// SBK: Strikes Below Market
        /// SNK: Strikes Near Market
        /// ALL: All Strikes
        /// Default is ALL.
        /// </summary>
        public Enums.Range? Range { get; set; }
        /// <summary>
        /// Only return expirations after this date. For strategies,
        /// expiration refers to the nearest term expiration in the strategy.
        /// </summary>
        public DateOnly? FromDate { get; set; }
        /// <summary>
        /// 'Only return expirations before this date. For strategies,
        /// expiration refers to the nearest term expiration in the strategy.
        /// </summary>
        public DateOnly? ToDate { get; set; }
        /// <summary>
        /// Volatility to use in calculations. Applies only to ANALYTICAL strategy chains (see strategy param).
        /// </summary>
        public double? Volatility { get; set; }
        /// <summary>
        /// Underlying price to use in calculations. Applies only to ANALYTICAL strategy chains (see strategy param).
        /// </summary>
        public double? UnderlyingPrice { get; set; }
        /// <summary>
        /// Interest rate to use in calculations. Applies only to ANALYTICAL strategy chains (see strategy param).
        /// </summary>
        public double? InterestRate { get; set; }
        /// <summary>
        /// Days to expiration to use in calculations. Applies only to ANALYTICAL strategy chains (see strategy param).
        /// </summary>
        public ushort? DaysToExpiration { get; set; }
        /// <summary>
        /// Return only options expiring in the specified month.
        /// Month is given in the three character format.
        /// Example: JAN
        // Default is ALL.
        /// </summary>
        public Enums.ExpirationMonth? ExpMonth { get; set; }
        /// <summary>
        /// Type of contracts to return. Possible values are:
        /// S: Standard contracts
        /// NS: Non-standard contracts
        /// ALL: All contracts
        /// Default is ALL.''
        /// </summary>
        public Enums.OptionType? OptionType { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> result = new()
            {
                { "symbol", Symbol }
            };
            if (Volatility != null || UnderlyingPrice != null || InterestRate != null || DaysToExpiration != null)
                Strategy = OptionChain.Enums.Strategy.ANALYTICAL;
            if (ContractType != null) result.Add("contractType", ContractType.ToString()!);
            if (StrikeCount != null) result.Add("strikeCount", StrikeCount.ToString()!);
            if (IncludeQuotes != null) result.Add("includeQuotes", IncludeQuotes.ToString()!.ToUpper());
            if (Strategy != null) result.Add("strategy", IncludeQuotes.ToString()!);
            if (Interval != null) result.Add("interval", Interval.ToString()!);
            if (Strike != null) result.Add("strike", Strike.ToString()!);
            if (Range != null) result.Add("range", Range.ToString()!);
            if (FromDate != null) result.Add("fromDate", FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)!);
            if (ToDate != null) result.Add("toDate", ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)!);
            if (Volatility != null) result.Add("volatility", Volatility.ToString()!);
            if (UnderlyingPrice != null) result.Add("underlyingPrice", UnderlyingPrice.ToString()!);
            if (InterestRate != null) result.Add("interestRate", InterestRate.ToString()!);
            if (DaysToExpiration != null) result.Add("daysToExpiration", DaysToExpiration.ToString()!);
            if (ExpMonth != null) result.Add("expMonth", ExpMonth.ToString()!);
            if (OptionType != null) result.Add("optionType", OptionType.ToString()!);

            return result;
        }
    }
}
