using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LdaLibrary
{
    // This acts as our struct for our command line arguments
    public struct Option
    {
        private double alphaVal;
        private double betaVal;
        private string dfileVal;
        private string dirVal;
        private bool estVal;
        private bool estcVal;
        private bool infVal;
        private string modelVal;
        private int nitersVal;
        private int ntopicsVal;
        private int savestepVal;
        private int twordsVal;
        private bool withrawdataVal;
        private string wordmapVal;

        public double alpha
        {
            set
            {
                alphaVal = value;
            }
            get
            {
                return alphaVal;
            }
        }

        public double beta
        {
            set
            {
                betaVal = value;
            }
            get
            {
                return betaVal;
            }
        }

        public string dfile
        {
            set
            {
                dfileVal = value;
            }
            get
            {
                return dfileVal;
            }
        }

        public string dir
        {
            set
            {
                dirVal = value;
            }
            get
            {
                return dirVal;
            }
        }
        public bool est
        {
            set
            {
                estVal = value;
            }
            get
            {
                return estVal;
            }
        }

        public bool estc
        {
            set
            {
                estcVal = value;
            }
            get
            {
                return estcVal;
            }
        }

        public bool inf
        {
            set
            {
                infVal = value;
            }
            get
            {
                return infVal;
            }
        }

        public string model
        {
            set
            {
                modelVal = value;
            }
            get
            {
                return modelVal;
            }
        }

        public int niters
        {
            set
            {
                nitersVal = value;
            }
            get
            {
                return nitersVal;
            }
        }

        public int ntopics
        {
            set
            {
                ntopicsVal = value;
            }
            get
            {
                return ntopicsVal;
            }
        }



        public int savestep
        {
            set
            {
                savestepVal = value;
            }
            get
            {
                return savestepVal;
            }
        }

        public int twords
        {
            set
            {
                twordsVal = value;
            }
            get
            {
                return twordsVal;
            }
        }

        public bool withrawdata
        {
            set
            {
                withrawdataVal = value;
            }
            get
            {
                return withrawdataVal;
            }
        }

        public string wordmap
        {
            set
            {
                wordmapVal = value;
            }
            get
            {
                return wordmapVal;
            }
        }


    }

}
