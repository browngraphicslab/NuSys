using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkElementController : ElementController
    {
        public delegate void AnchorUpdatedEventHandler(object source);

        public event AnchorUpdatedEventHandler AnchorUpdated;

        public delegate void AnnotationChangedEventHandler(string text);

        public event AnnotationChangedEventHandler AnnotationChanged;

        public ElementController InElement { get; set; }
        public ElementController OutElement { get; set; }

        public LinkElementController(LinkModel model) : base(model)
        {
            InElement = SessionController.Instance.IdToControllers[model.InAtomId];
            OutElement = SessionController.Instance.IdToControllers[model.OutAtomId];
            InElement.AddLink(this);
            OutElement.AddLink(this);

            PositionChanged += OnPositionChanged;
        }

        private void OnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            InElement.SetPosition(InElement.Model.X + dx, InElement.Model.Y + dy);
            OutElement.SetPosition(OutElement.Model.X + dx, OutElement.Model.Y + dy);
        }

        public override void Dispose()
        {
            InElement = null;
            OutElement = null;
            PositionChanged -= OnPositionChanged;
            base.Dispose();
        }

        public override async Task RequestMoveToCollection(string newCollectionContentID, double x = 50000, double y = 50000)
        {
            var metadata = new Dictionary<string, object>();
            var link = Model as LinkModel;

            var m1 = new Message();
            m1["metadata"] = metadata;
            m1["contentId"] = Model.LibraryId;
            m1["nodeType"] = Model.ElementType;
            m1["x"] = x;
            m1["y"] = y;
            m1["width"] = 200;
            m1["height"] = 200;
            m1["autoCreate"] = true;
            m1["creator"] = newCollectionContentID;
            m1["id1"] = link.InAtomId;
            m1["id2"] = link.OutAtomId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewLinkRequest(m1));
        }

        public void SetAnnotation(string text)
        {
            var linkModel = (LinkModel) Model;
            linkModel.Annotation = text;
            AnnotationChanged?.Invoke(text);

            _debouncingDictionary.Add("annotation", text);
        }

        public void UpdateAnchor()
        {
            AnchorUpdated?.Invoke(this);
        }

        public override Task UnPack(Message props)
        {
            if (props.ContainsKey("annotation"))
                AnnotationChanged?.Invoke(props.GetString("annotation"));
            return base.UnPack(props);
        }
    }
}
