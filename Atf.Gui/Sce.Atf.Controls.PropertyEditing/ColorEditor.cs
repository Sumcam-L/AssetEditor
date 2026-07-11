using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class ColorEditor : System.Drawing.Design.ColorEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		TypeConverter converter = context.PropertyDescriptor.Converter;
		if (value == null)
		{
			value = Color.Transparent;
		}
		Color color = (Color)converter.ConvertTo(value, typeof(Color));
		Color color2 = (Color)base.EditValue(context, provider, (object)color);
		return converter.ConvertFrom(color2);
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		if (e.Value is Color)
		{
			base.PaintValue(e);
			return;
		}
		TypeConverter converter = e.Context.PropertyDescriptor.Converter;
		if (converter.ConvertTo(e.Value, typeof(Color)) is Color color)
		{
			base.PaintValue(new PaintValueEventArgs(e.Context, color, e.Graphics, e.Bounds));
		}
	}
}
