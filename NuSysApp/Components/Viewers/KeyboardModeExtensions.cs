using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NuSysApp.Keyboard;

namespace NuSysApp
{
    public static class KeyboardModeExtensions
    {

        public static bool IsUpperCase(this KeyboardMode mode)
        {
            if (mode == KeyboardMode.UpperCaseAlphabeticalTapped || mode == KeyboardMode.UpperCaseAlphabeticalHeld)
            {
                return true;
            }
            return false;
        }

        public static bool IsAlphabetical(this KeyboardMode mode)
        {
            if (mode == KeyboardMode.UpperCaseAlphabeticalTapped || mode == KeyboardMode.UpperCaseAlphabeticalHeld || mode == KeyboardMode.LowerCaseAlphabetical)
            {
                return true;
            }
            return false;
        }
    }
}