namespace TDAmeritradeAPI.DataModels
{
    public class SecuritiesAccount
    {
        // Enums
        public enum AccountType { CASH, MARGIN }

        // Structs
        public class Structs
        {
            public struct Position
            {
                public double? ShortQuantity { get; set; }
                public double? AveragePrice { get; set; }
                public double? CurrentDayProfitLoss { get; set; }
                public double? CurrentDayProfitLossPercentage { get; set; }
                public double? LongQuantity { get; set; }
                public double? SettledLongQuantity { get; set; }
                public double? SettledShortQuantity { get; set; }
                public double? AgedQuantity { get; set; }
                public OrderInstrument? Instrument { get; set; }
                public double? MarketValue { get; set; }
            }
            public struct Balance
            {
                public double? AccruedInterest { get; set; }
                public double? CashAvailableForTrading { get; set; }
                public double? CashAvailableForWithdrawal { get; set; }
                public double? CashBalance { get; set; }
                public double? BondValue { get; set; }
                public double? CashReceipts { get; set; }
                public double? LiquidationValue { get; set; }
                public double? LongOptionMarketValue { get; set; }
                public double? LongStockValue { get; set; }
                public double? MoneyMarketFund { get; set; }
                public double? MutualFundValue { get; set; }
                public double? ShortOptionMarketValue { get; set; }
                public double? ShortStockValue { get; set; }
                public bool? IsInCall { get; set; }
                public double? UnsettledCash { get; set; }
                public double? CashDebitCallValue { get; set; }
                public double? PendingDeposits { get; set; }
                public double? AccountValue { get; set; }
                public double? LongMarketValue { get; set; }
                public double? ShortMarketValue { get; set; }
                public double? LongNonMarginableMarketValue { get; set; }
                public double? Savings { get; set; }
                public double? CashCall { get; set; }
                public double? TotalCash { get; set; }
            }
        }

        // Properties
        public AccountType? Type { get; set; }
        public string? AccountId { get; set; }
        public int? RoundTrips { get; set; }
        public bool? IsDayTrader { get; set; }
        public bool? IsClosingOnlyRestricted { get; set; }
        public List<Structs.Position>? Positions { get; set; }
        public List<Order>? OrderStrategies { get; set; }
        public Structs.Balance? InitialBalances { get; set; }
        public Structs.Balance? CurrentBalances { get; set; }
        public Structs.Balance? ProjectedBalances { get; set; }
    }
}
