using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoundedIntEditor : UITypeEditor, IPropertyEditor, IAnnotatedParams
{
	protected class BoundedIntControl : IntInputControl, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		private bool m_refreshing;

		public virtual bool Cacheable => true;

		public BoundedIntControl(PropertyEditorControlContext context, int min, int max)
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
				base.Value = (int)value;
				base.Enabled = !m_context.IsReadOnly;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

	private int m_min;

	private int m_max = 100;

	public int Min
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

	public int Max
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

	public BoundedIntEditor()
	{
		Min = 0;
		Max = 100;
	}

	public BoundedIntEditor(int min, int max)
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
		BoundedIntControl boundedIntControl = new BoundedIntControl(context, m_min, m_max);
		SkinService.ApplyActiveSkin(boundedIntControl);
		return boundedIntControl;
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
			if (!(value is int))
			{
				value = m_min;
			}
			IntInputControl intInputControl = new IntInputControl((int)value, m_min, m_max);
			windowsFormsEditorService.DropDownControl(intInputControl);
			if (!intInputControl.Value.Equals(value))
			{
				value = intInputControl.Value;
			}
		}
		return value;
	}

	public void Initialize(string[] parameters)
	{
		if (parameters.Length < 2 || !int.TryParse(parameters[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || !int.TryParse(parameters[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) || result >= result2)
		{
			throw new ArgumentException("Can't parse bounds for BoundedIntEditor");
		}
		Min = result;
		Max = result2;
	}
}
