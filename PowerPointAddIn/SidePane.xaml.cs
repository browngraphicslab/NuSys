using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Office.Interop.PowerPoint;
using System.Drawing.Imaging;

namespace PowerPointAddIn
{
    /// <summary>
    /// Interaction logic for SidePane.xaml
    /// </summary>
    public partial class SidePane : UserControl
    {
        public ObservableCollection<SelectionItem> Selections { get; set; }

        public SidePane()
        {
            InitializeComponent();
            ic.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Selections.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OnSelectionAdded();
        }

        private void OnSelectionAdded()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";
            var mediaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";

            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            selection.Copy();

            System.Windows.Forms.IDataObject data = System.Windows.Forms.Clipboard.GetDataObject();
            if (null != data)
            {
                foreach (var f in data.GetFormats())
                {
                    Debug.WriteLine(f);
                }

                string result = string.Empty;
                var imgFileName = string.Format(@"{0}", Guid.NewGuid());
                imgFileName = imgFileName + ".png";

                if (data.GetDataPresent(System.Windows.Forms.DataFormats.Rtf))
                {
                    result = (string)data.GetData(System.Windows.Forms.DataFormats.Rtf);
                }
                else if (data.GetDataPresent(System.Windows.Forms.DataFormats.Html))
                {
                    string html = (string)data.GetData(System.Windows.Forms.DataFormats.Html);
                    var converter = new SautinSoft.HtmlToRtf();
                    result = converter.ConvertString(html);
                }
                else if (data.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    result = imgFileName;
                    Bitmap bitmap = (data.GetData(System.Windows.Forms.DataFormats.Bitmap, true) as Bitmap);
                    bitmap.Save(mediaDir + "\\" + imgFileName, System.Drawing.Imaging.ImageFormat.Png);
                    System.IO.File.SetLastWriteTimeUtc(mediaDir + "\\" + imgFileName, DateTime.UtcNow);
                }

                Bitmap thumbnail = null;
                if (data.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    result = imgFileName;
                    thumbnail = (data.GetData(DataFormats.Bitmap, true) as Bitmap);
                    thumbnail.Save(mediaDir + "\\" + imgFileName, ImageFormat.Png);
                }

                if (result == string.Empty)
                    return;


                var imgSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(thumbnail.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                // Create a comment
                var posX = selection.ShapeRange.Left;
                var posY = selection.ShapeRange.Top;
                var currentSlide = (Slide)Globals.ThisAddIn.Application.ActiveWindow.View.Slide;
                var c = currentSlide.Comments.Add(posX, posY, "NuSys", "NuSys", "This region to NuSys");
                Selections.Add(new SelectionItem { Content = result, Comment = c, Slide = currentSlide, SlideNumber = currentSlide.SlideNumber, Thumbnail = imgSrc });
            }
        }

        private void Send()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";
            var fileDir = dir + "\\selection";
            int count = 0;
            foreach (var result in Selections)
            {
                var f = fileDir + count + ".nusys";
                File.WriteAllText(f, result.Content);
                File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                File.Move(f, f);
                count++;
            }

            File.WriteAllText(dir + "\\update.nusys", "update");
        }

    }
}
