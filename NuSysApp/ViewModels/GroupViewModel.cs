using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
        public GroupViewModel(WorkspaceViewModel vm): base(vm)
        {
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.View = new GroupView(this);
        }

        public void AddNode(NodeViewModel toAdd)
        {
            AtomViewList.Add(toAdd.View);
            NodeViewModelList.Add(toAdd);
            toAdd.Transform = new MatrixTransform();
            ArrangeNodes();
            Canvas.SetZIndex(toAdd.View, 10);

            //TODO Handle links
        }
        
        public void ArrangeNodes()
        {

            var currentX = 0.0;
            var currentY = 0.0;
            for (var i = 0; i < AtomViewList.Count;i++) {
                var toArr = NodeViewModelList[i];
                Canvas.SetLeft(toArr.View, currentX);
                Canvas.SetTop(toArr.View, currentY);

                if (Height < currentY + toArr.Height + 20)
                {
                    Height = currentY + toArr.Height + 20;
                }
                if (Width < currentX + toArr.Width + 20)
                {
                    Width = currentX + toArr.Width + 20;
                }
                if (i % 3 == 2)
                {
                    currentX = 0;
                    currentY += toArr.Height + 20;
                }
                else
                {
                    currentX += toArr.Width + 20;
                }
            }
        }

        public void RemoveNode(NodeViewModel toRemove)
        {
            this.AtomViewList.Remove(toRemove.View);
            NodeViewModelList.Remove(toRemove);
            ArrangeNodes();
            //TODO Handle links
        }

        public ObservableCollection<UserControl> AtomViewList { get; private set;}
        public ObservableCollection<LinkViewModel> LinkViewModelList { get; private set; }
        public ObservableCollection<NodeViewModel> NodeViewModelList { get; private set; }
    }
}