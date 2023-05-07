namespace TDAmeritradeAPI
{
    public class Transaction
    {
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
        public Enums.AchievedStatus? AchStatus { get; set; }
        public double? AccruedInterest { get; set; }
    }
}
