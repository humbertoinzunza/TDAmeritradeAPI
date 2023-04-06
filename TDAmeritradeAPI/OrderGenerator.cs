using static TDAmeritradeAPI.OrderGenerator;

namespace TDAmeritradeAPI
{
    public class OrderGenerator
    {
        public enum OptionType { Call, Put }
        public enum PositionEffect { ToOpen, ToClose }
        public enum NetEffect { NetDebit, NetCredit, NetZero };
        private enum SpreadType { Vertical, Horizontal, Custom }

        private static readonly Order.Enums.Instruction[] _validEquityInstruction = { Order.Enums.Instruction.BUY, Order.Enums.Instruction.SELL,
            Order.Enums.Instruction.BUY_TO_COVER, Order.Enums.Instruction.SELL_SHORT };

        private static readonly Order.Enums.Instruction[] _validOptionInstruction = { Order.Enums.Instruction.BUY_TO_OPEN, Order.Enums.Instruction.SELL_TO_CLOSE,
            Order.Enums.Instruction.BUY_TO_CLOSE, Order.Enums.Instruction.SELL_TO_CLOSE };

        private enum OrderError { InvalidSymbol, InvalidInstruction, InvalidPrice, InvalidQuantity, InvalidSpread, InvalidVerticalSpread, InvalidHorizontalSpread }

        /// <summary>
        /// Object representation of an options symbol with format 'ABC_MMddyyC0.0'.
        /// Make sure to call the constructor with 
        /// </summary>
        public class Option
        {
            private readonly string _underlying;
            private readonly DateTime _expiration;
            private readonly OptionType _type;
            private readonly double _strikePrice;
            private readonly string _symbol;
            public Option(string symbol)
            {
                try
                {
                    string[] splitSymbol = symbol.Split('_');
                    _underlying = splitSymbol[0];
                    _expiration = DateTime.ParseExact(splitSymbol[1][..6], "MMddyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (splitSymbol[1][6] == 'C')
                        _type = OptionType.Call;
                    else if (splitSymbol[1][6] == 'P')
                        _type = OptionType.Put;
                    else
                        throw new FormatException("The option is neither a put nor a call.");
                    splitSymbol[0] = splitSymbol[1][7..]; // Put the strike price into splitSymbol[1]
                    _strikePrice = Convert.ToDouble(splitSymbol[0]);
                    _symbol = symbol;
                }
                catch (Exception ex)
                {
                    if (ex is IndexOutOfRangeException || ex is FormatException)
                        Console.WriteLine("The symbol passed to the option's constructor was not in the correct format.");
                    throw;
                }
            }
            public string Underlying
            {
                get { return _underlying; }
            }

            public DateTime Expiration
            {
                get { return _expiration; }
            }
            public OptionType Type
            {
                get { return _type; }
            }
            public double StrikePrice
            {
                get { return _strikePrice; }
            }
            public string Symbol
            {
                get { return _symbol; }
            }
        }

        /// <summary>
        /// Verifies whether the order's instruction is valid given the current type of security.
        /// Throws an ArgumentException if the order's instruction is not valid given the current type of security.
        /// </summary>
        /// <param name="securityType">The type of security.</param>
        /// <param name="instruction">The instruction for the order.</param>
        /// <remarks>The valid instructions for either stock or options are defined in https://developer.tdameritrade.com/content/place-order-samples.</remarks>
        private static void ValidateInstruction(Order.Enums.SecurityType securityType, Order.Enums.Instruction instruction)
        {
            if (securityType == Order.Enums.SecurityType.Equity)
                if (!_validEquityInstruction.Contains(instruction))
                    throw new ArgumentException($"Error. The only valid instructions for equities are {string.Join(',', _validEquityInstruction)}.",
                        nameof(instruction));
            else if (securityType == Order.Enums.SecurityType.Option)
                if (!_validOptionInstruction.Contains(instruction))
                    throw new ArgumentException($"Error. The only valid instructions for options are {string.Join(',', _validOptionInstruction)}.",
                        nameof(instruction));
        }

