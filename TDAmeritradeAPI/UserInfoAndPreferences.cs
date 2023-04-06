using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TDAmeritradeAPI
{
    public class Account
    {
        public string? AccountID { get; set; }
        public string? Description { get; set; }
        public string? DisplayName { get; set; }
        public string? AccountCdDomainId { get; set; }
        public string? Company { get; set; }
        public string? Segment { get; set; }
        public Dictionary<string, string>? SurrogateIds { get; set; }
        public Preferences? Preferences { get; set; }
        public string? Acl { get; set; }
        public Authorizations? Authorizations { get; set; }
    }

    public class Authorizations
    {
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum OptionTradingLevel { COVERED, FULL, LONG, SPREAD, NONE }
        }
        public bool Apex { get; set; }
        public bool LevelTwoQuotes { get; set; }
        public bool StockTrading { get; set; }
        public bool MarginTrading { get; set; }
        public bool StreamingNews { get; set; }
        public Enums.OptionTradingLevel OptionTradingLevel { get; set; }
        public bool StreamerAccess { get; set; }
        public bool AdvancedMargin { get; set; }
        public bool ScottradeAccount { get; set; }
    }

    public class Preferences
    {
        // Enums
        public class Enums
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum EquityOrderLegInstruction { BUY, SELL, BUY_TO_COVER, SELL_SHORT, NONE };

            [JsonConverter(typeof(StringEnumConverter))]
            public enum EquityOrderType { MARKET, LIMIT, STOP, STOP_LIMIT, TRAILING_STOP, MARKET_ON_CLOSE, NONE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum EquityOrderPriceLinkType { VALUE, PERCENT, NONE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum EquityOrderDuration { DAY, GOOD_TILL_CANCEL, NONE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum EquityOrderMarketSession { AM, PM, NORMAL, SEAMLESS, NONE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum TaxLotMethod { FIFO, LIFO, HIGH_COST, LOW_COST, MINIMUM_TAX, AVERAGE_COST, NONE }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum AdvancedToolLaunch { TA, N, Y, TOS, NONE, CC2 }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum AuthTokenTimeout { FIFTY_FIVE_MINUTES, TWO_HOURS, FOUR_HOURS, EIGHT_HOURS }
        }

        // Properties
        public bool ExpressTrading { get; set; }
        public bool? DirectOptionsRouting { get; set; }
        public bool? DirectEquityRouting { get; set; }
        public Enums.EquityOrderLegInstruction DefaultEquityOrderLegInstruction { get; set; }
        public Enums.EquityOrderType DefaultEquityOrderType { get; set; }
        public Enums.EquityOrderPriceLinkType DefaultEquityOrderPriceLinkType { get; set; }
        public Enums.EquityOrderDuration DefaultEquityOrderDuration { get; set; }
        public Enums.EquityOrderMarketSession DefaultEquityOrderMarketSession { get; set; }
        public int DefaultEquityQuantity { get; set; }
        public Enums.TaxLotMethod MutualFundTaxLotMethod { get; set; }
        public Enums.TaxLotMethod OptionTaxLotMethod { get; set; }
        public Enums.TaxLotMethod EquityTaxLotMethod { get; set; }
        public Enums.AdvancedToolLaunch DefaultAdvancedToolLaunch { get; set; }
        public Enums.AuthTokenTimeout AuthTokenTimeout { get; set; }
    }

    public class UserPrincipal
    {
        // Enums
        public class Enums
        {
            public enum AdditionalField { StreamerSubscriptionKeys, StreamerConnectionInfo, Preferences, SurrogateIds }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum ProfessionalStatusTypes { PROFESSIONAL, NON_PROFESSIONAL, UNKNOWN_STATUS }
        }
        // Structs
        public class Structs
        {
            public struct Keys
            {
                public string? Key { get; set; }
            }
            public struct StreamerInfo
            {
                public string? StreamerBinaryUrl { get; set; }
                public string? StreamerSocketUrl { get; set; }
                public string? Token { get; set; }

                // ISO 8601 formatted DateTime object
                public string? TokenTimestamp { get; set; }
                public string? UserGroup { get; set; }
                public string? AccessLevel { get; set; }
                public string? Acl { get; set; }
                public string? AppId { get; set; }
            }
            public struct SubscriptionKeys
            {
                public Keys[] Keys { get; set; }
            }
        }
        public string? AuthToken { get; set; }
        public string? UserId { get; set; }
        public string? UserCdDomainId { get; set; }
        public string? PrimaryAccountId { get; set; }
        public string? LastLoginTime { get; set; }
        public string? TokenExpirationTime { get; set; }
        public string? LoginTime { get; set; }
        public string? AccessLevel { get; set; }
        public bool StalePassword { get; set; }
        public Structs.StreamerInfo? StreamerInfo { get; set; }
        public Enums.ProfessionalStatusTypes? ProfessionalStatus { get; set; }
        public Dictionary<string, bool>? Quotes { get; set; }
        public Structs.SubscriptionKeys? StreamerSubscriptionKeys { get; set; }
        public List<Account>? Accounts { get; set; }
    }
}
