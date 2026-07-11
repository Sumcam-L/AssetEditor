using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Dom;

public abstract class PropertyDescriptor : System.ComponentModel.PropertyDescriptor
{
	private Func<bool> IsReadOnlyFunctor = () => false;

	private readonly Type m_type;

	private readonly string m_category;

	private readonly string m_description;

	private readonly object m_editor;

	private readonly TypeConverter m_typeConverter;

	private static readonly char[] PathDelimiters = new char[3] { '/', '\\', '.' };

	private const string AnnotationsNameAttribute = "name";

	private const string AnnotationsDisplayNameAttribute = "displayName";

	private const string AnnotationsLegacyEnumeration = "scea.dom.editors.enumeration";

	public virtual IEnumerable<ChildInfo> Path => EmptyEnumerable<ChildInfo>.Instance;

	public override string Category
	{
		get
		{
			if (m_category != null)
			{
				return m_category;
			}
			return base.Category;
		}
	}

	public override string Description => m_description;

	public override bool IsReadOnly => IsReadOnlyFunctor();

	public override Type PropertyType => m_type;

	public override Type ComponentType => typeof(DomNode);

	public override TypeConverter Converter
	{
		get
		{
			if (m_typeConverter != null)
			{
				return m_typeConverter;
			}
			return base.Converter;
		}
	}

	public object LocalEditor => m_editor;

	public PropertyDescriptor(string name, Type type, string category, string description, bool isReadOnly)
		: this(name, type, category, description, isReadOnly, null, null)
	{
		IsReadOnlyFunctor = () => isReadOnly;
	}

	public PropertyDescriptor(string name, Type type, string category, string description, bool isReadOnly, object editor)
		: this(name, type, category, description, isReadOnly, editor, null)
	{
		IsReadOnlyFunctor = () => isReadOnly;
	}

	public PropertyDescriptor(string name, Type type, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, type, category, description, isReadOnly, editor, typeConverter, null)
	{
		IsReadOnlyFunctor = () => isReadOnly;
	}

	public PropertyDescriptor(string name, Type type, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attributes)
	{
		m_type = type;
		m_category = category;
		m_description = description;
		IsReadOnlyFunctor = () => isReadOnly;
		m_editor = editor;
		m_typeConverter = typeConverter;
	}

	public PropertyDescriptor(string name, Type type, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attributes)
	{
		m_type = type;
		m_category = category;
		m_description = description;
		IsReadOnlyFunctor = isReadOnlyFunctor;
		m_editor = editor;
		m_typeConverter = typeConverter;
	}

	public override bool ShouldSerializeValue(object component)
	{
		return true;
	}

	public override object GetEditor(Type editorBaseType)
	{
		if (m_editor != null && editorBaseType.IsAssignableFrom(m_editor.GetType()))
		{
			return m_editor;
		}
		return base.GetEditor(editorBaseType);
	}

