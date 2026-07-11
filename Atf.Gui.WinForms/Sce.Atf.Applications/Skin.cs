using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Sce.Atf.Applications;

public class Skin : ISkin
{
	private List<SkinStyle> m_styles = new List<SkinStyle>();

	private static readonly Dictionary<string, Type> s_stringToType = new Dictionary<string, Type>();

	private static readonly DefaultTypeConverter s_defaultTypeConverter = new DefaultTypeConverter();

	protected const string EmbeddedSkinString = "Embedded Skin";

	private const string UnsavedSkinString = "Unsaved Skin";

	private const string SkinSchema = "Sce.Atf.Applications.SkinService.Schemas.skin.xsd";

	private const string ValueInfoElement = "valueInfo";

	private const string ListInfoElement = "listInfo";

	private const string SetterElement = "setter";

	private const string ConstructorParamsElement = "constructorParams";

	private const string StyleElement = "style";

	private const string PropertyNameAttribute = "propertyName";

	private const string ValueAttribute = "value";

	private const string TypeAttribute = "type";

	private const string TargetTypeAttribute = "targetType";

	private const string ConverterAttribute = "converter";

	public static readonly BindingFlags PropertyLookupType = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

	public IList<SkinStyle> Styles => m_styles;

	public Uri Uri { get; private set; }

	public Skin(XmlDocument xmlDoc, Uri fileUri)
	{
		Uri = fileUri;
		LoadSkin(xmlDoc);
	}

