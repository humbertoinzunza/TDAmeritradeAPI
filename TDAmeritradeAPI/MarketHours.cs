namespace TDAmeritradeAPI
{
    public enum MarketType { EQUITY, OPTION, FUTURE, BOND, FOREX }
    public struct Hours
    {
        public string Start { get; set; }
        public string End { get; set; }
    }

    public struct SessionHours
    {
        public List<Hours>? PreMarket { get; set; }
        public List<Hours>? RegularMarket { get; set; }
        public List<Hours>? OutcryMarket { get; set; }
    }
    public class MarketHours
    {
        public string? Category { get; set; }
        public string? Date { get; set; }
        public string? Exchange { get; set; }
        public bool? IsOpen { get; set; }
        public MarketType? MarketType { get; set; }
        public string? Product { get; set; }
        public string? ProductName { get; set; }
        public SessionHours SessionHours { get; set; }
    }
}
