namespace NuSysApp
{
    public interface ISelectable
    {
        bool IsSelected
        {
            get;
            set;
        }

        void ToggleSelection();
    }
}