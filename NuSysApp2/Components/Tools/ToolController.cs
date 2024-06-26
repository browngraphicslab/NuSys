﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyToolkit.Utilities;
using NetTopologySuite.Utilities;
using SharpDX.DirectWrite;

namespace NuSysApp2
{
    public abstract class ToolController
    {
        public static Dictionary<string, ToolController> ToolControllers = new Dictionary<string, ToolController>();

        public delegate void LibraryIdsChangedEventHandler(object sender, HashSet<string> libraryIds);
        public delegate void LocationChangedEventHandler(object sender, double x, double y);
        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public delegate void DisposedEventHandler(string parentid);
        public delegate void ParentsIdsChangedEventHandler();
        public delegate void NumberOfParentsChangedEventHandler (int numberOfParents);

        public event LibraryIdsChangedEventHandler LibraryIdsChanged;
        public event LocationChangedEventHandler LocationChanged;
        public event SizeChangedEventHandler SizeChanged;
        public event DisposedEventHandler Disposed;
        public event ParentsIdsChangedEventHandler ParentsLibraryIdsChanged;
        public event NumberOfParentsChangedEventHandler NumberOfParentsChanged;




        public ToolModel Model { get;}
        public ToolController(ToolModel model)
        {
            Debug.Assert(model != null);
            Model = model;
            ToolControllers.Add(model.Id, this);
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));

            //CODE TO DELETE Non RMS STUFF
            /*
             foreach (var id in new HashSet<string>(SessionController.Instance.ContentController.IdList))
             {
                 var s = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                 if(s.Creator.ToLower() != "rms" && s.Creator.ToLower() != "rosemary"){
                     Task.Run(async delegate
                     {
                         await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteLibraryElementRequest(id));
                     });
                }
             }
             */
            