	private void LoadSkin(XmlDocument xmlDoc)
	{
		m_styles.Clear();
		List<SkinStyle> list = new List<SkinStyle>();
		XmlElement documentElement = xmlDoc.DocumentElement;
		XmlNodeList xmlNodeList = documentElement.SelectNodes("style");
		if (xmlNodeList == null)
		{
			throw new FileFormatException("Error loading the skin file.");
		}
		foreach (XmlElement item in xmlNodeList)
		{
			try
			{
				string attribute = item.GetAttribute("targetType");
				Type typeFromString = GetTypeFromString(attribute);
				if (typeFromString != null)
				{
					SkinStyle skinStyle = new SkinStyle(typeFromString);
					skinStyle.Setters.AddRange(GetSetters(item));
					InsertStyleIntoTree(skinStyle, list);
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
		SetInheritedSetters(new List<Setter>(), list);
		if (list.Count > 0)
		{
			m_styles.AddRange(list);
		}
	}

	private static void InsertStyleIntoTree(SkinStyle newStyle, List<SkinStyle> roots)
	{
		foreach (SkinStyle root in roots)
		{
			if (newStyle.TargetType.IsSubclassOf(root.TargetType))
			{
				InsertStyleIntoTree(newStyle, root.Dependents);
				return;
			}
		}
		int num = 0;
		for (int i = 0; i < roots.Count; i++)
		{
			SkinStyle skinStyle = roots[i];
			if (skinStyle.TargetType.IsSubclassOf(newStyle.TargetType))
			{
				newStyle.Dependents.Add(skinStyle);
			}
			else
			{
				roots[num++] = skinStyle;
			}
		}
		roots.RemoveRange(num, roots.Count - num);
		roots.Add(newStyle);
	}

	private static void SetInheritedSetters(List<Setter> inheritedSetters, List<SkinStyle> roots)
	{
		foreach (SkinStyle root in roots)
		{
			foreach (Setter inheritedSetter in inheritedSetters)
			{
				bool flag = false;
				foreach (Setter setter in root.Setters)
				{
					if (inheritedSetter.PropertyName == setter.PropertyName)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					root.Setters.Add(inheritedSetter);
				}
			}
			SetInheritedSetters(root.Setters, root.Dependents);
		}
	}

	private IEnumerable<Setter> GetSetters(XmlElement parentElement)
	{
		List<Setter> list = new List<Setter>();
		XmlNodeList xmlNodeList = parentElement.SelectNodes("setter");
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return list;
		}
		foreach (XmlElement item in xmlNodeList)
		{
			string attribute = item.GetAttribute("propertyName");
			XmlNodeList xmlNodeList2 = item.SelectNodes("valueInfo");
			XmlNodeList xmlNodeList3 = item.SelectNodes("listInfo");
			if (xmlNodeList2 != null && xmlNodeList2.Count > 0)
			{
				if (xmlNodeList3 != null && xmlNodeList3.Count > 0)
				{
					throw new FileFormatException("Setter cannot define both a ValueInfo and ListInfo.");
				}
				if (xmlNodeList2.Count != 1)
				{
					throw new FileFormatException("Setter can specify no more than one ValueInfo");
				}
				ValueInfo value = GetValue((XmlElement)xmlNodeList2[0]);
				list.Add(new Setter(attribute, value));
			}
			else
			{
				if (xmlNodeList3 == null)
				{
					throw new FileFormatException("Each setter must specify one ValueInfo, or one ListInfo");
				}
				if (xmlNodeList3.Count != 1)
				{
					throw new FileFormatException("Setter can specify no more than one ListInfo");
				}
				ListInfo list2 = GetList((XmlElement)xmlNodeList3[0]);
				list.Add(new Setter(attribute, list2));
			}
		}
		return list;
	}

	private ValueInfo GetValue(XmlElement valueElement)
	{
		ValueInfo valueInfo = new ValueInfo
		{
			Type = GetValueType(valueElement)
		};
		valueInfo.Converter = GetValueConverter(valueElement, valueInfo.Type);
		valueInfo.ConstructorParams.AddRange(GetValueConstructorParams(valueElement));
		valueInfo.Setters.AddRange(GetSetters(valueElement));
		if (valueInfo.Setters.Count == 0 && valueInfo.ConstructorParams.Count == 0)
		{
			valueInfo.Value = valueElement.GetAttribute("value");
			if (valueInfo.Converter == null)
			{
				throw new FileFormatException(string.Concat("There is no converter for the type \"", valueInfo.Type, "\""));
			}
		}
		return valueInfo;
	}

	private ListInfo GetList(XmlElement listElement)
	{
		ListInfo listInfo = new ListInfo();
		listInfo.Values.AddRange(from XmlElement valueElement in listElement.SelectNodes("valueInfo")
			select GetValue(valueElement));
		return listInfo;
	}

	private IEnumerable<ValueInfo> GetValueConstructorParams(XmlElement valueElement)
	{
		List<ValueInfo> list = new List<ValueInfo>();
		XmlNodeList xmlNodeList = valueElement.SelectNodes("constructorParams");
		if (xmlNodeList == null || xmlNodeList.Count > 1)
		{
			throw new FileFormatException("This skin file is corrupt.");
		}
		foreach (XmlElement item in xmlNodeList)
		{
			list.AddRange(from XmlElement constructorParamValue in item.SelectNodes("valueInfo")
				select GetValue(constructorParamValue));
		}
		return list;
	}

	private static Type GetTypeFromString(string typeString)
	{
		if (string.IsNullOrEmpty(typeString))
		{
			return null;
		}
		switch (typeString)
		{
		case "System.Drawing.Color":
			return typeof(Color);
		case "string":
			return typeof(string);
		case "int":
			return typeof(int);
		case "float":
			return typeof(float);
		case "char":
			return typeof(char);
		case "byte":
			return typeof(byte);
		default:
		{
			if (s_stringToType.TryGetValue(typeString, out var value))
			{
				return value;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				value = assembly.GetType(typeString);
				if (value != null)
				{
					break;
				}
			}
			s_stringToType.Add(typeString, value);
			return value;
		}
		}
	}

	private Type GetSetterType(XmlElement setterElement)
	{
		XmlElement xmlElement = setterElement.ParentNode as XmlElement;
		string attribute = setterElement.GetAttribute("propertyName");
		if (string.CompareOrdinal(xmlElement.Name, "style") == 0)
		{
			string attribute2 = xmlElement.GetAttribute("targetType");
			Type typeFromString = GetTypeFromString(attribute2);
			if (typeFromString == null)
			{
				throw new TypeLoadException($"{attribute2} doesn't exist in this application. Skin cannot load.");
			}
			PropertyInfo property = typeFromString.GetProperty(attribute, PropertyLookupType);
			return property.PropertyType;
		}
		Type valueType = GetValueType(xmlElement);
		PropertyInfo property2 = valueType.GetProperty(attribute, PropertyLookupType);
		return property2.PropertyType;
	}

	private Type GetValueType(XmlElement valueElement)
	{
		XmlElement xmlElement = valueElement.ParentNode as XmlElement;
		string attribute = valueElement.GetAttribute("type");
		if (string.IsNullOrEmpty(attribute) && string.CompareOrdinal(xmlElement.Name, "constructorParams") == 0)
		{
			throw new FileFormatException("Constructor parameters must have their type explicitly specified in the skin file.");
		}
		return GetTypeFromString(attribute) ?? GetSetterType(xmlElement);
	}

	private static TypeConverter GetValueConverter(XmlElement valueElement, Type destinationType)
	{
		TypeConverter typeConverter = null;
		Type typeFromString = GetTypeFromString(valueElement.GetAttribute("converter"));
		if (typeFromString != null)
		{
			typeConverter = Activator.CreateInstance(typeFromString) as TypeConverter;
		}
		if (typeConverter == null && s_defaultTypeConverter.CanConvertTo(null, destinationType))
		{
			typeConverter = s_defaultTypeConverter;
		}
		return typeConverter;
	}
}
