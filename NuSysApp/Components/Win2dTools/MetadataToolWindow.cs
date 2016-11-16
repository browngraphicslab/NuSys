using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class MetadataToolWindow : ToolWindow
    {
        private ListViewUIElementContainer<string> _metadataKeysList;
        private ListViewUIElementContainer<KeyValuePair<string, double>> _metadataValuesList;
        public MetadataToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, MetadataToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            SetUpMetadataLists();
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            (vm.Controller as MetadataToolController).SelectionChanged += On_SelectionChanged;
            vm.ReloadPropertiesToDisplay();
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
            //_metadataKeysList.RowDragged += _listView_RowDragged;
            //_metadataKeysList.RowDragCompleted += _listView_RowDragCompleted;

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
            //_metadataValuesList.RowTapped += _listView_RowTapped;
            //_metadataValuesList.RowDragged += _listView_RowDragged;
            //_metadataValuesList.RowDragCompleted += _listView_RowDragCompleted;
            //_metadataValuesList.RowDoubleTapped += _listView_RowDoubleTapped;
            _metadataValuesList.Transform.LocalPosition = new Vector2(Width/2, FILTER_CHOOSER_HEIGHT + UIDefaults.TopBarHeight);


            AddChild(_metadataValuesList);
        }

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