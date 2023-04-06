using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Timers;
using System.Web;


namespace TDAmeritradeAPI
{
    public class Client
    {
        private bool _firstTimer = true;
        private System.Timers.Timer? _refreshTokenTimer = null;
        private readonly HttpClient _httpClient = new();
        private OAuth2Data _oAuth2Data = new();
        private enum Token { AccessToken, RefreshToken };

        public async Task Init()
        {

            _oAuth2Data = new();
            // If we don't have the 4 pieces of data necessary to connect using OAuth2.0 then get them from the user
            if (_oAuth2Data.AccessToken == null || _oAuth2Data.ClientID == null ||
                _oAuth2Data.RedirectURI == null || _oAuth2Data.RefreshToken == null)
            {
                // Get the tokens for the first time
                await FirstTimeTokens().ConfigureAwait(false);
                // 28 minutes timer
                InitializeTimer(1680000);
            }
            else
            {
                // Take care of the epiration dates of the tokens
                await RefreshTokens().ConfigureAwait(false);
            }

            // Save the changes to the file
            _oAuth2Data.SaveChanges();
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                  OAuth 2.0 Functions                                            *
         *                                                                                                 *
         * Based on:                                                                                       *
         * https://developer.tdameritrade.com/content/authentication-faq                                   *
         * https://developer.tdameritrade.com/content/simple-auth-local-apps                               *
         * https://developer.tdameritrade.com/content/getting-started                                      *
         * https://developer.tdameritrade.com/content/phase-1-authentication-update-xml-based-api          *
         * https://developer.tdameritrade.com/authentication/apis/post/token-0                             *
         *                                                                                                 *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Obtains the refresh and access tokens for the first time.
        /// </summary>
        private async Task FirstTimeTokens()
        {
            Console.WriteLine("Obtaining tokens. Please follow the instructions on screen.\n");

            // Get the Client ID and redirect URI if they are nonexistent
            if (_oAuth2Data.ClientID == null || _oAuth2Data.ClientID.Length == 0)
            {
                Console.WriteLine("ERROR. Client ID not found.\n\n");
                Console.Write("Please enter the Client ID (consumer key): ");
                _oAuth2Data.ClientID = Console.ReadLine()!.Trim();
            }
            if (_oAuth2Data.RedirectURI == null || _oAuth2Data.RedirectURI.Length == 0)
            {
                Console.WriteLine("ERROR. Redirect URI not found.\n\n");
                Console.Write("Please enter the redirect URI: ");
                _oAuth2Data.RedirectURI = Console.ReadLine()!.Trim();
            }

            // Save the changes to the file
            _oAuth2Data.SaveChanges();

            // Generate and encode the Auth URL as explained in https://developer.tdameritrade.com/content/authentication-faq
            string auth_url = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri=" +
                $"{HttpUtility.UrlEncode(_oAuth2Data.RedirectURI)}" +
                $"&client_id={HttpUtility.UrlEncode(_oAuth2Data.ClientID)}%40AMER.OAUTHAP";

            // Get the latest Chromedriver
            await GetChromedriver().ConfigureAwait(false);

            // Set chrome options
            ChromeOptions options = new();
            //options.AddArgument("--log-level=0");
            options.AddArgument("--start-maximized");

            // Instantiate a Chromedriver object and set the directory path of the driver
            ChromeDriver chromedriver = new("chromedriver.exe", options);

            // Set the current URL to the Auth URL
            chromedriver.Navigate().GoToUrl(auth_url);

            Console.WriteLine("Enter your TD Ameritrade API credentials to obtain an authorization code. " +
                "The web driver will timeout in 3 minutes.");

            // Use a WebDriverWait to wait until the URL returns the authorization code. Allow for a time window of 3 minutes
            WebDriverWait webDriverWait = new(chromedriver, TimeSpan.FromSeconds(180));
            webDriverWait.Until(x => x.Url.Contains("code="));

            // Get the authorization code (everything after the 'code=' in the URL
            string authorizationCode = chromedriver.Url[(chromedriver.Url.IndexOf("code=") + 5)..];

            // Decode the authorization code
            authorizationCode = HttpUtility.UrlDecode(authorizationCode);

            // Close Chromedriver
            chromedriver.Quit();

            // Create the parameters for the HTTP request
            Dictionary<string, string> parameters = new()
            {
                ["grant_type"] = "authorization_code",
                ["refresh_token"] = "",
                ["access_type"] = "offline",
                ["code"] = authorizationCode,
                ["client_id"] = _oAuth2Data.ClientID,
                ["redirect_uri"] = _oAuth2Data.RedirectURI
            };

            // Use the HTTP POST method to get a refresh token
            await PostAccessToken(parameters);

            // Save the expiration time of both tokens
            _oAuth2Data.WriteTokenExpiration(OAuth2Data.TokenType.Both);
        }

        /// <summary>
        /// Downloads the latest Selenium Chromedriver.
        /// </summary>
        private async Task GetChromedriver()
        {
            // Check if there is already a chromedriver.exe
            if (File.Exists("chromedriver.exe"))
            {
                return;
            }

            //This will be either chromedriver_win32.zip or chromedriver_linux64.zip depending on the OS
            string chromedrDriverType = "";

            // Check if the current OS is Windows or Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                chromedrDriverType = "/chromedriver_win32.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                chromedrDriverType = "/chromedriver_linux64.zip";

            // Get version number
            string versionNumber = await _httpClient.GetStringAsync("https://chromedriver.storage.googleapis.com/LATEST_RELEASE").ConfigureAwait(false);

            // Build download URL
            string downloadURL = "https://chromedriver.storage.googleapis.com/" + versionNumber + chromedrDriverType;

            Console.WriteLine("Downloading the latest version of Chromedriver...");

            // Download the file
            byte[] resultStream = await _httpClient.GetByteArrayAsync(downloadURL).ConfigureAwait(false); ;
            File.WriteAllBytes("chromedriver.zip", resultStream);

            // Delete any old chromedriver file
            if (File.Exists("chromedriver.exe"))
                File.Delete("chromedriver.exe");

            // Extract the executable file
            ZipFile.ExtractToDirectory("chromedriver.zip", Directory.GetCurrentDirectory());

            // Delete the zip file
            if (File.Exists("chromedriver.zip"))
                File.Delete("chromedriver.zip");
        }

        /// <summary>
        /// Gets a new access token or a new refresh token.
        /// </summary>
        /// <param name="token">
        /// Specifies which token must be obtained.
        /// </param>
        /// <remarks>
        /// Access tokens last 30 minutes and refresh tokens 90 days, so this function must be called accordingly.
        /// If invalid_grant error is returned when using the 'grant_type' as 'refresh_token',
        /// it means that your refresh token has expired or is no longer valid since it was first created over 90 days ago.
        /// You will need to go through the initial log in process again to recreate a new set of tokens. 
        /// </remarks>
        private async Task GetNewToken(Token token)
        {
            // Create the parameters for the HTTP request
            Dictionary<string, string> parameters = new()
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = _oAuth2Data.RefreshToken!,
                ["access_type"] = "offline",
                ["code"] = "",
                ["client_id"] = _oAuth2Data.ClientID!,
                ["redirect_uri"] = ""
            };

            // Parameters for both tokens are the same, except the 'access_type' parameter
            // which must be left empty for a new access_token
            if (token == Token.AccessToken)
                parameters["access_type"] = "";

            // Use the HTTP POST method to get a refresh token
            await PostAccessToken(parameters).ConfigureAwait(false);

            // Save the expiration time of the appropriate token
            _oAuth2Data.WriteTokenExpiration(token == Token.RefreshToken ? OAuth2Data.TokenType.RefreshToken :
                OAuth2Data.TokenType.AccessToken);
        }

