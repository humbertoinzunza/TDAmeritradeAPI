using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using TDAmeritradeAPI.DataModels;
using TDAmeritradeAPI.Utils;
using TDAmeritradeAPI.Serializers;
using System.Text.Json.Serialization;

namespace TDAmeritradeAPI
{
    public class TDAmeritradeStreamingClient : IDisposable
    {
        private readonly ClientWebSocket _webSocket;
        private readonly TDAmeritradeClient _client;
        private UserPrincipals? _principal;
        private JsonSerializerOptions _serializerOptions;

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

        public async Task Login()
        {
            // Parse the timestamp to UNIX time
            DateTime dateTime = DateTime.Parse(_principal!.StreamerInfo!.Value.TokenTimestamp!);
            double unixTimestamp = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            // Build the credentials struct
            AdminRequest.Structs.Credential credential = new()
            {
                UserId = _principal.Accounts![0].AccountID!,
                Token = _principal.StreamerInfo!.Value.Token!,
                Company = _principal.Accounts![0].Company!,
                Segment = _principal.Accounts![0].Segment!,
                CdDomain = _principal.Accounts![0].AccountCdDomainId!,
                UserGroup = _principal.StreamerInfo.Value.UserGroup!,
                AccessLevel = _principal.StreamerInfo.Value.AccessLevel!,
                Authorized = 'Y',
                Timestamp = unixTimestamp,
                AppId = _principal.StreamerInfo.Value.AppId!,
                Acl = _principal.StreamerInfo.Value.Acl!
            };
            // Build the parameters struct
            AdminRequest.Structs.Parameters parameters = new()
            {
                Credential = credential,
                Token = _principal.StreamerInfo.Value.Token!,
                Version = "1.0"
            };
            // Build the AdminRequest and the StreamingRequest objects for the websocket request
            AdminRequest adminRequest = new(AdminRequest.Enums.Command.LOGIN, 0, _principal.Accounts[0].AccountID!,
                _principal.StreamerInfo.Value.AppId!, parameters);
            StreamingRequests streamingRequests = new(new[] { adminRequest });
            // Get the data to send as a JSON string
            string jsonString = JsonSerializer.Serialize(streamingRequests, _serializerOptions);
            await SendToServer(jsonString);
        }

        public async Task LogOut()
        {
            // Build the AdminRequest and the StreamingRequest objects for the websocket request
            AdminRequest adminRequest = new(AdminRequest.Enums.Command.LOGOUT, 1, _principal!.Accounts![0].AccountID!,
                _principal.StreamerInfo!.Value.AppId!, new AdminRequest.Structs.Parameters());
            StreamingRequests streamingRequests = new(new[] { adminRequest });
            // Get the data to send as a JSON string
            string jsonString = JsonSerializer.Serialize(streamingRequests, _serializerOptions);
            await SendToServer(jsonString);
        }

        private async Task SendToServer(string jsonString)
        {
            // Create an buffer of the JSON string in UTF8 encoding
            ArraySegment<byte> buffer = new(Encoding.UTF8.GetBytes(jsonString));
            // Send the data to the server
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }

        public void Dispose()
        {

        }
    }
}
