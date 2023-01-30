namespace TDAmeritradeAPI
{
    public class OrderGenerator
    {
        public enum PositionEffect { ToOpen, ToClose }

        private static readonly Order.Enums.Instruction[] _validEquityInstruction = { Order.Enums.Instruction.BUY, Order.Enums.Instruction.SELL,
            Order.Enums.Instruction.BUY_TO_COVER, Order.Enums.Instruction.SELL_SHORT };

        private static readonly Order.Enums.Instruction[] _validOptionInstruction = { Order.Enums.Instruction.BUY_TO_OPEN, Order.Enums.Instruction.SELL_TO_CLOSE,
            Order.Enums.Instruction.BUY_TO_CLOSE, Order.Enums.Instruction.SELL_TO_CLOSE };

        private enum OrderError { InvalidSymbol, InvalidInstruction, InvalidPrice, InvalidQuantity }

        /// <summary>
        /// Verifies whether the order's instruction is valid given the current security's type.
        /// </summary>
        /// <param name="securityType">The type of security.</param>
        /// <param name="instruction">The instruction for the order.</param>
        /// <returns>True if the instruction type is valid for the type of security. False otherwise.</returns>
        /// <remarks>The valid instructions for either stock or options are defined in https://developer.tdameritrade.com/content/place-order-samples.</remarks>
        private static bool ValidateInstruction(Order.Enums.SecurityType securityType, Order.Enums.Instruction instruction)
        {
            if (securityType == Order.Enums.SecurityType.Equity)
            {
                if (_validEquityInstruction.Contains(instruction))
                    return true;
                else
                {
                    PrintError(OrderError.InvalidInstruction);
                    return false;
                }
            }
            else if (securityType == Order.Enums.SecurityType.Option)
            {
                if (_validOptionInstruction.Contains(instruction))
                    return true;
                else
                {
                    PrintError(OrderError.InvalidInstruction);
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Verifies that the price passed in is not a negative number.
        /// </summary>
        /// <param name="price">The price to verify.</param>
        /// <returns>True if the price is a positive number or null (for market orders). False otherwise.</returns>
        private static bool ValidatePrice(double? price)
        {
            if (price == null)
                return true;
            else
            {
                if (price < 0.0)
                {
                    PrintError(OrderError.InvalidPrice);
                    return false;
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// Verifies that the quantity is greater than zero.
        /// </summary>
        /// <param name="quantity">The quantity in question.</param>
        /// <returns>True if quantity is greater than zero. False otherwise.</returns>
        private static bool ValidateQuantity(uint quantity)
        {
            if (quantity > 0)
                return true;
            else
            {
                PrintError(OrderError.InvalidQuantity);
                return false;
            }
        }

        /// <summary>
        /// Verifies whether the symbol passed in is of the expected type (equity or option).
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="expectedType">The expected security type of the symbol.</param>
        /// <returns>True if the security's type matches the expected type. False otherwise. </returns>
        private static bool ValidateSymbol(ref string symbol, Order.Enums.SecurityType expectedType)
        {
            Order.Enums.SecurityType symbolType = Client.ParseSymbol(ref symbol);
            if (symbolType == expectedType)
                return true;
            else
            {
                PrintError(OrderError.InvalidSymbol);
                return false;
            } 
        }

        private static void PrintError(OrderError error)
        {
            switch(error)
            {
                case OrderError.InvalidSymbol:
                    Console.WriteLine("Error. The symbol passed is not valid for this type of order.");
                    break;
                case OrderError.InvalidInstruction:
                    Console.WriteLine($"Error. The only valid instructions for equities are {string.Join(',', _validEquityInstruction)}" +
                        $" and for options and spreads are {string.Join(',', _validOptionInstruction)}");
                    break;
                case OrderError.InvalidPrice:
                    Console.WriteLine("Error. The price must be a nonnegative number.");
                    break;
                case OrderError.InvalidQuantity:
                    Console.WriteLine("Error. The quantity must be a number greater than zero.");
                    break;
            }
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
        public static Order? BuyEquity(string symbol, uint quantity, PositionEffect positionEffect, double? price = null, 
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
        public static Order? SellEquity(string symbol, uint quantity, PositionEffect positionEffect, double? price = null,
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
        /// Creates a basic order for an equity or an option.
        /// </summary>
        /// <param name="symbol">The security's symbol.</param>
        /// <param name="quantity">Indicates how many will be bought or sold.</param>
        /// <param name="instruction">The type of order that will be executed. Valid options for equities are BUY, SELL, BUY_TO_COVER, and SELL_SHORT.
        /// Valid options for options are BUY_TO_OPEN, BUY_TO_CLOSE, SELL_TO_OPEN, and SELL_TO_CLOSE.</param>
        /// <param name="price">The price at which to set the order. Set this to null to performa a market order.</param>
        /// <param name="duration">Indicates the order duration.</param>
        /// <returns>An order object for an equity.</returns>
        private static Order? BasicOrder(string symbol, Order.Enums.SecurityType securityType, uint quantity, Order.Enums.Instruction instruction, double? price,
            Order.Enums.Duration duration = Order.Enums.Duration.DAY)
        {
            bool isErrorFree = ValidateSymbol(ref symbol, securityType) & ValidateQuantity(quantity)
                & ValidateInstruction(securityType, instruction) & ValidatePrice(price);

            if (isErrorFree)
            {
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
            else
                return null;
        }
    }
}
