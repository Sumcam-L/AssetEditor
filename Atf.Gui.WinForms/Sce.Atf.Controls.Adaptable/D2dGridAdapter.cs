using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable;

public class D2dGridAdapter : ControlAdapter, ILayoutConstraint
{
	private ITransformAdapter m_transformAdapter;

	private Color m_gridColor;

	private float m_gridContrast = 0.25f;

	private int m_horizontalGridSpacing = 32;

	private int m_verticalGridSpacing = 32;

	private bool m_enabled = true;

	private bool m_visible = true;

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				Invalidate();
			}
		}
	}

	public float GridContrast
	{
		get
		{
			return m_gridContrast;
		}
		set
		{
			m_gridContrast = value;
			Invalidate();
		}
	}

	public int HorizontalGridSpacing
	{
		get
		{
			return m_horizontalGridSpacing;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_horizontalGridSpacing = value;
			Invalidate();
		}
	}

	public int VerticalGridSpacing
	{
		get
		{
			return m_verticalGridSpacing;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_verticalGridSpacing = value;
			Invalidate();
		}
	}

	public string Name => "Grid".Localize("Grid location constraint");

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
			m_visible = value;
		}
	}

	public bool ConstraintEnabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public Rectangle Constrain(Rectangle bounds, BoundsSpecified specified)
	{
		if ((specified & BoundsSpecified.X) != BoundsSpecified.None)
		{
			bounds.X = MathUtil.Snap(bounds.X, m_horizontalGridSpacing);
			if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
			{
				bounds.Width = MathUtil.Snap(bounds.Width, m_horizontalGridSpacing);
				bounds.Width = Math.Max(bounds.Width, 0);
			}
		}
		if ((specified & BoundsSpecified.Y) != BoundsSpecified.None)
		{
			bounds.Y = MathUtil.Snap(bounds.Y, m_verticalGridSpacing);
			if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
			{
				bounds.Height = MathUtil.Snap(bounds.Height, m_verticalGridSpacing);
				bounds.Height = Math.Max(bounds.Height, 0);
			}
		}
		return bounds;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		SetGridColor();
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.DrawingD2d += control_DrawingD2d;
		control.BackColorChanged += control_BackColorChanged;
	}

	protected override void Unbind(AdaptableControl control)
	{
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.DrawingD2d -= control_DrawingD2d;
	}

	protected virtual void SetGridColor()
	{
		Color backColor = base.AdaptedControl.BackColor;
		float num = ((float)(int)backColor.R + (float)(int)backColor.G + (float)(int)backColor.B) / 3f;
		float num2 = ((num < 128f) ? m_gridContrast : (0f - m_gridContrast));
		m_gridColor = ColorUtil.GetShade(backColor, 1f + num2);
	}

	private void control_BackColorChanged(object sender, EventArgs e)
	{
		SetGridColor();
	}

	private void control_DrawingD2d(object sender, EventArgs e)
	{
		if (m_visible)
		{
			D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
			Matrix3x2F transform = d2dAdaptableControl.D2dGraphics.Transform;
			d2dAdaptableControl.D2dGraphics.Transform = Matrix3x2F.Identity;
			Matrix transform2 = m_transformAdapter.Transform;
			RectangleF r = base.AdaptedControl.ClientRectangle;
			RectangleF graphRect = GdiUtil.InverseTransform(transform2, r);
			D2dAntialiasMode antialiasMode = d2dAdaptableControl.D2dGraphics.AntialiasMode;
			d2dAdaptableControl.D2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
			ChartUtil.DrawHorizontalGrid(transform2, graphRect, m_verticalGridSpacing, m_gridColor, d2dAdaptableControl.D2dGraphics);
			ChartUtil.DrawVerticalGrid(transform2, graphRect, m_horizontalGridSpacing, m_gridColor, d2dAdaptableControl.D2dGraphics);
			d2dAdaptableControl.D2dGraphics.Transform = transform;
			d2dAdaptableControl.D2dGraphics.AntialiasMode = antialiasMode;
		}
	}

	private void Invalidate()
	{
		if (base.AdaptedControl != null)
		{
			base.AdaptedControl.Invalidate();
		}
	}
}
