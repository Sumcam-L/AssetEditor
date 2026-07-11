using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class NumericTupleEditor : UITypeEditor, IPropertyEditor, IAnnotatedParams
{
	private class NumericTupleControl : Sce.Atf.Controls.NumericTupleControl, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		private bool m_refreshing;

		public bool Cacheable => true;

		public NumericTupleControl(Type type, string[] names, PropertyEditorControlContext context)
			: base(type, names)
		{
			m_context = context;
			RefreshValue();
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if (!m_refreshing)
			{
				m_context.SetValue(base.LastChange, (Array)base.Value);
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

	private Color[] m_labelColors;

	private Type m_numericType;

	private string[] m_names;

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

	public bool HideAxisLabel { get; set; }

	public NumericTupleEditor()
		: this(typeof(float), new string[3] { "x", "y", "z" })
	{
	}

	public NumericTupleEditor(Type numericType, string[] names)
	{
		Define(numericType, names);
	}

	public void Define(Type numericType, string[] names)
	{
		m_numericType = numericType;
		m_names = names;
		m_labelColors = new Color[4]
		{
			Color.FromArgb(200, 40, 0),
			Color.FromArgb(100, 160, 0),
			Color.FromArgb(40, 120, 240),
			Color.FromArgb(20, 20, 20)
		};
	}

	void IAnnotatedParams.Initialize(string[] parameters)
	{
		Type type = Type.GetType(parameters[0]);
		string[] array = new string[parameters.Length - 1];
		Array.Copy(parameters, 1, array, 0, array.Length);
		Define(type, array);
	}

	public void SetLabelBackColors(Color[] color)
	{
		m_labelColors = color;
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		NumericTupleControl numericTupleControl = new NumericTupleControl(m_numericType, m_names, context);
		numericTupleControl.Height = numericTupleControl.Font.Height + 2;
		numericTupleControl.ScaleFactor = m_scaleFactor;
		numericTupleControl.HideAxisLabel = HideAxisLabel;
		numericTupleControl.SetLabelBackColors(m_labelColors);
		SkinService.ApplyActiveSkin(numericTupleControl);
		return numericTupleControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		float width = g.MeasureString("#.###", f).Width;
		float num = 0f;
		float num2 = 0f;
		string[] names = m_names;
		foreach (string text in names)
		{
			SizeF sizeF = g.MeasureString(text, f);
			num += sizeF.Width + width;
			num2 = Math.Max(num2, sizeF.Height);
		}
		return new SizeF(num, num2);
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			Sce.Atf.Controls.NumericTupleControl numericTupleControl = new Sce.Atf.Controls.NumericTupleControl(m_numericType, m_names);
			numericTupleControl.HideAxisLabel = HideAxisLabel;
			numericTupleControl.SetLabelBackColors(m_labelColors);
			if (value != null)
			{
				numericTupleControl.Value = value;
			}
			windowsFormsEditorService.DropDownControl(numericTupleControl);
			value = numericTupleControl.LastEdit;
		}
		return value;
	}
}
