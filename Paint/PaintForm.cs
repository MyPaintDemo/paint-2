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

namespace Paint
{
    public partial class PaintForm : Form
    {
        /// <summary>
        /// Maintain a history of shapes added to canvas
        /// </summary>
        private List<Shape> History = new List<Shape>();

        /// <summary>
        /// Store list of shapes on canvas
        /// </summary>
        private List<Shape> shapeList = new List<Shape>();

        /// <summary>
        /// Starting Point
        /// </summary>
        private Point anchor = Point.Empty;

        /// <summary>
        /// Ending point
        /// </summary>
        private Point delta = Point.Empty; 

            //when the user leftclick on the canvas, then he will be able to paint , otherwise no

        /// <summary>
        /// Dimension of object, which stores the initial point(x and y) and the size
        /// </summary>
        private Dimensions dimensions = Dimensions.Empty;

        /// <summary>
        /// Item type to draw on canvas
        /// </summary>
        private DrawMode drawingMode = DrawMode.Square;  
 
        /// <summary>
        /// Initial settings for rectangle, pen
        /// </summary>
        private Pen dragRectPen = new Pen(Brushes.Purple, 1) { DashStyle = DashStyle.Solid }, drawnRectPen = new Pen(Brushes.Black, 2) { DashStyle = DashStyle.Solid }, crosshairPen = new Pen(Brushes.LightSteelBlue, 1) { DashStyle = DashStyle.Dot };  
        private Brush dragRectBrush = new SolidBrush(Color.Purple);
        private Color Pen = Color.Orange;
        private Color Fill = Color.FromArgb(255, 192, 192, 255);

        /// <summary>
        /// Declare a textbox a, it will be created dynamically when user wanst to insert text in the paint application
        /// </summary>
        private TextBox dummyTextBox;               //
        
        /// <summary>
        /// Textbox will be inserted into a string, so that we can use it in g.drawstring method
        /// </summary>
        private String tbtext;
                       
        /// <summary>
        /// No more than 1 textbox is created for inserting text
        /// </summary>
        private bool allowDummyTextBox = true;

        private Color brushColor;
        private Color fontColor;
        private Font font;
        private bool isDrawingMode;
        private bool cursorInCanvas;
        private bool allowResize;
        private Point oldPointerX = Point.Empty;           //these points are used for PEN
        private Point newPointerX = Point.Empty;           //these points are used for PEN

        private String imagePath = String.Empty;

        public PaintForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            toolStrip_font.Visible = false;
            toolStrip_Brush.Visible = false;
            toolStrip_Draw.Visible = true;

            for (int k = 8; k < 37; k++)
            {
                FontSizeComboBox.Items.Add(k);
                k++;
            }
            FontSizeComboBox.SelectedIndex = 6;

            InstalledFontCollection f = new InstalledFontCollection();
            FontFamily[] ff = FontFamily.Families;

            for (int x = 0; x < f.Families.Length; x++)
            {
                FontComboBox.Items.Add(ff[x].Name);
            }

            FontComboBox.SelectedIndex = 0;

            FontStyleComboBox.Items.Add(FontStyle.Bold.ToString());
            FontStyleComboBox.Items.Add(FontStyle.Italic.ToString());
            FontStyleComboBox.Items.Add(FontStyle.Regular.ToString());
            FontStyleComboBox.Items.Add(FontStyle.Strikeout.ToString());
            FontStyleComboBox.Items.Add(FontStyle.Underline.ToString());
            FontStyleComboBox.SelectedIndex = 0;

            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel4.ForeColor = statusStrip1.BackColor;

            fontColor = FontColorBox.BackColor;
            PenColorBox.BackColor = Color.Plum;
            Pen = Color.Plum;
            BrushWidthComboBox.SelectedIndex = 3;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            brushColor = BrushColorBox.BackColor;
            toolStrip_font.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_Brush.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_Draw.BackColor = Color.FromArgb(50, 247, 247, 247);
            toolStrip_PaintTool.BackColor = Color.FromArgb(50, 247, 247, 247);

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int n = shapeList.Count - 1;
                History.Add(shapeList[n]);
                shapeList.RemoveAt(n);
                PaintCanvas.Invalidate();
            }
            catch (Exception) { }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int o = History.Count - 1;
                shapeList.Add(History[o]);
                History.RemoveAt(o);
                o--;
                PaintCanvas.Invalidate();
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
                tbtext = dummyTextBox.Text + "\r\n";

                e.Handled = true;
                dummyTextBox.Clear();
                dummyTextBox.Dispose();

