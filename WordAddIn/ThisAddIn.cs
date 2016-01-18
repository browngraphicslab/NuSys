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
        public String _selectionId;
        public List<SelectionItemIdView> _allSelectionItems = new List<SelectionItemIdView>();
        public String _fileToken;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            GetToken();
            
            if (String.IsNullOrEmpty(_fileToken))
            {
                GetTokenFromFile();
            }

            GetBookmarkFromFile();

            if (!String.IsNullOrEmpty(_selectionId))
            {
                BuildSidebar();
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        private void readSelectionData()
        {
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name.Contains("NuSysSelection"))
                    {
                        string json = prop.Value.ToString();
                        _allSelectionItems.Add(JsonConvert.DeserializeObject<SelectionItemIdView>(json));

                    }
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void saveSelectionData() {
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name.StartsWith("NuSysSelection"))
                    {
                        prop.Delete();
                    }
                }

                int count = 0;
                foreach (SelectionItem expSel in Globals.ThisAddIn.SidePane.ExportedSelections)
                {
                    SelectionItemIdView view = expSel.GetIdView();
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(view);

                    properties.Add("NuSysSelection" + count, false, 4, json);
                    count++;
                }

                foreach (SelectionItem unexpSel in Globals.ThisAddIn.SidePane.UnexportedSelections)
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
        
        private void GetToken()
        {
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name == "FileToken")
                    {
                        _fileToken = properties["FileToken"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void GetTokenFromFile()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\OpenDocParams";
                string fileName = "FirstTimeWord";

                using (StreamReader sr = new StreamReader(path + "\\" + fileName))
                {
                    // Read the stream to a string, and write the string to the console.
                    _fileToken = sr.ReadToEnd();
                }

                File.WriteAllText(path + "\\" + fileName, String.Empty);
            }
            catch (Exception ex)
            {
                //TODO error handing
            }
        }

        private void GetBookmarkFromFile()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\OpenDocParams";
                string fileName = _fileToken;

                using (StreamReader sr = new StreamReader(path + "\\" + fileName))
                {
                    // Read the stream to a string, and write the string to the console.
                    _selectionId = sr.ReadToEnd();
                }

                File.WriteAllText(path + "\\" + fileName, String.Empty);
            }
            catch (Exception ex)
            {
                //TODO error handing
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
