
namespace TDAmeritradeAPI
{
    public class PriceHistory
    {
        public List<Candle>? Candles { get; set; }
        public bool? Empty { get; set; }
        public string? Symbol { get; set; }
    }

    public struct Candle
    {
        public double Close { get; set; }
        public long Datetime { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public long Volume { get; set; }
    }

    /// <summary>
    /// Class that represents the options available for the enpoint
    /// https://api.tdameritrade.com/v1/marketdata/{symbol}/pricehistory.
    /// </summary>
    public class PriceHistoryOptions
    {
        // Enums
        public enum FrequencyTypes { Minute, Daily, Weekly, Monthly };
        public enum PeriodTypes { Day, Month, Year, YTD };

        // Class fields
        public int EndDate { get; }
        public int Frequency { get; }
        public FrequencyTypes FrequencyType { get; }
        public bool NeedExtendedHoursData { get; }
        public int Period { get; set; }
        public PeriodTypes PeriodType { get; }
        public int StartDate { get; }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="periodType">Period type.</param>
        /// <param name="period">Period amount.</param>
        /// <param name="frequencyType">Frequency type.</param>
        /// <param name="frequency">Frequency amount.</param>
        /// <param name="needExtendedHoursData">Establishes whether extended hours data will be required or not.</param>
        public PriceHistoryOptions(PeriodTypes periodType, int period, FrequencyTypes frequencyType,
            int frequency, bool needExtendedHoursData = true)
        {
            ValidatePeriod(periodType, period);
            PeriodType = periodType;
            Period = period;
            ValidateFrequencyType(periodType, frequencyType);
            FrequencyType = frequencyType;
            ValidateFrequency(frequencyType, frequency);
            Frequency = frequency;
            NeedExtendedHoursData = needExtendedHoursData;
            // Default values to know they weren't initialized
            StartDate = -1;
            EndDate = -1;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="periodType">Period type.</param>
        /// <param name="frequencyType">Frequency type.</param>
        /// <param name="frequency">Frequency amount.</param>
        /// <param name="startDate">Start date of the data in milliseconds since epoch.</param>
        /// <param name="endDate">End date of the data in milliseconds since epoch.</param>
        /// <param name="needExtendedHoursData">Establishes whether extended hours data will be required or not.</param>
        public PriceHistoryOptions(PeriodTypes periodType, FrequencyTypes frequencyType, int frequency,
            int startDate, int endDate, bool needExtendedHoursData = true)
        {
            PeriodType = periodType;
            ValidateFrequencyType(periodType, frequencyType);
            FrequencyType = frequencyType;
            ValidateFrequency(frequencyType, frequency);
            Frequency = frequency;
            NeedExtendedHoursData = needExtendedHoursData;
            ValidateDates(startDate, endDate);
            StartDate = startDate;
            EndDate = endDate;
            // Default value to know it wasn't initialized.
            Period = -1;
        }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> result = new();
            // If this is true, then the user is not using these two parameters
            if (StartDate == -1 || EndDate == -1)
            {
                result["periodType"] = PeriodType.ToString().ToLower();
                result["period"] = Period.ToString();
                result["frequencyType"] = FrequencyType.ToString().ToLower();
                result["frequency"] = Frequency.ToString();
                result["needExtendedHoursData"] = NeedExtendedHoursData.ToString().ToLower();
            }
            // If this is true, then the user is not using this parameter and is instead using
            // StartDate and EndDate.
            else if (Period == -1)
            {
                result["periodType"] = PeriodType.ToString().ToLower();
                result["frequencyType"] = FrequencyType.ToString().ToLower();
                result["frequency"] = Frequency.ToString();
                result["endDate"] = EndDate.ToString();
                result["startDate"] = StartDate.ToString();
                result["needExtendedHoursData"] = NeedExtendedHoursData.ToString().ToLower();
            }
            return result;
        }

        /// <summary>
        /// Validates the startDate and endDate inputs.
        /// </summary>
        /// <param name="startDate">The start date in milliseconds since epoch.</param>
        /// <param name="endDate">The end date in milliseconds since epoch.</param>
        /// <exception cref="Exception">Generic exception thrown when the end date is not larger than the start date
        /// or if one of the dates is less than zero.</exception>
        private static void ValidateDates(int startDate, int endDate)
        {
            if (startDate < 0 || endDate < 0)
                throw new Exception("Both the start and the end date must be greater than zero.");
            if (startDate >= endDate)
                throw new Exception("The end date must be greater than the start date.");
        }

