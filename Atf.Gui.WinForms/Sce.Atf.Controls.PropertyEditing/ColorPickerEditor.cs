using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class ColorPickerEditor : Sce.Atf.Controls.ColorPickerEditor, IAnnotatedParams
{
	public void Initialize(string[] parameters)
	{
		if (parameters.Length >= 1)
		{
			base.EnableAlpha = bool.Parse(parameters[0]);
		}
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		TypeConverter converter = context.PropertyDescriptor.Converter;
		Color color = (Color)converter.ConvertTo(value, typeof(Color));
		Color color2 = (Color)base.EditValue(context, provider, (object)color);
		return converter.ConvertFrom(color2);
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		TypeConverter converter = e.Context.PropertyDescriptor.Converter;
		object obj = converter.ConvertTo(e.Value, typeof(Color));
		if (obj != null && obj is Color)
		{
			Color color = (Color)obj;
			base.PaintValue(new PaintValueEventArgs(e.Context, color, e.Graphics, e.Bounds));
		}
	}
}
