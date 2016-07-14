using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Net;

namespace WordAddIn1
{
    public partial class ThisAddIn
    {
        private WordSaveHandler _wsh;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _wsh = new WordSaveHandler(Application);
            RegisterSaveEvents();

        }

        private void SendToNuSys()
        {   
            UnregisterSaveEvents();
            _wsh.Kill();
           
            // Read bytes of current document
            var tmp = Path.GetTempFileName();
            IPersistFile compoundDocument = Application.ActiveDocument as IPersistFile;
            compoundDocument.Save(tmp, false);
            var bytes = File.ReadAllBytes(tmp);
            File.Delete(tmp);


            var request = (HttpWebRequest)WebRequest.Create("http://nusysrepo.azurewebsites.net/api/uploadworddoc/2234");

            var data = Encoding.Default.GetBytes(Convert.ToBase64String(bytes));

            request.Method = "POST";
            request.ContentType = "application/xml";    
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
                

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            _wsh = new WordSaveHandler(Application);
            RegisterSaveEvents();
        }

        private void RegisterSaveEvents()
        {
            _wsh.AfterAutoSaveEvent += wsh_AfterAutoSaveEvent;
            _wsh.AfterSaveEvent += wsh_AfterSaveEvent;
            _wsh.AfterUiSaveEvent += wsh_AfterUiSaveEvent;
        }

        private void UnregisterSaveEvents()
        {
            _wsh.AfterAutoSaveEvent -= wsh_AfterAutoSaveEvent;
            _wsh.AfterSaveEvent -= wsh_AfterSaveEvent;
            _wsh.AfterUiSaveEvent -= wsh_AfterUiSaveEvent;
        }

        private void wsh_AfterUiSaveEvent(Word.Document doc, bool isClosed)
        {
            if (!isClosed)
                Debug.WriteLine("After SaveAs Event");
            else
                Debug.WriteLine("After Close and SaveAs Event");

            SendToNuSys();
        }

        private void wsh_AfterSaveEvent(Word.Document doc, bool isClosed)
        {
            if (!isClosed)
                Debug.WriteLine("After Save Event");
            else
                Debug.WriteLine("After Close and Save Event");

            SendToNuSys();
        }

        private void wsh_AfterAutoSaveEvent(Word.Document doc, bool isClosed)
        {
            Debug.WriteLine("After AutoSave Event");
            SendToNuSys();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            UnregisterSaveEvents();
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
