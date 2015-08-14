using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Net.Http;
using Windows.Networking.Connectivity;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace NuSysApp
{

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private NetworkConnector _networkConnector; //TODO find better way than have instance variable
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public static Microsoft.ApplicationInsights.TelemetryClient TelemetryClient;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();

            this.InitializeComponent();
            this.Suspending += OnSuspending;

          //  var r = new ResourceLoader();
           // r.GetString("paragraph");
           //  Debug.WriteLine("paragraph uploaded"  + r.GetString("paragraph"));
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(WaitingRoomView), e.Arguments);
                _networkConnector = ((WaitingRoomView)rootFrame.Content).NetworkConnector;
            }
            // Ensure the current window is active
            Window.Current.Activate();

            //var storageFile =
            //    await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///MISC/CortanaCommands.xml"));
            //if (storageFile == null) return;
            //await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);

        }

        ///// <summary>
        ///// Invoked when the application is activated.
        ///// </summary>
        ///// <param name="e">Details about the launch request and process.</param>
        //protected override void OnActivated(IActivatedEventArgs e)
        //{
        //    // Was the app activated by a voice command?
        //    if (e.Kind != Windows.ApplicationModel.Activation.ActivationKind.VoiceCommand)
        //    {
        //        return;
        //    }

        //    var commandArgs = e as VoiceCommandActivatedEventArgs;
        //    var speechRecognitionResult = commandArgs.Result;

        //    // The commandMode is either "voice" or "text", and it indicates how the voice command was entered by the user.
        //    // We should respect "text" mode by providing feedback in a silent form.
        //    var commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);

        //    // If so, get the name of the voice command, the actual text spoken, and the value of Command/Navigate@Target.
        //    var voiceCommandName = speechRecognitionResult.RulePath[0];
        //    var textSpoken = speechRecognitionResult.Text;
        //    var navigationTarget = this.SemanticInterpretation("NavigationTarget", speechRecognitionResult);

        //    var navigateToPageType = typeof(WorkspaceView);
        //    var navigationParameterString = string.Empty;

        //    switch (voiceCommandName)
        //    {
        //        case "showASection":
        //        case "goToASection":
        //            string newspaperSection = this.SemanticInterpretation("newspaperSection", speechRecognitionResult);
        //            navigateToPageType = typeof(ShowASectionPage);
        //            navigationParameterString = string.Format("{0}|{1}", commandMode, newspaperSection);
        //            break;

        //        case "message":
        //        case "text":
        //            string contact = this.SemanticInterpretation("contact", speechRecognitionResult);
        //            string msgText = this.SemanticInterpretation("msgText", speechRecognitionResult);
        //            navigateToPageType = typeof(MessagePage);
        //            navigationParameterString = string.Format("{0}|{1}|{2}", commandMode, contact, msgText);
        //            break;

        //        case "playAMovie":
        //            string movieSearch = this.SemanticInterpretation("movieSearch", speechRecognitionResult);
        //            navigateToPageType = typeof(PlayAMoviePage);
        //            navigationParameterString = string.Format("{0}|{1}", commandMode, movieSearch);
        //            break;

        //        default:
        //            // There is no match for the voice command name.
        //            break;
        //    }

        //    this.EnsureRootFrame(e.PreviousExecutionState);
        //    if (!this.rootFrame.Navigate(navigateToPageType, navigationParameterString))
        //    {
        //        throw new Exception("Failed to create voice command page");
        //    }
        //}

        private string SemanticInterpretation(string key, Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult)
        {
            if (speechRecognitionResult.SemanticInterpretation.Properties.ContainsKey(key))
            {
                return speechRecognitionResult.SemanticInterpretation.Properties[key][0];
            }
            else
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            string URL = "http://aint.ch/nusys/clients.php";
            string urlParameters = "?action=remove&ip=" + NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null).RawName; 
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            _networkConnector.RemoveIP(
                NetworkInformation.GetHostNames()
                    .FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null)
                    .RawName);



            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
