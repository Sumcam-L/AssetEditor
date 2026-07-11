using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class EnumTypeConverter : TypeConverter, IAnnotatedParams
{
	private string[] m_names = EmptyArray<string>.Instance;

	private int[] m_values = EmptyArray<int>.Instance;

	protected IList<string> Names => m_names;

	protected IList<int> Values => m_values;

	public EnumTypeConverter()
	{
	}

	public EnumTypeConverter(string[] names)
	{
		DefineEnum(names);
	}

	public EnumTypeConverter(string[] names, int[] values)
	{
		DefineEnum(names, values);
	}

	public void DefineEnum(string[] names)
	{
		EnumUtil.ParseEnumDefinitions(names, out m_names, out m_values);
	}

	public void DefineEnum(string[] names, int[] values)
	{
		if (names == null || values == null || names.Length != values.Length)
		{
			throw new ArgumentException("names and/or values null, or of unequal length");
		}
		m_names = names;
		m_values = values;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
	{
		return false;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (value is int && destType == typeof(string))
		{
			int num = (int)value;
			for (int i = 0; i < m_values.Length; i++)
			{
				if (num == m_values[i])
				{
					return m_names[i];
				}
			}
			return string.Empty;
		}
		return base.ConvertTo(context, culture, value, destType);
	}

	public void Initialize(string[] parameters)
	{
		DefineEnum(parameters);
	}
}
