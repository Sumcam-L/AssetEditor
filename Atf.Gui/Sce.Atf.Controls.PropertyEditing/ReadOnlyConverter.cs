using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class ReadOnlyConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return false;
	}
}
