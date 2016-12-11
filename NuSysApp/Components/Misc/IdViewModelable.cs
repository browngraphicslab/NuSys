namespace NuSysApp
{
    public interface IdViewModelable
    {
        string Id { get; }
        double Width { get; }
        double Height { get; }
        double X { get; }
        double Y { get; }
        bool IsSelected { get; }
    }
}
