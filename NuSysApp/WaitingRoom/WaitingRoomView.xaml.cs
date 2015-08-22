﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingRoomView : Page
    {
        public WorkspaceView _workspaceView;
        public WaitingRoomView()
        {
            this.InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (WorkspaceView));
        }
        private void Local_OnClick(object sender, RoutedEventArgs e)
        {
            IsLocal = true;
            this.Frame.Navigate(typeof(WorkspaceView));
        }
        private void clear_OnClick(object sender, RoutedEventArgs e)
        {
            const string URL = "http://aint.ch/nusys/clients.php";
            var urlParameters = "?action=clear";
            var client = new HttpClient { BaseAddress = new Uri(URL) };
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(urlParameters).Result;
        }
        public static bool IsLocal { get; set; }
    }
}
