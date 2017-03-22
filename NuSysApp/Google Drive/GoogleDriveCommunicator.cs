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

namespace NuSysApp
{
    class GoogleDriveCommunicator
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";
        private static string client_id = "502237683379-6q994pvhq1ef5r9fm2d621httc37b560.apps.googleusercontent.com";
        private static string client_secret = "P-MknZ0HJN40QLguNrpaYjSZ";
        private static string redirectionUri = "urn:ietf:wg:oauth:2.0:oob";

        public GoogleDriveCommunicator()
        {
            run();
        }

        public static Uri GetAutenticationURI(string clientId)
        {
            string scopes = DriveService.Scope.DriveReadonly;

            
            
            string oauth = string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code", clientId, redirectionUri, scopes);
           
            return new Uri(oauth);
        }

        public async Task run()
        {
            UserCredential credential;
            try
            {
                //using (var stream =
                //new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
                //{
                    try
                    {

                        //var secrets = GoogleClientSecrets.Load(stream).Secrets;
                        //var x = new GoogleAuthorizationCodeFlow.Initializer()
                        //{
                        //    ClientSecrets = secrets,
                        //    Scopes = Scopes,
                        //};
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new Uri("client_id.json"), 
                    Scopes,
                    "user",
                    CancellationToken.None
                    ).Result;
                        //credential = new UserCredential(new GoogleAuthorizationCodeFlow(x), "user", new TokenResponse());
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        credential = null;
                    }
                //}
            }
            catch
            {
                credential = null;
            }
            

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
                
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
           
            Debug.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Debug.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Debug.WriteLine("No files found.");
            }
            //Console.Read();
        }
    }
}
