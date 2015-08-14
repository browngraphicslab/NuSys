﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageNodeViewModel : NodeViewModel
    {
        private ImageModel _imgm;
        private CompositeTransform _inkScale;

        public ImageNodeViewModel(WorkspaceViewModel vm, BitmapImage igm) : base(vm)
        {
            this.View = new ImageNodeView2(this);
            this.Transform = new MatrixTransform();
            this.Width = igm.PixelWidth;
            this.Height = igm.PixelHeight;
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.ImageModel = new ImageModel(igm, 0);
            this.Model = this.ImageModel;
            var C = new CompositeTransform
            {
                ScaleX = 1,
                ScaleY = 1,
                CenterX = 0,
                CenterY = 0
            };
            this.InkScale = C;
        }

        public ImageNodeViewModel(WorkspaceViewModel vm) : base(vm)
        {
            this.View = new ImageNodeView2(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public async Task InitializeImageNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            if (!Constants.ImageFileTypes.Contains(storageFile.FileType.ToLower())) return;
            using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                this.ImageModel = new ImageModel(bitmapImage, 0);
                ImageModel.FilePath = storageFile.Path;
                this.Width = bitmapImage.PixelWidth;
                this.Height = bitmapImage.PixelHeight;
                var C = new CompositeTransform
                {
                    ScaleX = 1,
                    ScaleY = 1
                };
                this.InkScale = C;
            }
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * ImageModel.Image.PixelWidth / ImageModel.Image.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * ImageModel.Image.PixelHeight / ImageModel.Image.PixelWidth;
            }
            if (newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / WorkSpaceViewModel.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            this.InkScale = ct;

            base.Resize(newDx, newDy);
        }

        public override string CreateXML()
        {
            string XML = "";
            ImageModel currModel = (ImageModel)this.Model;

            XML = XML + "<" + " id='" + currModel.ID + "' x='" + (int)currModel.Transform.Matrix.OffsetX +
                    "' y='" + (int)currModel.Transform.Matrix.OffsetY + "' width='" + (int)currModel.Width + "' height='" + (int)currModel.Height +
                    "'Image='" + currModel.GetContentSource() + "'content='" + currModel.Content + "'>";
            return XML;

        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            ImageModel currModel = (ImageModel)this.Model;

            //Main XmlElement 
            XmlElement imageNode = doc.CreateElement(string.Empty, currModel.GetType().ToString(), string.Empty); //TODO: fix how we get element name
            doc.AppendChild(imageNode);

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                imageNode.SetAttributeNode(attr);
            }

            //Source for image
            XmlAttribute source = doc.CreateAttribute("Source");
            source.Value = currModel.FilePath;
            imageNode.SetAttributeNode(source);

            return imageNode;
        }

        public ImageModel ImageModel
        {
            get { return _imgm; }
            set
            {
                if (_imgm == value)
                {
                    return;
                }
                _imgm = value;
                RaisePropertyChanged("ImageModel");
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