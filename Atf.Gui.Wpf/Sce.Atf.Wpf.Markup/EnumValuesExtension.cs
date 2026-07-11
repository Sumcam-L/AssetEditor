using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup;

[ContentProperty("EnumType")]
[MarkupExtensionReturnType(typeof(object[]))]
public class EnumValuesExtension : MarkupExtension
{
	public class EnumerationMember
	{
		public object Value { get; set; }

		public string DisplayString { get; set; }

		public string Description { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(DisplayString) ? Value.ToString() : DisplayString;
		}
	}

	private Type m_enumType;

	[ConstructorArgument("enumType")]
	public Type EnumType
	{
		get
		{
			return m_enumType;
		}
		private set
		{
			if (m_enumType != value)
			{
				Type type = Nullable.GetUnderlyingType(value) ?? value;
				if (!type.IsEnum)
				{
					throw new ArgumentException("Type must be an Enum.");
				}
				m_enumType = value;
			}
		}
	}

	public EnumValuesExtension()
	{
	}

	public EnumValuesExtension(Type enumType)
	{
		Requires.NotNull(enumType, "The enum type is not set");
		EnumType = enumType;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		Array values = Enum.GetValues(EnumType);
		IEnumerable<EnumerationMember> source = from object enumValue in values
			select new EnumerationMember
			{
				Value = enumValue,
				Description = GetDescription(enumValue),
				DisplayString = GetDisplayString(enumValue)
			};
		return source.ToArray();
	}

	private string GetDescription(object enumValue)
	{
		return (EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false).FirstOrDefault() is DescriptionAttribute descriptionAttribute) ? descriptionAttribute.Description : enumValue.ToString();
	}

	private string GetDisplayString(object enumValue)
	{
		return (EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(DisplayStringAttribute), inherit: false).FirstOrDefault() is DisplayStringAttribute displayStringAttribute) ? displayStringAttribute.Value : enumValue.ToString();
	}
}
