using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace MicrosoftOfficeInterop
{
    class OfficeInterop
    {
        /// <summary>
        /// Saves PowerPoint at specified filepath and returns the destination path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public static string SavePresentationAsPdf(string filePath, string destinationPath = null)
        {
            var presentation = OpenPresentation(filePath, false);
            //string destination = null;
            //presentation?.SaveAs(destinationPath ?? filePath, PowerPoint.PpSaveAsFileType.ppSaveAsPDF);
            var destination = destinationPath ?? filePath;
            presentation.SaveAs(destination, PowerPoint.PpSaveAsFileType.ppSaveAsPDF);
            return destination;
        }

        private static PowerPoint.Presentation OpenPresentation(string filePath, bool isVisible = true, bool readOnly = false)
        {
            var pptApplication = new PowerPoint.Application();
            var readOnlyState = MsoTriState.msoFalse;
            if (readOnly)
            {
                readOnlyState = MsoTriState.msoTrue;
            }
            var isVisibleState = MsoTriState.msoTrue;
            if (!isVisible)
            {
                isVisibleState = MsoTriState.msoFalse;
            }
            return pptApplication.Presentations.Open(filePath, readOnlyState, Untitled: MsoTriState.msoFalse, WithWindow: isVisibleState);
        }
    }
}
