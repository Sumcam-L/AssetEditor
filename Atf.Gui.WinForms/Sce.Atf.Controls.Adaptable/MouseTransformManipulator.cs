using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class MouseTransformManipulator : ControlAdapter
{
	private Keys? m_modifierKeys;

	private static Keys s_defaultModifierKeys = Keys.Alt;

	private Keys? m_constrainModifierKeys;

	private static Keys s_defaultConstrainModifierKeys = Keys.None;

	private MouseButtons? m_translationButton;

	private static MouseButtons m_defaultTranslationButton = MouseButtons.Left;

	private MouseButtons? m_scaleButton;

	private static MouseButtons s_defaultScaleButton = MouseButtons.Right;

	private readonly ITransformAdapter m_transformAdapter;

	private Cursor m_saveCursor;

	private Point m_firstPoint;

	private PointF m_startingTranslation;

	private PointF m_scaleStart;

	private PointF m_scaleCenterStart;

	private bool m_isTranslating;

	private bool m_isScaling;

	private bool m_constrainXDetermined;

	private bool m_constrainX;

	private PointF m_selfSetTranslation;

	public Keys ModifierKeys
	{
		get
		{
			return m_modifierKeys.HasValue ? m_modifierKeys.Value : s_defaultModifierKeys;
		}
		set
		{
			m_modifierKeys = value;
		}
	}

	public static Keys DefaultModifierKeys
	{
		get
		{
			return s_defaultModifierKeys;
		}
		set
		{
			s_defaultModifierKeys = value;
		}
	}

	public Keys ConstrainModifierKeys
	{
		get
		{
			return m_constrainModifierKeys.HasValue ? m_constrainModifierKeys.Value : s_defaultConstrainModifierKeys;
		}
		set
		{
			m_constrainModifierKeys = value;
		}
	}

	public static Keys DefaultConstrainModifierKeys
	{
		get
		{
			return s_defaultConstrainModifierKeys;
		}
		set
		{
			s_defaultConstrainModifierKeys = value;
		}
	}

	public MouseButtons TranslationButton
	{
		get
		{
			return m_translationButton.HasValue ? m_translationButton.Value : m_defaultTranslationButton;
		}
		set
		{
			m_translationButton = value;
		}
	}

	public static MouseButtons DefaultTranslationButton
	{
		get
		{
			return m_defaultTranslationButton;
		}
		set
		{
			m_defaultTranslationButton = value;
		}
	}

	public MouseButtons ScaleButton
	{
		get
		{
			return m_scaleButton.HasValue ? m_scaleButton.Value : s_defaultScaleButton;
		}
		set
		{
			m_scaleButton = value;
		}
	}

	public static MouseButtons DefaultScaleButton
	{
		get
		{
			return s_defaultScaleButton;
		}
		set
		{
			s_defaultScaleButton = value;
		}
	}

	public MouseTransformManipulator(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseDown += control_MouseDown;
		control.MouseMove += control_MouseMove;
		control.MouseUp += control_MouseUp;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.MouseDown -= control_MouseDown;
		control.MouseMove -= control_MouseMove;
		control.MouseUp -= control_MouseUp;
	}

	protected virtual bool TestForTranslation(Keys modifiers, MouseEventArgs e)
	{
		if (modifiers == Keys.None && e.Button == MouseButtons.Middle)
		{
			return true;
		}
		return (modifiers & ModifierKeys) == ModifierKeys && (e.Button & TranslationButton) != 0;
	}

	protected virtual bool TestForScale(Keys modifiers, MouseEventArgs e)
	{
		return (modifiers & ModifierKeys) == ModifierKeys && (e.Button & ScaleButton) != 0;
	}

	private void control_MouseDown(object sender, MouseEventArgs e)
	{
		if (!base.AdaptedControl.Capture)
		{
			Keys modifierKeys = Control.ModifierKeys;
			m_firstPoint = new Point(e.X, e.Y);
			if (TestForTranslation(modifierKeys, e))
			{
				m_isTranslating = true;
				m_startingTranslation = m_transformAdapter.Translation;
				base.AdaptedControl.Capture = true;
				m_saveCursor = base.AdaptedControl.Cursor;
				base.AdaptedControl.Cursor = Cursors.Hand;
			}
			else if (TestForScale(modifierKeys, e))
			{
				m_isScaling = true;
				PointF translation = m_transformAdapter.Translation;
				m_scaleStart = m_transformAdapter.Scale;
				m_scaleCenterStart = new PointF(((float)m_firstPoint.X - translation.X) / m_scaleStart.X, ((float)m_firstPoint.Y - translation.Y) / m_scaleStart.Y);
				base.AdaptedControl.Capture = true;
				m_saveCursor = base.AdaptedControl.Cursor;
				base.AdaptedControl.Cursor = Cursors.SizeAll;
			}
		}
	}

	private void control_MouseMove(object sender, MouseEventArgs e)
	{
		if (!m_isTranslating && !m_isScaling)
		{
			return;
		}
		Point firstPoint = new Point(e.X, e.Y);
		int num = firstPoint.X - m_firstPoint.X;
		int num2 = firstPoint.Y - m_firstPoint.Y;
		if (m_selfSetTranslation != m_transformAdapter.Translation)
		{
			m_startingTranslation = m_transformAdapter.Translation;
			m_firstPoint = firstPoint;
		}
		Keys modifierKeys = Control.ModifierKeys;
		bool flag = (ConstrainModifierKeys != Keys.None && (modifierKeys & ConstrainModifierKeys) == ConstrainModifierKeys) || (m_isScaling && m_transformAdapter.UniformScale);
		if (flag && !m_constrainXDetermined)
		{
			float num3 = Math.Abs(num);
			float num4 = Math.Abs(num2);
			Size dragSize = SystemInformation.DragSize;
			if (num3 > (float)dragSize.Width || num4 > (float)dragSize.Height)
			{
				m_constrainX = num3 < num4;
				m_constrainXDetermined = true;
			}
		}
		if (m_isTranslating)
		{
			if (flag)
			{
				if (m_constrainX)
				{
					num = 0;
				}
				else
				{
					num2 = 0;
				}
			}
			PointF selfSetTranslation = new PointF(m_startingTranslation.X + (float)num, m_startingTranslation.Y + (float)num2);
			m_transformAdapter.Translation = (m_selfSetTranslation = selfSetTranslation);
		}
		else
		{
			if (!m_isScaling)
			{
				return;
			}
			float num5 = (float)(4 * num) / (float)base.AdaptedControl.Width;
			float num6 = (float)(4 * num2) / (float)base.AdaptedControl.Height;
			if (flag)
			{
				if (m_constrainX)
				{
					num5 = num6;
				}
				else
				{
					num6 = num5;
				}
			}
			PointF pointF = TransformAdapters.ConstrainScale(scale: new PointF(Math.Max(0.001f, m_scaleStart.X * (float)Math.Pow(2.0, num5)), Math.Max(0.001f, m_scaleStart.Y * (float)Math.Pow(2.0, num6))), adapter: m_transformAdapter);
			PointF selfSetTranslation2 = new PointF((float)m_firstPoint.X - m_scaleCenterStart.X * pointF.X, (float)m_firstPoint.Y - m_scaleCenterStart.Y * pointF.Y);
			m_transformAdapter.SetTransform(pointF.X, pointF.Y, selfSetTranslation2.X, selfSetTranslation2.Y);
			m_selfSetTranslation = selfSetTranslation2;
		}
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		if (m_isTranslating || m_isScaling)
		{
			m_isTranslating = false;
			m_isScaling = false;
			m_constrainXDetermined = false;
			base.AdaptedControl.Cursor = m_saveCursor;
			base.AdaptedControl.Capture = false;
		}
	}
}