        /// <summary>
        /// Verifies that the price passed in is not a negative number.
        /// Throws an ArgumentOutOfRangeException if the price is both not null and not greater than zero.
        /// </summary>
        /// <param name="price">The price to verify.</param>
        private static void ValidatePrice(double? price)
        {
            if (price != null && price < 0.0)
                throw new ArgumentOutOfRangeException(nameof(price), "Error. The price must be a nonnegative number.");
        }

        /// <summary>
        /// Verifies that the quantity is greater than zero.
        /// Throws an ArgumentOutOfRangeException if the quantity is not greater than zero.
        /// </summary>
        /// <param name="quantity">The quantity in question.</param>
        private static void ValidateQuantity(uint quantity)
        {
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity), "Error. The order's quantity must be a non-zero positive integer.");
        }

        /// <summary>
        /// Verifies that the options' underlyings in a spread match.
        /// Throws an ArgumentException if the spread is invalid.
        /// </summary>
        /// <param name="longOption">Option object that describes the option contract that will be opened as
        /// a long position.</param>
        /// <param name="shortOption">Option object that describes the option contract that will be opened as
        /// a short position.</param>
        /// <param name="spreadType">Indicates the type of spread that is being verified.</param>
        private static void ValidateSpread(Option longOption, Option shortOption, SpreadType spreadType)
        {
            if (longOption.Underlying != shortOption.Underlying)
                throw new ArgumentException("Error. Invalid spread. The underlying of the two options doesn't match.");
            if (spreadType == SpreadType.Vertical)
                // Vertical spreads must have different strike prices and same expiration date
                if (longOption.StrikePrice == shortOption.StrikePrice || longOption.Expiration.Date != shortOption.Expiration.Date)
                    throw new ArgumentException("Error. Invalid vertical spread." +
                        " The strike prices must be different and the expiration dates must match.");
            else if (spreadType == SpreadType.Horizontal)
                // Horizontal spreads must have same strike prices and different expiration date
                if (longOption.StrikePrice != shortOption.StrikePrice || longOption.Expiration.Date == shortOption.Expiration.Date)
                    throw new ArgumentException("Error. Invalid vertical spread." +
                        " The strike prices must match and the expiration dates must be different.");
        }

        /// <summary>
        /// Verifies that the symbol passed in is of the expected type (equity or option).
        /// Throws an ArgumentException if it fails to verify the symbol.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="expectedType">The expected security type of the symbol.</param>
        private static void ValidateSymbol(ref string symbol, Order.Enums.SecurityType expectedType)
        {
            Order.Enums.SecurityType symbolType = Client.ParseSymbol(ref symbol);
            if (symbolType != expectedType)
                throw new ArgumentException("Error. The symbol passed is not valid for this type of order.");
        }

        /// <summary>
        /// Creates an order to buy an equity.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">How many units to buy.</param>
        /// <param name="positionEffect">Whether this is a simple buy or to cover a short position.</param>
        /// <param name="price">Limit price. Set to null to perform a market order.</param>
        /// <param name="duration">Duration of the order.</param>
        /// <returns>An order to buy an equity.</returns>
        public static Order BuyEquity(string symbol, uint quantity, PositionEffect positionEffect, double? price = null,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            Order.Enums.Instruction instruction;
            if (positionEffect == PositionEffect.ToOpen)
                instruction = Order.Enums.Instruction.BUY;
            else
                instruction = Order.Enums.Instruction.BUY_TO_COVER;

            return BasicOrder(symbol, Order.Enums.SecurityType.Equity, quantity, instruction, price, duration);
        }

