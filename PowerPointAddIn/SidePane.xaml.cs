using Microsoft.Office.Interop.PowerPoint;
using MicrosoftOfficeInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PowerPointAddIn
{
    /// <summary>
    /// Interaction logic for SidePane.xaml
    /// </summary>
    public partial class SidePane : UserControl
    {

        private static string mediaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";

        private static string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\PowerPointTransfer";

        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }

        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        public ObservableCollection<SelectionItem> CheckedSelections { get; set; }

        private string commentAuthor = "NuSys";

        private string commentExported = "Exported to NuSys";

        public string Token { get; set; }

        public Microsoft.Office.Interop.PowerPoint.DocumentWindow CurWin { get; set; }

        public Microsoft.Office.Interop.PowerPoint.Presentation CurPres { get; set; }

        public SidePane(Microsoft.Office.Interop.PowerPoint.Presentation curPres, Microsoft.Office.Interop.PowerPoint.DocumentWindow curWin, string token)
        {
            InitializeComponent();
            this.DataContext = this;

            this.Token = token;
            this.CurWin = curWin;
            this.CurPres = curPres;

            ConvertToPdf();

            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();
            CheckedSelections = new ObservableCollection<SelectionItem>();

            CheckSelectionLabels();
        }

        private void ConvertToPdf()
        {
            try
            {
                if (this.CurPres != null && !String.IsNullOrEmpty(this.CurPres.FullName))
                {
                    MessageBox.Show("Converting to pdf for NuSys");
                    String path = this.CurPres.FullName;
                    String mediaFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\Media";
                    string pdfPath = mediaFolderPath + "\\" + this.Token + ".pdf";

                    OfficeInterop.SavePresentationAsPdf(path, pdfPath);
                }
            }
            catch (Exception ex)
            {
                //TODO error handling
            }
        }

        private void UnexpOnClick(object sender, RoutedEventArgs e)
        {
            if ((string)unexpBttn.Content == "+")
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

        //delete all checked selections
        private void OnDelete()
        {
            foreach (var selection in CheckedSelections)
            {
                try
                {
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
        private void OnExport()
        {
            var fileDir = dir + "\\selection";
            int count = 0;

            var temp_cs = new List<SelectionItem>();
            var hasNewSelection = false;

            List<SelectionItemView> selectionItemViews = new List<SelectionItemView>();

            foreach (var selection in CheckedSelections)
            {
                if (UnexportedSelections.Contains(selection))
                {
                    var selectionItemView = selection.GetView();

                    for (int i = 0; i < selection.ImageContent.Count; i++)
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
                            var posX = selection.ThisSelection.ShapeRange.Left;
                            var posY = selection.ThisSelection.ShapeRange.Top;
                            var currentSlide = (Slide)this.CurWin.View.Slide;
                            var c = currentSlide.Comments.Add(posX, posY, commentAuthor, commentAuthor, commentExported + " " + DateTime.Now.ToString());

                            selection.Comment.Delete();
                          
                            selection.Comment = c;
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
                Clipboard.Clear();

                var selection = this.CurWin.Selection;

                selection.Copy();

                var posX = selection.ShapeRange.Left;
                var posY = selection.ShapeRange.Top;
                var currentSlide = (Slide)this.CurWin.View.Slide;
                var c = currentSlide.Comments.Add(posX, posY, commentAuthor, commentAuthor, "");

                var ns = new SelectionItem(this) { Comment = c, ThisSelection = selection, IsExported = false };
                ns.AddSelection();
                UnexportedSelections.Add(ns);
                ic.Children.Add(ns);
                
            }
            catch (Exception ex)
            {
                //TODO error handling 
            }
        }
    }
}