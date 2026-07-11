using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoundedFloatEditor : UITypeEditor, IPropertyEditor, IAnnotatedParams
{
	protected class BoundedFloatControl : FloatInputControl, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		private bool m_refreshing;

		public virtual bool Cacheable => true;

		public BoundedFloatControl(PropertyEditorControlContext context, float min, float max)
			: base(min, min, max)
		{
			m_context = context;
			base.DrawBorder = false;
			DoubleBuffered = true;
			RefreshValue();
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if (!m_refreshing)
			{
				m_context.SetValue(base.Value);
			}
			base.OnValueChanged(e);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		private void RefreshValue()
		{
			try
			{
				m_refreshing = true;
				object value = m_context.GetValue();
				if (value == null)
				{
					base.Enabled = false;
					return;
				}
				base.Value = (float)value;
				base.Enabled = !m_context.IsReadOnly;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

	private float m_min;

	private float m_max = 100f;

	public float Min
	{
		get
		{
			return m_min;
		}
		set
		{
			m_min = value;
		}
	}

	public float Max
	{
		get
		{
			return m_max;
		}
		set
		{
			m_max = value;
		}
	}

	public BoundedFloatEditor()
	{
		Min = 0f;
		Max = 100f;
	}

	public BoundedFloatEditor(float min = 0f, float max = 0f)
	{
		if (min >= max)
		{
			throw new ArgumentOutOfRangeException("min must be less than max");
		}
		Min = min;
		Max = max;
	}

	public virtual Control GetEditingControl(PropertyEditorControlContext context)
	{
		BoundedFloatControl boundedFloatControl = new BoundedFloatControl(context, m_min, m_max);
		SkinService.ApplyActiveSkin(boundedFloatControl);
		return boundedFloatControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			if (!(value is float))
			{
				value = m_min;
			}
			FloatInputControl floatInputControl = new FloatInputControl((float)value, m_min, m_max);
			windowsFormsEditorService.DropDownControl(floatInputControl);
			if (!floatInputControl.Value.Equals(value))
			{
				value = floatInputControl.Value;
			}
		}
		return value;
	}

	public void Initialize(string[] parameters)
	{
		if (parameters.Length < 2 || !float.TryParse(parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || !float.TryParse(parameters[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) || result >= result2)
		{
			throw new ArgumentException("Can't parse bounds for float control");
		}
		Min = result;
		Max = result2;
	}
}
