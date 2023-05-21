using System.Text.Json;

namespace TDAmeritradeAPI.Utils
{
    public class OAuth2Data
    {
        private const string authFilePath = "oa2d.json";
        /// <summary>
        /// Enumarates the types of tokens used in the TD Ameritrade API class used in the WriteTokenExpiration function.
        /// </summary>
        public enum TokenType { AccessToken, Both, RefreshToken }
        public string? AccessToken { get; set; }
        public int? AccessTokenExpiration { get; set; }
        public string? ClientID { get; set; }
        public string? RedirectURI { get; set; }
        public string? RefreshToken { get; set; }
        public int? RefreshTokenExpiration { get; set; }

        /// <summary>
        /// Class constructor.
        /// </summary>
        public OAuth2Data() { }

        /// <summary>
        /// 4 = refresh token already expired OR refresh token wasn't found.
        /// 3 = refresh token is close to expiration AND access token is close to or already expired OR access token wasn't found.
        /// 2 = only refresh token is close to expiration.
        /// 1 = access token is close to expiration or already expired OR access token expiration read failure.
        /// 0 = both tokens are OK and were read properly.
        /// </summary>
        /// <returns>An int with the status code.</returns>
        public int TokensExpirationsStatus()
        {
            // Get the number of seconds since the epoch (January 1st 1970)
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            int code = 0;

            // Check if the access token expiration has been initialized at all
            if (AccessTokenExpiration < 0)
                code = 1;
            // Check if the access token expires within 3 minutes (in seconds) or already expired
            else if (secondsSinceEpoch + 180 >= AccessTokenExpiration)
                code = 1;

            // Check if the refresh token expiration has been initialized at all
            if (RefreshTokenExpiration < 0)
                return 4;
            // Refresh token already expired
            else if (secondsSinceEpoch >= RefreshTokenExpiration)
                return 4;
            // Check if the refresh token is within 24 hours (in seconds) to expire or already expired
            else if (secondsSinceEpoch + 86400 >= RefreshTokenExpiration)
                code |= 2;

            return code;
        }

        /// <summary>
        /// Writes the values of this object in a JSON file.
        /// </summary>
        public void SaveChanges()
        {
            // Serialize this object into a JSON string
            string data = AsJson();
            using StreamWriter sw = new(authFilePath, false);
            sw.Write(data);
        }

        /// <summary>
        /// Returns the seconds remaining until the requested token expires.
        /// </summary>
        /// <param name="token">Determines the token in question.</param>
        /// <returns>Number of seconds remaining until the requested token expires.</returns>
        public int SecondsUntilExpiration(TokenType token)
        {
            // Get the number of seconds since the epoch (January 1st 1970)
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            if (token == TokenType.Both)
                return -1;
            else
                return token == TokenType.AccessToken ? (int)AccessTokenExpiration! - secondsSinceEpoch
                : (int)RefreshTokenExpiration! - secondsSinceEpoch;
        }

        public string AsJson()
        {
            JsonSerializerOptions serializerOptions = new()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, serializerOptions);
        }

        /// <summary>
        /// Saves the expiration date in seconds since epoch of the two important tokens (access and refresh token) into a file.
        /// </summary>
        /// <param name="type">Indicates which token will be saved into a file.</param>
        public void WriteTokenExpiration(TokenType type)
        {
            // Get the number of seconds since the epoch (January 1st 1970)
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            // Add the values to the dictionary
            if (type == TokenType.AccessToken || type == TokenType.Both)
                // 30 minutes expiration for access token
                AccessTokenExpiration = secondsSinceEpoch + 1800;
            if (type == TokenType.RefreshToken || type == TokenType.Both)
                // 90 days expiration for refresh token
                RefreshTokenExpiration = secondsSinceEpoch + 7776000;

            SaveChanges();
        }
    }
}