            //CODE BELOW IS HACKY WAY TO DOWNLOAD ALL THE PDF'S 
            /*
            Task.Run(async delegate
            {
                int i = 0;
                foreach (var id in LibraryElementModel.PDFStrings)
                {
                    Debug.WriteLine((double)i++ / (double)LibraryElementModel.PDFStrings.Count);
                    if (SessionController.Instance.ContentController.GetLibraryElementModel(id) != null &&
                        SessionController.Instance.ContentController.GetLibraryElementModel(id).Type == ElementType.PDF)
                    {
                        await Task.Run(async delegate
                        {
                            await SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(id);
                            try
                            {
                                var lem = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                                var document = await MediaUtil.DataToPDF(lem.Data);
                                lem.Data = null;
                                string data = "";
                                int numPages = document.PageCount;
                                int currPage = 0;
                                while (currPage < numPages)
                                {
                                    data = data + document.GetAllTexts(currPage);
                                    currPage++;
                                }

                                using (
                                    var stream =
                                        new FileStream(
                                            @"C:\Users\graphics_lab\Documents\junsu_pdfs\" + lem.Title + ".txt",
                                            FileMode.OpenOrCreate,
                                            FileAccess.Write))
                                {
                                    using (var writer = new StreamWriter(stream))
                                    {
                                        writer.Write(data);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                        });

                    }
                }
            });*/
        }

        public void SetSize(double width, double height)
        {
            SizeChanged?.Invoke(this, width, height);
        }
        
        public void AddParent(ToolController parentController)
        {
            if (parentController != null)
            {
                if (Model.ParentIds.Add(parentController.Model?.Id))
                {
                    parentController.LibraryIdsChanged += ParentLibraryIdsChanged;
                    Model.SetLibraryIds(Filter(GetUpdatedDataList()));
                    LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
                    ParentsLibraryIdsChanged?.Invoke();
                    parentController.Disposed += OnParentDisposed;
                    NumberOfParentsChanged?.Invoke(Model.ParentIds.Count);
                    
                }
            }
        }

        public void SetParentOperator(ToolModel.ParentOperatorType parentOperator)
        {
            Model.SetParentOperator(parentOperator);
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
            ParentsLibraryIdsChanged?.Invoke();
        }

        public void OnParentDisposed(string parentid)
        {
            ToolControllers[parentid].LibraryIdsChanged -= ParentLibraryIdsChanged;
            Model.ParentIds.Remove(parentid);
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
            ParentsLibraryIdsChanged?.Invoke();
            ToolControllers[parentid].Disposed -= OnParentDisposed;
        }

        public virtual void RemoveParent(ToolController parentController)
        {
            if (Model.RemoveParentId(parentController?.Model?.Id))
            {
                if (parentController != null)
                {
                    parentController.LibraryIdsChanged -= ParentLibraryIdsChanged;

                }
            }
            NumberOfParentsChanged?.Invoke(Model.ParentIds.Count);
        }

        public abstract void UnSelect();

        public virtual void Dispose()
        {
            foreach(var parentController in new List<ToolController>(Model.ParentIds.Select(id => ToolControllers.ContainsKey(id) ? ToolControllers[id] : null)))
            {
                RemoveParent(parentController);
            }
            Disposed?.Invoke(Model.Id);
            ToolControllers.Remove(Model.Id);
        }

        public HashSet<string> Filter(HashSet<string> ids)
        {
            if (!Model.Selected)
            {
                return ids;
            }
            var ret = new HashSet<string>();
            foreach (string id in ids)
            {
                if (GetFunc()(id))
                {
                    ret.Add(id);
                }
            }
            return ret;
        }

        public void FireLibraryIdsChanged()
        {
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);

        }
        public abstract Func<string, bool> GetFunc();

        protected string GetDate(LibraryElementModel libraryElementModel)
        {
            if (libraryElementModel.Timestamp == null)
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            return DateTime.Parse(libraryElementModel.Timestamp).ToStartOfDay().ToString();
        }

        protected string GetLastEditedDate(LibraryElementModel libraryElementModel)
        {
            if (libraryElementModel.LastEditedTimestamp == null)
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            return DateTime.Parse(libraryElementModel.LastEditedTimestamp).ToStartOfDay().ToString();
        }
        public Dictionary<string, List<string>> GetMetadata(string libraryId)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (controller != null)
            {
                var metadata = (controller?.LibraryElementModel?.FullMetadata?.ToDictionary(k=>k.Key,v=>v.Value?.Values ?? new List<string>()) ?? new Dictionary<string, List<string>>());

                var element = controller.LibraryElementModel;
                Debug.Assert(element != null);
                metadata["Title"] = new List<string>(){ element.Title};
                metadata["Type"] = new List<string>() { element.Type.ToString()};
                metadata["Date"] = new List<string>() { GetDate(element)};
                metadata["LastEditedDate"] = new List<string>() { GetLastEditedDate(element) };
                metadata["Creator"] = new List<string>() { element.Creator};
                return metadata;
            }
            return new Dictionary<string, List<string>>();
        }
        private void ParentLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
            ParentsLibraryIdsChanged?.Invoke();
        }
        
        //Returns all the library ids of everything in the previous filter
        public HashSet<string> GetUpdatedDataList()
        {
            var controllers = Model.ParentIds.Select(item => ToolControllers.ContainsKey(item) ? ToolControllers[item] : null);
            if (controllers.Count() == 0)
            {
                return SessionController.Instance.ContentController.IdList;
            }
            IEnumerable<string> list = controllers?.First().Model.LibraryIds;
            foreach (var enumerable in controllers.Select(controller => controller?.Model.LibraryIds))
            {
                switch (Model.ParentOperator)
                {
                    case ToolModel.ParentOperatorType.And:
                        list = list.Intersect(enumerable);
                        break;
                    case ToolModel.ParentOperatorType.Or:
                        list = list.Concat(enumerable ?? new HashSet<string>());
                        break;
                }
            }
           
            return new HashSet<string>(list);
        }
        public void SetLocation(double x, double y)
        {
            LocationChanged?.Invoke(this, x, y);
        }
    }
    
}
