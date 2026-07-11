using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class NumericEditor : IPropertyEditor
{
	private class NumericTextBox : Sce.Atf.Controls.NumericTextBox, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		public bool Cacheable => true;

		public NumericTextBox(Type numericType, PropertyEditorControlContext context)
			: base(numericType)
		{
			m_context = context;
			base.BorderStyle = BorderStyle.None;
			RefreshValue();
		}

		protected override void OnValueEdited(EventArgs e)
		{
			m_context.SetValue(base.Value);
			base.OnValueEdited(e);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		private void RefreshValue()
		{
			object value = m_context.GetValue();
			if (value == null)
			{
				base.Enabled = false;
				return;
			}
			base.Value = value;
			base.Enabled = !m_context.IsReadOnly;
		}
	}

	private Type m_numericType;

	private double m_scaleFactor = 1.0;

	public Type NumericType
	{
		get
		{
			return m_numericType;
		}
		set
		{
			m_numericType = value;
		}
	}

	public double ScaleFactor
	{
		get
		{
			return m_scaleFactor;
		}
		set
		{
			if (value == 0.0)
			{
				throw new ArgumentException("value must be non-zero");
			}
			m_scaleFactor = value;
		}
	}

	public NumericEditor()
		: this(typeof(float))
	{
	}

	public NumericEditor(Type numericType)
	{
		m_numericType = numericType;
	}

	public virtual Control GetEditingControl(PropertyEditorControlContext context)
	{
		NumericTextBox numericTextBox = new NumericTextBox(m_numericType, context);
		numericTextBox.ScaleFactor = m_scaleFactor;
		SkinService.ApplyActiveSkin(numericTextBox);
		return numericTextBox;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}
