using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LdaLibrary
{
    public class Constants
    {       // replaced final to readonly, if errors occur, try with sealed
        	public static readonly long BUFFER_SIZE_LONG = 1000000;
	        public static readonly short BUFFER_SIZE_SHORT = 512;
	
	        public static readonly int MODEL_STATUS_UNKNOWN = 0;
	        public static readonly int MODEL_STATUS_EST = 1;
	        public static readonly int MODEL_STATUS_ESTC = 2;
            public static readonly int MODEL_STATUS_INF = 3;
    }
}
