﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordAddIn1
{
public class WordSaveHandler
{
    public delegate void AfterSaveDelegate(Microsoft.Office.Interop.Word.Document doc, bool isClosed);
    public event AfterSaveDelegate AfterUiSaveEvent;
    public event AfterSaveDelegate AfterAutoSaveEvent;
    public event AfterSaveDelegate AfterSaveEvent;
    private bool preserveBackgroundSave;
    private Microsoft.Office.Interop.Word.Application oWord;

        private bool _isKilled;

    public WordSaveHandler(Microsoft.Office.Interop.Word.Application oApp)
    {
        oWord = oApp;
        oApp.DocumentBeforeSave += oApp_DocumentBeforeSave;
    }

        public void Kill()
        {
            _isKilled = true;
        }

    /// <summary>
    /// WORD EVENT – fires before a save event.
    /// </summary>
    /// <param name="Doc"></param>
    /// <param name="SaveAsUI"></param>
    /// <param name="Cancel"></param>
    void oApp_DocumentBeforeSave(Microsoft.Office.Interop.Word.Document Doc, 
                                 ref bool SaveAsUI, ref bool Cancel)
    {
        // This could mean one of four things:
        // 1) we have the user clicking the save button
        // 2) Another add-in or process firing a resular Document.Save()
        // 3) A Save As from the user so the dialog came up
        // 4) Or an Auto-Save event…
        // so, we will start off by first:
        // 1) Grabbing the current background save flag. We want to force
        //    the save into the background so that Word will behave 
        //    asyncronously. Typically, this feature is on by default, 
        //    but we do not want to make any assumptions or this code 
        //    will fail.
        // 2) Next, we fire off a thread that will keep checking the
        //    BackgroundSaveStatus of Word. And when that flag is OFF
        //    no know we are AFTER the save event…
        preserveBackgroundSave = oWord.Options.BackgroundSave;
        oWord.Options.BackgroundSave = true;
        // kick off a thread and pass in the document object
        bool UiSave = SaveAsUI; // have to do this because the bool from Word
                                // is passed to us as ByRef…
        new Thread(c =>
        {
            Handle_WaitForAfterSave(Doc, UiSave);
        }).Start();
    }

    /// <summary>
    /// This method is the thread call that waits for the same to compelte. 
    /// The way we detect the After Save event is to essentially enter into
    /// a loop where we keep checking the background save status. If the
    /// status changes we know the save is compelte and we finish up by
    /// determineing which type of save it was:
    /// 1) UI
    /// 2) Regular
    /// 3) AutoSave
    /// </summary>
    /// <param name="Doc"></param>
    /// <param name="UiSave"></param>
    private void Handle_WaitForAfterSave(Microsoft.Office.Interop.Word.Document Doc, bool UiSave)
    {
        try
        {
            // we have a UI save, so we need to get stuck
            // here until the user gets rid of the SaveAs dialog
            if (UiSave)
            {
                while (isBusy(Doc) && !_isKilled)
                    Thread.Sleep(1);
            }

            // check to see if still saving in the background
            // we will hang here until this changes.
            while (oWord.BackgroundSavingStatus > 0 && !_isKilled)
                Thread.Sleep(1);
        }
        catch
        {
            oWord.Options.BackgroundSave = preserveBackgroundSave;
            return; // swallow the exception
        }

        if (_isKilled)
            return;
        try
        {
            // if it is a UI save, the Save As dialog was shown
            // so we fire the after ui save event
            if (UiSave)
            {
                // we need to check to see if the document is
                // saved, because of the user clicked cancel
                // we do not want to fire this event
                try
                {
                    if (Doc.Saved == true)
                        AfterUiSaveEvent(Doc, false);
                }
                catch
                {
                    // DOC is null or invalid. This occurs because the doc
                    // was closed. So we return doc closed and null as the
                    // document…
                    AfterUiSaveEvent(null, true);
                }
            }
            else
            {
                // if the document is still dirty
                // then we know an AutoSave happened
                try
                {
                    if (Doc.Saved == false)
                        AfterAutoSaveEvent(Doc, false); // fire autosave event
                    else
                        AfterSaveEvent(Doc, false); // fire regular save event
                }
                catch
                {
                    // DOC is closed
                    AfterSaveEvent(null, true);
                }
            }
        }
        catch {}
        finally
        {
            // reset and exit thread
            oWord.Options.BackgroundSave = preserveBackgroundSave;
        }
    }

    /// <summary>
    /// Determines if Word is busy – essentially that the File Save
    /// dialog is currently open
    /// </summary>
    /// <param name="oApp"></param>
    /// <returns></returns>
    private bool isBusy(Microsoft.Office.Interop.Word.Document oDoc)
    {
        try
        {
            // if we try to access the application property while
            // Word has a dialog open, we will fail
            var oApp = oDoc.Application;
            return false; // not busy
        }
        catch
        {
            // so, Word is busy and we return true
            return true;
        }
    }
}
}