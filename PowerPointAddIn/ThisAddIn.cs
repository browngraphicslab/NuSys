using Microsoft.Office.Tools;
using MicrosoftOfficeInterop;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Office = Microsoft.Office.Core;

namespace PowerPointAddIn
{
    public partial class ThisAddIn
    {
        public Hashtable NuSysTaskPanes = new Hashtable();

        public void BuildSidebar()
        {
            try
            {
                RemoveOrphanedTaskPanes();

                string token = GetToken();

                if (String.IsNullOrEmpty(token))
                {
                    token = GetTokenFromFile();
                }

                var standardUC = new UserControl();
                SidePane _sidePane = new SidePane(this.Application.ActivePresentation, this.Application.ActiveWindow, token);
                var wpfHost = new ElementHost { Child = _sidePane };
                wpfHost.Dock = DockStyle.Fill;
                standardUC.Controls.Add(wpfHost);
                CustomTaskPane _pane = CustomTaskPanes.Add(standardUC, "NuSys");
                _pane.Visible = false;
                _pane.Width = 450;
                _pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
                _pane.Visible = true;

                NuSysTaskPanes.Add(this.Application.ActivePresentation, _pane);
            }
            catch (Exception ex)
            {
                //TODO handle exception
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        private void RemoveOrphanedTaskPanes()
        {
            for (int i = this.CustomTaskPanes.Count; i > 0; i--)
            {
                var ctp = this.CustomTaskPanes[i - 1];
                if (ctp.Window == null)
                {
                    this.CustomTaskPanes.Remove(ctp);
                }
            }
        }

        private string GetToken()
        {
            string token = null;
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActivePresentation.CustomDocumentProperties;

                foreach (Office.DocumentProperty prop in properties)
                {
                    if (prop.Name == "FileToken")
                    {
                        token = prop.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }

            return token;
        }

        private string GetTokenFromFile()
        {
            string token = null;
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\OpenDocParams";
                string fileName = "FirstTimePowerpoint.txt";

                using (StreamReader sr = new StreamReader(path + "\\" + fileName))
                {
                    // Read the stream to a string, and write the string to the console.
                    token = (sr.ReadToEnd())?.Trim();
                }

                File.WriteAllText(path + "\\" + fileName, String.Empty);

                if (!String.IsNullOrEmpty(token))
                {
                    Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActivePresentation.CustomDocumentProperties;

                    foreach (Office.DocumentProperty prop in properties)
                    {
                        if (prop.Name == "FileToken")
                        {
                            prop.Delete();
                        }
                    }

                    properties.Add("FileToken", false, 4, token);
                }
                else
                {
                    token = null;
                }
            }
            catch (Exception ex)
            {
                //TODO error handing
            }

            return token;
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
