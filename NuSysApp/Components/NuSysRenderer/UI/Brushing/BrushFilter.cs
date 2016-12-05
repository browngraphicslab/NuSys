using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class BrushFilter
    {

        /// <summary>
        /// List of creators that we can search for
        /// </summary>
        public HashSet<string> Creators { get; }

        private DateTime? _creationDateStart;

        public DateTime? CreationDateStart
        {
            get { return _creationDateStart; }
            set
            {
                _creationDateStart = value;
                needsRefresh = true;
            }
        }

        private DateTime? _creationDateEnd;
        public DateTime? CreationDateEnd
        {
            get { return _creationDateEnd; }
            set
            {
                _creationDateEnd = value;
                needsRefresh = true;
            }
        }

        private DateTime? _lastEditedStart;

        public DateTime? LastEditedStart
        {
            get { return _lastEditedStart; }
            set
            {
                _lastEditedStart = value;
                needsRefresh = true;
            }
        }

        private DateTime? _lastEditedEnd;
        public DateTime? LastEditedEnd
        {
            get { return _lastEditedEnd; }
            set
            {
                _lastEditedEnd = value;
                needsRefresh = true;
            }
        }

        public HashSet<string> ParentCollectionIds { get; }

        public HashSet<NusysConstants.ElementType> Types { get; }

        /// <summary>
        /// True if the data the filter is based off of needs to be refreshed
        /// </summary>
        private bool needsRefresh;

        /// <summary>
        /// private enum used for filtering helper methods
        /// </summary>
        private enum DateType
        {
            Creation,
            LastEdit
        }

        private HashSet<LibraryElementController> _filteredLibraryElementControllers;

        private HashSet<ElementController> _filteredElementControllers;

        CollectionLibraryElementController _prevCollectionController;


        public BrushFilter()
        {
            Creators = new HashSet<string>();
            _creationDateStart = null;
            CreationDateEnd = null;
            LastEditedStart = null;
            LastEditedEnd = null;
            ParentCollectionIds = new HashSet<string>();
            Types = new HashSet<NusysConstants.ElementType>();
            needsRefresh = true;
        }

        /// <summary>
        /// Computes the brush given all the data
        /// </summary>
        /// <returns></returns>
        private HashSet<LibraryElementController> GetLibraryElementControllers()
        {
            if (!needsRefresh)
            {
                return _filteredLibraryElementControllers;
            }

            var controllers = SessionController.Instance.ContentController.AllLibraryElementControllers.Where(
                ctrl => Creators.Contains(ctrl.LibraryElementModel.Creator) && // first filter controllers by creator
                Types.Contains(ctrl.LibraryElementModel.Type)); // and by type

            controllers = FilterBetweenDates(controllers, _creationDateStart, CreationDateEnd, DateType.Creation); // filter by creation date
            controllers = FilterBetweenDates(controllers, LastEditedStart, LastEditedEnd, DateType.LastEdit); // filter by last edit date

            _filteredLibraryElementControllers = new HashSet<LibraryElementController>(controllers);

            return _filteredLibraryElementControllers;
        }

        private HashSet<ElementController> GetElementControllersForCollection(CollectionLibraryElementController collectionController)
        {
            if (!needsRefresh && collectionController == _prevCollectionController)
            {
                return _filteredElementControllers;
            }

            Debug.Assert(collectionController != null);
            _prevCollectionController = collectionController;

            var controllers = collectionController.CollectionModel.Children.Where(id => SessionController.Instance.IdToControllers.ContainsKey(id)).Select(id => SessionController.Instance.IdToControllers[id]);

            var filteredLibraryElements = GetLibraryElementControllers();

            controllers =
                controllers.Where(
                    controller =>
                        filteredLibraryElements.Count(
                            lem =>
                                lem.LibraryElementModel.LibraryElementId ==
                                controller.LibraryElementModel.LibraryElementId) > 0);
        }

        public void AddCreator(string creator)
        {
            if (!Creators.Contains(creator))
            {
                Creators.Add(creator);
                needsRefresh = true;
            }
        }

        public void RemoveCreator(string creator)
        {
            if (Creators.Contains(creator))
            {
                Creators.Remove(creator);
                needsRefresh = true;
            }
        }

        public void AddParentCollectionId(string parentCollectionId)
        {
            if (!ParentCollectionIds.Contains(parentCollectionId))
            {
                ParentCollectionIds.Add(parentCollectionId);
                needsRefresh = true;
            }
        }

        public void RemoveParentCollectionId(string parentCollectionId)
        {
            if (ParentCollectionIds.Contains(parentCollectionId))
            {
                ParentCollectionIds.Remove(parentCollectionId);
                needsRefresh = true;
            }
        }

        public void AddElementType(NusysConstants.ElementType elementType)
        {
            if (!Types.Contains(elementType))
            {
                Types.Add(elementType);
                needsRefresh = true;
            }
        }

        public void RemoveElementType(NusysConstants.ElementType elementType)
        {
            if (Types.Contains(elementType))
            {
                Types.Add(elementType);
                needsRefresh = true;
            }
        }





        /// <summary>
        /// Takes in an IEnumerable of Library element controllers and filters out those which are not created or edited
        /// between the passed in start and end date. if start is null and end if set, then gets all controllers where the desired
        /// date occurs before end. if end is null and start is set, gets all controllers where desired date occurs
        /// after start. If start and end are null returns all passed in controllers.
        /// </summary>
        /// <param name="toFilter"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        private IEnumerable<LibraryElementController> FilterBetweenDates(IEnumerable<LibraryElementController> toFilter,
            DateTime? dateStart, DateTime? dateEnd, DateType dateType )
        {
            // if the start time stamp and end time stamp have been set, filter by between start and end
            if (dateStart != null && dateEnd != null)
            {
                switch (dateType)
                {
                    case DateType.Creation:
                        toFilter = toFilter.Where(cntrl => cntrl.GetCreationDate().CompareTo(dateStart.Value) > -1 &&
                                   cntrl.GetCreationDate().CompareTo(dateEnd.Value) < 1);
                        break;
                    case DateType.LastEdit:
                        toFilter = toFilter.Where(cntrl => cntrl.GetLastEditedDate().CompareTo(dateStart.Value) > -1 &&
                                   cntrl.GetLastEditedDate().CompareTo(dateEnd.Value) < 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dateType), dateType, null);
                }

            }
            // otherwise if the end time stamp has been set and the start time stamp has not been set filter by before end
            else if (dateStart == null && dateEnd != null)
            {
                switch (dateType)
                {
                    case DateType.Creation:
                        toFilter = toFilter.Where(cntrl => cntrl.GetCreationDate().CompareTo(dateEnd.Value) < 1); 
                        break;
                    case DateType.LastEdit:
                        toFilter = toFilter.Where(cntrl => cntrl.GetLastEditedDate().CompareTo(dateEnd.Value) < 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dateType), dateType, null);
                }
            }
            // otherwise if the start time has been set and the end time has not been set filter by after start
            else if (dateStart != null)
            {
                switch (dateType)
                {
                    case DateType.Creation:
                        toFilter = toFilter.Where(cntrl => cntrl.GetCreationDate().CompareTo(dateStart.Value) > -1);
                        break;
                    case DateType.LastEdit:
                        toFilter = toFilter.Where(cntrl => cntrl.GetLastEditedDate().CompareTo(dateStart.Value) > -1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dateType), dateType, null);
                }
            }

            return toFilter;
        }
    }
}
