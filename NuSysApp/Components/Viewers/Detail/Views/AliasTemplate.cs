using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using WinRTXamlToolkit.Controls.DataVisualization;

namespace NuSysApp
{
    public class AliasTemplate : BaseINPC
    {
        private string _collectionTitle;
        
        public string Creator { get; private set; }
        public string Timestamp { get; private set; }
        public ElementModel ElementModel { get; private set; }
        public string CollectionTitle
        {
            get { return _collectionTitle; }
            set
            {
                _collectionTitle = value;
                RaisePropertyChanged("CollectionTitle");
            }
        }

        public string CollectionID { get; set; }

        public AliasTemplate(ElementModel model)
        {
            Debug.Assert(model != null);
            CollectionID = model.ParentCollectionId;
            
            var collectionController = SessionController.Instance.ContentController.GetLibraryElementController(CollectionID);
            
            Debug.Assert(collectionController != null);
            CollectionTitle = collectionController.Title;
            collectionController.TitleChanged += Controller_TitleChanged;
            Timestamp = collectionController.LibraryElementModel.LastEditedTimestamp;

            ElementModel = model;

            Creator = SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model?.CreatorId);
        }

        private void Controller_TitleChanged(object sender, string title)
        {
            CollectionTitle = title;
        }

    }
}
