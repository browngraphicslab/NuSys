namespace NuSysApp
{
    public interface IEditable : ISelectable
    {
        bool IsEditing { get; set; }
    }
}