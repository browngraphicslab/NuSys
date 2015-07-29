
namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, int id): base(id)
        {
            Text = data;
        }

        public string Text { get; set; }
    }
}
