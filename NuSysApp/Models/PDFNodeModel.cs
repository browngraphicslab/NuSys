using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfNodeModel : NodeModel
    {
        public int CurrentPageNumber { get; set; }
        public byte[] ByteArray { get; set; }
        public event PdfImagesCreatedHandler OnPdfImagesCreated;
        public event PdfImagesCreatedHandler OnPageChange;
        public delegate void PdfImagesCreatedHandler();

        public PdfNodeModel(string id) : base(id)
        {
            NodeType = NodeType.PDF;

        }
        
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("page", CurrentPageNumber.ToString());
            props.Add("data", Convert.ToBase64String(ByteArray));
            return props;
        }

        public override async Task UnPack(Message props)
        {
            CurrentPageNumber = props.GetInt("page", 0);
            ByteArray = props.GetByteArray("data");
            Content = new NodeContentModel(ByteArray, Id);
            await base.UnPack(props);

        }
    }
}