namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, int id): base(id)
        {
            Text = data;
            ID = id;
        }
        
        public string Text { get; set; }
        
        public int ID { get; set; }
    }
}
