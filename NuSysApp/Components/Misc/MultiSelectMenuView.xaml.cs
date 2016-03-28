﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MultiSelectMenuView : UserControl
    {
        public MultiSelectMenuView()
        {
            this.InitializeComponent();

            DeleteButton.Click += DeleteButtonOnClick;
            GroupButton.Click += GroupButtonOnClick;
        }

        private async void GroupButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var selections = SessionController.Instance.ActiveFreeFormViewer.Selections;
            var bb = Geometry.NodesToBoudingRect(selections);       

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            // TODO: add the graph/chart
            var contentId = SessionController.Instance.GenerateId();
            var newCollectionId = SessionController.Instance.GenerateId();

            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;

            var elementMsg = new Message();
            elementMsg["metadata"] = metadata;
            elementMsg["width"] = bb.Width;
            elementMsg["height"] = bb.Height;
            elementMsg["locationX"] = t.TranslateX;
            elementMsg["locationY"] = t.TranslateY;
            elementMsg["centerX"] = t.CenterX;
            elementMsg["centerY"] = t.CenterY;
            elementMsg["zoom"] = t.ScaleX;
            elementMsg["x"] = bb.X;
            elementMsg["y"] = bb.Y;
            elementMsg["contentId"] = contentId;
            elementMsg["nodeType"] = ElementType.Collection;
            elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            elementMsg["id"] = newCollectionId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Collection, "Search Results"));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SubscribeToCollectionRequest(contentId));

            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(elementMsg)); 

            var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(bb.X, bb.Y, contentId, bb.Width, bb.Height, newCollectionId, CollectionElementModel.CollectionViewType.FreeForm);

           
            foreach (var vm in selections.ToArray())
            {
                var libraryElementModel = vm.Controller.LibraryElementModel;
                var dict = new Message();
                dict["title"] = libraryElementModel?.Title;
                dict["width"] = vm.Width;
                dict["height"] = vm.Height;
                dict["nodeType"] = libraryElementModel.Type.ToString();
                dict["x"] = vm.Transform.TranslateX - bb.X + Constants.MaxCanvasSize/2.0;
                dict["y"] = vm.Transform.TranslateY - bb.Y + Constants.MaxCanvasSize / 2.0;
                dict["contentId"] = libraryElementModel.Id;
                dict["metadata"] = metadata;
                dict["autoCreate"] = true;
                dict["creator"] = controller.LibraryElementModel.Id;
                var request = new NewElementRequest(dict);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                vm.Controller.RequestDelete();
            }
        }

        private void DeleteButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var selections = SessionController.Instance.ActiveFreeFormViewer.Selections;
            foreach (var elementViewModel in selections)
            {
                elementViewModel.Controller.RequestDelete();
            }
        }

        public Button Delete
        {
            get { return DeleteButton; }
        }

        public Button Group
        {
            get { return GroupButton; }
        }
    }
}
