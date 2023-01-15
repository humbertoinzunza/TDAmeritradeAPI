using Newtonsoft.Json;
using System.Configuration;

namespace TDAmeritradeAPI
{
    internal class OAuth2Data
    {
        /// <summary>
        /// Enumarates the types of tokens used in the TD Ameritrade API class used in the WriteTokenExpiration function.
        /// </summary>
        public enum TokenType { AccessToken, Both, RefreshToken }

        private int _accessTokenExpiration;
        private int _refreshTokenExpiration;
        public string? AccessToken { get; set; }
        public int? AccessTokenExpiration
        {
            get { return _accessTokenExpiration; }
        }
        public string? ClientID { get; set; }
        public string? RedirectURI { get; set; }
        public string? RefreshToken { get; set; }

        public int? RefreshTokenExpiration
        {
            get { return _refreshTokenExpiration; }
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <exception cref="NullReferenceException">This exception will be thrown if App.Config couldn't be read.</exception>
        /// <exception cref="Exception">This exception will occur when there are issues reading the authorization file.</exception>
        public OAuth2Data()
        {
            // Initialize default values
            AccessToken = null;
            _accessTokenExpiration = -1;
            ClientID = null;
            RedirectURI = null;
            RefreshToken = null;
            _refreshTokenExpiration = -1;

            // Read the file paths for the OAuth2.0 data
            // Tell the compiler to ignore possible null assignment
            string authFilePath = ConfigurationManager.AppSettings["AuthorizationFile"]!;
            // If the strings are in fact null throw an exception
            if (authFilePath == null)
                throw new NullReferenceException("Error. Unable to read from configuration file.");
            try
            {
                using StreamReader sr = new StreamReader(authFilePath);

                string oAuth2DataString = sr.ReadToEnd();

                // Deserialize the string into the appropriate datatype
                Dictionary<string, string?> dataAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string?>>(oAuth2DataString!)!;

                AccessToken = dataAsDictionary["AccessToken"];
                _accessTokenExpiration = Convert.ToInt32(dataAsDictionary["AccessTokenExpiration"]);
                ClientID = dataAsDictionary["ClientID"];
                RedirectURI = dataAsDictionary["RedirectURI"];
                RefreshToken = dataAsDictionary["RefreshToken"];
                _refreshTokenExpiration = Convert.ToInt32(dataAsDictionary["RefreshTokenExpiration"]);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error. The attempt to read the authorization file failed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error. The attempt to read the authorization file failed.\n\n{ex.Message}");
            }
        }

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
            if (_accessTokenExpiration < 0)
                code = 1;
            // Check if the access token expires within 3 minutes (in seconds) or already expired
            else if ((secondsSinceEpoch + 180) >= _accessTokenExpiration)
                code = 1;

            // Check if the refresh token expiration has been initialized at all
            if (_refreshTokenExpiration < 0)
                return 4;
            // Refresh token already expired
            else if (secondsSinceEpoch >= _refreshTokenExpiration)
                return 4;
            // Check if the refresh token is within 24 hours (in seconds) to expire or already expired
            else if ((secondsSinceEpoch + 86400) >= _refreshTokenExpiration)
                code |= 2;

            return code;
        }

        /// <summary>
        /// Writes the values of this object in a JSON file.
        /// </summary>
        public void SaveChanges()
        {
            // Serialize this object into a JSON string
            string data = JsonConvert.SerializeObject(this);
            string authFilePath = ConfigurationManager.AppSettings["AuthorizationFile"]!;

            int attempts = 0;
            bool completed = false;
            while (!completed)
            {
                try
                {
                    using StreamWriter sw = new(authFilePath, false);

                    sw.Write(data);
                    completed = true;
                }
                catch (IOException)
                {
                    attempts++;
                    if (attempts >= 5)
                        throw new IOException("After several attempts to write the file is still in use by " +
                            "another process.");
                }
            }
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
                return (token == TokenType.AccessToken ? (_accessTokenExpiration - secondsSinceEpoch)
                : (_refreshTokenExpiration - secondsSinceEpoch));
        }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
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
                _accessTokenExpiration = secondsSinceEpoch + 1800;
            if (type == TokenType.RefreshToken || type == TokenType.Both)
                // 90 days expiration for refresh token
                _refreshTokenExpiration = secondsSinceEpoch + 7776000;

            SaveChanges();
        }
    }
}
