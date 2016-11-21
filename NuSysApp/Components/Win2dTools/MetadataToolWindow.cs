using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class MetadataToolWindow : ToolWindow
    {
        private ListViewUIElementContainer<string> _metadataKeysList;
        private ListViewUIElementContainer<KeyValuePair<string, double>> _metadataValuesList;
        private RectangleUIElement _dragFilterItem;

        public MetadataToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, MetadataToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            SetUpDragFilterItem();
            SetUpMetadataLists();
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            (vm.Controller as MetadataToolController).SelectionChanged += On_SelectionChanged;
            vm.ReloadPropertiesToDisplay();
        }

        /// <summary>
        /// Sets up the item to be shown under the pointer when you drag a row
        /// </summary>
        private void SetUpDragFilterItem()
        {
            _dragFilterItem = new RectangleUIElement(this, ResourceCreator)
            {
                Height = 50,
                Width = 50,
                Background = Colors.Red
            };
            AddChild(_dragFilterItem);
            _dragFilterItem.IsVisible = false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
            _metadataKeysList.Width = Width/2;
            _metadataKeysList.Height = Height - FILTER_CHOOSER_HEIGHT - UIDefaults.TopBarHeight - BUTTON_BAR_HEIGHT;

            _metadataValuesList.Width = Width / 2;
            _metadataValuesList.Height = Height - FILTER_CHOOSER_HEIGHT - UIDefaults.TopBarHeight - BUTTON_BAR_HEIGHT;
            _metadataValuesList.Transform.LocalX = Width/2;
        }

        /// <summary>
        ///Set the key list visual selection and refresh the value list
        /// </summary>
        private void On_SelectionChanged(object sender)
        {
            SetKeyListVisualSelection();
            RefreshValueList();
        }

        /// <summary>
        ///Set the key list visual selection and scrolls to include in view
        /// </summary>
        private void SetKeyListVisualSelection()
        {
            UITask.Run(delegate
            {
                var vm = Vm as MetadataToolViewModel;
                if (vm.Selection != null &&
                    (vm.Controller as MetadataToolController).Model.Selected &&
                    vm.Selection.Item1 != null)
                {
                    if (!_metadataKeysList.GetSelectedItems().Any() || _metadataKeysList.GetSelectedItems().First() != vm.Selection.Item1)
                    {
                        _metadataKeysList.SelectItem(vm.Selection.Item1);
                        _metadataKeysList.ScrollTo(vm.Selection.Item1);
                    }
                }
                else
                {
                    _metadataKeysList.DeselectAllItems();
                }
            });
        }


        /// <summary>
        ///  compares two lists
        /// </summary>
        public bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null || list2 == null)
            {
                return false;
            }
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        /// <summary>
        ///  Based on the selected key, and the search bar, refreshes the value list and sets visual value selection
        /// </summary>
        public void RefreshValueList()
        {
            UITask.Run(delegate {
                var vm = (Vm as MetadataToolViewModel);
                if (vm?.Selection?.Item1 != null && vm.Controller.Model.Selected)
                {
                    var filteredList = FilterValuesList(""); //FilterValuesList(xSearchBox.Text);
                    if (!ScrambledEquals(_metadataValuesList.GetItems().Select(item => ((KeyValuePair<string, double>)item).Key), filteredList.Select(item => ((KeyValuePair<string, double>)item).Key)))
                    {
                        //if new filtered list is different from old filtered list, set new list as item source, set the visual selection, and 
                        //scroll into view if necessary.
                        _metadataValuesList.ClearItems();
                        _metadataValuesList.AddItems(filteredList);
                        SetValueListVisualSelection();
                        if (_metadataValuesList.GetSelectedItems().Any())
                        {
                            _metadataValuesList.ScrollTo(_metadataValuesList.GetSelectedItems().First());
                        }
                    }
                    else
                    {
                        //if new filtered list is the same as old filtered list, just set the visual selection and do not refresh the value list item source
                        SetValueListVisualSelection();
                    }
                }
                else
                {
                    _metadataValuesList.ClearItems();
                }
            });
        }

        private List<KeyValuePair<string, double>> FilterValuesList(string search)
        {
            var filteredValuesList = new List<string>();
            var vm = (Vm as MetadataToolViewModel);
            if (vm == null || !vm.AllMetadataDictionary.ContainsKey(vm.Selection.Item1))
            {
                return new List<KeyValuePair<string, double>>();
            }
            var listOfKvpMetadataValueToNumOfOccurrences = vm.AllMetadataDictionary[vm.Selection.Item1].Where(
                    item => item.Key?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            return listOfKvpMetadataValueToNumOfOccurrences.OrderByDescending(item => item.Value).ToList();

            return listOfKvpMetadataValueToNumOfOccurrences.OrderBy(key => !string.IsNullOrEmpty(key.Key) && char.IsNumber(key.Key[0]))
                    .ThenBy(key => key.Key).ToList();
        }

        /// <summary>
        ///Set the values list visual selection
        /// </summary>
        private void SetValueListVisualSelection()
        {
            UITask.Run(delegate
            {
                var vm = Vm as MetadataToolViewModel;
                if (vm.Selection.Item1 != null && vm.Selection.Item2 != null)
                {
                    _metadataValuesList.DeselectAllItems();
                    if (_metadataValuesList.GetItems().Any())
                    {
                        foreach (var item in vm.Selection.Item2)
                        {
                            var toAdd = _metadataValuesList.GetItems().Where(kvp => ((KeyValuePair<string, double>)kvp).Key.Equals(item)).FirstOrDefault();

                            _metadataValuesList.SelectItem(toAdd);
                        }
                    }
                }
                else
                {
                    _metadataValuesList.DeselectAllItems();
                }
            });
        }

        /// <summary>
        ///item source of metadata keys list
        /// </summary>
        private void Vm_PropertiesToDisplayChanged()
        {
            UITask.Run(delegate
            {
                var vm = Vm as MetadataToolViewModel;
                Debug.Assert(vm != null);
                _metadataKeysList.ClearItems();
                _metadataKeysList.AddItems(vm.AllMetadataDictionary.Keys.ToList());
            });
        }

        /// <summary>
        /// This sets up the keys and values list
        /// </summary>
        private void SetUpMetadataLists()
        {
            //Set up keys list
            _metadataKeysList = new ListViewUIElementContainer<string>(this, ResourceCreator);
            _metadataKeysList.ShowHeader = true;
            _metadataKeysList.RowBorderThickness = 1;
            _metadataKeysList.DisableSelectionByClick = true;
            _metadataKeysList.MultipleSelections = false;
            var keysColumn = new ListTextColumn<string>();
            keysColumn.Title = "KEYS";
            keysColumn.RelativeWidth = 1;
            keysColumn.ColumnFunction = model => model;

            _metadataKeysList.AddColumns(new List<ListColumn<string>>() { keysColumn });
            _metadataKeysList.RowTapped += metadataKeysList_RowTapped; ;
            _metadataKeysList.RowDragged += _metadataKeysList_RowDragged; ;
            _metadataKeysList.RowDragCompleted += _metadataKeysList_RowDragCompleted; ;

            _metadataKeysList.AddItems(new List<string>() { "1", "2", "3", "4", "5", "6", "7", "9", "10", });
            _metadataKeysList.Transform.LocalPosition = new Vector2(0, FILTER_CHOOSER_HEIGHT + UIDefaults.TopBarHeight);
            _metadataKeysList.AddItems((Vm as MetadataToolViewModel)?.AllMetadataDictionary.Keys.ToList());

            AddChild(_metadataKeysList);

            //Set up values list
            _metadataValuesList = new ListViewUIElementContainer<KeyValuePair < string, double >> (this, ResourceCreator);
            _metadataValuesList.ShowHeader = true;
            _metadataValuesList.RowBorderThickness = 1;
            _metadataValuesList.DisableSelectionByClick = true;
            var listColumn = new ListTextColumn<KeyValuePair<string, double>>();
            listColumn.Title = "VALUES";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Key;

            _metadataValuesList.AddColumns(new List<ListColumn<KeyValuePair<string, double>>>() { listColumn });
            _metadataValuesList.RowTapped += _metadataValuesList_RowTapped;
            _metadataValuesList.RowDragged += _metadataValuesList_RowDragged; ;
            _metadataValuesList.RowDragCompleted += _metadataValuesList_RowDragCompleted; ;
            _metadataValuesList.RowDoubleTapped += _metadataValuesList_RowDoubleTapped; ;
            _metadataValuesList.Transform.LocalPosition = new Vector2(Width/2, FILTER_CHOOSER_HEIGHT + UIDefaults.TopBarHeight);


            AddChild(_metadataValuesList);
        }

        /// <summary>
        /// Gets called when dragging metadata key row is complete. It creates/adds a parent to a tool depending on what was under the pointer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadataKeysList_RowDragCompleted(string item, string columnName, CanvasPointer pointer)
        {
            if (_dragFilterItem.IsVisible)
            {
                _dragFilterItem.IsVisible = false;
                var vm = (Vm as MetadataToolViewModel);
                vm.Selection = new Tuple<string, HashSet<string>>(item, new HashSet<string>());
                var dragDestination = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, null, 2) as ToolWindow; //maybe replace null w render engine.root
                var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2(pointer.CurrentPoint.X, pointer.CurrentPoint.Y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);
                vm.FilterIconDropped(dragDestination, canvasCoordinate.X, canvasCoordinate.Y);
            }
        }

        /// <summary>
        /// Fires when a row of the metadata keys list is being dragged. It just makes the drag filter icon follow your pointer.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadataKeysList_RowDragged(string item, string columnName, CanvasPointer pointer)
        {
            DragFilterIcon(pointer);
        }

        /// <summary>
        /// Gets called when dragging metadata value row is complete. It creates/adds a parent to a tool depending on what was under the pointer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadataValuesList_RowDragCompleted(KeyValuePair<string, double> item, string columnName, CanvasPointer pointer)
        {
            if (_dragFilterItem.IsVisible)
            {
                _dragFilterItem.IsVisible = false;
                var vm = (Vm as MetadataToolViewModel);
                if (pointer.DeviceType == PointerDeviceType.Pen) // || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                {
                    vm.Selection.Item2.Add(item.Key);
                    vm.Selection = vm.Selection;
                }
                else
                {
                    vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                        new HashSet<string>() { item.Key });
                }
                var dragDestination = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, null, 2) as ToolWindow; //maybe replace null w render engine.root
                var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2(pointer.CurrentPoint.X, pointer.CurrentPoint.Y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);

                vm.FilterIconDropped(dragDestination, canvasCoordinate.X, canvasCoordinate.Y);
            }
        }

        /// <summary>
        /// Makes the drag filter icon go to the pointer if you are outside the tool area
        /// </summary>
        /// <param name="pointer"></param>
        public void DragFilterIcon(CanvasPointer pointer)
        {
            _dragFilterItem.Transform.LocalPosition = Vector2.Transform(pointer.CurrentPoint, this.Transform.ScreenToLocalMatrix);
            if (_dragFilterItem.Transform.LocalX > 0 && _dragFilterItem.Transform.LocalX < Width &&
                _dragFilterItem.Transform.LocalY < Height && _dragFilterItem.Transform.LocalY > 0)
            {
                _dragFilterItem.IsVisible = false;
            }
            else
            {
                _dragFilterItem.IsVisible = true;
            }
        }

        /// <summary>
        /// Fires when a row of the metadata values list is being dragged. It just makes the drag filter icon follow your pointer.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadataValuesList_RowDragged(KeyValuePair<string, double> item, string columnName, CanvasPointer pointer)
        {
            DragFilterIcon(pointer);
        }

        /// <summary>
        ///If the item that was double tapped is the only selected item, attempt to open the detail view.
        /// </summary>
        private void _metadataValuesList_RowDoubleTapped(KeyValuePair<string, double> item, string columnName, CanvasPointer pointer)
        {
            var vm = (Vm as MetadataToolViewModel);
            var textTapped = item.Key;
            if (!vm.Selection.Item2.Contains(textTapped) && vm.Selection.Item2.Count == 0)
            {
                vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                            new HashSet<string>() { textTapped });
            }
            if (vm.Selection.Item2.Count == 1 &&
                vm.Selection.Item2.First().Equals(textTapped))
            {
                vm.OpenDetailView();
            }
        }

        /// <summary>
        /// Fires when a row in the metadata values list gets tapped. Sets the logical selection based on the type of selection (multi/single).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadataValuesList_RowTapped(KeyValuePair<string, double> item, string columnName, CanvasPointer pointer)
        {
            var vm = (Vm as MetadataToolViewModel);
            if (vm.Controller.Model.Selected && vm.Selection.Item2 != null &&
                vm.Selection.Item2.Contains(item.Key))
            {
                if (pointer.DeviceType == PointerDeviceType.Pen) //|| CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                {
                    //if tapped item is already selected and in multiselect mode, remove item from selection
                    vm.Selection.Item2.Remove(item.Key);
                    vm.Selection = vm.Selection;
                }
                else
                {
                    //if tapped item is already selected and in single select mode, remove all selections
                    vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1, new HashSet<string>());
                }
            }
            else
            {
                Debug.Assert(vm != null);
                if (_metadataKeysList.GetSelectedItems().Count() == 1)
                {
                    if (pointer.DeviceType == PointerDeviceType.Pen) // || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                    {
                        //if tapped item is not selected and in multiselect mode, add item to selection

                        if (vm.Selection != null)
                        {
                            var selection = item.Key;
                            vm.Selection.Item2.Add(selection);
                            vm.Selection = vm.Selection;
                        }
                        else
                        {
                            vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                            new HashSet<string>() { item.Key });
                        }
                    }
                    else
                    {
                        //if tapped item is not selected and in single mode, set the item as the only selection
                        vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                             new HashSet<string>() { item.Key });
                    }
                }
            }
        }


        /// <summary>
        /// This is called when the row of metadataakeys is tapped. It sets the selection appropriatelyy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void metadataKeysList_RowTapped(string item, string columnName, CanvasPointer pointer)
        {
            var vm = (Vm as MetadataToolViewModel);
            if (vm.Controller.Model.Selected &&
                vm.Selection.Item1.Equals(item))
            {
                vm.Controller.UnSelect();
            }
            else
            {
                Debug.Assert(vm != null);
                vm.Selection = new Tuple<string, HashSet<string>>(item, new HashSet<string>());
            }
        }
        
    }
}