using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Data;

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
            }else
            {
                noExpSelectionsLabel.Visibility = Visibility.Collapsed;
            }

            if (UnexportedSelections.Count == 0)
            {
                noSelectionsLabel.Visibility = Visibility.Visible;
            }
            else
            {
                noSelectionsLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSelectionData()
        {
            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();
            CheckedSelections = new ObservableCollection<SelectionItem>();

            var comments = Globals.ThisAddIn.Application.ActiveDocument.Comments;
            Boolean first = true;
            IDataObject prevData = null;

            foreach (var commentObj in comments)
            {   
                Comment comment = ((Comment)commentObj);

                if (comment.Author == commentAuthor) {
                    if (first)
                    {
                        prevData = Clipboard.GetDataObject();
                        Clipboard.Clear();
                        first = false;
                    }

                    string commentTxt = comment.Range.Text;

                    comment.Scope.Select();
                    comment.Scope.Copy();

                    if (commentTxt == null)
                    {
                        var ns = new SelectionItem { Comment = comment, Range = comment.Scope, IsExported = false };
                        UnexportedSelections.Add(ns);
                    } else
                    {
                        var ns = new SelectionItem { Comment = comment, Range = comment.Scope, IsExported = true };
                        ExportedSelections.Add(ns);
                    }
                }
            }

            if (prevData != null)
            {
                Clipboard.Clear();
                Clipboard.SetDataObject(prevData);
            }
        }
		
		//delete all checked selections
		private void OnDelete(){
			foreach (var selection in CheckedSelections){
				selection.Comment.Delete();
                //may be a reference problem...?

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
                    selection.Comment.Range.Text = commentExported;
                    
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
            IDataObject prevData = Clipboard.GetDataObject();
            Clipboard.Clear();

            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            selection.Select();
            selection.Copy();
            
            if (Clipboard.ContainsData(System.Windows.DataFormats.Rtf))
            {
                Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(Globals.ThisAddIn.Application.Selection.Range, "");
                c.Author = commentAuthor;

                var ns = new SelectionItem { Comment = c, Range = selection.Range, IsExported = false };
                UnexportedSelections.Add(ns);
            }

            Clipboard.Clear();
            Clipboard.SetDataObject(prevData);
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
                bitmap.Save(mediaDir + "\\__ASDAD.png");
            }
            return bitmap;
        }
    }
}
