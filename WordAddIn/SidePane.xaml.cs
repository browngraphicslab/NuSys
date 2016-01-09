using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WordAddIn
{
    /// <summary>
    /// Interaction logic for SidePane.xaml
    /// </summary>
    public partial class SidePane : UserControl
    {

        private static string mediaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";

        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }

        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        public ObservableCollection<SelectionItem> CheckedSelections { get; set;}

        private string commentAuthor = "NuSys";

        private string commentExported = "Exported to NuSys";

        public SidePane()
        {
            InitializeComponent();
            ic.DataContext = this;
            ic2.DataContext = this;

            LoadSelectionData();
            CheckSelectionLabels();
        }

        private void UnexpOnClick(object sender, RoutedEventArgs e)
        {
            if((string)unexpBttn.Content == "+")
            {
                unexpBttn.Content = "-";
                ic.Visibility = Visibility.Visible;
            }
            else
            {
                unexpBttn.Content = "+";
                ic.Visibility = Visibility.Collapsed;
            }
        }

        private void ExpOnClick(object sender, RoutedEventArgs e)
        {
            if ((string)expBttn.Content == "+")
            {
                expBttn.Content = "-";
                ic2.Visibility = Visibility.Visible;
            }
            else
            {
                expBttn.Content = "+";
                ic2.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnDelete();
            CheckSelectionLabels();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
			OnExport();
            CheckSelectionLabels();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OnSelectionAdded();
            CheckSelectionLabels();
        }

        private void CheckSelectionLabels()
        {
            if (ExportedSelections.Count == 0)
            {
                noExpSelectionsLabel.Visibility = Visibility.Visible;
                expBttn.Visibility = Visibility.Hidden;
            }
            else if (ExportedSelections.Count > 0)
            {
                noExpSelectionsLabel.Visibility = Visibility.Collapsed;
                expBttn.Visibility = Visibility.Visible;
            }

            if (UnexportedSelections.Count == 0)
            {
                noSelectionsLabel.Visibility = Visibility.Visible;
                unexpBttn.Visibility = Visibility.Hidden;
            }
            else if (UnexportedSelections.Count > 0)
            {
                noSelectionsLabel.Visibility = Visibility.Collapsed;
                unexpBttn.Visibility = Visibility.Visible;
            }
        }

        private void LoadSelectionData()
        {
            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();
            CheckedSelections = new ObservableCollection<SelectionItem>();

            var bookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;

            //get rid of excesse bookmarks
            foreach (Bookmark bookmark in bookmarks)
            {
                if (bookmark.Name.StartsWith("NuSysSelection"))
                {
                    bookmark.Delete();
                }
            }

            var comments = Globals.ThisAddIn.Application.ActiveDocument.Comments;
            foreach (var commentObj in comments)
            {
                Comment comment = ((Comment)commentObj);

                if (comment.Author == commentAuthor)
                {
                    string commentTxt = comment.Range.Text;

                    Clipboard.Clear();

                    comment.Scope.Select();
                    comment.Scope.Copy();

                    if (commentTxt == null)
                    {
                        var bookmarkId = "NuSys" + (Guid.NewGuid().ToString()).Replace('-', 'b');
                        comment.Scope.Bookmarks.Add(bookmarkId);

                        var ns = new SelectionItem { Comment = comment, Bookmark = bookmarkId, Range = comment.Scope, IsExported = false };
                        UnexportedSelections.Add(ns);
                    }
                    else
                    {
                        var ns = new SelectionItem { Comment = comment, Range = comment.Scope, IsExported = true };
                        ExportedSelections.Add(ns);
                    }
                }
            }

            if (ExportedSelections.Count > 0)
            {
                expBttn.Content = "-";
            }

            if (UnexportedSelections.Count > 0)
            {
                unexpBttn.Content = "-";
            }
        }
		
		//delete all checked selections
		private void OnDelete(){
			foreach (var selection in CheckedSelections){
                try {
                    //checking if Comment has not been deleted
                    if (selection.Comment.Author != null)
                    {
                        if (selection.Comment.Scope.Bookmarks.Exists(selection.Bookmark))
                        {
                            selection.Comment.Scope.Bookmarks.get_Item(selection.Bookmark).Delete();
                        }
                        selection.Comment.Delete();
                    }

                }catch (Exception ex)
                {
                    //if exception is thrown, comment has been deleted already so do nothing
                }

                if (UnexportedSelections.Contains(selection))
                {
                    UnexportedSelections.Remove(selection);
                }else if (ExportedSelections.Contains(selection))
                {
                    ExportedSelections.Remove(selection);
                }
			}

            CheckedSelections.Clear();
		}
		
		//exports to NuSys all checked selections
		private void OnExport(){
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";
            var fileDir = dir + "\\selection";
            int count = 0;

            var temp_cs = new List<SelectionItem>();
            var hasNewSelection = false;

            foreach (var selection in CheckedSelections)
            {
                if (UnexportedSelections.Contains(selection))
                {
                    var f = fileDir + count + ".nusys";
                    //File.WriteAllText(f, selection.RtfContent);
                    //File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                    //File.Move(f, f);

                    selection.IsExported = true;
                    try
                    {
                        //checking if Comment has not been deleted
                        if (selection.Comment.Author != null)
                        {
                            selection.Comment.Range.Text = commentExported;
                        }
                    }
                    catch (Exception ex)
                    {
                        //if exception is thrown, comment has been deleted already so do nothing
                    }

                    UnexportedSelections.Remove(selection);
                    ExportedSelections.Add(selection);

                    hasNewSelection = true;
                    count++;
                }

                //need a seperate list to iterate and delete/uncheck
                temp_cs.Add(selection);
            }

            if (hasNewSelection)
            {
                //File.WriteAllText(dir + "\\update.nusys", "update");
            }

            //need seperate for loop because unchecking triggers a removal in CheckedSelections
            foreach (var cs in temp_cs)
            {
                cs.CheckBox.IsChecked = false;
            }
        }

        //add the highlighted content to the sidebar as a selection
        private void OnSelectionAdded()
        {
            try {
                Clipboard.Clear();

                var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                selection.Select();
                selection.Copy();

                //using b as an arbitrary char to create a valid bookmarkId
                var bookmarkId = "NuSys" + (Guid.NewGuid().ToString()).Replace('-', 'b');
                selection.Bookmarks.Add(bookmarkId);

                if (Clipboard.ContainsData(System.Windows.DataFormats.Rtf) ||
                    Clipboard.ContainsData(System.Windows.Forms.DataFormats.Html) ||
                    Clipboard.ContainsData(System.Windows.Forms.DataFormats.Bitmap))
                {
                    Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(Globals.ThisAddIn.Application.Selection.Range, "");
                    c.Author = commentAuthor;

                    var ns = new SelectionItem { Comment = c, Bookmark = bookmarkId, Range = selection.Range, IsExported = false };
                    UnexportedSelections.Add(ns);
                }
            }
            catch (Exception ex)
            {
                //TODO exception handling
            }
        }
    }
}