                shapeList.Add(new Shape(tbtext, dummyTextBox.Font, new SolidBrush(FontColorBox.BackColor), dummyTextBox.Location, drawingMode));
                allowDummyTextBox = true;
            }

            allowDummyTextBox = false;
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
            toolStripButton1.Checked = toolStripButton2.Checked = toolStripButton3.Checked = toolStripButton5.Checked = toolStripButton6.Checked = toolStripButton5.Checked = toolStripButton7.Checked = toolStripButton8.Checked = toolStripButton9.Checked = toolStripButton4.Checked = toolStripButton10.Checked = false;
            toolStrip_font.Visible = toolStrip_Brush.Visible = toolStrip_Draw.Visible = false;


            if (sender == toolStripButton1)
            {
                toolStripButton1.Checked = true;
                drawingMode = DrawMode.Square;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton2)
            {
                toolStripButton2.Checked = true;
                drawingMode = DrawMode.Circle;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton3)
            {
                toolStripButton3.Checked = true;
                drawingMode = DrawMode.Brush;
                toolStrip_Brush.Visible = true;
            }
            else if (sender == toolStripButton6)
            {
                toolStripButton6.Checked = true;
                drawingMode = DrawMode.Text;
                toolStrip_font.Visible = true;
            }
            else if (sender == toolStripButton1)
            {
                toolStripButton1.Checked = true;
                drawingMode = DrawMode.Square;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton9)
            {
                toolStripButton9.Checked = true;
                drawingMode = DrawMode.Line;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton7)
            {
                toolStripButton7.Checked = true;
                drawingMode = DrawMode.FillSquare;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton8)
            {
                toolStripButton8.Checked = true;
                drawingMode = DrawMode.FillCircle;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton5)
            {
                toolStripButton5.Checked = true;
                drawingMode = DrawMode.Pen;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton4)
            {
                toolStripButton4.Checked = true;
                drawingMode = DrawMode.Erase;
                toolStrip_Draw.Visible = true;
            }
            else if (sender == toolStripButton10)
            {
                toolStripButton10.Checked = true;
                drawingMode = DrawMode.Image;
                toolStrip_Draw.Visible = true;
            }
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                font = fontDialog.Font;
            }
        }

        private void toolStripTextBox2_Click(object sender, EventArgs e) //changing color of font
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                dummyTextBox.Select();
                dummyTextBox.ForeColor = colorDialog.Color;
                FontColorBox.BackColor = colorDialog.Color;
            }
        }

        private void toolStripTextBox1_Click_1(object sender, EventArgs e) //changing color of brush
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                BrushColorBox.BackColor = colorDialog.Color;
                brushColor = colorDialog.Color;
            }
        } //toolstrip changing color

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e) //when you change index of font combo box, it changes font of text
        {
            try
            {
                dummyTextBox.Select();
                dummyTextBox.Font = new Font(FontComboBox.SelectedItem.ToString(), dummyTextBox.Font.Size, dummyTextBox.Font.Style);
            }
            catch (Exception) { }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) //for newcanvas, we remove each shape from the list
        {
            List<Shape> valuesToDelete = new List<Shape>();

            foreach (Shape s in shapeList)
                valuesToDelete.Add(s);

            foreach (Shape s in valuesToDelete)
                shapeList.Remove(s);

            PaintCanvas.Invalidate();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            allowResize = false;
            pictureBox1.Location = new Point(PaintCanvas.Location.X + PaintCanvas.Width, PaintCanvas.Location.Y + PaintCanvas.Height);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (allowResize)
            {
                this.PaintCanvas.Height = pictureBox1.Top + e.Y - PaintCanvas.Top;
                this.PaintCanvas.Width = pictureBox1.Left + e.X - PaintCanvas.Left;
                pictureBox1.Location = new Point(PaintCanvas.Location.X + PaintCanvas.Width, PaintCanvas.Location.Y + PaintCanvas.Height);
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
            if (cursorInCanvas)
            {
                e.Graphics.DrawLine(crosshairPen, delta.X, 0, delta.X, Height);
                e.Graphics.DrawLine(crosshairPen, 0, delta.Y, Width, delta.Y);
            }

            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            switch (drawingMode) //for drawing rectangle as user drags the mouse
            {
                case DrawMode.Square:
                    e.Graphics.DrawRectangle(dragRectPen, new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case DrawMode.FillSquare:
                    e.Graphics.FillRectangle(new SolidBrush(Fill), new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case DrawMode.Circle:
                    e.Graphics.DrawEllipse(dragRectPen, new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case DrawMode.FillCircle:
                    e.Graphics.FillEllipse(new SolidBrush(Fill), new Rectangle(dimensions.Anchor, dimensions.Size));
                    break;

                case DrawMode.Line:
                    e.Graphics.DrawLine(drawnRectPen, dimensions.Anchor, dimensions.end);
                    break;
            }

            foreach (Shape s in shapeList) //drawing our shapes in the canvas
            {
                switch (s.draw)
                {
                    case DrawMode.Image:
                        e.Graphics.DrawImage(s.img, new Point(0, 0));
                        break;

                    case DrawMode.Square:
                        e.Graphics.DrawRectangle(s.pen, s.rect);
                        break;

                    case DrawMode.FillSquare:
                        e.Graphics.FillRectangle(s.brush, s.rect);
                        break;

                    case DrawMode.Circle:
                        e.Graphics.DrawEllipse(s.pen, s.rect);
                        break;

                    case DrawMode.FillCircle:
                        e.Graphics.FillEllipse(s.brush, s.rect);
                        break;

                    case DrawMode.Brush:
                        e.Graphics.FillEllipse(s.brush, s.rect);
                        break;

                    case DrawMode.Pen:
                        e.Graphics.DrawLine(s.pen, s.p1.X, s.p1.Y, s.p2.X, s.p2.Y);
                        break;

                    case DrawMode.Text:
                        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        e.Graphics.DrawString(s.text, s.font, s.brush, s.point);
                        if (allowDummyTextBox == false)
                            allowDummyTextBox = true;
                        break;

                    case DrawMode.Line:
                        e.Graphics.DrawLine(s.pen, s.p1, s.p2);
                        break;

                    case DrawMode.Erase:
                        e.Graphics.FillEllipse(s.brush, s.rect);
                        break;
                }
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                isDrawingMode = true;
                anchor.X = e.X;
                anchor.Y = e.Y;

                oldPointerX.X = e.X;
                oldPointerX.Y = e.Y;

                newPointerX = oldPointerX;

                if (isDrawingMode)
                {
                    if (drawingMode == DrawMode.Brush)
                    {
                        int r = (int)Convert.ToInt32(BrushWidthComboBox.SelectedItem.ToString());
                        Size a = new Size(r, r);
                        shapeList.Add(new Shape(delta, new SolidBrush(BrushColorBox.BackColor), a, drawingMode));
                    }

                    if (drawingMode == DrawMode.Pen)
                    {
                        shapeList.Add(new Shape(oldPointerX, newPointerX, new Pen(PenColorBox.BackColor, 1), drawingMode));

                    }

                    if (drawingMode == DrawMode.Erase)
                    {
                        Size a = new Size(25, 25);
                        shapeList.Add(new Shape(delta, new SolidBrush(PaintCanvas.BackColor), a, drawingMode));
                    }

                    if (drawingMode == DrawMode.Text)
                    {
                        if (allowDummyTextBox)
                        {
                            dummyTextBox = new TextBox();
                            PaintCanvas.Controls.Add(dummyTextBox);

                            dummyTextBox.Multiline = true;
                            dummyTextBox.WordWrap = true;
                            dummyTextBox.BackColor = PaintCanvas.BackColor;
                            dummyTextBox.Font = new Font(FontComboBox.SelectedItem.ToString(), Convert.ToInt32(FontSizeComboBox.SelectedItem.ToString()), dummyTextBox.Font.Style);
                            dummyTextBox.BorderStyle = BorderStyle.None;
                            fontStyle();
                            dummyTextBox.ForeColor = FontColorBox.BackColor;
                            dummyTextBox.Multiline = true;
                            dummyTextBox.WordWrap = true;

                            dummyTextBox.Height = 50;
                            dummyTextBox.Width = 400;
                            dummyTextBox.Location = delta;

                            dummyTextBox.Text = "";
                            dummyTextBox.Focus();

                            dummyTextBox.KeyPress += new KeyPressEventHandler(keypressed);
                            allowDummyTextBox = false;
                        }
                    }
                }
            }
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = String.Empty;
            cursorInCanvas = false;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            // update our delta location
            delta.X = e.X;
            delta.Y = e.Y;

            newPointerX.X = e.X;
            newPointerX.Y = e.Y;
            toolStripStatusLabel1.Text = Math.Abs(delta.X).ToString() + "," + "" + Math.Abs(delta.Y).ToString() + " px";

            if (isDrawingMode)
            {
                Point currentPt = new Point(delta.X - anchor.X, delta.Y - anchor.Y);
                toolStripStatusLabel2.Text = Math.Abs(currentPt.X).ToString() + "," + "" + Math.Abs(currentPt.Y).ToString() + " px";

                if (drawingMode == DrawMode.Brush)
                {
                    int r = (int)Convert.ToInt32(BrushWidthComboBox.SelectedItem.ToString());
                    Size a = new Size(r, r);
                    shapeList.Add(new Shape(delta, new SolidBrush(BrushColorBox.BackColor), a, drawingMode));
                }

                if (drawingMode == DrawMode.Pen)
                {
                    shapeList.Add(new Shape(oldPointerX, newPointerX, new Pen(PenColorBox.BackColor, 1), drawingMode));
                    oldPointerX = newPointerX;
                }

                if (drawingMode == DrawMode.Erase)
                {
                    Size a = new Size(25, 25);
                    shapeList.Add(new Shape(delta, new SolidBrush(PaintCanvas.BackColor), a, drawingMode));
                }

                if (drawingMode == DrawMode.Line)
                {
                    dimensions.UpdateLine(anchor, delta);
                }

                dimensions.Update(anchor, delta);
            }
            // redraw the form 
            PaintCanvas.Invalidate();
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            switch (drawingMode)
            {
                case DrawMode.Square:
                    shapeList.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new Pen(Pen, 2), drawingMode));
                    break;

                case DrawMode.Circle:
                    shapeList.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new Pen(Pen, 2), drawingMode));
                    break;

                case DrawMode.FillSquare:
                    shapeList.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new SolidBrush(Fill), drawingMode));
                    break;

                case DrawMode.FillCircle:
                    shapeList.Add(new Shape(new Rectangle(dimensions.Anchor, dimensions.Size), new SolidBrush(Fill), drawingMode));
                    break;

                case DrawMode.Line:
                    shapeList.Add(new Shape(dimensions.Anchor, dimensions.end, new Pen(Pen, 2), drawingMode));
                    break;
            }

            PaintCanvas.Invalidate();

            anchor = Point.Empty;
            dimensions.Update(anchor, Point.Empty);
            dimensions.UpdateLine(anchor, Point.Empty);
            isDrawingMode = false; //upon mouse release, user cannot draw unless he clicks on canvas again
        }


        private void toolStripTextBox3_Click_1(object sender, EventArgs e) //changing color of pen
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                PenColorBox.BackColor = colorDialog.Color;
                Pen = colorDialog.Color;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) //coding for saving the canvas size as bmp
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.DefaultExt = "bmp";
            saveFileDialog.Filter = "Bitmap files|*.bmp";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                int width = PaintCanvas.Width;
                int height = PaintCanvas.Height;

                Bitmap bitMap = new Bitmap(width, height);
                Rectangle rec = new Rectangle(0, 0, width, height);

                PaintCanvas.DrawToBitmap(bitMap, rec);
                bitMap.Save(saveFileDialog.FileName);
            }

        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e) //code for changing font size
        {
            try
            {
                dummyTextBox.Select();
                dummyTextBox.Font = new Font(dummyTextBox.Font.Name, Convert.ToInt32(FontSizeComboBox.SelectedItem.ToString()), dummyTextBox.Font.Style);
            }
            catch (Exception) { }
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e) //code for changing font Style
        {
            try
            {
                fontStyle();
            }
            catch (Exception) { }
        }

        private void fontStyle() //code for font Style
        {
            switch (FontStyleComboBox.SelectedItem.ToString())
            {
                case "Bold":
                    dummyTextBox.Font = new Font(dummyTextBox.Font.Name, dummyTextBox.Font.Size, FontStyle.Bold);
                    break;

                case "Italic":
                    dummyTextBox.Font = new Font(dummyTextBox.Font.Name, dummyTextBox.Font.Size, FontStyle.Italic);
                    break;

                case "Regular":
                    dummyTextBox.Font = new Font(dummyTextBox.Font.Name, dummyTextBox.Font.Size, FontStyle.Regular);
                    break;

                case "Strikeout":
                    dummyTextBox.Font = new Font(dummyTextBox.Font.Name, dummyTextBox.Font.Size, FontStyle.Strikeout);
                    break;

                case "Underline":
                    dummyTextBox.Font = new Font(dummyTextBox.Font.Name, dummyTextBox.Font.Size, FontStyle.Underline);
                    break;
            }
        }

        private void panel2_MouseEnter(object sender, EventArgs e)
        {
            cursorInCanvas = true;
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
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                FillColorBox.BackColor = colorDialog.Color;
                Fill = colorDialog.Color;
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e) //code for opening BMP file
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "bmp";
            openFileDialog.Filter = "Bitmap files|*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;
                Image imgBitmap = new Bitmap(imagePath);

                drawingMode = DrawMode.Image;
                shapeList.Add(new Shape(imgBitmap, drawingMode));
                this.PaintCanvas.Size = imgBitmap.Size;
            }

            drawingMode = DrawMode.Square;
            PaintCanvas.Invalidate();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(null, null);
            selectedButton(toolStripButton10);
        } //code for opening BMP

    }
}
