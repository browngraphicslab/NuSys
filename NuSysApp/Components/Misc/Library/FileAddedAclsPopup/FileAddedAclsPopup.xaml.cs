using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class FileAddedAclsPopup : UserControl
    {
        /// <summary>
        /// Used to store the datacontext so we don't have to cast DataContext
        /// </summary>
        private FileAddedAclsPopupViewModel _vm;

        /// <summary>
        /// A threading boolean that allows us to wait until the submit event is fired
        /// </summary>
        private TaskCompletionSource<bool> tcs = null;

        /// <summary>
        /// Boolean used to determine if we are in the middle of a select all call
        /// </summary>
        private bool isSelectAll;

        /// <summary>
        /// Instantiates a new FileAddedPopup, should only be called from xaml, use //todo insert method name here
        /// to enable the popup
        /// </summary>
        public FileAddedAclsPopup()
        {
            InitializeComponent();

            // called once when the datacontext is initially set from xaml, and again when the datacontext is set in code behind of sessionview
            DataContextChanged += delegate
            {
                // await the sessionController.instance.sessionview.maincanvas onloaded event to set the data context
                _vm = DataContext as FileAddedAclsPopupViewModel;
                if (_vm == null)
                {
                    return;
                }

                // set _vm variables
                _vm.Width = 400;
                _vm.Height = 300;
                _vm.IsEnabled = false;

                // place the box in the center of the screen
                Canvas.SetTop(this, (SessionController.Instance.SessionView.MainCanvas.ActualHeight / 2.0) - _vm.Height / 2.0);
                Canvas.SetLeft(this, (SessionController.Instance.SessionView.MainCanvas.ActualWidth / 2.0) - _vm.Width / 2.0);

                // for recentering when the canvas changes
                SessionController.Instance.SessionView.MainCanvas.SizeChanged += MainCanvas_SizeChanged;

            };
        }

        /// <summary>
        /// Invoked every time the main canvas size changes, recenters the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(this, (SessionController.Instance.SessionView.MainCanvas.ActualHeight / 2.0) - _vm.Height / 2.0);
            Canvas.SetLeft(this, (SessionController.Instance.SessionView.MainCanvas.ActualWidth / 2.0) - _vm.Width / 2.0);
        }

        /// <summary>
        /// //Todo determine an event this can attach to
        /// Cleanly dispose all events this method listens to
        /// </summary>
        private void Dispose()
        {
            SessionController.Instance.SessionView.MainCanvas.SizeChanged -= MainCanvas_SizeChanged;
        }


        /// <summary>
        /// Pass in a list of storage files, creates a popup which prompts the user to assign acls to each of the files
        /// returns a dictionary of storageFile.FolderRelativeId to acls
        /// RETURNS NULL IF USER CANCELS UPLOAD
        /// </summary>
        /// <param name="storageFiles"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, NusysConstants.AccessType>> GetAcls(IEnumerable<StorageFile> storageFiles)
        {
            if (_vm == null)
            {
                return null;
            }

            // clear the observable collection and the dictionary we are going to return
            _vm.Files.Clear();
            _vm.AccessDictionary.Clear();

            // add all the storage files to the observable collection
            foreach (var storageFile in storageFiles)
            {
                _vm.Files.Add(storageFile);
            }
            
            // enable the popup
            _vm.IsEnabled = true;

            // wait for the user to fill out all the necessary fields on the popup
            tcs = new TaskCompletionSource<bool>();
            var isSubmit = await tcs.Task;

            // if the user cancelled the upload, return null
            if (!isSubmit)
            {
                return null;
            }

            Debug.Assert(_vm.Files.Select(r => _vm.AccessDictionary.ContainsKey(r.FolderRelativeId)).All(r => r == true), "If this fails, the dictionary does not contain all the requested files when the submit button was clicked");

            // return the dictionary
            return _vm.AccessDictionary;
        }

        /// <summary>
        /// Called every time the user checks a radio button, enables the submit button if all the radio buttons have been checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Access_OnChecked(object sender, RoutedEventArgs e)
        {
            // if we are not in a select all call then deselect the selectall checkboxes
            if (!isSelectAll)
            {
                publicSelectAll.IsChecked = false;
                privateSelectAll.IsChecked = false;
            }

            // get the radio button that sends the access
            var radioButton = (sender as RadioButton);

            // get the file that the access is correlated to
            var file = radioButton.DataContext as StorageFile;

            // because we check the radio button in xaml, the datacontext is not always instantiated when this is called
            if (file == null)
            {
                return;
            }

            // get the access level from the radio button name, this is poor coding but simplifies the implementation immensely
            NusysConstants.AccessType selectedAcess = NusysConstants.AccessType.Public;
            if (radioButton.Name == "publicRadio")
            {
                selectedAcess = NusysConstants.AccessType.Public;
            } else if (radioButton.Name == "privateRadio")
            {
                selectedAcess = NusysConstants.AccessType.Private;
            }
            else
            {
                Debug.Fail("Sorry these are switched by name, make sure the strings above match the strings in the xaml list view");
            }

            // set the access level
            if (_vm.AccessDictionary.ContainsKey(file.FolderRelativeId))
            {
                _vm.AccessDictionary[file.FolderRelativeId] = selectedAcess;
            }
            else
            {
                _vm.AccessDictionary.Add(file.FolderRelativeId, selectedAcess);
            }
        }

        /// <summary>
        /// Called when the submit button is tapped, should only be called if all the files have been added to the _vm.AccessDictionary
        /// Disables the Popup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSubmitButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // foreach item in the listview, set the AccessDictionary value to the selected item
            foreach (var item in ListView.Items)
            {
                var container = ListView.ItemContainerGenerator.ContainerFromItem(item);

                // the two radio buttons in every list view item
                var radioButtons = AllRadioButtons(container);

                // get the radio button which was selected
                var radioButton = radioButtons.FirstOrDefault(c => c.IsChecked == true); // if this fails, check that the childName matches the radiobutton name in xaml list view
                Debug.Assert(radioButton != null,  "One of the rows in the list has no radio button selected, most likely occured if IsChecked was set to false programmatically somewhere");


                // get the access level from the radio button name, this is poor coding but simplifies the implementation immensely
                NusysConstants.AccessType selectedAcess = NusysConstants.AccessType.Public;
                if (radioButton.Name == "publicRadio")
                {
                    selectedAcess = NusysConstants.AccessType.Public;
                }
                else if (radioButton.Name == "privateRadio")
                {
                    selectedAcess = NusysConstants.AccessType.Private;
                }
                else
                {
                    Debug.Fail("Sorry these are switched by name, make sure the strings above match the strings in the xaml list view");
                }

                // get the file that the access is correlated to
                var file = radioButton.DataContext as StorageFile;
                Debug.Assert(file != null, "This should never occur, means data in listview was corrupted, or this is being called at an unexpected time");

                // set the access in the dictionary
                if (_vm.AccessDictionary.ContainsKey(file.FolderRelativeId))
                {
                    _vm.AccessDictionary[file.FolderRelativeId] = selectedAcess;
                }
                else
                {
                    _vm.AccessDictionary.Add(file.FolderRelativeId, selectedAcess);
                }
            }

            // disable the the popup
            _vm.IsEnabled = false;

            // notify the task completion source that the task is complete
            tcs?.TrySetResult(true);
        }

        /// <summary>
        /// Called when the select all checkbox is checked, checks all the proper radio buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAll_OnChecked(object sender, RoutedEventArgs e)
        {
            // set isSelectAll to true so that we know we are in this method
            isSelectAll = true;

            // get the checkbox that sends the event
            var checkBox = (sender as CheckBox);

            // deselect the opposite checkbox, and set the name of the child to search in the list
            string childName = "";
            if (publicSelectAll.Name == checkBox.Name)
            {
                // privateSelectall can be null because we use a default checkbox in xaml
                if (privateSelectAll != null)
                {
                    privateSelectAll.IsChecked = false;
                }
                childName = "publicRadio"; // if the select all isn't working its most likely because these names don't match the xaml names for radio buttons
            }
            else
            {
                // publicSelectAll can be null because we use a default checkbox in xaml
                if (publicSelectAll != null)
                {
                    publicSelectAll.IsChecked = false;
                }
                childName = "privateRadio"; // if the select all isn't working its most likely because these names don't match the xaml names for radio buttons
            }


            // foreach item in the listview, select the proper radio button
            foreach (var item in ListView.Items)
            {
                var container = ListView.ItemContainerGenerator.ContainerFromItem(item);

                // the two radio buttons in every list view item
                var radioButtons = AllRadioButtons(container);

                // the radio button we are going to select, automatically deselects the other one.
                var radioButton = radioButtons.First(c => c.Name == childName); // if this fails, check that the childName matches the radiobutton name in xaml list view
                radioButton.IsChecked = true;
            }

            // the call has finished so we can set isSelectAll to true, to reflect that we are no longer in the method
            isSelectAll = false;
        }

        /// <summary>
        /// Returns all the radio buttons for an item in the list view
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<RadioButton> AllRadioButtons(DependencyObject parent)
        {
            var _List = new List<RadioButton>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is RadioButton)
                    _List.Add(_Child as RadioButton);
                _List.AddRange(AllRadioButtons(_Child));
            }
            return _List;
        }

        /// <summary>
        /// Called when the user hits the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCancelButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // disable the popup, including visibility
            _vm.IsEnabled = false;

            // transition the underlying task completion source to show that the user has finished filling out the popup dialog
            tcs?.TrySetResult(false);
        }
    }
}
