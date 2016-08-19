using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAUser
{
    public class DocumentReformatter
    {
        public void main()
        {
            //nothing
        }

        public async void reformat()
        {
            string test = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\..\\..\\Desktop\\testInferenceMod.txt";
            System.IO.StreamReader reader = System.IO.File.OpenText(test);
            string line;
            string split2 = "";

            var text = await reader.ReadToEndAsync();

            text = text.Replace("\n", " ");
            text = text.Replace("\r\n", " ");
            text = text.Replace("\t", " ");
            text = text.Replace("\r", " ");
            text = text.Replace("\n\r", " ");

            /*
            while ((line = reader.ReadLineAsync()) != null)
            {
                string[] split = line.Split(new String[] { "", " ", "\n", "\r\n", "\t" }, StringSplitOptions.RemoveEmptyEntries); // need to test this!!
                split2 = split2 + split;
            }
            */




            string test2 = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\..\\..\\Desktop\\testInferenceMod2.txt";
            using (System.IO.FileStream writer = System.IO.File.Create(test2)) // may need a 1024 (buffer size) after that 
            {
                byte[] statement = System.Text.Encoding.UTF8.GetBytes(text); // is this the proper way to index z?
                await writer.WriteAsync(statement, 0, statement.Length);
            }
        }
    }
}
