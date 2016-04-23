using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;

namespace NuSysApp
{
    public class LinkModel : ElementModel
    {
        public LinkModel(string id) : base(id)
        {
            Id = id;
            ElementType = ElementType.Link;
        }

        public bool IsPresentationLink { get; set; }

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }
        public string Annotation { get; set; }

        public LinkedTimeBlockModel InFineGrain { get; set; }
        public RectangleViewModel RectangleMod { get; set; }

        public Dictionary<string, object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }

        public override async Task UnPack(Message props)
        {
            IsPresentationLink = props.GetBool("isPresentationLink", false);
            InAtomId = props.GetString("id1", InAtomId);
            OutAtomId = props.GetString("id2", InAtomId);
            InFGDictionary = props.GetDict<string, object>("inFGDictionary");
            OutFGDictionary = props.GetDict<string, object>("outFGDictionary");
            Annotation = props.GetString("annotation", "");

            if (props.ContainsKey("rectangleMod"))
            {
                var viewModel = JsonConvert.DeserializeObject<RectangleViewModel>(props.Get("rectangleMod"));
                List<RectangleViewModel> regionsList;

                if (SessionController.Instance.IdToControllers[OutAtomId].Model.ElementType == ElementType.PDF)
                {
                    PdfNodeModel model = (PdfNodeModel) SessionController.Instance.IdToControllers[OutAtomId].Model;
                    foreach (KeyValuePair<int, List<RectangleViewModel>> entry in model.PageRegionDict)
                    {
                        List<RectangleViewModel> list = entry.Value;
                        foreach (var rectangleViewModel in list)
                        {
                            if (rectangleViewModel.LeftRatio == viewModel.LeftRatio && rectangleViewModel.TopRatio == viewModel.TopRatio &&
                            rectangleViewModel.RectWidthRatio == viewModel.RectWidthRatio && rectangleViewModel.RectHeightRatio == viewModel.RectHeightRatio && 
                            rectangleViewModel.PdfPageNumber == viewModel.PdfPageNumber)
                            {
                                RectangleMod = rectangleViewModel;
                                break;
                            }
                        }
                    }

                } else if (SessionController.Instance.IdToControllers[OutAtomId].Model.ElementType == ElementType.Image)
                {
                    regionsList = ((ElementModel)SessionController.Instance.IdToControllers[OutAtomId].Model).RegionsModel;

                    foreach (var rectangle in regionsList)
                    {
                        if (rectangle.LeftRatio == viewModel.LeftRatio && rectangle.TopRatio == viewModel.TopRatio &&
                            rectangle.RectWidthRatio == viewModel.RectWidthRatio && rectangle.RectHeightRatio == viewModel.RectHeightRatio)
                        {
                            RectangleMod = rectangle;
                            break;
                        }
                    }
                }    
            }

            if (props.ContainsKey("inFineGrain"))
            {
                var v = JsonConvert.DeserializeObject<LinkedTimeBlockModel>(props.Get("inFineGrain"));
                
                ObservableCollection<LinkedTimeBlockModel> list;
                
                switch (SessionController.Instance.IdToControllers[OutAtomId].Model.ElementType)
                {
                    case ElementType.Audio:
                        list =
                    ((AudioNodeModel)SessionController.Instance.IdToControllers[OutAtomId].Model)
                        .LinkedTimeModels;
                        foreach (var element in list)
                        {
                            if (element.Start == v.Start && element.End == v.End)
                            {
                                InFineGrain = element;
                                break;
                            }
                        }
                        break;
                    case ElementType.Video:
                        list =
                    ((VideoNodeModel)SessionController.Instance.IdToControllers[OutAtomId].Model)
                        .LinkedTimeModels;
                        foreach (var element in list)
                        {
                            if (element.Start == v.Start && element.End == v.End)
                            {
                                InFineGrain = element;
                                break;
                            }
                        }
                        break;
                }
               

            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            dict.Add("id2", OutAtomId);
            dict.Add("inFGDictionary", InFGDictionary);
            dict.Add("outFGDictionary", OutFGDictionary);
            dict.Add("type", ElementType.ToString());
            dict.Add("annotation", Annotation);
            dict.Add("inFineGrain", InFineGrain);
            dict.Add("isPresentationLink", IsPresentationLink);
            dict.Add("rectangleModel", RectangleMod);
            return dict;
        }
    }
}