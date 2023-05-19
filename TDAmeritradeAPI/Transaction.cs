namespace TDAmeritradeAPI
{
    public class Transaction
    {
        public abstract class Structs
        {
            public struct Fees
            {
                public double? RFee { get; set; }
                public double? AdditionalFee { get; set; }
                public double? CdscFee { get; set; }
                public double? RegFee { get; set; }
                public double? OtherCharges { get; set; }
                public double? Commission { get; set; }
                public double? OptRegFee { get; set; }
                public double? SecFee { get; set; }
            }
            public struct Instrument
            {
                public string? Symbol { get; set; }
                public string? UnderlyingSymbol { get; set; }
                public string? OptionExpirationDate { get; set; }
                public double? OptionStrikePrice { get; set; }
                public Option.Enums.PutCall? PutCall { get; set; }
                public string? Cusip { get; set; }
                public string? Description { get; set; }
                public Enums.AssetType? AssetType { get; set; }
                public string? BondMaturityDate { get; set; }
                public double? BondInterestRate { get; set; }
            }
            public struct TransactionItem
            {
                public long? AccountId { get; set; }
                public double? Amount { get; set; }
                public double? Price { get; set; }
                public double? Cost { get; set; }
                public int? ParentOrderKey { get; set; }
                public string? ParentChildIndicator { get; set; }
                public Enums.Instruction? Instruction { get; set; }
                public Enums.PositionEffect? PositionEffect { get; set; }
                public Instrument? Instrument { get; set; }
            }
        }
        public class Enums
        {
            public enum SearchType : byte
            {
                ALL, TRADE, BUY_ONLY, SELL_ONLY, CASH_IN_OR_CASH_OUT, CHECKING, DIVIDEND,
                INTEREST, OTHER, ADVISOR_FEES
            }
            public enum Type : byte
            {
                TRADE, RECEIVE_AND_DELIVER, DIVIDEND_OR_INTEREST, ACH_RECEIPT, ACH_DISBURSEMENT,
                CASH_RECEIPT, CASH_DISBURSEMENT, ELECTRONIC_FUND, WIRE_OUT, WIRE_IN, JOURNAL,
                MEMORANDUM, MARGIN_CALL, MONEY_MARKET, SMA_ADJUSTMENT
            }
            public enum AchievedStatus : byte { APPROVED, REJECTED, CANCEL, ERROR }
            public enum Instruction : byte { BUY, SELL }
            public enum PositionEffect : byte { OPENING, CLOSING, AUTOMATIC }
            public enum AssetType : byte { EQUITY, MUTUAL_FUND, OPTION, FIXED_INCOME, CASH_EQUIVALENT }
        }
        public Enums.Type? Type { get; set; }
        public string? ClearingReferenceNumber { get; set; }
        public string? SubAccount { get; set; }
        public string? SettlementDate { get; set; }
        public string? OrderId { get; set; }
        public double? Sma { get; set; }
        public double? RequirementReallocationAmount { get; set; }
        public double? DayTradeBuyingPowerEffect { get; set; }
        public double? NetAmount { get; set; }
        public string? TransactionDate { get; set; }
        public string? OrderDate { get; set; }
        public string? TransactionSubType { get; set; }
        public long? TransactionId { get; set; }
        public bool? CashBalanceEffectFlag { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, double>? Fees { get; set; }
        public Structs.TransactionItem? TransactionItem { get; set; }
        public Enums.AchievedStatus? AchStatus { get; set; }
        public double? AccruedInterest { get; set; }
    }
}