        /// <summary>
        /// Simplified version of an HTTP request suitable for this library.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="httpMethod">The type of HTTP method required for this request.</param>
        /// <param name="parameters">Parameters to be sent.</param>
        /// <param name="parametersInQueryString">If this is true, the parameters will be passed as a
        /// query string in the URL.</param>
        /// <param name="authenticatedRequest">Indicates whether this is an authenticated request. If it is,
        /// the authorization header will be set.</param>
        /// <returns></returns>
        private async Task<string> HttpRequest(string requestUri, HttpMethod httpMethod,
            Dictionary<string, string>? parameters = null, bool parametersInQueryString = true, bool authenticatedRequest = true)
        {
            // If there are parameters and they must be passed as a query string then build a new request URI
            if (parameters != null && parametersInQueryString)
                requestUri = BuildURL(requestUri, parameters);

            // Create a new request
            HttpRequestMessage request = new()
            {
                Method = httpMethod,
                RequestUri = new Uri(requestUri)
            };

            // If there are parameters and they must be passed in the body of the request then add them to the
            // content of the request.
            if (parameters != null && !parametersInQueryString)
                request.Content = new FormUrlEncodedContent(parameters);

            // Set the authorization header
            if (authenticatedRequest)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _oAuth2Data.AccessToken);

            // Send the request
            using HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // Read the response as string
            string stringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;

            return stringResponse;
        }

