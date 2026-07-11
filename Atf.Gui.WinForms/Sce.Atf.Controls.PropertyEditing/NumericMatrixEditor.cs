using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class NumericMatrixEditor : UITypeEditor, IPropertyEditor
{
	private class NumericMatrixControl : Sce.Atf.Controls.NumericMatrixControl, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		private bool m_refreshing;

		public bool Cacheable => true;

		public NumericMatrixControl(Type type, int rows, int columns, PropertyEditorControlContext context)
			: base(type, rows, columns)
		{
			m_context = context;
			RefreshValue();
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if (!m_refreshing)
			{
				m_context.SetValue(base.LastChange, (Array)Value);
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
				base.Value = value;
				base.Enabled = !m_context.IsReadOnly;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

	private Type m_numericType;

	private int m_rows;

	private int m_columns;

	private double m_scaleFactor = 1.0;

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

	public NumericMatrixEditor()
		: this(typeof(float), 4, 4)
	{
	}

	public NumericMatrixEditor(Type numericType, int rows, int columns)
	{
		Define(numericType, rows, columns);
	}

	public void Define(Type numericType, int rows, int columns)
	{
		m_numericType = numericType;
		m_rows = rows;
		m_columns = columns;
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		NumericMatrixControl numericMatrixControl = new NumericMatrixControl(m_numericType, m_rows, m_columns, context);
		numericMatrixControl.ScaleFactor = m_scaleFactor;
		SkinService.ApplyActiveSkin(numericMatrixControl);
		return numericMatrixControl;
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
			Sce.Atf.Controls.NumericMatrixControl numericMatrixControl = new Sce.Atf.Controls.NumericMatrixControl(m_numericType, m_rows, m_columns);
			if (value != null)
			{
				numericMatrixControl.Value = value;
			}
			windowsFormsEditorService.DropDownControl(numericMatrixControl);
			value = numericMatrixControl.LastEdit;
		}
		return value;
	}
}
