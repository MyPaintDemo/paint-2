using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Paint
{
    public class Dimensions
    {
        private Point anchor;
        public Point end;
        private Size size;
        public static Dimensions Empty = new Dimensions(Point.Empty, Size.Empty);

        public Point Anchor
        {
            get { return anchor; }
        }

        public Size Size
        {
            get { return size; }
        }

        public void Update(Point initial, Point delta)
        {
            if (initial.X > delta.X)
            {
                anchor.X = delta.X;
                size.Width = (initial.X - delta.X);
            }
            else
            {
                anchor.X = initial.X;
                size.Width = (delta.X - initial.X);
            }
            if (initial.Y > delta.Y)
            {
                anchor.Y = delta.Y;
                size.Height = (initial.Y - delta.Y);
            }
            else
            {
                anchor.Y = initial.Y;
                size.Height = (delta.Y - initial.Y);
            }
        }

        public void UpdateLine(Point initial, Point delta)
        {
            anchor = initial;
            end = delta;
        }


        public Dimensions(Point anchor, Size size)
        {
            this.anchor = anchor;
            this.size = size;
        }
    }
}