        /// <summary>
        /// Simplified version of an HTTP request suitable for this library. Use this method when you must send data in JSON format.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="httpMethod">The type of HTTP method required for this request.</param>
        /// <param name="jsonParameters">String of JSON formatted parameters.</param>
        /// <param name="authenticatedRequest">Indicates whether this is an authenticated request. If it is,
        /// the authorization header will be set.</param>
        /// <returns></returns>
        private async Task<string> HttpRequest(string requestUri, HttpMethod httpMethod,
            string jsonParameters, bool authenticatedRequest = true)
        {
            // Create a new request
            HttpRequestMessage request = new()
            {
                Method = httpMethod,
                RequestUri = new Uri(requestUri),
                Content = new StringContent(jsonParameters, System.Text.Encoding.UTF8, "application/json")
            };

            // Set the authorization header
            if (authenticatedRequest)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _oAuth2Data.AccessToken);

            // Send the request
            using HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // Read the response as string
            string stringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;

            return stringResponse;
        }

        /// <summary>
        /// Initializes the refresh token timer.
        /// </summary>
        /// <param name="milliseconds">
        /// Length of the timer in milliseconds.
        /// </param>
        private void InitializeTimer(int milliseconds)
        {
            _refreshTokenTimer = new(milliseconds);
            _refreshTokenTimer.Elapsed += RefreshTokenTimerElapsed;
            _refreshTokenTimer.AutoReset = true;
            _refreshTokenTimer.Start();
        }

        /// <summary>
        /// Is triggered every 28 minutes to refresh the access token (which expires every 30 minutes).
        /// </summary>
        /// <param name="source">Source of the event.</param>
        /// <param name="e">Provides data of the event.</param>
        private async void RefreshTokenTimerElapsed(object? source, ElapsedEventArgs e)
        {
            await GetNewToken(Token.AccessToken).ConfigureAwait(false);
            if (_firstTimer)
            {
                Debug.WriteLine($"Second run of timer. Initialized to 1680 seconds.");
                _firstTimer = false;
                _refreshTokenTimer!.Interval = 1680000;
            }
        }

        /// <summary>
        /// Executes the HTTP request as explained in https://developer.tdameritrade.com/authentication/apis/post/token-0.
        /// </summary>
        /// <param name="parameters">
        /// A dictionary with the parameters that will be sent in the HTTP request.
        /// </param>
        private async Task PostAccessToken(Dictionary<string, string> parameters)
        {
            string endpoint = "https://api.tdameritrade.com/v1/oauth2/token";

            // Read the response as string
            string stringResponse = await HttpRequest(endpoint, HttpMethod.Post, parameters, false, false).ConfigureAwait(false);

            // Turn the response into a dictionary
            Dictionary<string, string>? jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);
            // Verify the response is not null
            if (jsonResponse == null)
                throw new NullReferenceException("Error. The PostAccessToken function did not retrieve a proper response from the server.");

            // If we are only getting a refresh token in the response    
            if (parameters["grant_type"] == "refresh_token")
            {
                // Only getting a new refresh token
                if (parameters["access_type"] == "offline")
                {
                    Debug.WriteLine("New refresh token obtained.");
                    _oAuth2Data.RefreshToken = jsonResponse["refresh_token"];
                    _oAuth2Data.WriteTokenExpiration(OAuth2Data.TokenType.RefreshToken);
                }
                // Only access token created
                else if (parameters["access_type"] == "")
                {
                    Debug.WriteLine("New access token obtained.");
                    _oAuth2Data.AccessToken = jsonResponse["access_token"];
                    _oAuth2Data.WriteTokenExpiration(OAuth2Data.TokenType.AccessToken);
                    // Change the default authorization header
                }
            }
            else if (parameters["grant_type"] == "authorization_code" && parameters["access_type"] == "offline")
            {
                Debug.WriteLine("New set of tokens obtained.");
                _oAuth2Data.AccessToken = jsonResponse["access_token"];
                _oAuth2Data.WriteTokenExpiration(OAuth2Data.TokenType.AccessToken);
                _oAuth2Data.RefreshToken = jsonResponse["refresh_token"];
                _oAuth2Data.WriteTokenExpiration(OAuth2Data.TokenType.RefreshToken);
            }
        }

        /// <summary>
        /// This function takes care of getting new tokens based on the code obtained from
        /// the function TokensExpirationsStatus() in OAuth2Data.
        /// </summary>
        private async Task RefreshTokens()
        {
            int code = _oAuth2Data.TokensExpirationsStatus();

            switch (code)
            {
                case 0:
                    Debug.WriteLine("Both tokens are OK.");
                    break;
                case 1:
                    Debug.WriteLine("The access token is within 3 minutes of expiring, it already expired, or it wasn't found.");
                    await GetNewToken(Token.AccessToken).ConfigureAwait(false);
                    break;
                case 2:
                    Debug.WriteLine("The refresh token is within 24 hours to expire or it already expired.");
                    await GetNewToken(Token.RefreshToken).ConfigureAwait(false);
                    break;
                case 3:
                    Debug.WriteLine("The access token is within 3 minutes of expiring, it already expired, or it wasn't found, " +
                        "and the refresh token is within 24 hours to expire or it already expired.");
                    await GetNewToken(Token.RefreshToken).ConfigureAwait(false);
                    await GetNewToken(Token.AccessToken).ConfigureAwait(false);
                    break;
                case 4:
                    Debug.WriteLine("The refresh token expiration has not been initialized at all or it already expired.");
                    await FirstTimeTokens().ConfigureAwait(false);
                    break;
            }

            _refreshTokenTimer?.Dispose();

            if (code == 0 || code == 2)
            {
                // Timer based on how many seconds there are left before the access token expires minus two minutes
                int timerSeconds = _oAuth2Data.SecondsUntilExpiration(OAuth2Data.TokenType.AccessToken) - 120;
                InitializeTimer(timerSeconds);
                Debug.WriteLine($"Timer initialized to {timerSeconds} seconds.");
            }
            else
                // 28 minutes timer
                InitializeTimer(1680000);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                          OAuth 2.0 End                                        *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                              Quotes                                           *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Creates an url with a URL-encoded set of key-value pairs appended in the query string.
        /// </summary>
        /// <param name="endpoint">The URL that the query string will be appended onto.</param>
        /// <param name="parameters">A dictionary of key-value pairs that will be added to the query string.</param>
        /// <returns>The full url with the query string appended.</returns>
        private static string BuildURL(string endpoint, Dictionary<string, string> parameters)
        {
            string url = endpoint + '?';

            foreach (KeyValuePair<string, string> parameter in parameters)
                url += parameter.Key + "=" + HttpUtility.UrlEncode(parameter.Value) + '&';

            // Remove the last & symbol
            url = url[..^1];

            return url;
        }

        /// <summary>
        /// Get quote for a symbol.
        /// </summary>
        /// <param name="symbol">The symbol in question.</param>
        /// <returns></returns>
        /// <remarks>Index symbols start with $. Options in TD Ameritrade: .AAPL210528C126 vs
        /// options in the API: AAPL_052821C126. It seems like futures, futures' options, and
        /// forex are not available using this endpoint. It is better to use the GetQuotes()
        /// function instead.</remarks>
        public async Task<string> GetQuote(string symbol)
        {
            ParseSymbol(ref symbol);

            string endpoint = $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        /// <summary>
        /// Get quote for one or more symbols.
        /// </summary>
        /// <param name="symbols">Comma-separated list of symbols.</param>
        /// <returns>A string with the quote data of the symbols.</returns>
        /// <remarks>Visit https://www.wintick.com/members/symbolGuide/tda to see the symbols a few
        /// common indices. The format for futures options is still unknown.
        /// </remarks>
        public async Task<string> GetQuotes(string symbols)
        {
            string endpoint = "https://api.tdameritrade.com/v1/marketdata/quotes";

            Dictionary<string, string> parameters = new()
            {
                { "symbol", ParseSymbols(symbols) }
            };

            return await HttpRequest(endpoint, HttpMethod.Get, parameters, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Parses a symbol into the format required by the TD Ameritrade API.
        /// </summary>
        /// <param name="symbol">
        /// The symbol of the security in question.
        /// </param>
        /// <remarks>
        /// So far it only parses stock options, but perhaps there is a way to parse futures and futures' options
        /// so the get_quote() function is not so useless.
        /// </remarks>
        internal static Order.Enums.SecurityType ParseSymbol(ref string symbol)
        {
            Order.Enums.SecurityType type = Order.Enums.SecurityType.Other;

            symbol = symbol.ToUpper().Trim();

            if (symbol.Length >= 2)
            {
                if (symbol[0] == '.')
                {
                    // Remove the first character '.'
                    symbol = symbol[1..];
                    // For stock options
                    if (symbol[0] != '/')
                    {
                        // Get the index of the number 2, since all option's name after the underlying's symbol have the date as YYMMDD
                        // so the first number that will appear is 2, until year 3000. Update this on Y3K.
                        int firstIndexOf2 = symbol.IndexOf('2');
                        // Get the date part of the string (format is YYMMDD, but the API requires MMDDYY)
                        string date = symbol[firstIndexOf2..(firstIndexOf2 + 6)];
                        // Rearrange the date
                        date = date[2..] + date[..2];
                        // Head is the letters of the option's underlying.
                        string head = symbol[..firstIndexOf2];
                        // Tail is the characters after the date characters.
                        string tail = symbol[(firstIndexOf2 + 6)..];
                        // Join the head, correctly formatted date, and tail and put an underscore in between them
                        // to get the required stock option symbol for the API.
                        symbol = head + '_' + date + tail;

                        type = Order.Enums.SecurityType.Option;
                    }
                }
                // For already parsed option symbols
                else if (symbol.Contains('_'))
                    type = Order.Enums.SecurityType.Option;
                else if (char.IsLetter(symbol[0]))
                    type = Order.Enums.SecurityType.Equity;
            }
            else if (symbol.Length == 0)
                type = Order.Enums.SecurityType.Other;
            else if (char.IsLetter(symbol[0]))
                type = Order.Enums.SecurityType.Equity;

            return type;
        }

        /// <summary>
        /// Parses a list of symbols into the required format for the TD Ameritrade API.
        /// </summary>
        /// <param name="symbols">A string of comma-separated symbols.</param>
        /// <returns>A formatted string of comma-separated symbols.</returns>
        private static string ParseSymbols(string symbols)
        {
            // Create an array of the symbols
            string[] symbolsArray = symbols.Split(',');

            // Parse each individual symbol
            for (int i = 0; i < symbolsArray.Length; i++)
                ParseSymbol(ref symbolsArray[i]);

            // Return the list of comma-separated symbols
            return string.Join(',', symbolsArray);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                            End Quotes                                         *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                          Price History                                        *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        public async Task<string> GetPriceHistory(string symbol, PriceHistoryOptions options)
        {
            ParseSymbol(ref symbol);

            string endpoint = $"https://api.tdameritrade.com/v1/marketdata/{symbol}/pricehistory";

            // Get the PriceHistoryOptions object as a dictionary
            Dictionary<string, string> parameters = options.ToDictionary();


            return await HttpRequest(endpoint, HttpMethod.Get, parameters, true).ConfigureAwait(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                         End Price History                                     *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                          Watchlists                                           *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Creates a new watchlist.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="watchlistName">The name of the watchlist.</param>
        /// <param name="symbols">List of key-value pairs where the key is the symbol and the value is its asset type.</param>
        /// <returns></returns>
        public async Task CreateWatchlist(string accountId, string watchlistName, List<WatchlistItem> symbols)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists";

            Watchlist watchlist = new(watchlistName, symbols);

            await HttpRequest(endpoint, HttpMethod.Post, watchlist.AsJson()).ConfigureAwait(false);

            Console.WriteLine("Watchlist created successfully.");
        }

        /// <summary>
        /// Deletes a watchlist for a specific account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="watchlistId">The ID of the watchlist that will be deleted.</param>
        /// <returns>Void.</returns>
        public async Task DeleteWatchlist(string accountId, string watchlistId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists/{watchlistId}";

            await HttpRequest(endpoint, HttpMethod.Delete).ConfigureAwait(false);

            Console.WriteLine("Watchlist deleted successfully.");
        }

        /// <summary>
        /// Gets a watchlist associated with a TD Ameritrade account.
        /// </summary>
        /// <param name="accountId">Account ID associated with the watchlist.</param>
        /// <param name="watchlistId">ID of the watchlist of interest.</param>
        /// <returns>Returns the watchlist of interest in the given account.</returns>
        public async Task<string> GetWatchlist(string accountId, string watchlistId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists/{watchlistId}";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all the watchlists of all the linked accounts.
        /// </summary>
        /// <returns>Returns all watchlists from all the linked accounts.</returns>
        public async Task<string> GetWatchlistForMultipleAccounts()
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/watchlists";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all the watchlists of a TD Ameritrade account.
        /// </summary>
        /// <param name="accountId">Account ID associated with the watchlists.</param>
        /// <returns>Returns all watchlists of the given account.</returns>
        public async Task<string> GetWatchlistForSingleAccount(string accountId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        public async Task ReplaceWatchlist(string accountId, string watchlistId, string watchlistName,
            List<WatchlistItem> symbols)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists/{watchlistId}";

            Watchlist watchlist = new(watchlistName, symbols);

            await HttpRequest(endpoint, HttpMethod.Put, watchlist.AsJson()).ConfigureAwait(false);

            Console.WriteLine("Watchlist replaced successfully.");
        }

        /// <summary>
        /// Updates a watchlist.
        /// </summary>
        /// <param name="accountId">The account's ID</param>
        /// <param name="watchlistId">The watchlist's ID.</param>
        /// <param name="modifiedWatchlistJson">Modified watchlist or parts of the watchlist that must be updated.</param>
        /// <returns>Void.</returns>
        /// <remarks>It is best to use this function by first obtaining the watchlist to modify, deserializing it into a
        /// Watchlist object, modifying the desired attributes, and serializing it back into a JSON string.
        /// To deserialize use either AsJson(), NameAsJson(), or ItemsAsJson() depending wether both the name and the
        /// items were changed, only the name was changed, or only the items were change. Pass the JSON string returned
        /// by these functions as the modifiedWatchlistJson.</remarks>
        public async Task UpdateWatchlist(string accountId, string watchlistId, string modifiedWatchlistJson)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/watchlists/{watchlistId}";

            await HttpRequest(endpoint, HttpMethod.Patch, modifiedWatchlistJson).ConfigureAwait(false);

            Console.WriteLine("Watchlist updated successfully.");
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                         End Watchlists                                        *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                             Accounts                                          *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Gets account balances, positions, and orders for a specific account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="getPositions">Set to true if the positions in the account must be obtained.</param>
        /// <param name="getOrders">Set to true if the orders in the account must be obtained.</param>
        /// <returns>A string with the account balances, positions, and orders for the given account.</returns>
        public async Task<string> GetAccount(string accountId, bool getPositions = false, bool getOrders = false)
        {
            // Small trick so this method is easily reused in GetAccounts() method
            if (accountId != "")
                accountId = '/' + accountId;

            Dictionary<string, string>? parameters = null;

            // Create a dictionary with the parameters
            if (getPositions)
                parameters = new Dictionary<string, string>() { { "fields", "positions" } };
            if (getOrders)
            {
                if (parameters == null)
                    parameters = new Dictionary<string, string>() { { "fields", "orders" } };
                else
                    parameters["fields"] = parameters["fields"] + ",orders";
            }

            string endpoint = $"https://api.tdameritrade.com/v1/accounts{accountId}";

            return await HttpRequest(endpoint, HttpMethod.Get, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets account balances, positions, and orders for all linked accounts.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="getPositions">Set to true if the positions in the account must be obtained.</param>
        /// <param name="getOrders">Set to true if the orders in the account must be obtained.</param>
        /// <returns>A string with the account balances, positions, and orders for the given account.</returns>
        public async Task<string> GetAccounts(bool getPositions = false, bool getOrders = false)
        {
            return await GetAccount("", getPositions, getOrders).ConfigureAwait(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                          End Accounts                                         *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                         Market Hours                                          *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Returns the market hours for the markets and day selected.
        /// </summary>
        /// <param name="markets">A list of the markets of interest.</param>
        /// <param name="date">The date of interest.</param>
        /// <returns></returns>
        public async Task<string> MarketHours(List<CustomEnums.Markets> markets, DateTime date)
        {
            string endpoint;
            // Add the date to the parameters
            Dictionary<string, string> parameters = new()
            {
                { "date", date.ToString("yyyy-MM-dd") }
            };
            // Select the right enpoint to use
            if (markets.Count == 1)
                endpoint = $"https://api.tdameritrade.com/v1/marketdata/{markets[0]}/hours";
            else
            {
                endpoint = "https://api.tdameritrade.com/v1/marketdata/hours";
                // Create the string of comma-separated markets and add it to the parameters
                parameters["markets"] = string.Join(',', markets);
            }
            return await HttpRequest(endpoint, HttpMethod.Get, parameters).ConfigureAwait(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                        End Market Hours                                       *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                     User Info & Preferences                                   *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Preferences for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <returns>A JSON formatted string containing the Preferences for the accounts.</returns>
        /// <remarks>It is possible to deserialize the JSON using JsonConvert into a Preferences struct.
        /// However, make sure to use the appropriate ContractResolver to parse the camel case.</remarks>
        public async Task<string> GetPreferences(string accountId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/preferences";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        /// <summary>
        /// SubscriptionKey for provided accounts or default accounts.
        /// </summary>
        /// <param name="accountIds">The comma-separated Ids.</param>
        /// <returns>A JSON formatted string containing the SubscriptionKeys for the accounts.</returns>
        /// <remarks>It is possible to deserialize the JSON using JsonConvert into a UserPrincipal.SubscriptionKeys
        /// struct. However, make sure to use the appropriate ContractResolver to parse the camel case.</remarks>
        public async Task<string> GetStreamerSubscriptionKeys(string accountIds)
        {
            string endpoint = "https://api.tdameritrade.com/v1/userprincipals/streamersubscriptionkeys";

            Dictionary<string, string> parameters = new()
            {
                { "accountIds", accountIds }
            };

            return await HttpRequest(endpoint, HttpMethod.Get, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// User Principal details.
        /// </summary>
        /// <param name="additionalFields">Additional fields to be returned in the request's response.</param>
        /// <returns>A JSON formatted string containing the user principal details.</returns>
        /// <remarks>It is possible to deserialize the JSON using JsonConvert into a UserPrincipal struct.
        /// However, make sure to use the appropriate ContractResolver to parse the camel case.</remarks>
        public async Task<string> GetUserPrincipals(List<UserPrincipal.Enums.AdditionalField> additionalFields)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/userprincipals";

            // Remove repeated fields
            additionalFields = additionalFields.Distinct().ToList();

            string fields = "";
            // Create a comma-separated list of Additional Fields in camel case formatting
            foreach (UserPrincipal.Enums.AdditionalField additionalField in additionalFields)
            {
                string currentField = additionalField.ToString();
                fields += char.ToLowerInvariant(currentField[0]) + currentField[1..] + ',';
            }

            Dictionary<string, string> parameters = new()
            {
                { "fields", fields }
            };

            return await HttpRequest(endpoint, HttpMethod.Get, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Update preferences for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="preferences">Preferences object.</param>
        /// <returns>Void.</returns>
        /// <remarks>Please note that the DirectOptionsRouting and DirectEquityRouting
        /// values cannot be modified via this operation. Therefore, after serializing
        /// the parameters from a previous request do not change these two parameters
        /// or set them to null.</remarks>
        public async Task UpdatePreferences(string accountId, Preferences preferences)
        {
            // Setting forbidden parameters as null
            preferences.DirectOptionsRouting = null;
            preferences.DirectEquityRouting = null;

            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/preferences";

            string jsonParameters = JsonConvert.SerializeObject(preferences, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()

            });

            await HttpRequest(endpoint, HttpMethod.Put, jsonParameters).ConfigureAwait(false);

            Console.WriteLine("Preferences updated successfully.");
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                  End User Info & Preferences                                  *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                             Trading                                           *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>
        /// Cancel a specific order for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="orderId">The order's ID.</param>
        /// <returns>Void.</returns>
        public async Task CancelOrder(string accountId, string orderId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/orders/{orderId}";

            await HttpRequest(endpoint, HttpMethod.Delete).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a specific order for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="orderId">The order's ID.</param>
        /// <returns>A JSON-formatted string with the order's details.</returns>
        /// <remarks>
        /// The JSON-formatted string can be deserialized into an Order object.
        /// </remarks>
        public async Task<string> GetOrder(string accountId, string orderId)
        {
            string endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/orders/{orderId}";

            return await HttpRequest(endpoint, HttpMethod.Get).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function for GetOrdersByPath() and GetOrdersByQuery().
        /// </summary>
        /// <param name="isByPath">Indicates if it should get the orders by path or by query.</param>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="maxResults">Maximum number of orders that will be fetched.</param>
        /// <param name="startDate">No order before this date will be returned.</param>
        /// <param name="endDate">No order past this date will be returned.</param>
        /// <param name="status">Specifies the type of orders that will be fetched.</param>
        /// <returns>A JSON-formatted string containing a list of orders.</returns>
        /// <remarks>
        /// If account ID isn't specified orders will be returned for all linked accounts.
        /// If 'startDate' is not sent, the default 'endDate' would be the current day.
        /// If 'startDate' is not sent, the default 'startDate' would be 60 days from 'endDate'.
        /// The JSON-formatted string can be deserialized into a List of Order objects.
        /// </remarks>
        private async Task<string> GetOrders(bool isByPath, string? accountId = null, int? maxResults = null,
            DateTime? startDate = null, DateTime? endDate = null, Order.Enums.Status? status = null)
        {
            string endpoint;

            Dictionary<string, string> parameters = new();
            if (isByPath)
            {
                if (accountId == null)
                    throw new Exception("Error. The GetOrdersByPath() function requires an account ID.");
                endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/orders";
            }
            else
                endpoint = "https://api.tdameritrade.com/v1/orders";

            // Add parameters that will be sent in the request
            if (accountId != null)
                parameters["accountId"] = accountId;
            if (maxResults != null)
                parameters["maxResults"] = maxResults.ToString()!;
            if (startDate != null)
            {
                DateTime notNullableDate = (DateTime)startDate;
                parameters["fromEnteredTime"] = notNullableDate.ToString("yyyy-MM-dd");
            }
            if (endDate != null)
            {
                DateTime notNullableDate = (DateTime)endDate;
                parameters["toEnteredTime"] = notNullableDate.ToString("yyyy-MM-dd");
            }
            if (status != null)
                parameters["status"] = status.ToString()!;

            return await HttpRequest(endpoint, HttpMethod.Get, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Orders for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="maxResults">Maximum number of orders that will be fetched.</param>
        /// <param name="startDate">No order before this date will be returned.</param>
        /// <param name="endDate">No order past this date will be returned.</param>
        /// <param name="status">Specifies the type of orders that will be fetched.</param>
        /// <returns>A JSON-formatted string containing a list of orders.</returns>
        /// <remarks>
        /// If account ID isn't specified orders will be returned for all linked accounts.
        /// If 'startDate' is not sent, the default 'endDate' would be the current day.
        /// If 'startDate' is not sent, the default 'startDate' would be 60 days from 'endDate'.
        /// The JSON-formatted string can be deserialized into a List of Order objects.
        /// </remarks>
        public async Task<string> GetOrdersByPath(string accountId, int? maxResults = null,
            DateTime? startDate = null, DateTime? endDate = null, Order.Enums.Status? status = null)
        {
            return await GetOrders(true, accountId, maxResults, startDate, endDate, status).ConfigureAwait(false);
        }

        /// <summary>
        /// All orders for a specific account or, if account ID isn't specified, orders will be returned for all linked accounts.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="maxResults">Maximum number of orders that will be fetched.</param>
        /// <param name="startDate">No order before this date will be returned.</param>
        /// <param name="endDate">No order past this date will be returned.</param>
        /// <param name="status">Specifies the type of orders that will be fetched.</param>
        /// <returns>A JSON-formatted string containing a list of orders.</returns>
        /// <remarks>
        /// If account ID isn't specified orders will be returned for all linked accounts.
        /// If 'startDate' is not sent, the default 'endDate' would be the current day.
        /// If 'startDate' is not sent, the default 'startDate' would be 60 days from 'endDate'.
        /// The JSON-formatted string can be deserialized into a List of Order objects.
        /// </remarks>
        public async Task<string> GetOrdersByQuery(string accountId, int? maxResults = null,
            DateTime? startDate = null, DateTime? endDate = null, Order.Enums.Status? status = null)
        {
            return await GetOrders(false, accountId, maxResults, startDate, endDate, status).ConfigureAwait(false);
        }

        /// <summary>
        /// Place an order for a specific account.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="order">Order object containing the details of the order.</param>
        /// <returns>Void.</returns>
        public async Task PlaceOrder(string accountId, Order order)
        {
            await PlaceOrderHelper(accountId, null, order).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function for PlaceOrder() and ReplaceOrder().
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="orderId">The ID of the order to replace. If it's null a new order will be placed.</param>
        /// <param name="order">Order object containing the details of the order.</param>
        /// <returns>Void.</returns>
        /// <remarks>
        /// Following parameters are not allowed when placing an order:
        /// OrderId, Cancelable, Editable, Status, EnteredTime, AccountId, FilledQuantity, RemainingQuantity,
        /// DestinationLinkName, LegId, PositionEffect, Cusip. So these parameters will be set to null by default.
        /// </remarks>
        private async Task PlaceOrderHelper(string accountId, string? orderId, Order order)
        {
            if (orderId != null)
            {
                // Validate status. Pretty empty right now since there is no documentation on
                // which order status prevent from replacing an order.
                // Only applicable if orderId is not null, meaning that the order is being replaced.
                switch (order.Status)
                {
                    case Order.Enums.Status.PENDING_REPLACE:
                        Console.WriteLine("Error. The status of the order is 'PENDING_REPLACE'." +
                            "The order cannot be replaced at the moment.");
                        break;
                }
            }
            // Set forbidden parameters null by default.
            order.OrderId = null;
            order.Cancelable = null;
            order.Editable = null;
            order.Status = null;
            order.EnteredTime = null;
            order.AccountId = null;
            order.FilledQuantity = null;
            order.RemainingQuantity = null;
            order.DestinationLinkName = null;
            if (order.OrderLegCollection != null)
            {
                foreach (Order.OrderLeg orderLeg in order.OrderLegCollection)
                {
                    orderLeg.LegId = null;
                    orderLeg.PositionEffect = null;
                    if (orderLeg.Instrument != null)
                        orderLeg.Instrument!.Cusip = null;
                }
            }

            string jsonParameter = JsonConvert.SerializeObject(order, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            string endpoint;

            if (orderId == null)
            {
                endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/orders";
                await HttpRequest(endpoint, HttpMethod.Post, jsonParameter).ConfigureAwait(false);
                Console.WriteLine("Order placed successfully.");
            }
            else
            {
                endpoint = $"https://api.tdameritrade.com/v1/accounts/{accountId}/orders/{orderId}";
                await HttpRequest(endpoint, HttpMethod.Put, jsonParameter).ConfigureAwait(false);
                Console.WriteLine("Order replaced successfully.");
            }
        }

        /// <summary>
        /// Replace an existing order for an account. The existing order will be replaced by the new order.
        /// Once replaced, the old order will be canceled and a new order will be created.
        /// </summary>
        /// <param name="accountId">The account's ID.</param>
        /// <param name="orderId">The ID of the order to replace.</param>
        /// <param name="order">Order object containing the details of the order.</param>
        /// <returns>Void.</returns>
        public async Task ReplaceOrder(string accountId, string orderId, Order order)
        {
            await PlaceOrderHelper(accountId, orderId, order).ConfigureAwait(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                           End Trading                                         *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    }
}