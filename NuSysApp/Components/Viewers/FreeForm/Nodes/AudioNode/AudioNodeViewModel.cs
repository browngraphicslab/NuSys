﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Storage;
using Windows.Storage.Streams;
using NAudio;
using NAudio.Wave;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using System.Diagnostics;

namespace NuSysApp
{
    public class AudioNodeViewModel : ElementViewModel
    {
        private Grid _visualGrid;

        public delegate void VisualizationLoadedEventHandler();

        public event VisualizationLoadedEventHandler OnVisualizationLoaded;

        public AudioNodeViewModel(ElementController controller) : base(controller)
        {
            Width = controller.Model.Width;
            Height = controller.Model.Height;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public override void SetSize(double width, double height)
        {
            var model = Model as AudioNodeModel;
            if (height < 200)
            {
                height = 200;
            }
            if (width < 150)
            {
                width = 150;
            }
            base.SetSize(width, height);
        }

        public Uri AudioSource
        {
            get { return Controller.LibraryElementController.GetSource(); }
        }

        public override async Task Init()
        {
            if (SessionController.Instance.ContentController.ContainsAndLoaded(ContentId))
            {
                InitWhenReady(this);
            }
            else
            {
                Controller.LibraryElementController.Loaded += InitWhenReady;
            }
        }

        private async void InitWhenReady(object sender)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(AudioSource);
            HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
            Stream resStream = response.GetResponseStream();

            byte[] dataBytes = new byte[(int) response.ContentLength];
            resStream.Read(dataBytes, 0, (int) response.ContentLength);
            resStream.Dispose();
            Controller.LibraryElementController.Loaded -= InitWhenReady;
            //Visualize(dataBytes);
        }

#region cool wavestream stuff

        private async void Visualize(byte[] bytes)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();


            WaveStream waveStream = new MediaFoundationReaderUniversal(stream);
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample/8)*waveStream.WaveFormat.Channels;
            waveStream.Position = 0;
            int bytesRead = 1;
            int samplesPerPixel = 1024;

            if (waveStream.TotalTime.TotalMinutes > 15)
            {
                samplesPerPixel = 65536;
            }
            else if (waveStream.TotalTime.TotalMinutes > 8)
            {
                samplesPerPixel = 32768;
            }
            else if (waveStream.TotalTime.TotalMinutes > 5)
            {
                samplesPerPixel = 16384;
            }
            else if (waveStream.TotalTime.TotalMinutes > 3)
            {
                samplesPerPixel = 8192;
            }
            else if (waveStream.TotalTime.TotalMinutes > 0.5)
            {
                samplesPerPixel = 2048;
            }

            byte[] waveData = new byte[samplesPerPixel*bytesPerSample];
            _visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel*bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float) low) - short.MinValue)/ushort.MaxValue);
                float highPercent = ((((float) high) - short.MinValue)/ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100*(highPercent);
                line.Y2 = 100*(lowPercent);
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 1;
                x++;
                _visualGrid.Children.Add(line);

            }
            _visualGrid.Height = 100;
            _visualGrid.Width = x;
            Line middleLine = new Line();
            middleLine.X1 = 0;
            middleLine.X2 = x;
            middleLine.Y1 = _visualGrid.Height/2;
            middleLine.Y2 = _visualGrid.Height/2;

            middleLine.Stroke = new SolidColorBrush(Colors.Crimson);
            middleLine.StrokeThickness = 1;
            _visualGrid.Children.Add(middleLine);

            OnVisualizationLoaded?.Invoke();
        }

        public Grid VisualGrid
        {
            get { return _visualGrid; }
        }

#endregion cool wavestream stuff

    }

}
