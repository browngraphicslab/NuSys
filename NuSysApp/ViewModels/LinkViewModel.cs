﻿using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;


namespace NuSysApp
{
    /// <summary>
    /// link view model class
    /// 
    /// parameters: Atom1 and Atom2 are the two atoms that the link connects, 
    /// and workspace is main workspace.
    /// 
    /// </summary>
    public class LinkViewModel : AtomViewModel
    {
        #region Private Members
  
        private AtomViewModel _atom1, _atom2;

        #endregion Private members

        public LinkViewModel(AtomViewModel atom1,
            AtomViewModel atom2, WorkspaceViewModel workspace): base(workspace)
        {
            this.Atom1 = atom1;
            this.Atom2 = atom2;
            this.Atom1.UpdateAnchor();
            this.Atom2.UpdateAnchor();

            switch (workspace.CurrentLinkMode)
            {
                case WorkspaceViewModel.LinkMode.BEZIERLINK:
                    this.View = new BezierLinkView(this);
                    break;
                default:
                    this.View = new LineLinkView(this);
                    break;
            }
        }

        public override void Remove()
        {
            this.Atom1.LinkList.Remove(this);
            this.Atom2.LinkList.Remove(this);
        }

        #region Public Properties

        public AtomViewModel Atom1
        {
            get { return _atom1; }
            set
            {
                if (_atom1 == value)
                {
                    return;
                }
                _atom1 = value;
                RaisePropertyChanged("Atom1");
            }
        }

        public AtomViewModel Atom2
        {
            get { return _atom2; }
            set
            {
                if (_atom2 == value)
                {
                    return;
                }
                _atom2 = value;
                RaisePropertyChanged("Atom2");
            }
        }

        public Line LineRepresentation
            => new Line() {X1 = Atom1.AnchorX, X2 = Atom2.AnchorX, Y1 = Atom1.AnchorY, Y2 = Atom2.AnchorY};

        #endregion Public Properties

        public override Rect GetBoudingRect()
        {
            throw new NotImplementedException();
        }

        public override void UpdateAnchor()
        {
            throw new NotImplementedException();
        }
    }
}