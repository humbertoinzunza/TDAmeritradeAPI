namespace TDAmeritradeAPI.DataModels
{
    public class Mover
    {
        public class Enums
        {
            public enum Direction : byte { UP, DOWN }
            public enum ValidIndex : byte { DOW_JONES, NASDAQ_COMPOSITE, SP500 }
            public enum ChangeType : byte { VALUE, PERCENT }
        }
        public double? Change { get; set; }
        public string? Description { get; set; }
        public Enums.Direction? Direction { get; set; }
        public double? Last { get; set; }
        public string? Symbol { get; set; }
        public long? TotalVolume { get; set; }
    }
}
