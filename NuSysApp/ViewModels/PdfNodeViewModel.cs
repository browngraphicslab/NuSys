﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.MISC;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;
        public PdfNodeViewModel(PdfNodeModel model, WorkspaceViewModel workspaceViewModel) : base(model, workspaceViewModel)
        {
            model.OnPdfImagesCreated += delegate
            {
                RenderedBitmapImage = model.RenderedPages[0];
            };
            if (model.RenderedPages.Count > 0)
                RenderedBitmapImage = model.RenderedPages[0];

            this.View = new PdfNodeView2(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            this.NodeType = NodeType.PDF;
            this.CurrentPageNumber = 0;
            this.InkContainer = new List<HashSet<InqLine>>((int)PageCount);
            _workspaceViewModel = workspaceViewModel;
            var C = new CompositeTransform {
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
        }

        public void FlipRight()
        {
            if (CurrentPageNumber >= (PageCount - 1)) return;
            RenderedBitmapImage = RenderedPages[(int)++CurrentPageNumber];
            RaisePropertyChanged("RenderedBitmapImage");
        }

        public void FlipLeft()
        {
            if (CurrentPageNumber == 0) return;
            RenderedBitmapImage = RenderedPages[(int)--CurrentPageNumber];
            RaisePropertyChanged("RenderedBitmapImage");
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * RenderedBitmapImage.PixelWidth / RenderedBitmapImage.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * RenderedBitmapImage.PixelHeight / RenderedBitmapImage.PixelWidth;
            }
            if (newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / WorkSpaceViewModel.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            base.Resize(newDx, newDy);
        }

        public BitmapImage RenderedBitmapImage
        {
            get; set;
        }

        public List<BitmapImage> RenderedPages
        {
            get { return ((PdfNodeModel)Model).RenderedPages; }
            set
            {
                ((PdfNodeModel)Model).RenderedPages = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public uint CurrentPageNumber
        {
            get { return ((PdfNodeModel)Model).CurrentPageNumber; }
            set
            {
                ((PdfNodeModel)Model).CurrentPageNumber = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public uint PageCount
        {
            get { return ((PdfNodeModel)Model).PageCount; }
            set
            {
                ((PdfNodeModel)Model).PageCount = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public List<HashSet<InqLine>> InkContainer
        {
            get { return ((PdfNodeModel)Model).InkContainer; }
            set
            {
                ((PdfNodeModel)Model).InkContainer = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public CompositeTransform InkScale
        {
            get { return _inkScale; }
            set
            {
                if (_inkScale == value)
                {
                    return;
                }
                _inkScale = value;
                RaisePropertyChanged("InkScale");
            }
        }
    }
}
