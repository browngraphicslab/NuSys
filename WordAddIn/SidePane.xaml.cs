﻿using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using MicrosoftOfficeInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
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

        private static string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\WordTransfer";

        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }

        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        public ObservableCollection<SelectionItem> CheckedSelections { get; set; }

        public SelectionItem SelectedSelection { get; set; }

        private string commentAuthor = "NuSys";

        private string commentExported = "Exported to NuSys";

        public string Token { get; set; }

        public string NuSysSelectionId { get; set; }

        public Microsoft.Office.Interop.Word.Document CurDoc { get; set; }

        public SidePane(Microsoft.Office.Interop.Word.Document curDoc, string token, string selectionId)
        {
            InitializeComponent();
            this.DataContext = this;

            this.Token = token;
            this.CurDoc = curDoc;
            this.NuSysSelectionId = selectionId;

            ConvertToPdf();

            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();
            CheckedSelections = new ObservableCollection<SelectionItem>();


            Microsoft.Office.Tools.Word.Document vstoDoc = Globals.Factory.GetVstoObject(this.CurDoc);
            vstoDoc.BeforeClose += new System.ComponentModel.CancelEventHandler(ThisDocument_BeforeClose);
        }

        private void ConvertToPdf()
        {
            try
            {
                if (!String.IsNullOrEmpty(this.Token) && this.CurDoc != null && !String.IsNullOrEmpty(this.CurDoc.FullName))
                {
                    MessageBox.Show("Converting to pdf for NuSys");
                    String path = this.CurDoc.FullName;
                    String mediaFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";
                    string pdfPath = mediaFolderPath + "\\" + this.Token + ".pdf";

                    OfficeInterop.SaveWordAsPdf(path, pdfPath);
                }
            }
            catch (Exception ex)
            {
                //TODO error handling
            }
        }

        private void ThisDocument_BeforeClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveSelectionData();
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


        private List<SelectionItemIdView> readSelectionData()
        {
            List<SelectionItemIdView> selectionItemIdViews = new List<SelectionItemIdView>();

            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Microsoft.Office.Core.DocumentProperties)this.CurDoc.CustomDocumentProperties;

                foreach (Microsoft.Office.Core.DocumentProperty prop in properties)
                {
                    if (prop.Name.Contains("NuSysSelection"))
                    {
                        string json = prop.Value.ToString();
                        selectionItemIdViews.Add(JsonConvert.DeserializeObject<SelectionItemIdView>(json));

                    }
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }

            return selectionItemIdViews;
        }

        private void saveSelectionData()
        {
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Microsoft.Office.Core.DocumentProperties)this.CurDoc.CustomDocumentProperties;

                foreach (Microsoft.Office.Core.DocumentProperty prop in properties)
                {
                    if (prop.Name.StartsWith("NuSysSelection"))
                    {
                        prop.Delete();
                    }
                }

                int count = 0;
                foreach (SelectionItem expSel in ExportedSelections)
                {
                    SelectionItemIdView view = expSel.GetIdView();
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(view);

                    properties.Add("NuSysSelection" + count, false, 4, json);
                    count++;
                }

                foreach (SelectionItem unexpSel in UnexportedSelections)
                {
                    SelectionItemIdView view = unexpSel.GetIdView();
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(view);

                    properties.Add("NuSysSelection" + count, false, 4, json);
                    count++;
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
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

        private void LoadSelectionData(object sender, RoutedEventArgs e)
        {
            List<SelectionItemIdView> selectionItemIdViews = readSelectionData();

            //delete all NuSys comments
            var comments = this.CurDoc.Comments;
            foreach (var commentObj in comments)
            {
                Comment comment = ((Comment)commentObj);

                if (comment.Author == commentAuthor)
                {
                    comment.Delete();
                }
            }

            var bookmarks = this.CurDoc.Bookmarks;

            foreach (SelectionItemIdView selection in selectionItemIdViews)
            {
                if (bookmarks.Exists(selection.BookmarkId)){
                    var docSelection = bookmarks[selection.BookmarkId].Range;
                    SelectionItem ns;

                    if (!selection.IsExported)
                    {
                        Comment c = this.CurDoc.Comments.Add(docSelection, "");
                        c.Author = commentAuthor;

                        ns = new SelectionItem(this) { Comment = c, Bookmark = bookmarks[selection.BookmarkId], Range = docSelection, IsExported = false };
                        ns.AddSelection();
                        UnexportedSelections.Add(ns);
                        ic.Children.Add(ns);
                    }
                    else
                    {
                        Comment c = this.CurDoc.Comments.Add(docSelection, commentExported + " " + selection.DateTimeExported);
                        c.Author = commentAuthor;

                        ns = new SelectionItem(this) { Comment = c, DateTimeExported = selection.DateTimeExported, Bookmark = bookmarks[selection.BookmarkId], Range = docSelection, IsExported = true };
                        ns.AddSelection();
                        ExportedSelections.Add(ns);
                        ic2.Children.Add(ns);
                    }

                    if (selection.BookmarkId == this.NuSysSelectionId)
                    {
                        ns.DropShadow.Opacity = 1.0;

                        if (SelectedSelection != null && SelectedSelection != ns)
                        {
                            SelectedSelection.DropShadow.Opacity = 0.0;
                        }

                        SelectedSelection = ns;
                    }
                }
            }

            CheckSelectionLabels();
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
                    ic.Children.Remove(selection);
                }
                else if (ExportedSelections.Contains(selection))
                {
                    ExportedSelections.Remove(selection);
                    ic2.Children.Remove(selection);
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

                    for (int i=0; i<selection.ImageContent.Count; i++)
                    {
                        selection.ImageContent[i].Save(mediaDir + "\\" + selectionItemView.ImageNames[i], ImageFormat.Png);
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
                    ic.Children.Remove(selection);

                    ExportedSelections.Add(selection);
                    ic2.Children.Add(selection);

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

        //add the highlighted content to the sidebar as a selection
        private void OnSelectionAdded()
        {
            try {
                var temp = this.CurDoc.Path;
                Selection selection = this.CurDoc.ActiveWindow.Selection;

                var ns = new SelectionItem(this) { Range = selection.Range, IsExported = false };
                ns.AddSelection();

                Comment c = this.CurDoc.Comments.Add(selection.Range, "");
                c.Author = commentAuthor;
                ns.Comment = c;

                //using b as an arbitrary char to create a valid bookmarkId
                var bookmarkId = "NuSys" + (Guid.NewGuid().ToString()).Replace('-', 'b');
                var bookmark = selection.Bookmarks.Add(bookmarkId);
                ns.Bookmark = bookmark;

                if (ns.ImageContent.Count > 0 || !String.IsNullOrEmpty(ns.RtfContent))
                {
                    ic.Children.Add(ns);
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
