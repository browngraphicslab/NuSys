using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class WebNodeModel : ElementModel
    {
        //private string _url = string.Empty;
        //private List<Webpage> _history;
        

        public WebNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Web;
            //_history = new List<Webpage>();
        }

        //public override async Task UnPack(Message props)
        //{
            //thelist = props.GetList<webpage>("thelist",thelist)
        //    await base.UnPack(props);
        //}

        //public override async Task<Dictionary<string, object>> Pack()
        //{
        //    var props = await base.Pack();
        //    return props;
        //}

        //public List<Webpage> History
        //{
        //    get { return _history; }
        //}

        //public class Webpage
        //{
        //    private string _url;
        //    private string _timestamp;
        //    public Webpage(string url, string timestamp)
        //    {
        //        _url = url;
        //        _timestamp = timestamp;

        //        Debug.WriteLine("Url: " + url + " timestamp: " + timestamp);
        //    }

        //    public string getUrl()
        //    {
        //        return _url;
        //    }
        //}
    }
}
