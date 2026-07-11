using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class ExclusiveEnumTypeConverter : EnumTypeConverter
{
	public ExclusiveEnumTypeConverter()
	{
	}

	public ExclusiveEnumTypeConverter(string[] names)
		: base(names)
	{
		DefineEnum(names);
	}

	public ExclusiveEnumTypeConverter(string[] names, int[] values)
		: base(names, values)
	{
		DefineEnum(names, values);
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return new StandardValuesCollection(new ReadOnlyCollection<string>(base.Names));
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}
}
