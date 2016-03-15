﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _stopped;
        private bool _loaded;
        private List<LinkedTimeBlockViewModel> _timeBlocks;


        public AudioNodeView(AudioNodeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _stopped = true;
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
            _loaded = false;
            (DataContext as AudioNodeViewModel).addTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);


        }


        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as AudioNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);

            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
        }

        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            playbackElement.Stop();
            _stopped = true;
            play.Opacity = 1;
            pause.Opacity = 1;
            e.Handled = true;

        }

        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {

            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            play.Opacity = .3;
            pause.Opacity = 1;
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                play.Opacity = 1;
            };
            playbackElement.Play();
        }



        private async void RenderImageSource(Grid RenderedGrid)
        {

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            double x = grid.Height;
            grid.Width = RenderedGrid.Width * 2;
            await renderTargetBitmap.RenderAsync(RenderedGrid, 1000, 100);
            grid.Width = x;
            VisualizationImage.Source = renderTargetBitmap;
            grid.Children.Remove(RenderedGrid);

        }


        private async void OnPause_Click(object sender, RoutedEventArgs e)
        {

            play.Opacity = 1;
            pause.Opacity = 0.3;
            playbackElement.Pause();


        }



        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (NodeViewModel)this.DataContext;

                if (vm.IsSelected)
                {
                    //var slideout = (Storyboard)Application.Current.Resources["slideout"];
                    slideout.Begin();
                }
                else
                {
                    //var slidein = (Storyboard)Application.Current.Resources["slidein"];
                    slidein.Begin();
                }
            }*/
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            return new RenderTargetBitmap();//TODO implement
        }

        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = time;
        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                double seconds = playbackElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                playbackElement.Position = time;
            }
            e.Handled = true;
        }

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                play.Opacity = 1;
            };
        }



        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var element in _timeBlocks)
            {
                element.ResizeLine1();
            }
        }
    }
}
