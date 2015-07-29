using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.PowerPoint;

namespace PowerPointAddIn
{
    public class SelectionViewModel
    {

            private string _content;
            private Comment _comment;
            private Slide _slide;
            private int _slideNumber;

            public string Content
            {
                get { return _content; }
                set { _content = value; }
            }

            public Comment Comment
            {
                get { return _comment; }
                set { _comment = value;  }
            }

            public Slide Slide
            {
                get { return _slide; }
                set { _slide = value; }
            }
            public int SlideNumber
            {
                get { return _slideNumber; }
                set { _slideNumber = value; }
            }
        }
}
