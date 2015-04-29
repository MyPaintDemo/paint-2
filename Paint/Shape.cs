using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Paint
{
    public class Shape
    {
        public DrawMode draw;
        public Rectangle rect;
        public String text;
        public Font font;
        public Brush brush;
        public Point point;
        public Pen pen;
        public Point p1;
        public Point p2;
        public Image img;

        public Shape()
        {
        }

        public Shape(Image a, DrawMode mode)
        {
            img = a;
            draw = mode;
        }

        public Shape(Point a, Point b, Pen p, DrawMode mode)
        {
            p1 = a;
            p2 = b;
            pen = p;
            draw = mode;
        }

        public Shape(Point x, Point y, Brush b,Size s,DrawMode mode)
        {
            rect.X = x.X;
            rect.Y = x.Y;
            rect.Width = s.Width;
            rect.Height = s.Height;
            draw = mode;
        }

        public Shape(Rectangle rectangle,Pen p, DrawMode mode)
        {
            rect = rectangle;
            pen = p;
            draw = mode;
        }

        public Shape(Rectangle rectangle, Brush b, DrawMode mode)
        {
            rect = rectangle;
            brush = b;
            draw = mode;
        }

        public Shape(String t,Font f,Brush b, Point p,  DrawMode mode)
        {
            text = t;
            draw = mode;
            font = f;
            brush = b;
            point = p;
        }

        public Shape(Point x, Brush b, Size s,DrawMode mode)
        {
            rect.X = x.X;
            rect.Y = x.Y;
            rect.Width = s.Width;
            rect.Height = s.Height;
            brush = b;
            draw = mode;
        }

        public Shape(Point x, Pen p, Size s, DrawMode mode)
        {
            rect.X = x.X;
            rect.Y = x.Y;
            rect.Width = s.Width;
            rect.Height = s.Height;
            pen = p;
            draw = mode;
        }
    }
}
