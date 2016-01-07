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
        private String _customPropKeyUnexp = "NuSys UnexportedSelections";
        private String _customPropKeyExp = "NuSys ExportedSelections";
		
        public ObservableCollection<SelectionItem> UnexportedSelections { get; set; }
        public ObservableCollection<SelectionItem> ExportedSelections { get; set; }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            var standardUC = new UserControl();
            _sidePane = new SidePane();
            LoadSelectionData();
            _sidePane.UnexportedSelections = UnexportedSelections;
            _sidePane.ExportedSelections = ExportedSelections;
            var wpfHost = new ElementHost { Child = _sidePane };
            wpfHost.Dock = DockStyle.Fill;
            standardUC.Controls.Add(wpfHost);
            _pane = CustomTaskPanes.Add(standardUC, "NuSys");
            _pane.Width = 430;
            _pane.Visible = false;
            _pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            _pane.Visible = true;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
			Microsoft.Office.Core.DocumentProperties properties;
			properties = (Microsoft.Office.Core.DocumentProperties) Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

            foreach (Office.DocumentProperty prop in properties)
			{
                if (prop.Name == _customPropKeyUnexp)
                {
                    properties[_customPropKeyUnexp].Delete();
                }
                else if (prop.Name == _customPropKeyExp)
                {
                    properties[_customPropKeyExp].Delete();
                }
            }

			properties.Add(
                _customPropKeyUnexp, 
				true,
                null,
                null,
				UnexportedSelections);

            properties.Add(
                _customPropKeyExp,
                true,
                null,
                null,
                ExportedSelections);
        }

		private void LoadSelectionData(){
			Microsoft.Office.Core.DocumentProperties properties;
			//properties = (Microsoft.Office.Core.DocumentProperties) Globals.ThisAddIn.Application.ActiveDocument.CustomDocumentProperties;

            UnexportedSelections = new ObservableCollection<SelectionItem>();
            ExportedSelections = new ObservableCollection<SelectionItem>();

            /*foreach (Office.DocumentProperty prop in properties)
			{
				if (prop.Name == _customPropKeyUnexp)
				{
                    UnexportedSelections = (ObservableCollection<SelectionItem>)prop.Value;
				}else if (prop.Name == _customPropKeyExp) {
                    ExportedSelections = (ObservableCollection<SelectionItem>)prop.Value;
                }
			}*/   
		}
		
        #region VSTO generated code

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
