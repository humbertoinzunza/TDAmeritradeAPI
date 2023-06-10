using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using TDAmeritradeAPI.DataModels;
using TDAmeritradeAPI.Serializers;
using System.Text.Json.Serialization;
using System.Web;
using static TDAmeritradeAPI.TDAmeritradeStreamingClient.Enums;
using System.Text.Json.Nodes;

namespace TDAmeritradeAPI
{
    public class TDAmeritradeStreamingClient : IDisposable
    {
        private readonly ClientWebSocket _webSocket;
        private readonly TDAmeritradeClient _client;
        private UserPrincipals? _principal;
        private readonly JsonSerializerOptions _serializerOptions;

        public class Enums
        {
            public enum QosLevel : byte { EXPRESS = 0, REAL_TIME = 1, FAST = 2, MODERATE = 3, SLOW = 4, DELAYED = 5 }
            internal enum Service : byte
            {
                ACCT_ACTIVITY, ADMIN, ACTIVES_NASDAQ, ACTIVES_NYSE, ACTIVES_OTCBB, ACTIVES_OPTIONS,
                FOREX_BOOK, FUTURES_BOOK, LISTED_BOOK, NASDAQ_BOOK, OPTIONS_BOOK, FUTURES_OPTIONS_BOOK,
                CHART_EQUITY, CHART_FUTURES, CHART_HISTORY_FUTURES, QUOTE, LEVELONE_FUTURES, LEVELONE_FOREX,
                LEVELONE_FUTURES_OPTIONS, OPTION, LEVELTWO_FUTURES, NEWS_HEADLINE, NEWS_STORY, NEWS_HEADLINE_LIST,
                STREAMER_SERVER, TIMESALE_EQUITY, TIMESALE_FUTURES, TIMESALE_FOREX, TIMESALE_OPTIONS
            }
        }

        public bool IsConnected { get; private set; }   
        public TDAmeritradeStreamingClient(TDAmeritradeClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            else if (!client.IsLoggedIn) throw new NullReferenceException(nameof(client));
            _client = client;
            _webSocket = new ClientWebSocket();
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task Connect()
        {
            // Get the user principals to build the credentials as explained in
            // https://developer.tdameritrade.com/content/streaming-data#_Toc504640554
            UserPrincipals.Enums.AdditionalField[] additionalFields = { UserPrincipals.Enums.AdditionalField.StreamerConnectionInfo };
            _principal = await _client.GetUserPrincipals(additionalFields).ConfigureAwait(false);
            // Build the URI and connect the websocket
            string uri = $"wss://" + _principal.StreamerInfo!.Value.StreamerSocketUrl + "/ws";
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
        }

        #region ADMIN Service
        public async Task Login()
        {
            // Parse the timestamp to UNIX time
            DateTime dateTime = DateTime.Parse(_principal!.StreamerInfo!.Value.TokenTimestamp!);
            double unixTimestamp = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            // Build the credentials dictionary
            Dictionary<string, string> credential = new()
            {
                { "userid", _principal.Accounts![0].AccountID! },
                { "token", _principal.StreamerInfo!.Value.Token! },
                { "company", _principal.Accounts![0].Company! },
                { "segment", _principal.Accounts![0].Segment! },
                { "cddomain", _principal.Accounts![0].AccountCdDomainId! },
                { "usergroup", _principal.StreamerInfo.Value.UserGroup! },
                { "accesslevel", _principal.StreamerInfo.Value.AccessLevel! },
                { "authorized", "Y" },
                { "timestamp", unixTimestamp.ToString() },
                { "appid", _principal.StreamerInfo.Value.AppId! },
                { "acl", _principal.StreamerInfo.Value.Acl! }
            };
            // Build the parameters JSON
            JsonObject parameters = new()
            {
                ["credential"] = DictionaryToQueryString(credential),
                ["token"] = _principal.StreamerInfo.Value.Token!,
                ["version"] = "1.0"
            };
            await SendToServer(Service.ADMIN, 0, "LOGIN", parameters).ConfigureAwait(false);
        }

        public async Task LogOut()
        {
            await SendToServer(Service.ADMIN, 1, "LOGOUT", new JsonObject()).ConfigureAwait(false);
            IsConnected = false;
        }

        public async Task SetQualityOfService(QosLevel qosLevel)
        {
            await SendToServer(Service.ADMIN, 2, "QOS", new JsonObject{ ["qoslevel"] = ((int)qosLevel).ToString() }).ConfigureAwait(false);
        }
        #endregion

        private static string DictionaryToQueryString(Dictionary<string, string> dictionary)
        {
            string result = "";
            foreach (KeyValuePair<string, string> kvp in dictionary)
                result += kvp.Key + "=" + kvp.Value + '&';

            // Remove the last & symbol
            result = result[..^1];

            return HttpUtility.UrlEncode(result);
        }

        private async Task SendToServer(Service service, byte requestId, string command, JsonObject parameters)
        {
            JsonObject request = new()
            {
                ["service"] = service.ToString(),
                ["requestid"] = requestId.ToString(),
                ["command"] = command,
                ["account"] = _principal!.Accounts![0].AccountID!,
                ["parameters"] = parameters
            };
            // Create an buffer of the JSON string in UTF8 encoding
            ArraySegment<byte> buffer = new(Encoding.UTF8.GetBytes(request.ToString()));
            // Send the data to the server
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }
}
