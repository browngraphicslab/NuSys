using System.Collections.Generic;
using NuSysApp;

namespace NuSysApp
{
    public class Gesturizer
    {
        private readonly IList<IGesture> _gestures = new List<IGesture>();

        public void AddGesture(IGesture gesture)
        {
            _gestures.Add(gesture);
        }

        public IGesture Recognize(InqLineModel stroq)
        {

            foreach (IGesture gesture in _gestures)
            {
                if (gesture.Recognize(stroq))
                {
                    return gesture;
                }
            }
            return null;
        }
    }
}