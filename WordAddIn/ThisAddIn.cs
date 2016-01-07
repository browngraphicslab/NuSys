using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools;
using Application = Microsoft.Office.Interop.Word.Application;
using Office = Microsoft.Office.Core;
using System.Collections.ObjectModel;
using System.Windows.Forms.Integration;

namespace WordAddIn
{
    public partial class ThisAddIn
    {
        private CustomTaskPane _pane;
        private SidePane _sidePane;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {

        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
			
        }

        public void BuildSidebar()
        {
            var standardUC = new UserControl();
            _sidePane = new SidePane();
            var wpfHost = new ElementHost { Child = _sidePane };
            wpfHost.Dock = DockStyle.Fill;
            standardUC.Controls.Add(wpfHost);
            _pane = CustomTaskPanes.Add(standardUC, "NuSys");
            _pane.Width = 430;
            _pane.Visible = false;
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
