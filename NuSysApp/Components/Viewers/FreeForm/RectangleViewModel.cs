using System.Collections.Generic;

namespace NuSysApp.Viewers
{

    public class RectangleViewModel : BaseINPC
    {
        public double RectWidth { get { return _rectWidth; } set { _rectWidth = value; RaisePropertyChanged("RectWidth"); } }
        public double RectHeight { get { return _rectHeight; } set { _rectHeight = value; RaisePropertyChanged("RectHeight"); } }
        public double Left { get { return _left; } set { _left = value; RaisePropertyChanged("Left"); } }
        public double Top { get { return _top; } set { _top = value; RaisePropertyChanged("Top"); } }
        private double _rectWidth;
        private double _rectHeight;
        private double _top;
        private double _left;

        public double RectWidthRatio { get { return _rectWidthRatio; } set { _rectWidthRatio = value; RaisePropertyChanged("RectWidthRatio"); } }
        public double RectHeightRatio { get { return _rectHeightRatio; } set { _rectHeightRatio = value; RaisePropertyChanged("RectHeightRatio"); } }
        public double LeftRatio { get { return _leftRatio; } set { _leftRatio = value; RaisePropertyChanged("LeftRatio"); } }
        public double TopRatio { get { return _topRatio; } set { _topRatio = value; RaisePropertyChanged("TopRatio"); } }
        public double NodeWidth { get { return _nodeWidth; } set { _nodeWidth = value; RaisePropertyChanged("NodeWidth"); } }
        public double NodeHeight { get { return _nodeHeight; } set { _nodeHeight = value; RaisePropertyChanged("NodeHeight"); } }

        public int PdfPageNumber { get { return _pdfPageNumber; } set { _pdfPageNumber = value; RaisePropertyChanged("PdfPageNumber"); } }

        public HashSet<string> ConnectedIds { get { return _connectedIds; } set { _connectedIds = value; RaisePropertyChanged("ConnectedIds"); } }

        public HashSet<string> _connectedIds;

        private int _pdfPageNumber;
        private double _rectWidthRatio;
        private double _rectHeightRatio;
        private double _topRatio;
        private double _leftRatio;
        private double _nodeWidth;
        private double _nodeHeight;

        public Dictionary<string, double> Attributes { get; set; } 

        public RectangleModel Model { get; set; }

        public RectangleViewModel(RectangleModel model, Dictionary<string, double> attributes)
        {
            Model = model;
            Attributes = attributes;

            if (attributes == null)
                return;

            ConnectedIds = new HashSet<string>();

            RectHeightRatio = Attributes["heightRatio"];
            RectWidthRatio = Attributes["widthRatio"];
            TopRatio = Attributes["topRatio"];
            LeftRatio = Attributes["leftRatio"];
            NodeWidth = Attributes["nodeWidth"];
            NodeHeight = Attributes["nodeHeight"];

            if (Attributes.ContainsKey("pdfPageNumber"))
            {
                PdfPageNumber = (int)Attributes["pdfPageNumber"];
            }

            RectWidth = NodeWidth*RectWidthRatio;
            RectHeight = NodeHeight*RectHeightRatio;
            Top = NodeHeight*TopRatio;
            Left = NodeWidth*LeftRatio;
        }
    }
}