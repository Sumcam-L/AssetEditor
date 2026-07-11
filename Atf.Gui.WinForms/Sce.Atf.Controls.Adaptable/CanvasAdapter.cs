using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class CanvasAdapter : ControlAdapter, ICanvasAdapter, ILayoutConstraint
{
	public Action<CanvasAdapter> UpdateTranslateMinMax;

	private ITransformAdapter m_transformAdapter;

	private Rectangle m_bounds;

	private Rectangle m_windowBounds;

	private PointF m_scale;

	private bool m_constraintEnabled = true;

	public Rectangle Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			if (m_bounds != value)
			{
				m_bounds = value;
				UpdateTranslationConstraints();
				OnBoundsChanged(EventArgs.Empty);
				this.BoundsChanged.Raise(this, EventArgs.Empty);
				base.AdaptedControl.Invalidate();
			}
		}
	}

	public Rectangle WindowBounds
	{
		get
		{
			return m_windowBounds;
		}
		set
		{
			if (m_windowBounds != value)
			{
				m_windowBounds = value;
				UpdateTranslationConstraints();
				OnWindowBoundsChanged(EventArgs.Empty);
				this.WindowBoundsChanged.Raise(this, EventArgs.Empty);
				base.AdaptedControl.Invalidate();
			}
		}
	}

	string ILayoutConstraint.Name => "Canvas Bounds".Localize();

	bool ILayoutConstraint.Enabled
	{
		get
		{
			return m_constraintEnabled;
		}
		set
		{
			m_constraintEnabled = value;
		}
	}

	public event EventHandler BoundsChanged;

	public event EventHandler WindowBoundsChanged;

	public CanvasAdapter()
	{
	}

	public CanvasAdapter(Rectangle bounds)
	{
		m_bounds = bounds;
	}

	protected virtual void OnBoundsChanged(EventArgs e)
	{
	}

	protected virtual void OnWindowBoundsChanged(EventArgs e)
	{
	}

	private void UpdateTranslationConstraints()
	{
		if (m_transformAdapter != null)
		{
			if (UpdateTranslateMinMax != null)
			{
				UpdateTranslateMinMax(this);
				return;
			}
			m_scale = m_transformAdapter.Scale;
			RectangleF rectangleF = new RectangleF((float)m_bounds.X * m_scale.X, (float)m_bounds.Y * m_scale.Y, (float)m_bounds.Width * m_scale.X, (float)m_bounds.Height * m_scale.Y);
			m_transformAdapter.MaxTranslation = new PointF(rectangleF.X, rectangleF.Y);
			m_transformAdapter.MinTranslation = new PointF(0f - Math.Max(0f, rectangleF.Width - (float)m_windowBounds.Width), 0f - Math.Max(0f, rectangleF.Height - (float)m_windowBounds.Height));
		}
	}

	Rectangle ILayoutConstraint.Constrain(Rectangle bounds, BoundsSpecified specified)
	{
		if (m_constraintEnabled)
		{
			if (bounds.X < m_bounds.X)
			{
				bounds.X = m_bounds.X;
			}
			else if (bounds.X >= m_bounds.Right)
			{
				bounds.X = m_bounds.Right - 1;
			}
			if (bounds.Y < m_bounds.Y)
			{
				bounds.Y = m_bounds.Y;
			}
			else if (bounds.Y >= m_bounds.Bottom)
			{
				bounds.Y = m_bounds.Bottom - 1;
			}
		}
		return bounds;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_windowBounds = control.ClientRectangle;
		m_transformAdapter = control.As<ITransformAdapter>();
		if (m_transformAdapter != null)
		{
			m_transformAdapter.TransformChanged += transformAdapter_TransformChanged;
		}
		control.ClientSizeChanged += control_ClientSizeChanged;
	}

	private void transformAdapter_TransformChanged(object sender, EventArgs e)
	{
		PointF scale = m_transformAdapter.Scale;
		if (m_scale != scale)
		{
			UpdateTranslationConstraints();
		}
	}

	protected override void Unbind(AdaptableControl control)
	{
		if (m_transformAdapter != null)
		{
			m_transformAdapter.TransformChanged -= transformAdapter_TransformChanged;
		}
		control.ClientSizeChanged -= control_ClientSizeChanged;
	}

	private void control_ClientSizeChanged(object sender, EventArgs e)
	{
		WindowBounds = base.AdaptedControl.ClientRectangle;
	}
}
