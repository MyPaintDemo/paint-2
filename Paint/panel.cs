using System;
using System.Windows.Forms;

public class panel : Panel
{
    public panel()
    {
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
    }
}