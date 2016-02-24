using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LdaLibrary
{
    public class Document
    {
        //----------------------------------------------------
        //Instance Variables
        //----------------------------------------------------
        public int[] words;
        public string rawStr;
        public int length;

        //----------------------------------------------------
        //Constructors
        //----------------------------------------------------
        public Document()
        {
            words = null;
            rawStr = "";
            length = 0;
        }

        public Document(int length)
        {
            this.length = length;
            rawStr = "";
            words = new int[length];
        }

        public Document(int length, int[] words)
        {
            this.length = length;
            rawStr = "";

            this.words = new int[length];
            for (int i = 0; i < length; ++i)
            {
                this.words[i] = words[i];
            }
        }

        public Document(int length, int[] words, string rawStr)
        {
            this.length = length;
            this.rawStr = rawStr;

            this.words = new int[length];
            for (int i = 0; i < length; ++i)
            {
                this.words[i] = words[i];
            }
        }

        public Document(System.Collections.Generic.List<int> doc) // replace vector in java for arraylist
        {
            this.length = doc.Count; // switch size with count
            rawStr = "";
            this.words = new int[length];
            for (int i = 0; i < length; i++)
            {
                this.words[i] = doc[i]; //switched get with a cast
            }
        }

        public Document(System.Collections.Generic.List<int> doc, string rawStr)
        {
            this.length = doc.Count;
            this.rawStr = rawStr;
            this.words = new int[length];
            for (int i = 0; i < length; ++i)
            {
                this.words[i] = doc[i];
            }
        }
    }
}
