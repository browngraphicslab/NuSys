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

        public Visibility IsUnexpVisible { get; set; }

        
        public SidePane()
        {
            InitializeComponent();
            ic.DataContext = this;
            ic2.DataContext = this;
            LoadSelectionData();
            IsUnexpVisible = Visibility.Visible;
        }

        private void UnexpOnClick(object sender, RoutedEventArgs e)
        {
            IsUnexpVisible = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnDelete();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
			OnExport();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OnSelectionAdded();
        }

        private void LoadSelectionData()
        {
            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();
            CheckedSelections = new ObservableCollection<SelectionItem>();

            var comments = Globals.ThisAddIn.Application.ActiveDocument.Comments;

            foreach (var commentObj in comments)
            {
                Comment comment = ((Comment)commentObj);
                string commentTxt = comment.Range.Text;

                if (commentTxt == null)
                {
                    var ns = new SelectionItem { Comment = comment, Range = comment.Scope, IsExported = false };
                    UnexportedSelections.Add(ns);
                }else
                {
                    var ns = new SelectionItem { Comment = comment, Range = comment.Scope, IsExported = true };
                    ExportedSelections.Add(ns);
                }
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

            foreach (var selection in CheckedSelections)
            {
                //var f = fileDir + count + ".nusys";
                //File.WriteAllText(f, selection.Content);
                //File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                //File.Move(f, f);

                if (UnexportedSelections.Contains(selection))
                {
                    selection.IsExported = true;
                    selection.Comment.Range.Text = "Exported to NuSys";
                    
                    UnexportedSelections.Remove(selection);
                    ExportedSelections.Add(selection);

                    count++;
                }

                //need a seperate list to iterate and delete/uncheck
                temp_cs.Add(selection);
            }

            //need seperate for loop because unchecking triggers a removal in CheckedSelections
            foreach (var cs in temp_cs)
            {
                //this also triggers a removal from CheckedSelections
                cs.CheckBox.IsChecked = false;
            }

            //File.WriteAllText(dir + "\\update.nusys", "update");
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
                c.Author = "NuSys";

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

        private static BitmapSource CopyScreen()
        {
            var left = System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.X);
            var top = System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.Y);
            var right = System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
            var bottom = System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
            var width = right - left;
            var height = bottom - top;

            using (var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

    }
}
