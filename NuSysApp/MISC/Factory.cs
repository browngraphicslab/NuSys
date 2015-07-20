namespace NuStarterProject
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
 