        /// <summary>
        /// Creates an order to buy an equity.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">How many units to buy.</param>
        /// <param name="positionEffect">Whether this is a simple sell or to start a short position.</param>
        /// <param name="price">Limit price. Set to null to perform a market order.</param>
        /// <param name="duration">Duration of the order.</param>
        /// <returns>An order to buy an equity.</returns>
        public static Order SellEquity(string symbol, uint quantity, PositionEffect positionEffect, double? price = null,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            Order.Enums.Instruction instruction;
            if (positionEffect == PositionEffect.ToOpen)
                instruction = Order.Enums.Instruction.SELL_SHORT;
            else
                instruction = Order.Enums.Instruction.SELL;

            return BasicOrder(symbol, Order.Enums.SecurityType.Equity, quantity, instruction, price, duration);
        }

        /// <summary>
        /// Creates an order to buy an option.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">How many units to buy.</param>
        /// <param name="positionEffect">Whether this is a buy that will open or close a position.</param>
        /// <param name="price">Limit price. Set to null to perform a market order.</param>
        /// <param name="duration">Duration of the order.</param>
        /// <returns>An order to buy an equity.</returns>
        public static Order BuyOption(string symbol, uint quantity, PositionEffect positionEffect, double? price = null,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            Order.Enums.Instruction instruction;
            if (positionEffect == PositionEffect.ToOpen)
                instruction = Order.Enums.Instruction.BUY_TO_OPEN;
            else
                instruction = Order.Enums.Instruction.BUY_TO_CLOSE;

            return BasicOrder(symbol, Order.Enums.SecurityType.Option, quantity, instruction, price, duration);
        }

        /// <summary>
        /// Creates an order to sell an option.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">How many units to buy.</param>
        /// <param name="positionEffect">Whether this is a sell that will open or close a position.</param>
        /// <param name="price">Limit price. Set to null to perform a market order.</param>
        /// <param name="duration">Duration of the order.</param>
        /// <returns>An order to buy an equity.</returns>
        public static Order SellOption(string symbol, uint quantity, PositionEffect positionEffect, double? price = null,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            Order.Enums.Instruction instruction;
            if (positionEffect == PositionEffect.ToOpen)
                instruction = Order.Enums.Instruction.SELL_TO_OPEN;
            else
                instruction = Order.Enums.Instruction.SELL_TO_CLOSE;

            return BasicOrder(symbol, Order.Enums.SecurityType.Option, quantity, instruction, price, duration);
        }

        /// <summary>
        /// Creates a basic spread Order.
        /// </summary>
        /// <param name="longSymbol">The option that is long in the spread.</param>
        /// <param name="shortSymbol">The option that is short in the spread.</param>
        /// <param name="quantity1">Units of long symbol.</param>
        /// <param name="quantity2">Units of short symbol.</param>
        /// <param name="positionEffect">Determines if it's opening or closing a position.</param>
        /// <param name="duration">The duration of the order.</param>
        /// <param name="net">It indicates if the order is net debit, net credit, or net zero. Set to null for a market order.</param>
        /// <returns>An order for a spread.</returns>
        /// <remarks>The order is incomplete and must be personalized based on the type of spread desired.</remarks>
        private static Order CreateSpread(Option longOption, uint longQuantity, Option shortOption, uint shortQuantity, PositionEffect positionEffect,
            SpreadType spreadType, Order.Enums.Duration duration = Order.Enums.Duration.DAY, NetEffect? netEffect = null, double? price = null)
        {
            if (price != null && netEffect == null)
                throw new ArgumentException("Error. If price is not null, then net effect cannot be also null.");

            // Verify that quantities are valid
            ValidateQuantity(longQuantity);
            ValidateQuantity(shortQuantity);
            // Check that it is a valid spread
            ValidateSpread(longOption, shortOption, spreadType);

            Order.Enums.OrderType? net;
            if (netEffect == null)
                net = null;
            else if (netEffect == NetEffect.NetDebit)
                net = Order.Enums.OrderType.NET_DEBIT;
            else if (netEffect == NetEffect.NetCredit)
                net = Order.Enums.OrderType.NET_CREDIT;
            else
                net = Order.Enums.OrderType.NET_ZERO;

            Order.Enums.Instruction instruction1, instruction2;
            if (positionEffect == PositionEffect.ToOpen)
            {
                instruction1 = Order.Enums.Instruction.BUY_TO_OPEN;
                instruction2 = Order.Enums.Instruction.SELL_TO_OPEN;
            }
            else
            {
                instruction1 = Order.Enums.Instruction.SELL_TO_CLOSE;
                instruction2 = Order.Enums.Instruction.BUY_TO_CLOSE;
            }
            Order order = new()
            {
                OrderType = net ?? Order.Enums.OrderType.MARKET,
                Price = price,
                OrderStrategyType = Order.Enums.OrderStrategyType.SINGLE,
                Duration = duration,
                Session = Order.Enums.Session.NORMAL,
                OrderLegCollection = new List<Order.OrderLeg>()
                {
                    new Order.OrderLeg(instruction1, longQuantity, new Instrument(longOption.Symbol, Instrument.AssetTypes.OPTION)),
                    new Order.OrderLeg(instruction2, shortQuantity, new Instrument(shortOption.Symbol, Instrument.AssetTypes.OPTION))
                }
            };
            return order;
        }


