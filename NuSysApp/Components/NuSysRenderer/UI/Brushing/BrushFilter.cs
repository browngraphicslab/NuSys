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
        /// List of creators that we are brushing for. To add or remove creators use
        /// AddCreator(), AddCreatorRange(), RemoveCreator(), or RemoveCreatorRange()
        /// </summary>
        public HashSet<string> Creators { get; }

        /// <summary>
        /// helper value for CreationDateStart property
        /// </summary>
        private DateTime? _creationDateStart;

        /// <summary>
        /// All brushed elements returned will have a creation date after this date. If set to null, no limit on the
        /// starting bound of the creation date is assumed
        /// </summary>
        public DateTime? CreationDateStart
        {
            get { return _creationDateStart; }
            set
            {
                _creationDateStart = value;
                needsRefresh = true;
            }
        }

        /// <summary>
        /// helper value for CreationDateEnd property
        /// </summary>
        private DateTime? _creationDateEnd;

        /// <summary>
        /// All brushed elements returned will have a creation date before this date. If set to null, no limit on the
        /// ending bound of the creation date is assumed
        /// </summary>
        public DateTime? CreationDateEnd
        {
            get { return _creationDateEnd; }
            set
            {
                _creationDateEnd = value;
                needsRefresh = true;
            }
        }

        /// <summary>
        /// helper value for LastEditedStart property
        /// </summary>
        private DateTime? _lastEditedStart;

        /// <summary>
        /// All brushed elements returned will have a last edited date after this date. If set to null, no limit on the
        /// starting bound of the last edited date is assumed
        /// </summary>
        public DateTime? LastEditedStart
        {
            get { return _lastEditedStart; }
            set
            {
                _lastEditedStart = value;
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Helper value for the LastEditedEnd property
        /// </summary>
        private DateTime? _lastEditedEnd;

        /// <summary>
        /// All brushed elements returned will have a last edited date before this date. If set to null, no limit on the
        /// ending bound of the last edited date is assumed
        /// </summary>
        public DateTime? LastEditedEnd
        {
            get { return _lastEditedEnd; }
            set
            {
                _lastEditedEnd = value;
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Hashset of element types that we are brushing for. To add or remove types use
        /// AddType(), AddTypeRange(), RemoveType(), or RemoveTypeRange()
        /// </summary>
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

        /// <summary>
        /// hash set of the library element controllers which the filter returns true for. 
        /// includes all library element controllers in the entire library
        /// </summary>
        private HashSet<LibraryElementController> _filteredLibraryElementControllers;

        /// <summary>
        /// hash set of the element controllers which the filter returns true for.
        /// includes only the element controllers which are in the collection associated with
        /// _prevCollectionController
        /// </summary>
        private HashSet<ElementController> _filteredElementControllers;

        /// <summary>
        /// private helper variable to store the most recent collection we quered for element controllers
        /// </summary>
        CollectionLibraryElementController _prevCollectionController;

        public BrushFilter()
        {
            Creators = new HashSet<string>();
            _creationDateStart = null;
            CreationDateEnd = null;
            LastEditedStart = null;
            LastEditedEnd = null;
            Types = new HashSet<NusysConstants.ElementType>();
            needsRefresh = true;
        }

        /// <summary>
        /// Returns all the library element controllers from the entire library that fulfill the brush's constraints
        /// </summary>
        /// <returns></returns>
        public HashSet<LibraryElementController> GetLibraryElementControllers()
        {
            // if we haven't changed anything since the last time we brushed the data, just return the previous results
            if (!needsRefresh)
            {
                return _filteredLibraryElementControllers;
            }

            // otherwise get all Libraryelementcontrollers which fulfill our creator and type constraints
            var controllers = SessionController.Instance.ContentController.AllLibraryElementControllers.Where(
                ctrl => Creators.Contains(ctrl.LibraryElementModel.Creator) && // first filter controllers by creator
                Types.Contains(ctrl.LibraryElementModel.Type)); // and by type

            // filter the controllers by the creation date constraint
            controllers = FilterBetweenDates(controllers, _creationDateStart, CreationDateEnd, DateType.Creation); // filter by creation date

            // filter the controllers by the last edited date constraint
            controllers = FilterBetweenDates(controllers, LastEditedStart, LastEditedEnd, DateType.LastEdit); // filter by last edit date

            // set the local results variable to the new results
            _filteredLibraryElementControllers = new HashSet<LibraryElementController>(controllers);

            // return the new results
            return _filteredLibraryElementControllers;
        }

        /// <summary>
        /// Returns all the element controllers from the collection associated with the 
        /// passed in collection controller which fulfill the brush constraints
        /// </summary>
        /// <param name="collectionController"></param>
        /// <returns></returns>
        public HashSet<ElementController> GetElementControllersForCollection(CollectionLibraryElementController collectionController)
        {
            // if we haven't changed anything since the last time we brushed the data, just return the previous results
            if (!needsRefresh && collectionController == _prevCollectionController)
            {
                return _filteredElementControllers;
            }

            Debug.Assert(collectionController != null);

            // update the most recently searched collection variable to reflect the new search
            _prevCollectionController = collectionController;

            // get all element controllers that are in the passed in collection
            var elementControllers = collectionController.CollectionModel.Children.Where(id => SessionController.Instance.IdToControllers.ContainsKey(id)).Select(id => SessionController.Instance.IdToControllers[id]);

            // get a list of library element controllers that fulfill the brush constraints
            var filteredLibraryElementControllers = GetLibraryElementControllers();

            // transform the list of library element controllers into a hashset of library element ids which fulfill the constraints
            var filteredLibraryElementControllerIds = new HashSet<string>(filteredLibraryElementControllers.Select(lec => lec.LibraryElementModel.LibraryElementId));

            // filter the element controllers into only those that have a library element id matching one which fulfilled our constraints
            elementControllers =
                elementControllers.Where(
                    controller =>
                        filteredLibraryElementControllerIds.Contains(controller.LibraryElementModel.LibraryElementId));

            // store the new results locally
            _filteredElementControllers = new HashSet<ElementController>(elementControllers);

            // return all the element controllers which fulfill the brush constraints
            return _filteredElementControllers;
        }

        /// <summary>
        /// Add the passed in string to the Creators filter
        /// </summary>
        /// <param name="creator"></param>
        public void AddCreator(string creator)
        {
            if (!Creators.Contains(creator))
            {
                Creators.Add(creator);
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Add each of the passed in strings to the Creators filter
        /// </summary>
        /// <param name="creators"></param>
        public void AddCreatorRange(IEnumerable<string> creators)
        {
            foreach (var creator in creators)
            {
                AddCreator(creator);
            }
        }

        /// <summary>
        /// Remove the passed in string from the Creators filter
        /// </summary>
        /// <param name="creator"></param>
        public void RemoveCreator(string creator)
        {
            if (Creators.Contains(creator))
            {
                Creators.Remove(creator);
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Remove each of the passed in strings from the Creators filter
        /// </summary>
        /// <param name="creators"></param>
        public void RemoveCreatorRange(IEnumerable<string> creators)
        {
            foreach (var creator in creators)
            {
                RemoveCreator(creator);
            }
        }

        /// <summary>
        /// Add the passed in type to the Types filter
        /// </summary>
        /// <param name="elementType"></param>
        public void AddType(NusysConstants.ElementType elementType)
        {
            if (!Types.Contains(elementType))
            {
                Types.Add(elementType);
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Add each of the passed in types to the Types filter
        /// </summary>
        /// <param name="elementTypes"></param>
        public void AddTypeRange(IEnumerable<NusysConstants.ElementType> elementTypes)
        {
            foreach (var type in elementTypes)
            {
                AddType(type);        
            }
        }

        /// <summary>
        /// Remove the passed in type from the Types filter
        /// </summary>
        /// <param name="elementType"></param>
        public void RemoveType(NusysConstants.ElementType elementType)
        {
            if (Types.Contains(elementType))
            {
                Types.Remove(elementType);
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Remove each of the passed in types from the Types filter
        /// </summary>
        /// <param name="elementTypes"></param>
        public void RemoveTypeRange(IEnumerable<NusysConstants.ElementType> elementTypes)
        {
            foreach (var type in elementTypes)
            {
                RemoveType(type);
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
