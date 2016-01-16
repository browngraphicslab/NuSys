using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Components.Misc
{
    public class SearchWindowViewModel
    {
        public ObservableCollection<string> SearchResults { get; set; }

        public SearchWindowViewModel()
        {
            SearchResults = new ObservableCollection<string>();
        }

        public void SearchFor(string queryString)
        {
            SearchResults.Clear();
            if (queryString == "")
                return;
            var found = new HashSet<AtomModel>();
            foreach (var kv in SessionController.Instance.IdToSendables)
            {
                var kvp = (KeyValuePair<string, Sendable>) kv;
                var atom = (AtomModel) kvp.Value;
                var tags = (List<string>) atom.GetMetaData("tags");

                foreach (var tag in tags)
                {
                    if (tag.ToLower().Contains(queryString))
                    {
                        found.Add(atom);
                    }
                }

                if (atom.Title.ToLower().Contains(queryString))
                {
                    found.Add(atom);
                }
            }

            foreach (var atomModel in found)
            {
                SearchResults.Add(atomModel.Title);
            }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
