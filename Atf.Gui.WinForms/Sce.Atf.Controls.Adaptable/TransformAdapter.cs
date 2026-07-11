using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable;

public class TransformAdapter : ControlAdapter, ITransformAdapter, IControlAdapter
{
	private Matrix m_transform = new Matrix();

	private PointF m_minTranslation = new PointF(float.MinValue, float.MinValue);

	private PointF m_maxTranslation = new PointF(float.MaxValue, float.MaxValue);

	private PointF m_minScale = new PointF(float.MinValue, float.MinValue);

	private PointF m_maxScale = new PointF(float.MaxValue, float.MaxValue);

	private bool m_uniformScale = true;

	private bool m_settingTransform;

	private bool m_enforceConstraints = true;

	public Matrix Transform => m_transform;

	public PointF Translation
	{
		get
		{
			return new PointF(m_transform.OffsetX, m_transform.OffsetY);
		}
		set
		{
			SetTranslation(value);
		}
	}

	public bool EnforceConstraints
	{
		get
		{
			return m_enforceConstraints;
		}
		set
		{
			m_enforceConstraints = value;
		}
	}

	public PointF MinTranslation
	{
		get
		{
			return m_minTranslation;
		}
		set
		{
			m_minTranslation = value;
			SetTranslation(Translation);
		}
	}

	public PointF MaxTranslation
	{
		get
		{
			return m_maxTranslation;
		}
		set
		{
			m_maxTranslation = value;
			SetTranslation(Translation);
		}
	}

	public PointF Scale
	{
		get
		{
			float[] elements = m_transform.Elements;
			return new PointF(elements[0], elements[3]);
		}
		set
		{
			SetScale(value);
		}
	}

	public PointF MinScale
	{
		get
		{
			return m_minScale;
		}
		set
		{
			if (value.X <= 0f || value.X > m_maxScale.X || value.Y <= 0f || value.Y > m_maxScale.Y)
			{
				throw new ArgumentException("minimum components must be > 0 and less than maximum");
			}
			m_minScale = value;
			SetScale(Scale);
		}
	}

	public PointF MaxScale
	{
		get
		{
			return m_maxScale;
		}
		set
		{
			if (value.X < m_minScale.X || value.Y < m_minScale.Y)
			{
				throw new ArgumentException("maximum components must be greater than minimum");
			}
			m_maxScale = value;
			SetScale(Scale);
		}
	}

	public bool UniformScale
	{
		get
		{
			return m_uniformScale;
		}
		set
		{
			m_uniformScale = value;
		}
	}

	public event EventHandler TransformChanged;

	public void SetTransform(float xScale, float yScale, float xTranslation, float yTranslation)
	{
		if (m_settingTransform)
		{
			return;
		}
		try
		{
			m_settingTransform = true;
			bool flag = false;
			float[] elements = m_transform.Elements;
			PointF pointF = (EnforceConstraints ? this.ConstrainScale(new PointF(xScale, yScale)) : new PointF(xScale, yScale));
			if (elements[0] != pointF.X || elements[3] != pointF.Y)
			{
				m_transform = new Matrix(pointF.X, 0f, 0f, pointF.Y, m_transform.OffsetX, m_transform.OffsetY);
				OnTransformChanged(EventArgs.Empty);
				this.TransformChanged.Raise(this, EventArgs.Empty);
				flag = true;
			}
			PointF pointF2 = (EnforceConstraints ? this.ConstrainTranslation(new PointF(xTranslation, yTranslation)) : new PointF(xTranslation, yTranslation));
			if (elements[4] != pointF2.X || elements[5] != pointF2.Y)
			{
				m_transform = new Matrix(pointF.X, 0f, 0f, pointF.Y, pointF2.X, pointF2.Y);
				OnTransformChanged(EventArgs.Empty);
				this.TransformChanged.Raise(this, EventArgs.Empty);
				flag = true;
			}
			if (flag)
			{
				if (base.AdaptedControl is D2dAdaptableControl d2dAdaptableControl)
				{
					d2dAdaptableControl.DrawD2d();
				}
				else if (base.AdaptedControl != null)
				{
					base.AdaptedControl.Invalidate();
				}
			}
		}
		finally
		{
			m_settingTransform = false;
		}
	}

	protected virtual void OnTransformChanged(EventArgs e)
	{
	}

	private void SetTranslation(PointF translation)
	{
		float[] elements = m_transform.Elements;
		SetTransform(elements[0], elements[3], translation.X, translation.Y);
	}

	private void SetScale(PointF scale)
	{
		SetTransform(scale.X, scale.Y, m_transform.OffsetX, m_transform.OffsetY);
	}
}
