using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LdaLibrary
{
    public class Conversion
    {
        public static string ZeroPad(int number, int width)
        {
            // need the equivalent of stringbuffer (System.Text.StringBuilder)
            System.Text.StringBuilder result = new StringBuilder("");
            for (int i = 0; i < width - number.ToString().Length; i++)
            {
                result.Append("0");
            }
            result.Append(number.ToString());
            return result.ToString();
        }
    }
}
