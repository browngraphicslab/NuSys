using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using NAudio;
using NAudio.Wave;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using NetTopologySuite.Utilities;
using NusysIntermediate;
using BitConverter = System.BitConverter;
using Line = Windows.UI.Xaml.Shapes.Line;

namespace NuSysApp
{
    public class AudioNodeViewModel : ElementViewModel
    {
        /// <summary>
        /// The audio library element controller associated with this audio node
        /// </summary>
        private AudioLibraryElementController _audioLibraryElementController;

        /// <summary>
        /// the normalized start time of this audio node's data
        /// </summary>
        public double NormalizedStartTime { get; private set; }

        /// <summary>
        /// the normalized duration of this audio node's data
        /// </summary>
        public double NormalizedDuration { get; set; }

        /// <summary>
        /// Invoked whenever the view model is dirty!, when the start time or duration changes
        /// </summary>
        public event EventHandler ViewModelIsDirty;


        /// <summary>
        /// The view model for the Audio Node
        /// </summary>
        /// <param name="controller"></param>
        public AudioNodeViewModel(ElementController controller) : base(controller)
        {
            Width = controller.Model.Width;
            Height = controller.Model.Height;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

            // get the audioLibraryElementModel and set the NormalizedStartTime and Duration based off of it
            var audioLibraryElementModel = controller.LibraryElementModel as AudioLibraryElementModel;
            Debug.Assert(audioLibraryElementModel != null);
            if (audioLibraryElementModel == null) // so we don't crash in beta, but this should never be null
                return;

            NormalizedStartTime = audioLibraryElementModel.NormalizedStartTime;
            NormalizedDuration = audioLibraryElementModel.NormalizedDuration;

            // get the audioLibraryElementController and listen to the events for start time and duration changed
            _audioLibraryElementController = controller.LibraryElementController as AudioLibraryElementController;
            Debug.Assert(_audioLibraryElementController != null);
            if (_audioLibraryElementController == null) // so we don't crash in beta, but this should never be null
                return;


            _audioLibraryElementController.TimeChanged += OnStartTimeChanged;
            _audioLibraryElementController.DurationChanged += OnDurationChanged;
        }

        /// <summary>
        /// called whenever the start time changes on the audio node view model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        private void OnStartTimeChanged(object sender, double start)
        {
            ViewModelIsDirty?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// called whenever the duration changes on the audio node view model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        private void OnDurationChanged(object sender, double start)
        {
            ViewModelIsDirty?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called to instantiate resources for the view model, loads the content data model
        /// </summary>
        /// <returns></returns>
        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
        }

        /// <summary>
        /// Remove any events we were listening to and clean up resources
        /// </summary>
        public override void Dispose()
        {
            if (_audioLibraryElementController != null)
            {
                _audioLibraryElementController.TimeChanged -= OnStartTimeChanged;
                _audioLibraryElementController.DurationChanged -= OnDurationChanged;
            }

            base.Dispose();
        }
    }
}
