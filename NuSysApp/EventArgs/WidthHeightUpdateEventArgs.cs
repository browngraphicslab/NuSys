﻿using System;

namespace NuSysApp
{
    public class WidthHeightUpdateEventArgs: EventArgs
    {
        private readonly string _eventInfo;

        public WidthHeightUpdateEventArgs(string text, double width, double height)
        {
            _eventInfo = text;
            Width = width;
            Height = height;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public double Width { get; }

        public double Height { get; }
    }
}
