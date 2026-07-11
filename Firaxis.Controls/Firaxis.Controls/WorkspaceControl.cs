using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Firaxis.Controls.Properties;
using Firaxis.MathEx;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class WorkspaceControl : UserControl
{
	private enum DragMode
	{
		Idle,
		Workspace,
		ZoomNub
	}

	private Vec2 origin = Vec2.Empty;

	private float zoom = 1f;

	private bool showGrid;

	private bool showZoomSlider;

	private const float maxZoom = 3f;

	private const float minZoom = 0.1f;

	private const int zoomNubMin = 42;

	private const int zoomNubMax = 184;

	private const int PadX = 5;

	private const int PadY = 5;

	private GraphicsState savedState;

	private DragMode dragMode = DragMode.Idle;

	private Point2 dragPoint = Point2.Empty;

	private Point2 mousePoint = Point2.Empty;

	private IContainer components = null;

	[Category("Appearance")]
	public Color GridColor { get; set; }

	[Category("Appearance")]
	public Size GridSpacing { get; set; }

	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowZoomSlider
	{
		get
		{
			return showZoomSlider;
		}
		set
		{
			showZoomSlider = value;
			Invalidate();
		}
	}

	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowGrid
	{
		get
		{
			return showGrid;
		}
		set
		{
			showGrid = value;
			Invalidate();
		}
	}

	[Category("Graph")]
	[DefaultValue(1f)]
	public float Zoom
	{
		get
		{
			return zoom;
		}
		set
		{
			zoom = Math.Min(Math.Max(0.1f, value), 3f);
			if (ScaledFont != null)
			{
				ScaledFont.Dispose();
			}
			ScaledFont = new Font(Font.Name, Font.Size * Zoom);
			this.ZoomChanged?.Invoke(this, EventArgs.Empty);
			if (!Updating)
			{
				Invalidate();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Updating { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Translating { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Font ScaledFont { get; private set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public Vec2 Origin
	{
		get
		{
			return origin;
		}
		set
		{
			origin = value;
			this.OriginChanged?.Invoke(this, EventArgs.Empty);
			if (!Updating)
			{
				Invalidate();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Vec2 Center => new Vec2((float)base.ClientRectangle.Width / 2f, (float)base.ClientRectangle.Height / 2f);

	private int NubY
	{
		get
		{
			float num = (Zoom - 0.1f) * 142f / 2.9f;
			return 184 - (int)num + 5;
		}
	}

	public event EventHandler InvokedBeginUpdate;

	public event EventHandler InvokedEndUpdate;

	public event EventHandler ZoomChanged;

	public event EventHandler OriginChanged;

	public WorkspaceControl()
	{
		InitializeComponent();
		base.MouseWheel += WorkspaceControl_MouseWheel;
		SetDefaults();
	}

	public void SetDefaults()
	{
		BeginUpdate();
		BackColor = Color.FromArgb(114, 123, 135);
		GridColor = Color.FromArgb(121, 130, 142);
		GridSpacing = new Size(16, 16);
		ShowGrid = true;
		ShowZoomSlider = true;
		Zoom = 1f;
		EndUpdate();
	}

	public void BeginUpdate()
	{
		if (!Updating)
		{
			Updating = true;
			this.InvokedBeginUpdate?.Invoke(this, EventArgs.Empty);
		}
	}

	public void EndUpdate()
	{
		if (Updating)
		{
			Updating = false;
			this.InvokedEndUpdate?.Invoke(this, EventArgs.Empty);
			Invalidate();
			Update();
		}
	}

	private void WorkspaceControl_MouseWheel(object sender, MouseEventArgs e)
	{
		float num = (float)e.Delta / 120f;
		Zoom += num * 0.125f;
	}

	public void ClientToWorld(ref Vec2 point)
	{
		ClientToWorld(ref point, Center);
	}

	public void ClientToWorld(ref RectangleF rect)
	{
		ClientToWorld(ref rect, Center);
	}

	private void ClientToWorld(ref Vec2 point, Vec2 center)
	{
		point = (point - center) / Zoom + Origin;
	}

	private void ClientToWorld(ref RectangleF rect, Vec2 center)
	{
		Vec2 vec = (new Vec2(rect.X, rect.Y) - center) / Zoom + Origin;
		Vec2 vec2 = (new Vec2(rect.Right, rect.Bottom) - center) / Zoom + Origin;
		rect = rect.SetEdges(vec.X, vec.Y, vec2.X, vec2.Y);
	}

	private void WorkspaceControl_Paint(object sender, PaintEventArgs e)
	{
		if (!Updating)
		{
			Graphics graphics = e.Graphics;
			Rectangle clientRectangle = base.ClientRectangle;
			PaintGrid(graphics, clientRectangle);
			PaintWorkspace(graphics, clientRectangle);
			if (ShowZoomSlider)
			{
				PaintZoomBar(graphics);
			}
		}
	}

	protected virtual void PaintWorkspace(Graphics g, Rectangle rect)
	{
	}

	public void BeginTranslate(Graphics g)
	{
		if (!Translating)
		{
			Translating = true;
			savedState = g.Save();
			Vec2 center = Center;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.PageUnit = GraphicsUnit.Pixel;
			g.TranslateTransform(0f - Origin.X + center.X / Zoom, 0f - Origin.Y + center.Y / Zoom);
			g.PageScale = Zoom;
		}
	}

	public void EndTranslate(Graphics g)
	{
		if (Translating)
		{
			Translating = false;
			g.Restore(savedState);
		}
	}

	private void PaintGrid(Graphics g, Rectangle r)
	{
		if (!ShowGrid)
		{
			return;
		}
		Vec2 vec = new Vec2(GridSpacing.Width, GridSpacing.Height);
		vec *= Zoom;
		using Pen pen = new Pen(GridColor);
		float num = 0f - Origin.X * Zoom % vec.X;
		float num2 = 0f - Origin.Y * Zoom % vec.Y;
		float num3 = 0f;
		while (num3 < (float)r.Width)
		{
			g.DrawLine(pen, num, 0f, num, r.Height);
			num3 += 1f;
			num += vec.X;
		}
		num3 = 0f;
		while (num3 < (float)r.Height)
		{
			g.DrawLine(pen, 0f, num2, r.Width, num2);
			num3 += 1f;
			num2 += vec.Y;
		}
	}

	private void PaintZoomBar(Graphics g)
	{
		int nubY = NubY;
		DrawingHelper.DrawImage(g, Firaxis.Controls.Properties.Resources.zoom_slider_shadow, 5f, 5f);
		DrawingHelper.DrawImage(g, Firaxis.Controls.Properties.Resources.zoom_slider, 5f, 5f);
		DrawingHelper.DrawImage(g, Firaxis.Controls.Properties.Resources.zoom_nub, 6f, nubY);
		if (dragMode == DragMode.ZoomNub)
		{
			string s = $"{Zoom:f2}x";
			g.DrawString(s, Font, Brushes.White, 25f, nubY);
		}
	}

	private bool HitZoomReset(int x, int y)
	{
		return Math.Abs(15 - y) < 10 && Math.Abs(x - 14) < 9;
	}

	private bool HitZoomNub(int x, int y)
	{
		return Math.Abs(NubY + 5 - y) < 5 && Math.Abs(x - 15) < 10;
	}

	private void WorkspaceControl_SizeChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void WorkspaceControl_MouseDown(object sender, MouseEventArgs e)
	{
		HandleMouseDown(sender, e);
	}

	private void WorkspaceControl_MouseMove(object sender, MouseEventArgs e)
	{
		HandleMouseMove(sender, e);
	}

	private void WorkspaceControl_MouseUp(object sender, MouseEventArgs e)
	{
		HandleMouseUp(sender, e);
	}

	protected virtual bool HandleMouseDown(object sender, MouseEventArgs e)
	{
		bool result = false;
		if (dragMode == DragMode.Idle)
		{
			if (e.Button == MouseButtons.Middle)
			{
				dragPoint.X = e.X;
				dragPoint.Y = e.Y;
				Cursor = CustomCursors.Get(CustomCursor.HandDrag);
				dragMode = DragMode.Workspace;
				base.Capture = true;
				result = true;
			}
			else if (ShowZoomSlider && e.Button == MouseButtons.Left)
			{
				if (HitZoomReset(e.X, e.Y))
				{
					Zoom = 1f;
					result = true;
				}
				else if (HitZoomNub(e.X, e.Y))
				{
					dragMode = DragMode.ZoomNub;
					base.Capture = true;
					result = true;
				}
			}
		}
		return result;
	}

	protected virtual bool HandleMouseMove(object sender, MouseEventArgs e)
	{
		bool flag = false;
		mousePoint.X = e.X;
		mousePoint.Y = e.Y;
		switch (dragMode)
		{
		case DragMode.ZoomNub:
		{
			int num = Math.Min(Math.Max(e.Y - 5, 47), 189) - 5;
			Zoom = (float)(184 - num) * 2.9f / 142f + 0.1f;
			break;
		}
		case DragMode.Workspace:
		{
			Vec2 vec = Vec2.Sum(dragPoint, -mousePoint);
			Origin += vec / Zoom;
			dragPoint = mousePoint;
			Invalidate();
			flag = true;
			break;
		}
		}
		if (!flag && !HandleSetCursor(sender, e))
		{
			Cursor = Cursors.Default;
		}
		return flag;
	}

	protected virtual bool HandleSetCursor(object sender, MouseEventArgs e)
	{
		if (ShowZoomSlider && (HitZoomNub(e.X, e.Y) || HitZoomReset(e.X, e.Y)))
		{
			Cursor = CustomCursors.Get(CustomCursor.FingerPoint);
			return true;
		}
		return false;
	}

	protected virtual bool HandleMouseUp(object sender, MouseEventArgs e)
	{
		bool result = false;
		if ((dragMode == DragMode.Workspace && e.Button == MouseButtons.Middle) || (dragMode == DragMode.ZoomNub && e.Button == MouseButtons.Left))
		{
			Cursor = Cursors.Default;
			dragMode = DragMode.Idle;
			base.Capture = false;
			result = true;
			Invalidate();
		}
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.DoubleBuffered = true;
		base.Name = "WorkspaceControl";
		base.Size = new System.Drawing.Size(256, 223);
		base.Paint += new System.Windows.Forms.PaintEventHandler(WorkspaceControl_Paint);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(WorkspaceControl_MouseMove);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(WorkspaceControl_MouseDown);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(WorkspaceControl_MouseUp);
		base.SizeChanged += new System.EventHandler(WorkspaceControl_SizeChanged);
		base.ResumeLayout(false);
	}
}
