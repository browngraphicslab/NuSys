namespace NuSysApp2
{
    public interface IEditable : ISelectable
    {
        bool IsEditing { get; set; }
    }
}