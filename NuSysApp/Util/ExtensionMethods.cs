using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Util
{
    public static class ExtensionMethods
    {
        public static string Limit(this string str, int characterCount)
        {
            if (str.Length <= characterCount) return str;
            else return str.Substring(0, characterCount).TrimEnd(' ');
        }

        /// <summary>
        /// Substring with elipses but OK if shorter, will take 3 characters off character count if necessary
        /// </summary>
        public static string LimitWithElipses(this string str, int characterCount)
        {
            if (characterCount < 5) return str.Limit(characterCount); // Can’t do much with such a short limit
            if (str.Length <= characterCount - 3)
                return str;
            else
                return str.Substring(0, characterCount - 3) + "...";
        }

        public static string LimitWithElipsesOnWordBoundary(this string str, int characterCount)
        {
            if (characterCount < 5) return str.Limit(characterCount); // Can’t do much with such a short limit
            if (str.Length <= characterCount - 3)
                return str;
            else
            {
                int lastspace = str.Substring(0, characterCount - 3).LastIndexOf(' ');
                if (lastspace > 0 && lastspace > characterCount - 10)
                {
                    return str.Substring(0, lastspace) + "...";
                }
                else
                {
                    // No suitable space was found
                    return str.Substring(0, characterCount - 3) + "...";
                }
            }
        }
    }
}
