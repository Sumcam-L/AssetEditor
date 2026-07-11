using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Controls.ColorEditing;

namespace Sce.Atf.Controls;

public class ColorPickerEditor : ColorEditor
{
	private bool m_enableAlpha;

	public bool EnableAlpha
	{
		get
		{
			return m_enableAlpha;
		}
		set
		{
			m_enableAlpha = value;
		}
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			ColorPicker colorPicker = new ColorPicker((Color)value, m_enableAlpha);
			Color color = (Color)value;
			colorPicker.Shown += delegate(object sender, EventArgs e)
			{
				ColorPicker colorPicker2 = (ColorPicker)sender;
				colorPicker2.SetStartColor(color, m_enableAlpha);
			};
			if (DialogResult.OK == windowsFormsEditorService.ShowDialog(colorPicker))
			{
				value = colorPicker.PrimaryColor;
			}
		}
		return value;
	}
}
