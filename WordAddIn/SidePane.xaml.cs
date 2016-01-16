using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WordAddIn
{
    /// <summary>
    /// Interaction logic for SidePane.xaml
    /// </summary>
    public partial class SidePane : UserControl
    {

        private static string mediaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";

        private static string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\WordTransfer";

        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }

        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        public ObservableCollection<SelectionItem> CheckedSelections { get; set;}

        private string commentAuthor = "NuSys";

        private string commentExported = "Exported to NuSys";

        public SelectionItem SelectedSelection { get; set;}

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

            //delete all NuSys comments
            var comments = Globals.ThisAddIn.Application.ActiveDocument.Comments;
            foreach (var commentObj in comments)
            {
                Comment comment = ((Comment)commentObj);

                if (comment.Author == commentAuthor)
                {
                    comment.Delete();
                }
            }

            var allSelections = Globals.ThisAddIn._allSelectionItems;
            var bookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;

            foreach (var selection in allSelections)
            {
                if (bookmarks.Exists(selection.BookmarkId)){
                    var docSelection = bookmarks[selection.BookmarkId].Range;
                    SelectionItem ns;

                    if (!selection.IsExported)
                    {
                        Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(docSelection, "");
                        c.Author = commentAuthor;

                        ns = new SelectionItem { Comment = c, Bookmark = bookmarks[selection.BookmarkId], Range = docSelection, IsExported = false };
                        ns.AddSelection();
                        UnexportedSelections.Add(ns);
                    }
                    else
                    {
                        Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(docSelection, "");
                        c.Author = commentAuthor;

                        ns = new SelectionItem { Comment = c, Bookmark = bookmarks[selection.BookmarkId], Range = docSelection, IsExported = true };
                        ns.AddSelection();
                        ExportedSelections.Add(ns);
                    }

                    if (selection.BookmarkId == Globals.ThisAddIn.SelectionId)
                    {
                        ns.DropShadowOpac = 1.0;

                        if (SelectedSelection != null && SelectedSelection != ns)
                        {
                            SelectedSelection.DropShadowOpac = 0.0;
                        }

                        SelectedSelection = ns;
                    }
                }
            }
        }

        //delete all checked selections
        private void OnDelete()
        {
            foreach (var selection in CheckedSelections)
            {
                try
                {
                    if (!selection.Bookmark.Empty)
                    {
                        selection.Bookmark.Delete();
                    }

                    //checking if Comment has not been deleted
                    if (selection.Comment.Author != null)
                    {
                        selection.Comment.Delete();
                    }

                }
                catch (Exception ex)
                {
                    //if exception is thrown, comment has been deleted already so do nothing
                }

                if (UnexportedSelections.Contains(selection))
                {
                    UnexportedSelections.Remove(selection);
                }
                else if (ExportedSelections.Contains(selection))
                {
                    ExportedSelections.Remove(selection);
                }
            }

            CheckedSelections.Clear();
        }

		//exports to NuSys all checked selections
		private void OnExport(){
            var fileDir = dir + "\\selection";
            int count = 0;

            var temp_cs = new List<SelectionItem>();
            var hasNewSelection = false;

            List<SelectionItemView> selectionItemViews = new List<SelectionItemView>();
            List<SelectionItemIdView> selectionItemIdViews = new List<SelectionItemIdView>();

            foreach (var selection in CheckedSelections)
            {
                if (!selection.IsExported)
                {
                    string dateTimeExported = DateTime.Now.ToString();
                    selection.DateTimeExported = dateTimeExported;

                    var selectionItemView = selection.GetView();

                    if (selection.ImageContent != null)
                    {
                        selection.ImageContent.Save(mediaDir + "\\" + selectionItemView.ImageName, ImageFormat.Png);
                    }

                    selectionItemViews.Add(selectionItemView);

                    selection.IsExported = true;
                    try
                    {
                        //checking if Comment has not been deleted
                        if (selection.Comment.Author != null)
                        {
                            selection.Comment.Range.Text = commentExported + " " + dateTimeExported;
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
                var selectionItemJson = Newtonsoft.Json.JsonConvert.SerializeObject(selectionItemViews);
                var f = fileDir + Guid.NewGuid().ToString() + ".nusys";
                File.WriteAllText(f, selectionItemJson);
                File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                File.Move(f, f);
            }

            //need seperate for loop because unchecking triggers a removal in CheckedSelections
            foreach (var cs in temp_cs)
            {
                cs.CheckBox.IsChecked = false;
            }
        }

        private void AddSelectionItem(Range range)
        {
            var ns = new SelectionItem { Range = range, IsExported = false };
            ns.AddSelection();

            Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(range, "");
            c.Author = commentAuthor;
            ns.Comment = c;

            //using b as an arbitrary char to create a valid bookmarkId
            var bookmarkId = "NuSys" + (Guid.NewGuid().ToString()).Replace('-', 'b');
            var bookmark = range.Bookmarks.Add(bookmarkId);
            ns.Bookmark = bookmark;

            if (ns.ImageContent != null || !String.IsNullOrEmpty(ns.RtfContent))
            {
                UnexportedSelections.Add(ns);
            }

            
        }


        //add the highlighted content to the sidebar as a selection
        private void OnSelectionAdded()
        {
            try {
                Selection selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                Range selectionRange = selection.Range;

                if (selectionRange.ShapeRange.Count > 0)
                {
                    foreach (Microsoft.Office.Interop.Word.Shape shape in selectionRange.ShapeRange)
                    {
                        shape.Select();
                        Selection shapeSelection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                        shapeSelection.Copy();
                        AddSelectionItem(selectionRange);
                    }

                    if (selectionRange.Text != null)
                    {
                        Range textRange = Globals.ThisAddIn.Application.ActiveDocument.Range(selectionRange.Start, selectionRange.End);
                        textRange.Select();
                        Selection textSelection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                        textSelection.Copy();
                        AddSelectionItem(selectionRange);
                    }
                }
                else
                {
                    selectionRange.Select();
                    Selection otherSelection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                    otherSelection.Copy();
                    AddSelectionItem(selectionRange);
                }
            }
            catch (Exception ex)
            {
                //TODO exception handling
            }

        }
    }
}
