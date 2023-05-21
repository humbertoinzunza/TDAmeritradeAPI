namespace TDAmeritradeAPI.DataModels
{
    public class Instrument
    {
        public class Enums
        {
            public enum AssetType : byte { EQUITY, ETF, FOREX, FUTURE, FUTURE_OPTION, INDEX, INDICATOR, MUTUAL_FUND, OPTION, UNKNOWN }
            public enum SearchType : byte { SYMBOL_SEARCH, SYMBOL_REGEX, DESC_SEARCH, DESC_REGEX };
        }
        public string? Cusip { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
        public string? Exchange { get; set; }
        public Enums.AssetType? AssetType { get; set; }
    }

    public class FundamentalData
    {
        public string? Cusip { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
        public string? Exchange { get; set; }
        public Instrument.Enums.AssetType? AssetType { get; set; }
        public Fundamental Fundamental { get; set; }
    }

    public struct Fundamental
    {
        public string? Symbol { get; set; }
        public double? High52 { get; set; }
        public double? Low52 { get; set; }
        public double? DividendAmount { get; set; }
        public double? DividendYield { get; set; }
        public string? DividendDate { get; set; }
        public double? PeRatio { get; set; }
        public double? PegRatio { get; set; }
        public double? PbRatio { get; set; }
        public double? PrRatio { get; set; }
        public double? PcfRatio { get; set; }
        public double? GrossMarginTTM { get; set; }
        public double? GrossMarginMRQ { get; set; }
        public double? NetProfitMarginTTM { get; set; }
        public double? NetProfitMarginMRQ { get; set; }
        public double? OperatingMarginTTM { get; set; }
        public double? OperatingMarginMRQ { get; set; }
        public double? ReturnOnEquity { get; set; }
        public double? ReturnOnAssets { get; set; }
        public double? ReturnOnInvestment { get; set; }
        public double? QuickRatio { get; set; }
        public double? CurrentRatio { get; set; }
        public double? InterestCoverage { get; set; }
        public double? TotalDebtToCapital { get; set; }
        public double? LtDebtToEquity { get; set; }
        public double? TotalDebtToEquity { get; set; }
        public double? EpsTTM { get; set; }
        public double? EpsChangePercentTTM { get; set; }
        public double? EpsChangeYear { get; set; }
        public double? EpsChange { get; set; }
        public double? RevChangeYear { get; set; }
        public double? RevChangeTTM { get; set; }
        public double? RevChangeIn { get; set; }
        public double? SharesOutstanding { get; set; }
        public double? MarketCapFloat { get; set; }
        public double? MarketCap { get; set; }
        public double? BookValuePerShare { get; set; }
        public double? ShortIntToFloat { get; set; }
        public double? ShortIntDayToCover { get; set; }
        public double? DivGrowthRate3Year { get; set; }
        public double? DividendPayAmount { get; set; }
        public string DividendPayDate { get; set; }
        public double? Beta { get; set; }
        public double? Vol1DayAvg { get; set; }
        public double? Vol10DayAvg { get; set; }
        public double? Vol3MonthAvg { get; set; }
    }
}