        /// <summary>
        /// Creates a basic order for an equity or an option.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">Indicates how many will be bought or sold.</param>
        /// <param name="instruction">The type of order that will be executed. Valid options for equities are BUY, SELL, BUY_TO_COVER, and SELL_SHORT.
        /// Valid options for options are BUY_TO_OPEN, BUY_TO_CLOSE, SELL_TO_OPEN, and SELL_TO_CLOSE.</param>
        /// <param name="price">The price at which to set the order. Set this to null to performa a market order.</param>
        /// <param name="duration">Indicates the order duration.</param>
        /// <returns>An order object for an equity.</returns>
        private static Order BasicOrder(string symbol, Order.Enums.SecurityType securityType, uint quantity, Order.Enums.Instruction instruction, double? price,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            ValidateSymbol(ref symbol, securityType);
            ValidateQuantity(quantity);
            ValidateInstruction(securityType, instruction);
            ValidatePrice(price);

            Order order = new();
            if (price == null)
                order.OrderType = Order.Enums.OrderType.MARKET;
            else
            {
                order.OrderType = Order.Enums.OrderType.LIMIT;
                order.Price = price;
            }
            order.Session = Order.Enums.Session.NORMAL;
            order.Duration = duration;
            order.OrderStrategyType = Order.Enums.OrderStrategyType.SINGLE;
            order.OrderLegCollection = new List<Order.OrderLeg>()
            {
                new Order.OrderLeg()
            };
            order.OrderLegCollection[0].Instruction = instruction;
            order.OrderLegCollection[0].Quantity = quantity;
            order.OrderLegCollection[0].Instrument =
                new Instrument(symbol, securityType == Order.Enums.SecurityType.Equity ? Instrument.AssetTypes.EQUITY : Instrument.AssetTypes.OPTION);

            return order;
        }

        public static Order TradeVerticalSpread(Option longOption, uint longQuantity, Option shortOption, uint shortQuantity,
            PositionEffect positionEffect, Order.Enums.Duration duration = Order.Enums.Duration.DAY, double? price = null, NetEffect? netEffect = null)
        {
            if (price != null && netEffect == null)
                throw new ArgumentException("Error. If price is not null, then net effect cannot be also null.");

            Order.Enums.OrderType? net;
            if (netEffect == null)
                net = null;
            else if (netEffect == NetEffect.NetDebit)
                net = Order.Enums.OrderType.NET_DEBIT;
            else if (netEffect == NetEffect.NetCredit)
                net = Order.Enums.OrderType.NET_CREDIT;
            else
                net = Order.Enums.OrderType.NET_ZERO;

            return CreateSpread(longOption, longQuantity, shortOption, shortQuantity, positionEffect, SpreadType.Vertical, duration, net, price);
        }
    }
}
