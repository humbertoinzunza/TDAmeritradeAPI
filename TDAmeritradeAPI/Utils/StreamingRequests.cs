namespace TDAmeritradeAPI.Utils
{
    internal class StreamingRequests
    {
        public StreamingRequests(StreamingRequest[] requests) 
        {
            Requests = requests;
        }
        public StreamingRequest[] Requests { get; set; }
    }
    internal abstract class StreamingRequest
    {
        public StreamingRequest(string service, byte requestId)
        {
            Service = service;
            RequestId = requestId;
        }
        public string Service { get; }
        public int RequestId { get; private set; }
    }
    internal class AdminRequest : StreamingRequest
    {
        public class Structs
        {
            public struct Credential
            {
                public string UserId { get; set; }
                public string Token { get; set; }
                public string Company { get; set; }
                public string Segment { get; set; }
                public string CdDomain { get; set; }
                public string UserGroup { get; set; }
                public string AccessLevel { get; set; }
                public char Authorized { get; set; }
                public double Timestamp { get; set; }
                public string AppId { get; set; }
                public string Acl { get; set; }
            }
            public struct Parameters
            {
                public Credential Credential { get; set; }
                public string Token { get; set; }
                public string Version { get; set; }
            }
        }
        public class Enums
        {
            public enum Command : byte { LOGIN, LOGOUT, QOS }
        }
        public AdminRequest(Enums.Command command, byte requestId, string account, string source, Structs.Parameters? parameters) : base("ADMIN", requestId)
        {
            Command = command;
            Account = account;
            Source = source;
            Parameters = parameters;
        }
        public Enums.Command Command { get; set; }
        public string Account { get; set; }
        public string Source { get; set; }
        public Structs.Parameters? Parameters { get; set; }        
    }
}
