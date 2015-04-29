using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using System.Drawing.Imaging;

public class TextBoxEx : TextBox
{

    public TextBoxEx() : base()
    {

        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);

        this.BackColor = Color.Transparent;
    }
}
