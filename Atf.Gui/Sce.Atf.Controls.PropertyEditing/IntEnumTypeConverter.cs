using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class IntEnumTypeConverter : TypeConverter, IAnnotatedParams
{
	private string[] m_names;

	private int[] m_values;

	public IntEnumTypeConverter()
	{
	}

	public IntEnumTypeConverter(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("enumType must Enum");
		}
		m_names = Enum.GetNames(enumType);
		m_values = new int[m_names.Length];
		for (int i = 0; i < m_values.Length; i++)
		{
			m_values[i] = (int)Enum.Parse(enumType, m_names[i]);
		}
	}

	public IntEnumTypeConverter(string[] names, int[] values)
	{
		DefineEnums(names, values);
	}

	void IAnnotatedParams.Initialize(string[] parameters)
	{
		EnumUtil.ParseEnumDefinitions(parameters, out var names, out var _, out var values);
		DefineEnums(names, values);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			for (int i = 0; i < m_names.Length; i++)
			{
				if (text == m_names[i])
				{
					return m_values[i];
				}
			}
			return -1;
		}
		throw new ArgumentException("value must be string");
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is int && destinationType == typeof(string))
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
		throw new ArgumentException("value must be an int and destinationType must be a string");
	}

	private void DefineEnums(string[] names, int[] values)
	{
		if (names == null || names.Length == 0 || values == null || names.Length != values.Length)
		{
			throw new ArgumentException();
		}
		m_names = names;
		m_values = values;
	}
}
