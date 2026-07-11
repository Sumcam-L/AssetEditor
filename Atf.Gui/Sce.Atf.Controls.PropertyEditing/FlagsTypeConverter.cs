using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sce.Atf.Controls.PropertyEditing;

public class FlagsTypeConverter : TypeConverter, IAnnotatedParams
{
	private string[] m_names = EmptyArray<string>.Instance;

	private int[] m_values = EmptyArray<int>.Instance;

	private string[] m_displayNames = EmptyArray<string>.Instance;

	private static readonly string NoFlags = "(none)".Localize("No flags");

	public FlagsTypeConverter()
	{
	}

	public FlagsTypeConverter(string[] definitions)
	{
		DefineFlags(definitions);
	}

	public FlagsTypeConverter(string[] names, int[] values)
	{
		DefineFlags(names, values);
	}

	public void DefineFlags(string[] definitions)
	{
		EnumUtil.ParseFlagDefinitions(definitions, out m_names, out m_displayNames, out m_values);
	}

	public void DefineFlags(string[] names, int[] values)
	{
		if (names == null || values == null || names.Length != values.Length)
		{
			throw new ArgumentException("names and/or values null, or of unequal length");
		}
		m_names = names;
		m_displayNames = names;
		m_values = values;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
	{
		return srcType == typeof(string);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			string text2 = string.Empty;
			string[] array = text.Split('|');
			string[] array2 = array;
			foreach (string displayName in array2)
			{
				string internalName = GetInternalName(displayName);
				if (internalName != null)
				{
					text2 = ((!(text2 != string.Empty)) ? internalName : (text2 + "|" + internalName));
				}
			}
			return text2;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(string) || destType == typeof(int) || destType == typeof(uint);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if ((value is int || value is uint) && destType == typeof(string))
		{
			int num = Convert.ToInt32(value);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < m_values.Length; i++)
			{
				if ((num & m_values[i]) != 0)
				{
					stringBuilder.Append(m_names[i]);
					stringBuilder.Append("|");
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length--;
			}
			else
			{
				stringBuilder.Append(NoFlags);
			}
			return stringBuilder.ToString();
		}
		if (value is string && destType == typeof(string))
		{
			string text = string.Empty;
			string[] array = ((string)value).Split('|');
			string[] array2 = array;
			foreach (string internalName in array2)
			{
				string displayName = GetDisplayName(internalName);
				if (displayName != null)
				{
					text = ((!(text != string.Empty)) ? displayName : (text + "|" + displayName));
				}
			}
			return text;
		}
		if (value is string && destType == typeof(int))
		{
			string[] source = ((string)value).Split('|');
			int num2 = source.Aggregate(0, (int current, string internalName2) => current | GetValue(internalName2));
			return num2;
		}
		if (value is string && destType == typeof(uint))
		{
			string[] source2 = ((string)value).Split('|');
			int value2 = source2.Aggregate(0, (int current, string internalName2) => current | GetValue(internalName2));
			return Convert.ToUInt32(value2);
		}
		return base.ConvertTo(context, culture, value, destType);
	}

	private string GetInternalName(string displayName)
	{
		for (int i = 0; i < m_displayNames.Length; i++)
		{
			if (m_displayNames[i] == displayName)
			{
				return m_names[i];
			}
		}
		return null;
	}

	private string GetDisplayName(string internalName)
	{
		for (int i = 0; i < m_names.Length; i++)
		{
			if (m_names[i] == internalName)
			{
				return m_displayNames[i];
			}
		}
		return null;
	}

	private int GetValue(string internalName)
	{
		for (int i = 0; i < m_names.Length; i++)
		{
			if (m_names[i] == internalName)
			{
				return m_values[i];
			}
		}
		return 0;
	}

	public void Initialize(string[] parameters)
	{
		DefineFlags(parameters);
	}
}