        /// <summary>
        /// Verifies that the period type and period amount combination are valid.
        /// </summary>
        /// <param name="periodType">The perdiod type.</param>
        /// <param name="period">The period amount.</param>
        /// <exception cref="Exception">Generic exception thrown if the period type and period amount
        /// combination is not valid or when the frequency type is invalid.</exception>
        private static void ValidateFrequency(FrequencyTypes frequencyType, int frequency)
        {
            string errorMessage = $"The frequency type '{frequencyType}' and frequency amount" +
                $" '{frequency}' combination is invalid.";
            bool isInvalid = false;
            int[] validFrequencies;
            if (frequencyType == FrequencyTypes.Minute)
            {
                validFrequencies = new int[] { 1, 5, 10, 15, 30 };
                if (!validFrequencies.Contains(frequency))
                    isInvalid = true;
            }
            else if (frequencyType == FrequencyTypes.Daily || frequencyType == FrequencyTypes.Weekly ||
                frequencyType == FrequencyTypes.Monthly)
                if (frequency != 1)
                    isInvalid = true;
                else
                    throw new Exception("The frequency type is not valid.");
            if (isInvalid) throw new Exception(errorMessage);
        }

        /// <summary>
        /// Verifies that the period type and frequency type combination are valid.
        /// </summary>
        /// <param name="periodType">The period type.</param>
        /// <param name="frequencyType">The frequency type.</param>
        /// <exception cref="Exception">Generic exception that is thrown when the period type
        /// and the frequency type are not compatible or when the period type is invalid.</exception>
        private static void ValidateFrequencyType(PeriodTypes periodType, FrequencyTypes frequencyType)
        {
            string errorMessage = $"The period type '{periodType}' and frequency type" +
                $" '{frequencyType}' combination is invalid.";
            bool isInvalid = false;
            if (periodType == PeriodTypes.Day)
            {
                if (frequencyType != FrequencyTypes.Minute)
                    isInvalid = true;
            }
            else if (periodType == PeriodTypes.Month || periodType == PeriodTypes.YTD)
            {
                if (frequencyType != FrequencyTypes.Daily && frequencyType != FrequencyTypes.Weekly)
                    isInvalid = true;
            }
            else if (periodType == PeriodTypes.Year)
            {
                if (frequencyType != FrequencyTypes.Daily || frequencyType != FrequencyTypes.Weekly ||
                    frequencyType != FrequencyTypes.Monthly)
                    isInvalid = true;
            }
            else
                throw new Exception("The period type is not valid.");

            if (isInvalid) throw new Exception(errorMessage);

        }

        /// <summary>
        /// Verifies that the period type and period amount combination are valid.
        /// </summary>
        /// <param name="periodType">The perdiod type.</param>
        /// <param name="period">The period amount.</param>
        /// <exception cref="Exception">Generic exception thrown if the period type and period amount
        /// combination is not valid or when the period type is invalid.</exception>
        private static void ValidatePeriod(PeriodTypes periodType, int period)
        {
            string errorMessage = $"The period type '{periodType}' and period amount" +
                $" '{period}' combination is invalid.";
            bool isInvalid = false;
            int[] validPeriods;
            switch (periodType)
            {
                case PeriodTypes.Day:
                    validPeriods = new int[] { 1, 2, 3, 4, 5, 10 };
                    if (!validPeriods.Contains(period))
                        isInvalid = true;
                    break;
                case PeriodTypes.Month:
                    validPeriods = new int[] { 1, 2, 3, 6 };
                    if (!validPeriods.Contains(period))
                        isInvalid = true;
                    break;
                case PeriodTypes.Year:
                    validPeriods = new int[] { 1, 2, 3, 5, 10, 15, 20 };
                    if (!validPeriods.Contains(period))
                        isInvalid = true;
                    break;
                case PeriodTypes.YTD:
                    validPeriods = new int[] { 1 };
                    if (!validPeriods.Contains(period))
                        isInvalid = true;
                    break;
                default:
                    throw new Exception("The period type is not valid.");
            }
            if (isInvalid) throw new Exception(errorMessage);
        }
    }
}
