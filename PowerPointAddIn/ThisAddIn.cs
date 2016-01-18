﻿using Microsoft.Office.Tools;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Office = Microsoft.Office.Core;

namespace PowerPointAddIn
{
    public partial class ThisAddIn
    {
        private CustomTaskPane _pane;
        private SidePane _sidePane;
        public string _fileToken;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            GetToken();

            if (String.IsNullOrEmpty(_fileToken))
            {
                GetTokenFromFile();
            }

            if (!String.IsNullOrEmpty(_fileToken))
            {
                BuildSidebar();
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        public SidePane SidePane
        {
            get { return _sidePane; }
        }

        public CustomTaskPane PaneControl
        {
            get { return _pane; }
        }

        private void GetToken()
        {
            try
            {
                Microsoft.Office.Core.DocumentProperties properties = (Office.DocumentProperties)Globals.ThisAddIn.Application.ActivePresentation.CustomDocumentProperties;

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
                string fileName = "FirstTimePowerpoint";

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
