using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace CommonControls {

	/// <summary>
	/// The class ColorPalette implements a palette of colors.
	/// </summary>
	public class ColorPalette : System.Windows.Forms.UserControl {

		// designer fields
		private System.ComponentModel.Container components = null;

		// internal fields
		private Bitmap _ColorPalette;
		private Color _ForeColor, _BackColor; /* the current selected foreground and background color */
		private int _ColorsPerColumn; /* the number of colors in a column */
		private Size _ColorSize; /* the size of a color rectangle */

		/// <summary>
		/// Creates a ColorPalette instance.
		/// </summary>
		public ColorPalette () {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			SetStyle (ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
			_ColorsPerColumn = 14;
			_ColorSize = new Size (12, 12);
			_ColorPalette = CreateColorPalette ();
		}


		/// <summary>
		/// Gets or sets the current selected background color.
		/// </summary>
		public new Color BackColor {
			get { return _BackColor; }
			set { _BackColor = value; }
		}
		/// <summary>
		/// Gets or sets the number of colors displayed per row.
		/// </summary>
		public int ColorsPerColumn {
			get { return _ColorsPerColumn; }
			set { _ColorsPerColumn = value; _ColorPalette = CreateColorPalette (); Invalidate (); }
		}
		/// <summary>
		/// Gets or sets the size of each color.
		/// </summary>
		public Size ColorSize {
			get { return _ColorSize; }
			set { _ColorSize = value; _ColorPalette = CreateColorPalette (); }
		}
		/// <summary>
		/// Gets or sets the current selected foreground color.
		/// </summary>
		public new Color ForeColor {
			get { return _ForeColor; }
			set { _ForeColor = value; }
		}

		/// <summary>
		/// Creates a bitmap containing the color palette.
		/// </summary>
		private Bitmap CreateColorPalette () {
			// local variables
			Bitmap						bitmap;
			Rectangle					rect;
			int							bmp_width, bmp_height;
			PropertyInfo[]				properties;
			ArrayList					colors;
			Color						color;
			SolidBrush					brush;

			// get all colors defined as properties in the Color class
			properties = typeof (Color).GetProperties (BindingFlags.Public | BindingFlags.Static);
			colors = new ArrayList ();
			foreach (PropertyInfo prop in properties) {
				color = (Color) prop.GetValue (null, null);
				if (color == Color.Transparent) continue;
				if (color == Color.Empty) continue;
				// create brush of this color
				brush = new SolidBrush (color);
				colors.Add (brush);
			}
			// sort array
			colors.Sort (new _ColorSorter ());

			// calculate the width & height
			bmp_height = _ColorsPerColumn * _ColorSize.Height;
			bmp_width =  (colors.Count / _ColorsPerColumn) * _ColorSize.Width;
			// if count colors does not exactly match an whole number of columns, then add an extra column to
			// the bitmap
			if ((colors.Count % _ColorsPerColumn) != 0) {
				bmp_width += _ColorSize.Width;
			}
			// create a base bitmap where each color is a square of 6x6 pixels
			bitmap = new Bitmap (bmp_width, bmp_height);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				// initialize drawing
				rect = new Rectangle (0, 0, _ColorSize.Width, _ColorSize.Height);

				// iterate the colors and draw the color
				foreach (Brush b in colors) {
					g.FillRectangle (b, rect);
					// move down 
					rect.Offset (0, _ColorSize.Height);
					if (rect.Bottom > bmp_height) {
						// move right
						rect.Location = new Point (rect.Left + _ColorSize.Width, 0);
					}
				}
			}

			// adjust the controls size
			Size = bitmap.Size;

			return bitmap;
		}
		/// <summary>
		/// Converts a client coordinate to a color based on the location on the color palette.
		/// </summary>
		private Color LocationToColor (Point point) {
			return _ColorPalette.GetPixel (point.X, point.Y);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose (bool disposing) {
			if (disposing) {
				if (components != null) {
					components.Dispose ();
				}
			}
			base.Dispose (disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// ColorPalette
			// 
			this.Name = "ColorPalette";
			this.Size = new System.Drawing.Size(64, 256);
			this.Resize += new System.EventHandler(this.ColorPalette_Resize);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorPalette_MouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorPalette_Paint);

		}
		#endregion

		/// <summary>
		/// Event fired when the control needs to be repainted.
		/// </summary>
		private void ColorPalette_Paint (object sender, System.Windows.Forms.PaintEventArgs e) {
			e.Graphics.DrawImageUnscaled (_ColorPalette, 0, 0);
		}
		/// <summary>
		/// Event fired when the mouse button is released over the control.
		/// </summary>
		private void ColorPalette_MouseUp (object sender, System.Windows.Forms.MouseEventArgs e) {
			// local variables
			Point						point;

			point = new Point (e.X, e.Y);
			switch (e.Button) {
				case MouseButtons.Left : _ForeColor = LocationToColor (point); break;
				case MouseButtons.Right : _BackColor = LocationToColor (point); break;
			}
		}
		/// <summary>
		/// Event fired if the control is resized.
		/// </summary>
		private void ColorPalette_Resize (object sender, System.EventArgs e) {
			if (_ColorPalette != null) Size = _ColorPalette.Size;
		}

		
		/// <summary>
		/// The class _ColorSorter orders the colors based on the hue, saturation and brightness. This is the
		/// order that is also used by visual studio.
		/// </summary>
		internal class _ColorSorter: System.Collections.IComparer {

			#region IComparer Members

			public int Compare (object x, object y) {
				// local variables
				Color					cx, cy;
				float					hx, hy, sx, sy, bx, by;

				// get Color values
				cx = ((SolidBrush) x).Color;
				cy = ((SolidBrush) y).Color;
				// get saturation values
				sx = cx.GetSaturation ();
				sy = cy.GetSaturation ();
				// get hue values
				hx = cx.GetHue ();
				hy = cy.GetHue ();
				// get brightness values
				bx = cx.GetBrightness ();
				by = cy.GetBrightness ();

				// determine order
				// 1 : hue       
				if (hx < hy) return -1; 
				else if (hx > hy) return 1;
				else {
					// 2 : saturation
					if (sx < sy) return -1;
					else if (sx > sy) return 1;
					else {
						// 3 : brightness
						if (bx < by) return -1;
						else if (bx > by) return 1;
						else return 0;
					}
				}
			}

			#endregion

		}

	}
}
