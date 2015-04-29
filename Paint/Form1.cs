using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace Paint_app
{
    public partial class Form1 : Form
    {
        private List<Shape> Items = new List<Shape>(); //List, it stores object of type Shape
        private Point anchor = Point.Empty; //starting point
        private Point delta = Point.Empty;  //ending point
        private bool drawing;               //when the user leftclick on the canvas, then he will be able to paint , otherwise no
        private bool inform = false;
        private Dimensions dimensions = Dimensions.Empty; //object of dimensions, which store the initial point(x and y) and the size
        private drawMode mode = drawMode.Square;    //enum which basically tells us which item to draw on canvas, rectangle, text or pen
        private Pen dragRectPen = new Pen(Brushes.Purple, 1) { DashStyle = DashStyle.Solid},    drawnRectPen = new Pen(Brushes.Black, 2) { DashStyle = DashStyle.Solid },    crosshairPen = new Pen(Brushes.LightSteelBlue, 1) { DashStyle = DashStyle.Dot };
        private Brush dragRectBrush = new SolidBrush(Color.Purple);
        public Color Pen = Color.Orange;
        public Color Fill = Color.FromArgb(255, 192, 192, 255);
        public TextBox a;               //devlare a textbox a, it will be created dynamically when user wanst to insert text in the paint application
        public String tbtext;           //the text from textbox a will be inserted into a string, so that we can use it in g.drawstring method
        bool tc=true;                  //boolean will make sure that no more than 1 textbox is created for inserting text
        public Color brush_color;
        public Color fnt_color;
        public Font font;
        bool allowResize = false;

        private Point OldX = Point.Empty;           //these points are used for PEN
        private Point NewX = Point.Empty;           //these points are used for PEN

        private List<Point[]> listp = new List<Point[]>();

        String imagePath = String.Empty;

        


      

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |    ControlStyles.OptimizedDoubleBuffer |    ControlStyles.ResizeRedraw |    ControlStyles.UserPaint, true);
            toolStrip_font.Visible = false;
            toolStrip_Brush.Visible = false;
            toolStrip_Draw.Visible = true;

            

            for(int k = 8; k < 37; k++)
            {
                combobox_fontsize.Items.Add(k);
                k++;
            }
            combobox_fontsize.SelectedIndex = 6;

            InstalledFontCollection f = new InstalledFontCollection();
            FontFamily[] ff = FontFamily.Families;

            for (int x = 0; x < f.Families.Length; x++)
            {

             
                Combobox_fntname.Items.Add(ff[x].Name);


            }
            Combobox_fntname.SelectedIndex = 0;

            combobox_fontstyle.Items.Add(FontStyle.Bold.ToString());
            combobox_fontstyle.Items.Add(FontStyle.Italic.ToString());
            combobox_fontstyle.Items.Add(FontStyle.Regular.ToString());
            combobox_fontstyle.Items.Add(FontStyle.Strikeout.ToString());
            combobox_fontstyle.Items.Add(FontStyle.Underline.ToString());
            combobox_fontstyle.SelectedIndex = 0;

            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel4.ForeColor = statusStrip1.BackColor;


            fnt_color = txt_fntcolor.BackColor;
            txt_pencolor.BackColor = Color.Plum;
            Pen = Color.Plum;
            combobox_brushwidth.SelectedIndex = 3;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            brush_color = txt_brushcolor.BackColor;
            toolStrip_font.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_Brush.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_Draw.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_PaintTool.BackColor = Color.FromArgb(50, 247, 247, 247); 

        }




        int s = 0;


        List<Shape> UNDO = new List<Shape>();

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int n = Items.Count - 1;
                UNDO.Add(Items[n]);
                s = s + 1;
                Items.RemoveAt(n);
                panel2.Invalidate();
            }
            catch (Exception) { }
        }

        int o = 0;

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                o = UNDO.Count - 1;
                Items.Add(UNDO[o]);
                UNDO.RemoveAt(o);
                o--;
                panel2.Invalidate();
            }
            catch (Exception) { }
        }




        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            Invalidate();
            
        }

        private void keypressed(Object o, KeyPressEventArgs e) //this is handling event,, it is used to override the return key press when inserting text in paint application
        {
            
            if (e.KeyChar == (char)Keys.Return)
            {

                      tbtext = a.Text + "\r\n";

                
                
                e.Handled = true;
                a.Clear();
                a.Dispose();

                Items.Add(new Shape(tbtext, a.Font, new SolidBrush(txt_fntcolor.BackColor), a.Location, mode));
                tc=true;
            }

            tc = false;
        }


        private void toolStrip1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Rectangle rect = new Rectangle(0, 0, toolStrip_PaintTool.Width, toolStrip_PaintTool.Height);
            System.Drawing.Drawing2D.LinearGradientBrush b = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.AliceBlue, Color.WhiteSmoke, LinearGradientMode.Vertical);
            g.FillRectangle(b, rect);
        }


        private void toolStripButton1_Click(object sender, EventArgs e) //drawing rectangle
        {
            selectedButton(toolStripButton1);

        }

        private void toolStripButton2_Click(object sender, EventArgs e) //draw circle
        {
            selectedButton(toolStripButton2);
        }



        private void toolStripButton3_Click(object sender, EventArgs e) //brush tool
        {
            selectedButton(toolStripButton3);
            
        }

        private void toolStripButton6_Click(object sender, EventArgs e) //insert text in canvas
        {

            selectedButton(toolStripButton6);
        }

        private void selectedButton(object sender) //this code handels the draw MODE, which are named shapes in PAINT
        {
            toolStripButton1.Checked = toolStripButton2.Checked = toolStripButton3.Checked = toolStripButton5.Checked = toolStripButton6.Checked = toolStripButton5.Checked = toolStripButton7.Checked = toolStripButton8.Checked = toolStripButton9.Checked = toolStripButton4.Checked = toolStripButton10.Checked=false;
            toolStrip_font.Visible = toolStrip_Brush.Visible = toolStrip_Draw.Visible = false;


            if (sender == toolStripButton1)
            {
                toolStripButton1.Checked = true;
                mode = drawMode.Square;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton2)
            {
                toolStripButton2.Checked = true;
                mode = drawMode.Circle;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton3)
            {
                toolStripButton3.Checked = true;
                mode = drawMode.Brush;
                toolStrip_Brush.Visible = true;
            }
            else if (sender == toolStripButton6)
            {                  
                toolStripButton6.Checked = true;
                mode = drawMode.Text;
                toolStrip_font.Visible = true;
            }
            else if (sender == toolStripButton1)
            {
                toolStripButton1.Checked = true;
                mode = drawMode.Square;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton9)
            {
                toolStripButton9.Checked = true;
                mode = drawMode.Line;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton7)
            {
                toolStripButton7.Checked = true;
                mode = drawMode.fillSquare;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton8)
            {
                toolStripButton8.Checked = true;
                mode = drawMode.fillCircle;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton5)
            {
                toolStripButton5.Checked = true;
                mode = drawMode.Pen;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton4)
            {
                toolStripButton4.Checked = true;
                mode = drawMode.Erase;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton10)
            {
                toolStripButton10.Checked = true;
                mode = drawMode.Image;
                toolStrip_Draw.Visible = true;
            }




        }



        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            FontDialog fnt_dlg = new FontDialog();

            if (fnt_dlg.ShowDialog() == DialogResult.OK)
            {
                font = fnt_dlg.Font;
            }
        }

        private void toolStripTextBox2_Click(object sender, EventArgs e) //changing color of font
        {
            ColorDialog clr_dlg = new ColorDialog();
            clr_dlg.FullOpen = true;

            if (clr_dlg.ShowDialog() == DialogResult.OK)
            {
               a.Select();
               a.ForeColor = clr_dlg.Color;
               txt_fntcolor.BackColor = clr_dlg.Color;
            }
        }



        private void toolStripTextBox1_Click_1(object sender, EventArgs e) //changing color of brush
        {
            ColorDialog clr_dlg = new ColorDialog();
            clr_dlg.FullOpen = true;

            if (clr_dlg.ShowDialog() == DialogResult.OK)
            {
                txt_brushcolor.BackColor = clr_dlg.Color;
                brush_color = clr_dlg.Color;
            }
        } //toolstrip changing color




        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e) //when you change index of font combo box, it changes font of text
        {
            try
            {
                a.Select();
                a.Font = new Font(Combobox_fntname.SelectedItem.ToString(), a.Font.Size, a.Font.Style);
            }
            catch (Exception) { }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) //for newcanvas, we remove each shape from the list
        {
            List<Shape> valuesToDelete = new List<Shape>();

            open_bm = null;
            foreach (Shape s in Items)
            {

                valuesToDelete.Add(s);

            }

            foreach (Shape s in valuesToDelete)
            {
                Items.Remove(s);
            }

            panel2.Invalidate();
        }





        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            allowResize = false;
            pictureBox1.Location = new Point(panel2.Location.X + panel2.Width, panel2.Location.Y + panel2.Height);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (allowResize)
            {
                this.panel2.Height = pictureBox1.Top + e.Y-panel2.Top;
                this.panel2.Width = pictureBox1.Left + e.X-panel2.Left;
                pictureBox1.Location = new Point(panel2.Location.X + panel2.Width, panel2.Location.Y + panel2.Height);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            allowResize = true;
        }



        private void panel2_Paint(object sender, PaintEventArgs e) //paint event for panel, all drawing is done in paint event
        {
            base.OnPaint(e);
            // draw crosshairs

            if (inform)
            {
                
                e.Graphics.DrawLine(crosshairPen, delta.X, 0, delta.X, Height);
                e.Graphics.DrawLine(crosshairPen, 0, delta.Y, Width, delta.Y);
            }


            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            switch (mode) //for drawing rectangle as user drags the mouse
            {
                    

                case drawMode.Square:
                    e.Graphics.DrawRectangle(dragRectPen, new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case drawMode.fillSquare:
                    e.Graphics.FillRectangle(new SolidBrush(Fill), new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case drawMode.Circle:
                    e.Graphics.DrawEllipse(dragRectPen, new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case drawMode.fillCircle:
                    e.Graphics.FillEllipse(new SolidBrush(Fill), new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case drawMode.Line:
                    e.Graphics.DrawLine(drawnRectPen, dimensions.Anchor, dimensions.end);
                    break;



            }

            


            foreach (Shape s in Items) //drawing our shapes in the canvas
            {


                    switch (s.draw)
                    {

                        case drawMode.Image:
                            e.Graphics.DrawImage(s.img,new Point(0,0));
                            break;

                        case drawMode.Square:
                            e.Graphics.DrawRectangle(s.pen, s.rect);
                            break;

                        case drawMode.fillSquare:
                            e.Graphics.FillRectangle(s.brush, s.rect);
                            break;

                        case drawMode.Circle:
                            e.Graphics.DrawEllipse(s.pen, s.rect);
                            break;

                        case drawMode.fillCircle:
                            e.Graphics.FillEllipse(s.brush, s.rect);
                            break;

                        case drawMode.Brush:
                            e.Graphics.FillEllipse(s.brush, s.rect);
                            break;

                        case drawMode.Pen:
                            e.Graphics.DrawLine(s.pen, s.p1.X, s.p1.Y, s.p2.X, s.p2.Y);
                            break;


                        case drawMode.Text:
                            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                            e.Graphics.DrawString(s.text, s.font, s.brush, s.point);
                            if(tc==false)
                                tc = true;
                            break;

                        case drawMode.Line:
                            e.Graphics.DrawLine(s.pen, s.p1, s.p2);
                            break;

                        case drawMode.Erase:
                            e.Graphics.FillEllipse(s.brush, s.rect);
                            break;

  



                    }

            }
            
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            /*
             if (e.Button == MouseButtons.Left)
            {
                start = e.Location;
                panel1.MouseUp += new MouseEventHandler(panel1_MouseUp);
                panel1.MouseMove += new MouseEventHandler(panel1_MouseMove);
                pictureBox1.Location = new Point(panel1.Location.X+panel1.Width, panel1.Location.Y+panel1.Height);
            }
             */

            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                drawing = true;
                anchor.X = e.X;
                anchor.Y = e.Y;

                OldX.X = e.X;
                OldX.Y = e.Y;

                NewX = OldX;


                if (drawing)
                {


                    if (mode == drawMode.Brush)
                    {
                        int r = (int)Convert.ToInt32(combobox_brushwidth.SelectedItem.ToString());
                        Size a = new Size(r, r);
                        Items.Add(new Shape(delta, new SolidBrush(txt_brushcolor.BackColor), a, mode));
                    }

                    if (mode == drawMode.Pen)
                    {
                        Items.Add(new Shape(OldX, NewX, new Pen(txt_pencolor.BackColor, 1), mode));

                    }

                    if (mode == drawMode.Erase)
                    {
                        Size a = new Size(25, 25);
                        Items.Add(new Shape(delta, new SolidBrush(panel2.BackColor), a, mode));
                    }

                    if (mode == drawMode.Text)
                    {
                        if (tc)
                        {

                            a = new TextBox();
                            panel2.Controls.Add(a);

                            a.Multiline = true;
                            a.WordWrap = true;
                            a.BackColor = panel2.BackColor;
                            a.Font = new Font(Combobox_fntname.SelectedItem.ToString(), Convert.ToInt32(combobox_fontsize.SelectedItem.ToString()), a.Font.Style);
                            a.BorderStyle = BorderStyle.None;
                            fontStyle();
                            a.ForeColor = txt_fntcolor.BackColor;
                            a.Multiline = true;
                            a.WordWrap = true;

                            a.Height = 50;
                            a.Width = 400;
                            a.Location = delta;

                            a.Text = "";
                            a.Focus();

                            a.KeyPress += new KeyPressEventHandler(keypressed);
                            tc = false;
                        }
                    }
                }
            }
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = String.Empty;
            inform = false;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {

            



            // update our delta location
            delta.X = e.X;
            delta.Y = e.Y;

            NewX.X = e.X;
            NewX.Y = e.Y;
            toolStripStatusLabel1.Text = Math.Abs(delta.X).ToString() + "," + "" + Math.Abs(delta.Y).ToString() + " px";

            if (drawing)
            {
                
                Point Sz = new Point(delta.X - anchor.X, delta.Y - anchor.Y);
                toolStripStatusLabel2.Text = Math.Abs(Sz.X).ToString() + "," + "" + Math.Abs(Sz.Y).ToString() + " px";

                if (mode == drawMode.Brush)
                {
                    int r = (int)Convert.ToInt32(combobox_brushwidth.SelectedItem.ToString());
                    Size a = new Size(r, r);
                    Items.Add(new Shape(delta, new SolidBrush(txt_brushcolor.BackColor), a, mode));
                }
                if (mode == drawMode.Pen)
                {

                    Items.Add(new Shape(OldX, NewX, new Pen(txt_pencolor.BackColor, 1), mode));

                    OldX = NewX;


                   
                }
                if (mode == drawMode.Erase)
                {
                    Size a = new Size(25, 25);
                    Items.Add(new Shape(delta, new SolidBrush(panel2.BackColor), a, mode));
                }
                
                dimensions.Update(anchor, delta);
                
                
                if (mode == drawMode.Line)
                {
                   dimensions.UpdateLine(anchor, delta); 
                }

                



                
                


            }
            // redraw the form 
            panel2.Invalidate();
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            /*
            panel1.MouseMove -= new MouseEventHandler(panel1_MouseMove);
            panel1.MouseUp -= new MouseEventHandler(panel1_MouseUp);
            pictureBox1.Location = new Point(panel1.Location.X + panel1.Width, panel1.Location.Y + panel1.Height);
          */


            switch (mode)
            {
                case drawMode.Square:
                    Items.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new Pen(Pen, 2), mode));
                    break;

                case drawMode.Circle:
                    Items.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size),new Pen(Pen,2), mode));
                    break;

                case drawMode.fillSquare:
                    Items.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new SolidBrush(Fill), mode));
                    break;

                case drawMode.fillCircle:
                    Items.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new SolidBrush(Fill), mode));
                    break;

                case drawMode.Line:
                    Items.Add(new Shape(dimensions.Anchor,dimensions.end , new Pen(Pen, 2), mode));
                    break;

     
            }


            panel2.Invalidate();

            anchor = Point.Empty;
            dimensions.Update(anchor, Point.Empty);
            dimensions.UpdateLine(anchor, Point.Empty);
            drawing = false; //upon mouse release, user cannot draw unless he clicks on canvas again
        }


        private void toolStripTextBox3_Click_1(object sender, EventArgs e) //changing color of pen
        {
            ColorDialog clr_dlg = new ColorDialog();
            clr_dlg.FullOpen = true;
            if (clr_dlg.ShowDialog() == DialogResult.OK)
            {
                txt_pencolor.BackColor = clr_dlg.Color;
                Pen = clr_dlg.Color;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) //coding for saving the canvas size as bmp
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.DefaultExt = "bmp";
            saveFileDialog.Filter = "Bitmap files|*.bmp";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                int width = panel2.Width;
                int height = panel2.Height;

                Bitmap bitMap = new Bitmap(width, height);
                Rectangle rec = new Rectangle(0, 0, width, height);

                panel2.DrawToBitmap(bitMap, rec);

                

                bitMap.Save(saveFileDialog.FileName);
            }

        }

   

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e) //code for changing font size
        {
            
            try
            {
                a.Select();
                a.Font = new Font(a.Font.Name, Convert.ToInt32(combobox_fontsize.SelectedItem.ToString()), a.Font.Style);
            }
            catch (Exception) { }
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e) //code for changing font Style
        {
            try
            {
                fontStyle();
            }
            catch(Exception){}
        }

        private void fontStyle() //code for font Style
        {
            switch (combobox_fontstyle.SelectedItem.ToString())
            {
                case "Bold":
                    a.Font = new Font(a.Font.Name, a.Font.Size, FontStyle.Bold);
                    break;

                case "Italic":
                    a.Font = new Font(a.Font.Name, a.Font.Size, FontStyle.Italic);
                    break;

                case "Regular":
                    a.Font = new Font(a.Font.Name, a.Font.Size, FontStyle.Regular);
                    break;

                case "Strikeout":
                    a.Font = new Font(a.Font.Name, a.Font.Size, FontStyle.Strikeout);
                    break;

                case "Underline":
                    a.Font = new Font(a.Font.Name, a.Font.Size, FontStyle.Underline);
                    break;



            }
        }

        private void panel2_MouseEnter(object sender, EventArgs e)
        {
            inform = true;
        }

        private void toolStripButton4_Click(object sender, EventArgs e) //erase
        {
            selectedButton(toolStripButton4);
        }

        private void toolStripButton7_Click(object sender, EventArgs e) //fill rectangle
        {

            selectedButton(toolStripButton7);
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e) //code for changing fill color, for fill rectangle etc
        {
            ColorDialog clr_dlg = new ColorDialog();
            clr_dlg.FullOpen = true;
            if (clr_dlg.ShowDialog() == DialogResult.OK)
            {
                txt_fillcolor.BackColor = clr_dlg.Color;
                Fill = clr_dlg.Color;
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e) //Fill circle
        {

            selectedButton(toolStripButton8);
        }

        private void toolStripButton9_Click(object sender, EventArgs e) //draw line
        {

            selectedButton(toolStripButton9);
            
        }

        private void toolStripButton5_Click(object sender, EventArgs e) //pen tool
        {

            selectedButton(toolStripButton5);
        }

        Image open_bm;

        private void openToolStripMenuItem_Click(object sender, EventArgs e) //code for opening BMP file
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "bmp";
            ofd.Filter = "Bitmap files|*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath = ofd.FileName;          
                    open_bm = new Bitmap(imagePath);

                    mode = drawMode.Image;
                    Items.Add(new Shape(open_bm,mode));

                    this.panel2.Size = open_bm.Size;
                
                    
                    
                }

                mode = drawMode.Square;

                panel2.Invalidate();

        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(null, null);
            selectedButton(toolStripButton10);
        } //code for opening BMP











    }


}
