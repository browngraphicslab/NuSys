using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{

    public class RandomColorGenerator
    {
        private Random _random;
        // seed the generator with 2 because
        // this gives a good sequence of colors
        const int RandomSeed = 2;
        public RandomColorGenerator()
        {
            _random = new Random(RandomSeed);
        }

        /// <summary>
        /// Returns a random pastel brush
        /// </summary>
        /// <returns></returns>
        public SolidColorBrush GetNextBrush()
        {
            SolidColorBrush brush = new SolidColorBrush(GetNext());

            return brush;
        }

        /// <summary>
        /// Returns a random pastel color
        /// </summary>
        /// <returns></returns>
        public Color GetNext()
        {
            // to create lighter colours:
            // take a random integer between 0 & 128 (rather than between 0 and 255)
            // and then add 127 to make the colour lighter
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)(_random.Next(128) + 127);
            colorBytes[1] = (byte)(_random.Next(128) + 127);
            colorBytes[2] = (byte)(_random.Next(128) + 127);

            Color color = new Color();

            // make the color fully opaque
            color.A = 255;
            color.R = colorBytes[0];
            color.B = colorBytes[1];
            color.G = colorBytes[2];

            return color;
        }
    }
}