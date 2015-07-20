using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChromeNusysIntermediate
{
    class Intermediate
    {

        static Stream stdin = Console.OpenStandardInput();

        static void Main(string[] args)
        {
            Client client = new Client();

            string input = OpenStandardStreamIn();

            //if the input is the empty string, then we are no longer connected to chrome and should shut down
            while (input != null && input != "")
            {
                    string[] line = {input};
                    string[] line2 = { "a line" };
                    System.IO.File.WriteAllLines(@"C:\Users\Ben\Documents\WriteLins2.txt", line2);
                    System.IO.File.WriteAllLines(@"C:\Users\Ben\Documents\WriteLins.txt", line);

                    input = "length=" + input.Length + "begin input:" + input;
                    client.Send(input);
                    input = OpenStandardStreamIn();
                
            }
        }

        private static string OpenStandardStreamIn()
        {


            //// We need to read first 4 bytes for length information

            int length = 0;
            byte[] bytes = new byte[4];

            stdin.Read(bytes, 0, 4);
            length = System.BitConverter.ToInt32(bytes, 0);

            var buffer = new byte[length];
            stdin.Read(buffer, 0, length);
            var str = System.Text.Encoding.Default.GetString(buffer);

            return str;
        }

    }
}
