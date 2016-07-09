using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkControllerDataStructure
    {
        private List<LinkController>[,] _array;

        //to store the index of a libraryElementId
        private Dictionary<string, int> _contentToIndex = new Dictionary<string, int>();

        //To map content ID to list of element IDs
        private Dictionary<string, List<string>> _contentToAliases = new Dictionary<string, List<string>>();

        //to store linkable id to the linkable
        private Dictionary<string, ILinkable> _linkables = new Dictionary<string, ILinkable>();

        private int _size = 64;
        public void LinkLibraryElementController(int initialSize)
        {
            initialSize = Math.Max(initialSize, _size);
            _array = new List<LinkController>[initialSize,initialSize];
        }

        /// <summary>
        /// to eb called to double the size of the array (actually quadruple memory-wise)
        /// </summary>
        private void Expand()
        {
            _size *= 2;
            ResizeBidimArrayWithElements(ref _array,_size, _size);
        }

        /// <summary>
        /// #StackOverflow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        private void ResizeBidimArrayWithElements<T>(ref T[,] original, int rows, int cols)
        {
            T[,] newArray = new T[rows, cols];
            int minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
            int minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

            for (int i = 0; i < minX; ++i)
            {
                Array.Copy(original, i*original.GetLength(1), newArray, i*newArray.GetLength(1), minY);
            }

            original = newArray;
        }

        public bool AddAlias(ElementController alias)
        {
            var contentId = alias?.LibraryElementModel?.LibraryElementId;
            Debug.Assert(contentId != null);
            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementController(contentId) != null);
            Debug.Assert(SessionController.Instance.IdToControllers[alias.Id] != null);

            if (!_contentToAliases.ContainsKey(contentId))
            {
                _contentToAliases[contentId] = new List<string>() {alias.Id};
            }
            else
            {
                _contentToAliases[contentId].Add(alias.Id);
            }

            // TODO finish writing this, temp return currently
            return false;
        }
    }
}
