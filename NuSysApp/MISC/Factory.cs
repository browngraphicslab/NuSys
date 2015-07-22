namespace NuSysApp
{
    public class Factory
    {
        WorkspaceViewModel _workSpaceViewModel;
        public Factory(WorkspaceViewModel vm)
        {
            _workSpaceViewModel = vm;
        }

        public TextNodeViewModel CreateNewText(string data)
        {
            TextNodeViewModel textVM = new TextNodeViewModel(_workSpaceViewModel);
            textVM.Data = data;
            return textVM;
        }

        public RichTextNodeViewModel CreateNewRichText(string html)
        {
            RichTextNodeViewModel richTextVM = new RichTextNodeViewModel(_workSpaceViewModel);
            var block = HtmlRichTextConverter.GenerateBlockFromHtml(html, "https://en.wikipedia.org/");
            richTextVM.Data = block;
            return richTextVM;
        }


        public InkNodeViewModel CreateNewInk()
        {
            return new InkNodeViewModel(_workSpaceViewModel);
        }
   
        public NodeViewModel CreateFromDataString(string data)
        {
            //TO DO
            return null;
        }
    }

    
}
 