using System.Text.Json;
using System.Text.Json.Serialization;

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
            public enum OptionTradingLevel : byte { COVERED, FULL, LONG, SPREAD, NONE }
        }
        public bool? Apex { get; set; }
        public bool? LevelTwoQuotes { get; set; }
        public bool? StockTrading { get; set; }
        public bool? MarginTrading { get; set; }
        public bool? StreamingNews { get; set; }
        public Enums.OptionTradingLevel? OptionTradingLevel { get; set; }
        public bool? StreamerAccess { get; set; }
        public bool? AdvancedMargin { get; set; }
        public bool? ScottradeAccount { get; set; }
    }

    public class Preferences
    {
        // Enums
        public class Enums
        {
            public enum EquityOrderLegInstruction : byte { BUY, SELL, BUY_TO_COVER, SELL_SHORT, NONE };

            public enum EquityOrderType : byte { MARKET, LIMIT, STOP, STOP_LIMIT, TRAILING_STOP, MARKET_ON_CLOSE, NONE }

            public enum EquityOrderPriceLinkType : byte { VALUE, PERCENT, NONE }

            public enum EquityOrderDuration : byte { DAY, GOOD_TILL_CANCEL, NONE }

            public enum EquityOrderMarketSession : byte { AM, PM, NORMAL, SEAMLESS, NONE }

            public enum TaxLotMethod : byte { FIFO, LIFO, HIGH_COST, LOW_COST, MINIMUM_TAX, AVERAGE_COST, NONE }

            public enum AdvancedToolLaunch : byte { TA, N, Y, TOS, NONE, CC2 }

            public enum AuthTokenTimeout : byte { FIFTY_FIVE_MINUTES, TWO_HOURS, FOUR_HOURS, EIGHT_HOURS }
        }

        // Properties
        public bool? ExpressTrading { get; set; }
        public bool? DirectOptionsRouting { get; set; }
        public bool? DirectEquityRouting { get; set; }
        public Enums.EquityOrderLegInstruction? DefaultEquityOrderLegInstruction { get; set; }
        public Enums.EquityOrderType? DefaultEquityOrderType { get; set; }
        public Enums.EquityOrderPriceLinkType? DefaultEquityOrderPriceLinkType { get; set; }
        public Enums.EquityOrderDuration? DefaultEquityOrderDuration { get; set; }
        public Enums.EquityOrderMarketSession? DefaultEquityOrderMarketSession { get; set; }
        public int? DefaultEquityQuantity { get; set; }
        public Enums.TaxLotMethod? MutualFundTaxLotMethod { get; set; }
        public Enums.TaxLotMethod? OptionTaxLotMethod { get; set; }
        public Enums.TaxLotMethod? EquityTaxLotMethod { get; set; }
        public Enums.AdvancedToolLaunch? DefaultAdvancedToolLaunch { get; set; }
        public Enums.AuthTokenTimeout? AuthTokenTimeout { get; set; }

        public string ToJson()
        {
            JsonSerializerOptions _serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
            return JsonSerializer.Serialize(this, _serializerOptions);
        }
    }

    public class UserPrincipal
    {
        // Enums
        public class Enums
        {
            public enum AdditionalField { StreamerSubscriptionKeys, StreamerConnectionInfo, Preferences, SurrogateIds }

            public enum ProfessionalStatusTypes { PROFESSIONAL, NON_PROFESSIONAL, UNKNOWN_STATUS }
        }
        // Structs
        public class Structs
        {
            public struct StreamerSubscriptionKey
            {
                public string? Key { get; set; }
            }
            public struct StreamerInfo
            {
                public string? StreamerBinaryUrl { get; set; }
                public string? StreamerSocketUrl { get; set; }
                public string? Token { get; set; }
                public string? TokenTimestamp { get; set; }
                public string? UserGroup { get; set; }
                public string? AccessLevel { get; set; }
                public string? Acl { get; set; }
                public string? AppId { get; set; }
            }
            public struct SubscriptionKeys
            {
                public StreamerSubscriptionKey[] Keys { get; set; }
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
        public bool? StalePassword { get; set; }
        public Structs.StreamerInfo? StreamerInfo { get; set; }
        public Enums.ProfessionalStatusTypes? ProfessionalStatus { get; set; }
        public Dictionary<string, bool>? Quotes { get; set; }
        public Structs.SubscriptionKeys? StreamerSubscriptionKeys { get; set; }
        public Account[]? Accounts { get; set; }
    }
}