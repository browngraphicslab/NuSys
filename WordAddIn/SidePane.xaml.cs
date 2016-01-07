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

		//collection of all selection objects
        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }
        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        //collection of all checked selections
        //only unexported selections can be selected
        public List<SelectionItem> CheckedSelections { get; set;}
		
        public SidePane()
        {
            InitializeComponent();
            ic.DataContext = this;
            ic2.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //OnDelete();
            UnexportedSelections.Clear();
			//RefreshSelectionViews();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
			//OnExport();
            Send();
			//RefreshSelectionViews();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OnSelectionAdded();
			//RefreshSelectionViews();
        }

		private void RefreshSelectionViews(){
			//UnexportedSelections.Refresh();
			//ExportedSelections.Refresh();
		}
		
		//add checked selection to a list of checked selections
		private void OnCheckSelection(){
			//CheckedSelections.Add();
		}
		
		//delete all checked selections
		private void OnDelete(){
			foreach (var selection in CheckedSelections){
				selection.Comment.Delete();
                //may be a reference problem...?
                UnexportedSelections.Remove(selection);
			}
		}
		
		//exports to NuSys all checked selections
		private void OnExport(){
		
			var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";
            var fileDir = dir + "\\selection";
            int count = 0;
            foreach (var result in CheckedSelections)
            {
                var f = fileDir + count + ".nusys";
                File.WriteAllText(f, result.Content);
                File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                File.Move(f, f);
				
				result.IsExported = true;
                //set comment text "Exported To NuSys" 
                var temp = result.Comment.Reference;

                UnexportedSelections.Remove(result);
                ExportedSelections.Add(result);
                count++;
            }

            File.WriteAllText(dir + "\\update.nusys", "update");
		}
		
		//add the highlighted content to the sidebar as a selection
        private void OnSelectionAdded()
        {
            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            selection.Select();
            selection.Copy();          
           
            IDataObject data = Clipboard.GetDataObject();

            if (null != data)
            {
                foreach (var f in data.GetFormats())
                {
                    Debug.WriteLine(f);
                }

                var doc = Globals.ThisAddIn.Application.ActiveDocument;

                string result = string.Empty;
                var imgFileName = string.Format(@"{0}", Guid.NewGuid());
                imgFileName = imgFileName + ".png"; 

                Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(Globals.ThisAddIn.Application.Selection.Range, "");
                c.Author = "NuSys";

                if (data.GetDataPresent(System.Windows.DataFormats.Html))
                {
                    result = (string)data.GetData(System.Windows.DataFormats.Html);
                    int n = 2;
                    string[] lines = result.Split(Environment.NewLine.ToCharArray()).Skip(n).ToArray();
                    result = string.Join(Environment.NewLine, lines);
                    var ns = new SelectionItem { Content = result, Comment = c, Range = selection.Range, DOcument = doc };
                    UnexportedSelections.Add(ns);
                }
                else if (data.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    // result = imgFileName;
                    // Bitmap bitmap = (Bitmap)(data.GetData(System.Windows.Forms.DataFormats.Bitmap, true));
                    // bitmap.Save(mediaDir + "\\" + imgFileName, System.Drawing.Imaging.ImageFormat.Png);
                    // System.IO.File.SetLastWriteTimeUtc(mediaDir + "\\" + imgFileName, DateTime.UtcNow);
                }

               /*     if (data.GetDataPresent(System.Windows.DataFormats.Rtf))
                {
                    result = (string)data.GetData(System.Windows.DataFormats.Rtf);
                    Selections.Add(new SelectionItem { Content = result, Comment = c, Range = selection.Range, DOcument = doc });

                }*/
            


                if (result == string.Empty)
                    return;

                Bitmap thumbnail = null;
                if (data.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    result = imgFileName;
                    thumbnail = (data.GetData(DataFormats.Bitmap, true) as Bitmap);
                    thumbnail.Save(mediaDir + "\\" + imgFileName, ImageFormat.Png);
                    UnexportedSelections.Add(new SelectionItem { Comment = c, Range = selection.Range, DOcument = doc });
                }
                
                 
                // Create a comment                
                //var posX = selection.ShapeRange.Left;
                //var posY = selection.ShapeRange.Top;
                //var currentSlide = (Document)Globals.ThisAddIn.Application.ActiveDocument;
                //var c = currentSlide.Comments.Add(posX, posY, "NuSys", "NuSys", "This region was added to NuSys");
            }
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


        private void Send()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";
            var fileDir = dir + "\\selection";
            int count = 0;
            foreach (var result in UnexportedSelections)
            {
                var f = fileDir + count + ".nusys";
                File.WriteAllText(f, result.Content);
                File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                File.Move(f, f);
				 
				result.IsExported = true;
                result.Comment.Delete();

                Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(result.Range, "Exported to NuSys");
                c.Author = "NuSys";
                result.Comment = c;
                ExportedSelections.Add(result);

                count++;
            }

            UnexportedSelections.Clear();
            File.WriteAllText(dir + "\\update.nusys", "update");
        }

    }
}
