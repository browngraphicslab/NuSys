using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class ElementModel : Sendable
    {       
        private double _alpha = 1;
        private double _height;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private string _title = string.Empty;
        private double _width;
        private double _x;
        private double _y;
        

        public ElementModel(string id) : base(id)
        {
            
        }

        public NusysConstants.ElementType ElementType { get; set; } //TODO probably get rid of this if possible
 
        public string LibraryId { set; get; }

        public string ParentCollectionId { get; set; }   

        // TODO: Move color to higher level type

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                _width = value;
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                
                _height = value;
            }
        }

        public virtual double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
            }
        }

        public virtual double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
            }
        }

        public virtual double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;

            }
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                _title = value;
            }
        }
        public string CreatorId { get; set; }

        public virtual void Delete()
        {
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("x", X);
            dict.Add("y", Y);
            dict.Add("width", Width);
            dict.Add("height", Height);
            dict.Add("alpha", Alpha);
            dict.Add("scaleX", ScaleX);
            dict.Add("scaleY", ScaleY);
            dict.Add("title", Title);
            dict.Add("type", ElementType.ToString());
            dict.Add("contentId", LibraryId);
            return dict;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey(NusysConstants.ALIAS_LOCATION_X_KEY))
            {
                X = props.GetDouble(NusysConstants.ALIAS_LOCATION_X_KEY, X);
            }

            if (props.ContainsKey(NusysConstants.ALIAS_LOCATION_Y_KEY))
            {
                Y = props.GetDouble(NusysConstants.ALIAS_LOCATION_Y_KEY, Y);
            }
            
            if (props.ContainsKey(NusysConstants.ALIAS_SIZE_WIDTH_KEY))
            {
                Width = props.GetDouble(NusysConstants.ALIAS_SIZE_WIDTH_KEY, Width);
            }
            if (props.ContainsKey(NusysConstants.ALIAS_SIZE_HEIGHT_KEY))
            {
                Height = props.GetDouble(NusysConstants.ALIAS_SIZE_HEIGHT_KEY, Height);
            }

            if (props.ContainsKey(NusysConstants.ALIAS_CREATOR_ID_KEY))
            {
                CreatorId = props.GetString(NusysConstants.ALIAS_CREATOR_ID_KEY, null);
            }
            if (props.ContainsKey(NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY))
            {
                ParentCollectionId = props.GetString(NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY, ParentCollectionId);
            }
            if (props.ContainsKey(NusysConstants.ALIAS_LIBRARY_ID_KEY))
            {
                LibraryId = props.GetString(NusysConstants.ALIAS_LIBRARY_ID_KEY, "");
            }

            //TODO wtf our server refactor should have no hardcoded strings like these
            /*
            //may not need some of this stuff below
            Alpha = props.GetDouble("alpha", Alpha);
            ScaleX = props.GetDouble("scaleX", ScaleX);
            ScaleY = props.GetDouble("scaleY", ScaleY);
            Title = props.GetString("title", "");

            if (props.ContainsKey("type"))
            {
                string t = props.GetString("type");
                ElementType = (NusysConstants.ElementType)Enum.Parse(typeof(NusysConstants.ElementType), t);
            }
            else if (props.ContainsKey("nodeType"))
            {
                string t = props.GetString("nodeType");
                ElementType = (NusysConstants.ElementType)Enum.Parse(typeof(NusysConstants.ElementType), t);
            }
            */
            

            base.UnPackFromDatabaseMessage(props);
        }
    }
}