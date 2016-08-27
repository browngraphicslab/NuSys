using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

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
            //harsh is this an actual event handler?
        }

        /// <summary>
        /// This method responds to the addition of a new  alias
        /// </summary>
        /// <param name="model"></param>
        private void ContentController_OnNewAlias(ElementModel model)
        {
            //does this correspond to an actual event?
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
        /// Method to set all the element models.  
        /// Should be called after a server request has returned with all the element models for this tab.
        /// Will create a new alias template for each element model given, after clearing the already-existing alias templates.
        /// Will check that the ienumerable is not null, but will not null-check each entry;
        /// </summary>
        /// <param name="elementModels"></param>
        public void SetElementModels(IEnumerable<ElementModel> elementModels)
        {
            Debug.Assert(elementModels != null);
            AliasTemplates.Clear();
            elementModels.ForEach(element => AliasTemplates.Add(new AliasTemplate(element)));//for each element model, add it to the alias templates
        }

        /// <summary>
        /// Generic sort function.  
        /// Allows you to sort the list of alias templates by whatever sorting function you pass in. 
        /// The sorting function simply needs to take in an AliasTemplate, and return a string from it. 
        /// 
        /// Usage: 
        ///     SortBy(aliasTemplate => aliasTemplate?.CollectionTitle ?? "");
        /// 
        /// That usage example would safely sort by the collection title. 
        /// </summary>
        /// <param name="sortFunction"></param>
        public void SortBy(Func<AliasTemplate, string> sortFunction)
        {
            if (AliasTemplates.Count <= 1) //don't really need to sort anything with 0 or 1 items
            {
                return;
            }

            var list = new List<AliasTemplate>(AliasTemplates.OrderBy(sortFunction)); //create a list of the sorted elements

            AliasTemplates.Clear();
            foreach (var aliasTemplate in list)
            {
                AliasTemplates.Add(aliasTemplate);//add the sorted elements in order to the template
            }
        }


    }
}
