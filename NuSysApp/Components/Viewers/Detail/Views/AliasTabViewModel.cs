using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using NusysIntermediate;

namespace NuSysApp
{
    public class AliasTabViewModel
    {
        public ObservableCollection<AliasTemplate> AliasTemplates { get; }
        
        public AliasTabViewModel()
        {
            AliasTemplates = new ObservableCollection<AliasTemplate>();
            

        }

        /// <summary>
        /// This method responds to the deletion of an alias
        /// </summary>
        /// <param name="model"></param>
        private void ContentControllerOnAliasDelete(ElementModel model)
        {
            
        }

        /// <summary>
        /// This method responds to the addition of a new  alias
        /// </summary>
        /// <param name="model"></param>
        private void ContentController_OnNewAlias(ElementModel model)
        {
            
        }

        /// <summary>
        /// This is the method where we make a request to the server and get the information about the aliases. 
        /// The element view models are con verted to alias templates and are added to the AliasTemplates which
        /// displays them in the list view.
        /// Takes in a library element id.
        /// </summary>
        /// <param name="newLibraryElementModelId"></param>
        public async void ChangeAliasTemplates(string libraryElemetId)
        {
            if (libraryElemetId == null)
            {
                return;
            }
            AliasTemplates.Clear();
            
            
        }
        
        /// <summary>
        /// This methods sorts the collection of alias templates by the Collection title
        /// </summary>
        public void SortByTitle()
        {
            if (AliasTemplates.Count < 1)
            {
                return;
            }

            List<AliasTemplate> list = new List<AliasTemplate>(AliasTemplates.OrderBy(template => template.CollectionTitle));

            AliasTemplates.Clear();
            foreach (var aliasTemplate in list)
            {
                AliasTemplates.Add(aliasTemplate);
            }

        }

        /// <summary>
        /// This methods sorts the collection of alias templates by the Collection creator
        /// </summary>
        public void SortByCreator()
        {
            if (AliasTemplates.Count < 1)
            {
                return;
            }

            List<AliasTemplate> list = new List<AliasTemplate>(AliasTemplates.OrderBy(template => template.Creator));

            AliasTemplates.Clear();
            foreach (var aliasTemplate in list)
            {
                AliasTemplates.Add(aliasTemplate);
            }

        }

        /// <summary>
        /// This methods sorts the collection of alias templates by the timestamp the collection was last edited
        /// </summary>
        public void SortByTimestamp()
        {
            if (AliasTemplates.Count < 1)
            {
                return;
            }

            List<AliasTemplate> list = new List<AliasTemplate>(AliasTemplates.OrderBy(template => template.Timestamp));

            AliasTemplates.Clear();
            foreach (var aliasTemplate in list)
            {
                AliasTemplates.Add(aliasTemplate);
            }

        }


    }
}
