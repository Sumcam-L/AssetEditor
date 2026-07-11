using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace Sce.Atf.Wpf.Markup;

[ContentProperty("TypeArguments")]
public class GenericExtension : MarkupExtension, IValueConverter
{
	private Collection<Type> _typeArguments = new Collection<Type>();

	private string _typeName;

	public Collection<Type> TypeArguments => _typeArguments;

	public string TypeName
	{
		get
		{
			return _typeName;
		}
		set
		{
			_typeName = value;
		}
	}

	public GenericExtension()
	{
	}

	public GenericExtension(string typeName)
	{
		TypeName = typeName;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string[] array = _typeName.Split(':');
		if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlNamespaceResolver xamlNamespaceResolver))
		{
			throw new Exception("The Generic markup extension requires an IXamlNamespaceResolver service provider");
		}
		if (!(serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) is IXamlSchemaContextProvider xamlSchemaContextProvider))
		{
			throw new Exception("The Generic markup extension requires an IXamlSchemaContextProvider service provider");
		}
		string xamlNamespace = xamlNamespaceResolver.GetNamespace(array[0]);
		string name = array[1] + "`" + TypeArguments.Count;
		XamlTypeName xamlTypeName = new XamlTypeName(xamlNamespace, name);
		XamlType xamlType = xamlSchemaContextProvider.SchemaContext.GetXamlType(xamlTypeName);
		if (xamlType == null)
		{
			throw new Exception("The type could not be resolved");
		}
		Type underlyingType = xamlType.UnderlyingType;
		Type[] array2 = new Type[TypeArguments.Count];
		TypeArguments.CopyTo(array2, 0);
		Type type = underlyingType.MakeGenericType(array2);
		return Activator.CreateInstance(type);
	}

	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
