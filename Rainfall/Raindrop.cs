using System;
using System.Collections.Generic;
using System.Text;

namespace Rainfall
{
    class Raindrop
    {
        public string Text { get; set; }

        public LargeText LargeText { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ConsoleColor Color { get; set; }
        public RaindropState State { get; set; }

        public Raindrop PressedPart { get; set; }

        public Raindrop (string text, LargeText largeText, int x, int y, ConsoleColor color)
        {
            Text = text;
            LargeText = largeText;
            X = x;
            Y = y;
            Color = color;
            State = RaindropState.Nonexisting;
        }
    }
}
