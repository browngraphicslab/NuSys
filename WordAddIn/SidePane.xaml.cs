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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using System.Drawing.Imaging;
using GemBox.Document;

namespace WordAddIn
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
            var mediaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";

            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            selection.Select();
            selection.Copy();

            var left = Globals.ThisAddIn.Application.Selection.get_Information(WdInformation.wdHorizontalPositionRelativeToPage);
            var top = Globals.ThisAddIn.Application.Selection.get_Information(WdInformation.wdVerticalPositionRelativeToPage);


            IDataObject data = Clipboard.GetDataObject();

            if (null != data)
            {
                foreach (var f in data.GetFormats())
                {
                    Debug.WriteLine(f);
                }

                string result = string.Empty;
                var imgFileName = string.Format(@"{0}", Guid.NewGuid());
                imgFileName = imgFileName + ".png"; 

                if (data.GetDataPresent(System.Windows.DataFormats.Rtf))
                {
                    result = (string)data.GetData(System.Windows.DataFormats.Rtf);               
                }
       

                if (result == string.Empty)
                    return;

                Comment c = Globals.ThisAddIn.Application.ActiveDocument.Comments.Add(Globals.ThisAddIn.Application.Selection.Range, "");
                c.Author = "NuSys";

                var doc = Globals.ThisAddIn.Application.ActiveDocument;
                Selections.Add(new SelectionItem { Content = result, Comment = c, DOcument = doc});
                
                // Create a comment                
                //var posX = selection.ShapeRange.Left;
                //var posY = selection.ShapeRange.Top;
                //var currentSlide = (Document)Globals.ThisAddIn.Application.ActiveDocument;
                //var c = currentSlide.Comments.Add(posX, posY, "NuSys", "NuSys", "This region was added to NuSys");
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
