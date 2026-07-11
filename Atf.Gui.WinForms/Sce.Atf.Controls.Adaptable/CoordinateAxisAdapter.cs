using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class CoordinateAxisAdapter : ControlAdapter
{
	private readonly DiagramTheme m_theme;

	private ITransformAdapter m_transformAdapter;

	private ICanvasAdapter m_canvasAdapter;

	private int m_horizontalTickSpacing = 64;

	private int m_verticalTickSpacing = 64;

	private float m_minimumGraphStep = 1f;

	private bool m_horizontalVisible = true;

	private bool m_verticalVisible = true;

	public bool HorizontalVisible
	{
		get
		{
			return m_horizontalVisible;
		}
		set
		{
			if (m_horizontalVisible != value)
			{
				m_horizontalVisible = value;
				Invalidate();
			}
		}
	}

	public bool VerticalVisible
	{
		get
		{
			return m_verticalVisible;
		}
		set
		{
			if (m_verticalVisible != value)
			{
				m_verticalVisible = value;
				Invalidate();
			}
		}
	}

	[DefaultValue(64)]
	public int HorizontalTickSpacing
	{
		get
		{
			return m_horizontalTickSpacing;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_horizontalTickSpacing = value;
			Invalidate();
		}
	}

	[DefaultValue(64)]
	public int VerticalTickSpacing
	{
		get
		{
			return m_verticalTickSpacing;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_verticalTickSpacing = value;
			Invalidate();
		}
	}

	[DefaultValue(1f)]
	public float MinimumGraphStep
	{
		get
		{
			return m_minimumGraphStep;
		}
		set
		{
			if (m_minimumGraphStep != value)
			{
				m_minimumGraphStep = value;
				Invalidate();
			}
		}
	}

	public CoordinateAxisAdapter(DiagramTheme theme)
	{
		m_theme = theme;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		m_canvasAdapter = control.As<ICanvasAdapter>();
		control.Paint += control_Paint;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.Paint -= control_Paint;
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		Matrix matrix = new Matrix();
		if (m_transformAdapter != null)
		{
			matrix = m_transformAdapter.Transform;
		}
		RectangleF r = base.AdaptedControl.ClientRectangle;
		if (m_canvasAdapter != null)
		{
			r = m_canvasAdapter.WindowBounds;
		}
		RectangleF graphRect = GdiUtil.InverseTransform(matrix, r);
		if (m_horizontalVisible)
		{
			ChartUtil.DrawHorizontalScale(matrix, graphRect, top: false, m_verticalTickSpacing, 0f, m_theme.OutlinePen, m_theme.Font, m_theme.TextBrush, e.Graphics);
		}
		if (m_verticalVisible)
		{
			ChartUtil.DrawVerticalScale(matrix, graphRect, left: true, m_horizontalTickSpacing, 0f, m_theme.OutlinePen, m_theme.Font, m_theme.TextBrush, e.Graphics);
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
