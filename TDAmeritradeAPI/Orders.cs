using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TDAmeritradeAPI
{
    public class Order : ICloneable
    {
        public class Enums
        {
            // This enum is only for the OrderGenerator class to know if the symbol in question is from an equity, an option, or something else
            public enum SecurityType { Equity, Option, Other }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Session { NORMAL, AM, PM, SEAMLESS }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Duration { DAY, GOOD_TILL_CANCEL, FILL_OR_KILL }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum OrderType
            {
                MARKET, LIMIT, STOP, STOP_LIMIT, TRAILING_STOP, MARKET_ON_CLOSE,
                EXERCISE, TRAILING_STOP_LIMIT, NET_DEBIT, NET_CREDIT, NET_ZERO
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum ComplexOrderStrategyType
            {
                NONE, COVERED, VERTICAL, BACK_RATIO, CALENDAR, DIAGONAL,
                STRADDLE, STRANGLE, COLLAR_SYNTHETIC, BUTTERFLY, CONDOR, IRON_CONDOR, VERTICAL_ROLL,
                COLLAR_WITH_STOCK, DOUBLE_DIAGONAL, UNBALANCED_BUTTERFLY, UNBALANCED_CONDOR, UNBALANCED_IRON_CONDOR,
                UNBALANCED_VERTICAL_ROLL, CUSTOM
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum RequestedDestination { INET, ECN_ARCA, CBOE, AMEX, PHLX, ISE, BOX, NYSE, NASDAQ, BATS, C2, AUTO }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum PriceLinkBasis { MANUAL, BASE, TRIGGER, LAST, BID, ASK, ASK_BID, MARK, AVERAGE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum PriceLinkType { VALUE, PERCENT, TICK }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum StopType { STANDARD, BID, ASK, LAST, MARK }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum TaxLotMethod { FIFO, LIFO, HIGH_COST, LOW_COST, AVERAGE_COST, SPECIFIC_LOT }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum OrderLegType { EQUITY, OPTION, INDEX, MUTUAL_FUND, CASH_EQUIVALENT, FIXED_INCOME, CURRENCY }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Instruction
            {
                BUY, SELL, BUY_TO_COVER, SELL_SHORT, BUY_TO_OPEN, BUY_TO_CLOSE, SELL_TO_OPEN,
                SELL_TO_CLOSE, EXCHANGE
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum PositionEffect { OPENING, CLOSING, AUTOMATIC }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum QuantityType { ALL_SHARES, DOLLARS, SHARES }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum SpecialInstruction { ALL_OR_NONE, DO_NOT_REDUCE, ALL_OR_NONE_DO_NOT_REDUCE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum OrderStrategyType { SINGLE, OCO, TRIGGER }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Status
            {
                AWAITING_PARENT_ORDER, AWAITING_CONDITION, AWAITING_MANUAL_REVIEW, ACCEPTED,
                AWAITING_UR_OUT, PENDING_ACTIVATION, QUEUED, WORKING, REJECTED, PENDING_CANCEL, CANCELED,
                PENDING_REPLACE, REPLACED, FILLED, EXPIRED
            }
        }

        // Structs
        public class OrderLeg
        {
            public OrderLeg() { }
            public OrderLeg(Enums.Instruction instruction, double quantity, Instrument instrument)
            {
                Instruction = instruction;
                Quantity = quantity;
                Instrument = instrument;
            }
            public OrderLeg(Enums.OrderLegType? orderLegType, long? legId, Instrument? instrument, Enums.Instruction? instruction,
                Enums.PositionEffect? positionEffect, double? quantity, Enums.QuantityType? quantityType)
            {
                OrderLegType = orderLegType;
                LegId = legId;
                Instrument = instrument;
                Instruction = instruction;
                PositionEffect = positionEffect;
                Quantity = quantity;
                QuantityType = quantityType;
            }

            public Enums.OrderLegType? OrderLegType { get; set; }
            public long? LegId { get; set; }
            public Instrument? Instrument { get; set; }
            public Enums.Instruction? Instruction { get; set; }
            public Enums.PositionEffect? PositionEffect { get; set; }
            public double? Quantity { get; set; }
            public Enums.QuantityType? QuantityType { get; set; }
        }

        // Private variables
        private static readonly JsonSerializerSettings _serializerSettings = new();

        // Properties
        public Enums.Session? Session { get; set; }
        public Enums.Duration? Duration { get; set; }
        public Enums.OrderType? OrderType { get; set; }
        public string? CancelTime { get; set; }
        public Enums.ComplexOrderStrategyType? ComplexOrderStrategyType { get; set; }
        public double? Quantity { get; set; }
        public double? FilledQuantity { get; set; }
        public double? RemainingQuantity { get; set; }
        public Enums.RequestedDestination? RequestedDestination { get; set; }
        public string? DestinationLinkName { get; set; }
        public string? ReleaseTime { get; set; }
        public double? StopPrice { get; set; }
        public Enums.PriceLinkBasis? StopPriceLinkBasis { get; set; }
        public Enums.PriceLinkType? StopPriceLinkType { get; set; }
        public double? StopPriceOffset { get; set; }
        public Enums.StopType? StopType { get; set; }
        public Enums.PriceLinkBasis? PriceLinkBasis { get; set; }
        public Enums.PriceLinkType? PriceLinkType { get; set; }
        public double? Price { get; set; }
        public Enums.TaxLotMethod? TaxLotMethod { get; set; }
        public List<OrderLeg>? OrderLegCollection { get; set; }
        public double? ActivationPrice { get; set; }
        public Enums.SpecialInstruction? SpecialInstruction { get; set; }
        public Enums.OrderStrategyType? OrderStrategyType { get; set; }
        public long? OrderId { get; set; }
        public bool? Cancelable { get; set; }
        public bool? Editable { get; set; }
        public Enums.Status? Status { get; set; }
        public string? EnteredTime { get; set; }
        public string? CloseTime { get; set; }
        public string? Tag { get; set; }
        public long? AccountId { get; set; }
        public List<Execution>? OrderActivityCollection { get; set; }
        public List<object>? ReplacingOrderCollection { get; set; }
        public List<object>? ChildOrderStrategies { get; set; }
        public string? StatusDescription { get; set; }
        public Order()
        {
            _serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }
        public string AsJson()
        {
            string jsonString = JsonConvert.SerializeObject(this, _serializerSettings);
            return jsonString;
        }
        public object Clone()
        {
            Order clone = (Order)MemberwiseClone();
            clone.OrderLegCollection = OrderLegCollection is null ? null : new List<OrderLeg>(OrderLegCollection);
            clone.OrderActivityCollection = OrderActivityCollection is null ? null : new List<Execution>(OrderActivityCollection);
            clone.ReplacingOrderCollection = ReplacingOrderCollection is null ? null : new List<object>(ReplacingOrderCollection);
            clone.ChildOrderStrategies = ChildOrderStrategies is null ? null : new List<object>(ChildOrderStrategies);
            return clone;
        }
        internal static Order GetChildOrderStrategy(Order order)
        {
            Order newOrder = new() {
                OrderType = order.OrderType,
                Session = order.Session,
                Price = order.Price,
                Duration = order.Duration,
                OrderStrategyType = order.OrderStrategyType,
                OrderLegCollection = order.OrderLegCollection is not null ? new List<OrderLeg>(order.OrderLegCollection) : null
        };
            
            return newOrder;
        }
    }

    public class Execution
    {
        //Enums
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum ActivityType { EXECUTION, ORDER_ACTION }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum ExecutionType { FILL }
        }
        // Structs
        public class Structs
        {
            public struct ExecutionLeg
            {
                public int? LegId { get; set; }
                public double? Quantity { get; set; }
                public double? MismarkedQuantity { get; set; }
                public double? Price { get; set; }
                public string? Time { get; set; }
            }
        }
        public Enums.ActivityType? ActivityType { get; set; }
        public Enums.ExecutionType? ExecutionType { get; set; }
        public double? Quantity { get; set; }
        public double? OrderRemainingQuantity { get; set; }
        public List<Structs.ExecutionLeg>? ExecutionLegs { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "AssetType")]
    [JsonSubtypes.KnownSubType(typeof(Equity), AssetTypes.EQUITY)]
    [JsonSubtypes.KnownSubType(typeof(FixedIncome), AssetTypes.FIXED_INCOME)]
    [JsonSubtypes.KnownSubType(typeof(MutualFund), AssetTypes.MUTUAL_FUND)]
    [JsonSubtypes.KnownSubType(typeof(CashEquivalent), AssetTypes.CASH_EQUIVALENT)]
    [JsonSubtypes.KnownSubType(typeof(Option), AssetTypes.OPTION)]
    public class Instrument
    {
        // Enums
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AssetTypes { EQUITY, OPTION, INDEX, MUTUAL_FUND, CASH_EQUIVALENT, FIXED_INCOME, CURRENCY }
        // Properties
        public Instrument() { }
        public Instrument(string? symbol, AssetTypes? assetType)
        {
            Symbol = symbol;
            AssetType = assetType;
        }
        public AssetTypes? AssetType { get; set; }
        public string? Cusip { get; set; }
        public string? Symbol { get; set; }
        public string? Description { get; set; }
    }

    public class Equity : Instrument { }

    public class FixedIncome : Instrument
    {
        // Properties
        public string? MaturityDate { get; set; }
        public double? VariableRate { get; set; }
        public double? Factor { get; set; }
    }

    public class MutualFund : Instrument
    {
        // Enums
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type { NOT_APPLICABLE, OPEN_END_NON_TAXABLE, OPEN_END_TAXABLE, NO_LOAD_NON_TAXABLE, NO_LOAD_TAXABLE }
        }
        // Properties
        public Enums.Type? Type { get; set; }
    }

    public class CashEquivalent : Instrument
    {
        // Enums
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type { SAVINGS, MONEY_MARKET_FUND }
        }
        // Properties
        public Enums.Type? Type { get; set; }
    }

    public class Option : Instrument
    {
        // Enums
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type { VANILLA, BINARY, BARRIER }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum PutCall { PUT, CALL }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum CurrencyType { USD, CAD, EUR, JPY }
        }
        // Structs
        public class Structs
        {
            public struct OptionDeliverable
            {
                public string? Symbol { get; set; }
                public double? DeliverableUnits { get; set; }
                public Enums.CurrencyType? CurrencyType { get; set; }
                public AssetTypes? AssetType { get; set; }
            }
        }
        // Properties
        public Enums.Type? Type { get; set; }
        public Enums.PutCall? PutCall { get; set; }
        public string? UnderlyingSymbol { get; set; }
        public int? OptionMultiplier { get; set; }
        public List<Structs.OptionDeliverable>? OptionDeliverables { get; set; }
    }
}