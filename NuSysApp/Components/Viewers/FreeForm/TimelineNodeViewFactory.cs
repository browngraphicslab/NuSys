﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;
using NuSysApp.Controller;


namespace NuSysApp
{
    public class TimelineNodeViewFactory
    {
        public async Task<Image> CreateFromSendable(ElementController controller)
        {
            Image view = null;

            var model = controller.Model;

            switch (model.ElementType)
            {
                case NusysConstants.ElementType.Text:
                    view = new Image();
                    BitmapImage textimage = new BitmapImage(new Uri("ms-appx:///Assets/icon_text.png", UriKind.Absolute));
                    view.Source = textimage;
                    break;
                case NusysConstants.ElementType.Collection:
                    view = new Image();
                    //TODO change icon
                    BitmapImage collectionImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_tag.png", UriKind.Absolute));
                    view.Source = collectionImage;
                    break;
                case NusysConstants.ElementType.Tag:
                    view = new Image();
                    BitmapImage tagImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_tag.png", UriKind.Absolute));
                    view.Source = tagImage;
                    break;
                case NusysConstants.ElementType.Image:
                    view = new Image();
                    BitmapImage imageImage = new BitmapImage(new Uri(controller.LibraryElementController.Data));
                    view.Source = imageImage;
                    break;
                case NusysConstants.ElementType.Word:
                    view = new Image();
                    BitmapImage wordImage = new BitmapImage(new Uri("ms-appx:///Assets/wordIcon.png", UriKind.Absolute));
                    view.Source = wordImage;
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    view = new Image();
                    BitmapImage pptImage = new BitmapImage(new Uri("ms-appx:///Assets/powerpointIcon.png", UriKind.Absolute));
                    view.Source = pptImage;
                    break;
                case NusysConstants.ElementType.Audio:
                    view = new Image();
                    BitmapImage audioImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_recording.png", UriKind.Absolute));
                    view.Source = audioImage;
                    break;
                case NusysConstants.ElementType.PDF:
                    view = new Image();
                    var data = controller.LibraryElementController.Data;
                    var dataBytes = Convert.FromBase64String(data);
                    var ms = new MemoryStream(dataBytes);
                        //deleted a bunch of shit for beta release.  this is old code anyways
                        break;

                case NusysConstants.ElementType.Video:
                    //TODO change icon
                    view = new Image();
                    BitmapImage videoImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_recording.png", UriKind.Absolute));
                    view.Source = videoImage;
                    break;
                case NusysConstants.ElementType.Web:
                    view = new Image();
                    BitmapImage webImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_web_color.png", UriKind.Absolute));
                    view.Source = webImage;
                    break;
                //case ElementType.Area:
                //    view = new AreaNodeView(new AreaNodeViewModel((ElementCollectionController)controller));
                //    break;
                //case ElementType.Link:
                //    view = new BezierLinkView(new LinkViewModel((LinkController)controller));
                //     break;
            }
            //await ((ElementViewModel)view.DataContext).Init();
            view.Width = 130;
            view.Height = 80;
            return view;
        }  
    }
}