	public override bool Equals(object obj)
	{
		if (obj is PropertyDescriptor b)
		{
			return PropertyUtils.PropertyDescriptorsEqual(this, b);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.GetPropertyDescriptorHash();
	}

	public virtual DomNode GetNode(object component)
	{
		return component.As<DomNode>();
	}

	public static PropertyDescriptorCollection ParseXml(DomNodeType type, IEnumerable<XmlNode> annotations)
	{
		PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(EmptyArray<PropertyDescriptor>.Instance);
		foreach (XmlNode annotation in annotations)
		{
			try
			{
				XmlAttribute xmlAttribute = annotation.Attributes["name"];
				string text = null;
				string[] segments = null;
				if (xmlAttribute != null)
				{
					text = xmlAttribute.Value;
					segments = text.Split(PathDelimiters, StringSplitOptions.RemoveEmptyEntries);
				}
				PropertyDescriptor descriptor = GetDescriptor(type, annotation, text, segments);
				if (descriptor != null)
				{
					propertyDescriptorCollection.Add(descriptor);
				}
			}
			catch (AnnotationException ex)
			{
				Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
			}
		}
		return propertyDescriptorCollection;
	}

	private static PropertyDescriptor GetDescriptor(DomNodeType type, XmlNode annotation, string name, string[] segments)
	{
		PropertyDescriptor result = null;
		XmlAttribute xmlAttribute = annotation.Attributes["displayName"];
		if (xmlAttribute != null)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new AnnotationException($"Required name attribute is null or empty.\r\nType: {type.Name}\r\nElement: {annotation.Name}");
			}
			string value = xmlAttribute.Value;
			if (string.IsNullOrEmpty(value))
			{
				value = name;
			}
			string annotation2 = GetAnnotation(annotation, "category");
			string annotation3 = GetAnnotation(annotation, "description");
			bool isReadOnly = GetAnnotation(annotation, "readOnly") == "true";
			object editor = CreateObject<object>(type, annotation, "editor");
			TypeConverter typeConverter = CreateObject<TypeConverter>(type, annotation, "converter");
			if (annotation.Name == "scea.dom.editors.attribute")
			{
				if (segments == null)
				{
					throw new AnnotationException("Unnamed attribute");
				}
				if (segments.Length == 1)
				{
					AttributeInfo attributeInfo = type.GetAttributeInfo(name);
					if (attributeInfo == null)
					{
						throw new AnnotationException("Type doesn't have this attribute");
					}
					result = new AttributePropertyDescriptor(value, attributeInfo, annotation2, annotation3, isReadOnly, editor, typeConverter);
				}
				else
				{
					ChildInfo[] path = GetPath(type, segments, segments.Length - 1);
					DomNodeType type2 = path[segments.Length - 2].Type;
					AttributeInfo attributeInfo2 = type2.GetAttributeInfo(segments[segments.Length - 1]);
					if (attributeInfo2 == null)
					{
						throw new AnnotationException("Descendant type doesn't have this attribute");
					}
					result = new ChildAttributePropertyDescriptor(value, attributeInfo2, path, annotation2, annotation3, isReadOnly, editor, typeConverter);
				}
			}
			else if (annotation.Name == "scea.dom.editors.child")
			{
				ChildInfo childInfo = type.GetChildInfo(name);
				if (childInfo == null)
				{
					throw new AnnotationException("Type doesn't have this element");
				}
				result = new ChildPropertyDescriptor(value, childInfo, annotation2, annotation3, isReadOnly, editor, typeConverter);
			}
		}
		return result;
	}

	private static T CreateObject<T>(DomNodeType domNodeType, XmlNode annotation, string attribute) where T : class
	{
		string text = GetAnnotation(annotation, attribute);
		string text2 = string.Empty;
		if (text != null)
		{
			int num = text.IndexOf(':');
			if (num >= 0)
			{
				int num2 = num + 1;
				text2 = text.Substring(num2, text.Length - num2);
				text = text.Substring(0, num);
			}
			Type type = Type.GetType(text);
			if (type == null)
			{
				throw new AnnotationException("Couldn't find type " + text);
			}
			object obj = Activator.CreateInstance(type);
			if (obj is IAnnotatedParams annotatedParams)
			{
				string[] array = (string.IsNullOrEmpty(text2) ? TryGetEnumeration(domNodeType, annotation) : text2.Split(','));
				if (array != null)
				{
					annotatedParams.Initialize(array);
				}
			}
			if (!(obj is T result))
			{
				throw new AnnotationException("Object must be " + typeof(T));
			}
			return result;
		}
		return null;
	}

	private static string[] TryGetEnumeration(DomNodeType domNodeType, XmlNode annotation)
	{
		string[] result = null;
		XmlAttribute xmlAttribute = annotation.Attributes["name"];
		if (xmlAttribute != null)
		{
			AttributeInfo attributeInfo = domNodeType.GetAttributeInfo(xmlAttribute.Value);
			if (attributeInfo != null)
			{
				AttributeType type = attributeInfo.Type;
				IEnumerable<XmlNode> tag = type.GetTag<IEnumerable<XmlNode>>();
				if (tag != null)
				{
					List<string> list = new List<string>();
					foreach (XmlNode item in tag)
					{
						if (item.Name == "scea.dom.editors.enumeration")
						{
							string value = item.Attributes["name"].Value;
							XmlNode namedItem = item.Attributes.GetNamedItem("displayName");
							if (namedItem != null)
							{
								list.Add(value + "==" + namedItem.Value);
							}
							else
							{
								list.Add(value);
							}
						}
						result = list.ToArray();
					}
				}
			}
		}
		return result;
	}

	private static string GetAnnotation(XmlNode annotation, string attributeName)
	{
		string result = null;
		if (annotation != null)
		{
			XmlAttribute xmlAttribute = annotation.Attributes[attributeName];
			if (xmlAttribute != null)
			{
				result = xmlAttribute.Value;
			}
		}
		return result;
	}

	private static ChildInfo[] GetPath(DomNodeType type, string[] segments, int length)
	{
		ChildInfo[] array = new ChildInfo[length];
		for (int i = 0; i < length; i++)
		{
			ChildInfo childInfo = type.GetChildInfo(segments[i]);
			if (childInfo == null)
			{
				throw new AnnotationException("Invalid path");
			}
			array[i] = childInfo;
			type = childInfo.Type;
		}
		return array;
	}
}
