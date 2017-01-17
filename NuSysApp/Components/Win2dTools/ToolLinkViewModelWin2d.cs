﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class ToolLinkViewModelWin2d: BaseINPC, IEditable
    {
        public ObservableCollection<ToolLinkController> LinkList { get; set; }



        private string _title;
        private string _annotation;
        private int _numDirectionButtonClicks;
        private readonly ToolLinkController _controller;
        private bool _selected;
        private SolidColorBrush _color;

        //   private SolidColorBrush _selectedColor = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x98, 0x1A, 0x4D));
        //   private SolidColorBrush _notSelectedColor = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x11, 0x3D, 0x40));

        public ToolLinkModel LinkModel
        {
            get
            {
                Debug.Assert(Controller != null && Controller.Model != null);
                return Controller.Model;
            }
        }


        public ToolLinkController Controller
        {
            get
            {
                Debug.Assert(_controller != null);
                return _controller;
            }
        }
        public Point2d Anchor
        {
            get { return Controller.Anchor; }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public ToolLinkViewModelWin2d(ToolLinkController controller)
        {
            _controller = controller;
            _numDirectionButtonClicks = 0;
            //Debug.Assert(controller.LibraryElementController != null);

            controller.TitleChanged += TitleChanged;
            //Title = controller.Title;
            IsSelected = false;
            controller.AnchorChanged += ChangeAnchor;
            RaisePropertyChanged("Anchor");
            controller.Disposed += Controller_Disposed;
        }

        private void Controller_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ChangeAnchor(object sender, Point2d e)
        {
            UpdateAnchor();
        }

        private void TitleChanged(object sender, string title)
        {
            Title = title;
        }


        public void Dispose()
        {
            Controller.TitleChanged -= TitleChanged;
            Controller.AnchorChanged -= ChangeAnchor;
            Controller.Disposed -= Controller_Disposed;
        }


        public void UpdateAnchor()
        {
            RaisePropertyChanged("Anchor");
        }

        public void UpdateTitle(string title)
        {
            Controller.TitleChanged -= TitleChanged;
            //Controller.LibraryElementController.SetTitle(title);
            Controller.TitleChanged += TitleChanged;
        }

        public void DirectionButtonClicked()
        {
            /*
            var linkLibElemCont = _controller.LibraryElementController as LinkLibraryElementController;
            Debug.Assert(linkLibElemCont != null);
            //switching to monodirectional
            if (linkLibElemCont.LinkLibraryElementModel.LinkedDirection.Equals(LinkDirectionEnum.Mono1))
            {
                linkLibElemCont.RaiseLinkDirectionChanged(linkLibElemCont, LinkDirectionEnum.Mono2);
            }
            //swapping direction
            else if (linkLibElemCont.LinkLibraryElementModel.LinkedDirection.Equals(LinkDirectionEnum.Mono2))
            {
                linkLibElemCont.RaiseLinkDirectionChanged(linkLibElemCont, LinkDirectionEnum.Bi);
            }
            //going back to bidirectional
            else
            {
                linkLibElemCont.RaiseLinkDirectionChanged(linkLibElemCont, LinkDirectionEnum.Mono1);
            }*/
        }


        public bool ContainsSelectedLink { get; }

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        /// <summary>
        /// From the ISelectable Interface, used to cull the screen, basically if something is null it is always visible
        ///  but calculating all the points for a link would require a bunch of math.
        /// </summary>
        public PointCollection ReferencePoints
        {

            get { return null; }
        }

        /// <summary>
        /// From the ISelectable Interface, used to implement selection in the free form viewer
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                // Change the color of the link based on selection
                //    Color = _selected == true ? _selectedColor : _notSelectedColor;
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// From the IEditable interface
        /// </summary>
        public bool IsEditing { get; set; }

    }
}