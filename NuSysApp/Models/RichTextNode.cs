
namespace NuSysApp
{
    public class RichTextNode : Node
    {
        public RichTextNode(string data, int id): base(id)
        {
            Text = data;
        }

        public string Text { get; set; }
    }
}
