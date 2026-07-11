using System;
using System.Windows.Markup;
using Sce.Atf.Wpf.ValueConverters;

namespace Sce.Atf.Wpf.Markup;

public sealed class TypeConverterExtension : MarkupExtension
{
	private readonly TypeExtension m_sourceTypeExtension;

	private readonly TypeExtension m_targetTypeExtension;

	[ConstructorArgument("sourceType")]
	public Type SourceType
	{
		get
		{
			return m_sourceTypeExtension.Type;
		}
		set
		{
			m_sourceTypeExtension.Type = value;
		}
	}

	[ConstructorArgument("targetType")]
	public Type TargetType
	{
		get
		{
			return m_targetTypeExtension.Type;
		}
		set
		{
			m_targetTypeExtension.Type = value;
		}
	}

	[ConstructorArgument("sourceTypeName")]
	public string SourceTypeName
	{
		get
		{
			return m_sourceTypeExtension.TypeName;
		}
		set
		{
			m_sourceTypeExtension.TypeName = value;
		}
	}

	[ConstructorArgument("targetTypeName")]
	public string TargetTypeName
	{
		get
		{
			return m_targetTypeExtension.TypeName;
		}
		set
		{
			m_targetTypeExtension.TypeName = value;
		}
	}

	public TypeConverterExtension()
	{
		m_sourceTypeExtension = new TypeExtension();
		m_targetTypeExtension = new TypeExtension();
	}

	public TypeConverterExtension(Type sourceType, Type targetType)
	{
		m_sourceTypeExtension = new TypeExtension(sourceType);
		m_targetTypeExtension = new TypeExtension(targetType);
	}

	public TypeConverterExtension(string sourceTypeName, string targetTypeName)
	{
		m_sourceTypeExtension = new TypeExtension(sourceTypeName);
		m_targetTypeExtension = new TypeExtension(targetTypeName);
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		Type sourceType = null;
		Type targetType = null;
		if (m_sourceTypeExtension.Type != null || m_sourceTypeExtension.TypeName != null)
		{
			sourceType = m_sourceTypeExtension.ProvideValue(serviceProvider) as Type;
		}
		if (m_targetTypeExtension.Type != null || m_targetTypeExtension.TypeName != null)
		{
			targetType = m_targetTypeExtension.ProvideValue(serviceProvider) as Type;
		}
		return new TypeConverter(sourceType, targetType);
	}
}
