namespace NuSysApp
{
    public class ToolLinkModel
    {
        //public string LibraryId { get; private set; }
        public ToolLinkModel(string id = null)
        {
            Id = id ?? SessionController.Instance.GenerateId();
        }
        public string Id { get; private set; }

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }
        //TODO: public RegionView

        //public void SetLibraryId(string libraryElementId)
        //{
        //    LibraryId = libraryElementId;
        //}
    }
}