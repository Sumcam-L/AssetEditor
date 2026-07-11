using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class DateTimeEditor : System.ComponentModel.Design.DateTimeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		TypeConverter converter = context.PropertyDescriptor.Converter;
		DateTime dateTime = GetDateTime(value, converter);
		DateTime dateTime2 = (DateTime)base.EditValue(context, provider, (object)dateTime);
		return GetValue(dateTime2, value.GetType(), converter);
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		DateTime dateTime = GetDateTime(e.Value, e.Context.PropertyDescriptor.Converter);
		base.PaintValue(new PaintValueEventArgs(e.Context, dateTime, e.Graphics, e.Bounds));
	}

	private DateTime GetDateTime(object value, TypeConverter converter)
	{
		if (typeof(DateTime).IsAssignableFrom(value.GetType()))
		{
			return (DateTime)value;
		}
		return (DateTime)converter.ConvertTo(value, typeof(DateTime));
	}

	private object GetValue(DateTime dateTime, Type type, TypeConverter converter)
	{
		if (type.IsAssignableFrom(typeof(DateTime)))
		{
			return dateTime;
		}
		return converter.ConvertFrom(dateTime);
	}
}
