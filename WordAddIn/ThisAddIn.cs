using Microsoft.Office.Tools;
using Microsoft.Office.Tools.Word;
using System;
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

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
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
                foreach (Bookmark bookmark in bookmarks)
                {
                    if (bookmark.Name.Equals(_selectionId))
                    {
                        bookmark.Range.Select();
                    }
                }
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

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


        public void BuildSidebar()
        {
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
