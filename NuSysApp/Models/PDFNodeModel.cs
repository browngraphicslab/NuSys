using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfNodeModel : NodeModel
    {
        public delegate void PdfImagesCreatedHandler();

        public PdfNodeModel(byte[] bytes, string id) : base(id)
        {
            NodeType = NodeType.PDF;
            ByteArray = bytes;
            Content = new NodeContentModel(ByteArray, id);
        }

        public int CurrentPageNumber { get; set; }

        public byte[] ByteArray { get; }
        public event PdfImagesCreatedHandler OnPdfImagesCreated;
        public event PdfImagesCreatedHandler OnPageChange;

        public override async Task<Dictionary<string, string>> Pack()
        {
            var props = await base.Pack();
            props.Add("nodeType", NodeType.PDF.ToString());
            props.Add("page", CurrentPageNumber.ToString());
            props.Add("data", Convert.ToBase64String(ByteArray));
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("page"))
            {
                CurrentPageNumber = int.Parse(props["page"]);
            }
            await base.UnPack(props);

        }
    }
}