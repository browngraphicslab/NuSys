using Microsoft.Office.Tools;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Office = Microsoft.Office.Core;

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

        public SidePane SidePane
        {
            get { return _sidePane;  }
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
