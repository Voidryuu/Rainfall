using System;
using System.Collections.Generic;
using System.Text;

namespace Rainfall
{
    class LargeText
    {
        public string[] Text { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public LargeText(string[] text, int height, int width)
        {
            Text = text;
            Height = height;
            Width = width;
        }
    }
}
