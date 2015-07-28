using GemBox.Document;

namespace MicrosoftOfficeInterop
{
    class Program
    {
        static void Main(string[] args)
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            var doc = new DocumentModel();
            doc.Sections.Add(new Section(doc, new Paragraph(doc, "Hello world!")));
            doc.Save(@"C:\Users\Gary\Documents\Document.docx");
            doc.Save(@"C:\Users\Gary\Documents\Document.pdf");
        }
    }
}
