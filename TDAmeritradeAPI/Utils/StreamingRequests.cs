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
    internal class StreamingRequest
    {
        public StreamingRequest(string service, byte requestId, string command, string account, string source, Dictionary<string, string> parameters)
        {
            Service = service;
            RequestId = requestId;
            Command = command;
            Account = account;
            Source = source;
            Parameters = parameters;
        }
        public string Service { get; }
        public int RequestId { get; private set; }
        public string Command { get; private set; }
        public string Account { get; private set; }
        public string Source { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
    }
}