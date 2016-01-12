using Microsoft.Office.Tools;
using Microsoft.Office.Tools.Word;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Office = Microsoft.Office.Core;

namespace WordAddIn
{
    public partial class ThisAddIn
    {
        private CustomTaskPane _pane;
        private SidePane _sidePane;
        private String _selectionId;
        public List<SelectionItemIdView> _allSelectionItems = new List<SelectionItemIdView>();

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try { 
                using (StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\LauncherArguments\\OpenWord.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    _selectionId = sr.ReadToEnd();
                }

                if (!String.IsNullOrEmpty(_selectionId))
                {
                    BuildSidebar();

                    var bookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;

                    //get rid of excesse bookmarks
                    foreach (Microsoft.Office.Tools.Word.Bookmark bookmark in bookmarks)
                    {
                        if (bookmark.Name.Equals(_selectionId))
                        {
                            bookmark.Range.Select();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        private void saveSelectionData() {
            try
            {
                List<SelectionItemIdView> allSelectionItems = new List<SelectionItemIdView>();
                foreach (SelectionItem expSel in Globals.ThisAddIn.SidePane.ExportedSelections)
                {
                    allSelectionItems.Add(expSel.GetIdView());
                }

                foreach (SelectionItem unexpSel in Globals.ThisAddIn.SidePane.UnexportedSelections)
                {
                    allSelectionItems.Add(unexpSel.GetIdView());
                }

                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name == "NuSysSelections")
                    {
                        properties["NuSysSelections"].Delete();
                    }
                }

                string selectionItemJson = Newtonsoft.Json.JsonConvert.SerializeObject(allSelectionItems);
                properties.Add("NuSysSelections", false, 4, selectionItemJson);
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void readSelectionData()
        {
            try
            {
                //read in any customproperties
                string selectionItemJson = String.Empty;
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name == "NuSysSelections")
                    {
                        selectionItemJson = prop.Value.ToString();
                    }
                }
                if (!String.IsNullOrEmpty(selectionItemJson))
                {
                    _allSelectionItems = JsonConvert.DeserializeObject<List<SelectionItemIdView>>(selectionItemJson);
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void taskPaneValue_VisibleChanged(object sender, System.EventArgs e)
        {
            if (_pane.Visible) //opened
            {
                readSelectionData();
            }
            else //closed
            {
                saveSelectionData();
            }
        }
        public SidePane SidePane
        {
            get { return _sidePane;  }
        }

        public CustomTaskPane PaneControl
        {
            get { return _pane; }
        }

        public String SelectionId
        {
            get { return _selectionId; }
        }

        private void ThisDocument_BeforeClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveSelectionData();
        }

        public void BuildSidebar()
        {
            readSelectionData();
            var standardUC = new UserControl();
            _sidePane = new SidePane();
            var wpfHost = new ElementHost { Child = _sidePane };
            wpfHost.Dock = DockStyle.Fill;
            standardUC.Controls.Add(wpfHost);
            _pane = CustomTaskPanes.Add(standardUC, "NuSys");
            _pane.Visible = false;
            _pane.Width = 450;
            _pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            _pane.Visible = true;
            _pane.VisibleChanged += taskPaneValue_VisibleChanged;
            Document vstoDoc = Globals.Factory.GetVstoObject(this.Application.ActiveDocument);
            vstoDoc.BeforeClose += new System.ComponentModel.CancelEventHandler(ThisDocument_BeforeClose);
        }

        #region VSTO generated code

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new NuSysRibbon();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }
}
