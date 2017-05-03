using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json.Linq;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// HOW TO SET UP THE GOOGLE DRIVE COMMUNICATOR
    /// STEP 1: Call GoogleDriveCommunicator.Run() to make the web view show up and have the user authenticate nusys to access its data
    /// STEP 2: Once the user clicks accept, and clicks the "open nusys" pop up, the flow of control goes to App.xaml.cs.OnActivated(IActivatedEventArgs args);
    /// STEP 3: In App.xaml.cs.OnActivated(IActivatedEventArgs args), there is a line GoogleDriveCommunicator.GetAuthenticationFromUri(uri). This function trades the 
    ///         code for an access token which can be used to make API calls
    /// STEP 4: From this point on you can use the functions such as SearchDrive(String searchString, int maxResults);
    /// </summary>
    public static class GoogleDriveCommunicator
    {
        /// <summary>
        /// OAuth 2.0 client configuration.
        /// </summary>
        const string clientID = "502237683379-3s011g0c0eqm8id8ealas1nrrkav39bu.apps.googleusercontent.com";

        private const string redirectURI = "com.nusys:/oauth2redirect";
        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string listFilesEndpoint = "https://www.googleapis.com/drive/v2/files";
        private const string searchListFilesEndpoint = "https://www.googleapis.com/drive/v2/files?maxResults=5&q=";
        private static string accessToken;



        /// <summary>
        /// Starts an OAuth 2.0 Authorization Request.
        /// </summary>
        public static void run()
        {
            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method={6}",
                authorizationEndpoint,
                DriveService.Scope.DriveReadonly,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);
            output("Opening authorization request URI: " + authorizationRequest);
            // Opens the Authorization URI in the browser.
            //var success = Windows.System.Launcher.LaunchUriAsync(new Uri(authorizationRequest));
            SessionController.Instance.SessionView.FreeFormViewer.ShowWebPreview(authorizationRequest, 500, 500);

        }
        /// <summary>
        /// Processes the OAuth 2.0 Authorization Response
        /// </summary>
        /// <param name="e"></param>
        public static void GetAuthenticationFromUri(Uri authorizationResponse)
        {
            //if (e.Parameter is Uri)
            //{
            //    // Gets URI from navigation parameters.
            //    Uri authorizationResponse = (Uri)e.Parameter;
            string queryString = authorizationResponse.Query;
            output("MainPage received authorizationResponse: " + authorizationResponse);
            // Parses URI params into a dictionary
            // ref: http://stackoverflow.com/a/11957114/72176
            Dictionary<string, string> queryStringParams =
                    queryString.Substring(1).Split('&')
                         .ToDictionary(c => c.Split('=')[0],
                                       c => Uri.UnescapeDataString(c.Split('=')[1]));
            if (queryStringParams.ContainsKey("error"))
            {
                output(String.Format("OAuth authorization error: {0}.", queryStringParams["error"]));
                return;
            }
            if (!queryStringParams.ContainsKey("code")
                || !queryStringParams.ContainsKey("state"))
            {
                output("Malformed authorization response. " + queryString);
                return;
            }
            // Gets the Authorization code & state
            string code = queryStringParams["code"];
            string incoming_state = queryStringParams["state"];
            // Retrieves the expected 'state' value from local settings (saved when the request was made).
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string expected_state = (String)localSettings.Values["state"];
            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization
            if (incoming_state != expected_state)
            {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            // Resets expected state value to avoid a replay attack.
            localSettings.Values["state"] = null;
            // Authorization Code is now ready to use!
            output(Environment.NewLine + "Authorization code: " + code);
            string code_verifier = (String)localSettings.Values["code_verifier"];
            performCodeExchangeAsync(code, code_verifier);
            //}
            //else
            //{
            //    Debug.WriteLine(e.Parameter);
            //}
        }
        async static void performCodeExchangeAsync(string code, string code_verifier)
        {
            // Builds the Token request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier
                );
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            output(Environment.NewLine + "Exchanging code for tokens...");
            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();
            output(responseString);
            if (!response.IsSuccessStatusCode)
            {
                output("Authorization code exchange failed.");
                return;
            }
            // Sets the Authentication header of our HTTP client using the acquired access token.
            JsonObject tokens = JsonObject.Parse(responseString);
            accessToken = tokens.GetNamedString("access_token");
            
        }
        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public static void output(string output)
        {
            //textBoxOutput.Text = textBoxOutput.Text + output + Environment.NewLine;
            Debug.WriteLine(output);
        }
        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);
            return base64urlencodeNoPadding(buffer);
        }
        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static IBuffer sha256(string inputStirng)
        {
            HashAlgorithmProvider sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }
        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(IBuffer buffer)
        {
            string base64 = CryptographicBuffer.EncodeToBase64String(buffer);
            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");
            return base64;
        }

        /// <summary>
        /// Given the search URL, returns the first page of the search query.
        /// </summary>
        /// <param name="searchUrl"></param>
        /// <returns></returns>
        public static async Task<Page<GoogleDriveFileResult>> GetFileSearchResult(String searchUrl)
        {
            if (accessToken != null && !accessToken.Equals(""))
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = true;
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                // Makes a call to the Userinfo endpoint, and prints the results.
                output("Making API Call to Userinfo...");

                HttpResponseMessage userinfoResponse = client.GetAsync(searchUrl).Result;

                string userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
                //Parse the return JSON
                var driveReturnJson = JObject.Parse(userinfoResponseContent);
                //Get the drive items
                var driveItems = driveReturnJson["items"];
                var files = new List<GoogleDriveFileResult>();
                //Add titles to the list
                foreach(var item in driveItems)
                {
                    files.Add(GoogleDriveFileResult.fromJson(item));
                }
                //Return a page depending on whether there is a next page or not.
                if (driveReturnJson["nextLink"] != null)
                {
                    return new Page<GoogleDriveFileResult>(files, driveReturnJson["nextLink"].ToString());
                }
                else
                {
                    return new Page<GoogleDriveFileResult>(files);
                }

            }
            else
            {
                //If the access token has not been found, try to get the authorization from the user.
                GoogleDriveCommunicator.run();
                return null;
            }
        }

        /// <summary>
        /// Searches the drive for any titles that contain the search string. Returns the first page with maxResults number of items.
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public static async Task<Page<GoogleDriveFileResult>> SearchDrive(String searchString, int maxResults)
        {
            var builder = new StringBuilder();
            builder.Append(searchListFilesEndpoint);
            builder.Append("maxResults=");
            builder.Append(maxResults);
            builder.Append("&");
            builder.Append("q=");
            var qValue = "title contains '" + searchString + "'";
            var encodedValue = Uri.EscapeDataString(qValue);
            builder.Append("encodedValue");
            var finalURL = searchListFilesEndpoint + encodedValue;
            return await GetFileSearchResult(finalURL);
        }
    }
}
