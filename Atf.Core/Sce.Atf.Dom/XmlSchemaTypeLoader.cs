using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Sce.Atf.Dom;

public class XmlSchemaTypeLoader
{
	private readonly Dictionary<string, XmlSchemaTypeCollection> m_typeCollections = new Dictionary<string, XmlSchemaTypeCollection>();

	private readonly Dictionary<string, XmlAttributeType> m_attributeTypes = new Dictionary<string, XmlAttributeType>();

	private readonly Dictionary<string, DomNodeType> m_nodeTypes = new Dictionary<string, DomNodeType>();

	private readonly Dictionary<string, ChildInfo> m_rootElements = new Dictionary<string, ChildInfo>();

	private readonly Dictionary<ChildInfo, XmlQualifiedName> m_refElements = new Dictionary<ChildInfo, XmlQualifiedName>();

	private IDictionary<NamedMetadata, IList<XmlNode>> m_annotations;

	private HashSet<string> m_typeNameSet;

	private Dictionary<XmlSchemaElement, XmlQualifiedName> m_localElementSet;

	private XmlResolver m_schemaResolver = new XmlUrlResolver();

	private static readonly XmlAttributeType s_mixedTextFieldSimpleType = new XmlAttributeType("mixed_text_field", typeof(string), 1, XmlTypeCode.String);

	private static readonly XmlQualifiedName s_anyTypeName = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");

	public XmlResolver SchemaResolver
	{
		get
		{
			return m_schemaResolver;
		}
		set
		{
			m_schemaResolver = value;
		}
	}

	public XmlSchema Load(string schemaFileName)
	{
		XmlSchema xmlSchema = null;
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.XmlResolver = m_schemaResolver;
		using (XmlReader reader = XmlReader.Create(schemaFileName, xmlReaderSettings))
		{
			xmlSchema = XmlSchema.Read(reader, null);
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			xmlSchemaSet.XmlResolver = m_schemaResolver;
			xmlSchemaSet.Add(xmlSchema);
			Load(xmlSchemaSet);
		}
		return xmlSchema;
	}

	public void Load(XmlSchemaSet schemaSet)
	{
		if (!schemaSet.IsCompiled)
		{
			schemaSet.Compile();
		}
		ICollection collection = schemaSet.Schemas();
		foreach (XmlSchema item in collection)
		{
			string targetNamespace = item.TargetNamespace;
			if (string.IsNullOrEmpty(targetNamespace))
			{
				throw new InvalidOperationException("Schema has no target namespace");
			}
			if (!m_typeCollections.ContainsKey(targetNamespace))
			{
				XmlQualifiedName[] namespaces = item.Namespaces.ToArray();
				XmlSchemaTypeCollection value = new XmlSchemaTypeCollection(namespaces, targetNamespace, this);
				m_typeCollections.Add(targetNamespace, value);
			}
		}
		try
		{
			m_annotations = new Dictionary<NamedMetadata, IList<XmlNode>>();
			m_typeNameSet = new HashSet<string>();
			m_localElementSet = new Dictionary<XmlSchemaElement, XmlQualifiedName>();
			foreach (XmlSchemaElement value5 in schemaSet.GlobalElements.Values)
			{
				m_typeNameSet.Add(value5.QualifiedName.Name);
			}
			foreach (XmlSchemaType value6 in schemaSet.GlobalTypes.Values)
			{
				if (value6 is XmlSchemaComplexType)
				{
					m_typeNameSet.Add(value6.Name);
				}
			}
			Multimap<XmlQualifiedName, ChildInfo> multimap = new Multimap<XmlQualifiedName, ChildInfo>();
			foreach (XmlSchemaElement value7 in schemaSet.GlobalElements.Values)
			{
				XmlSchemaType elementSchemaType = value7.ElementSchemaType;
				DomNodeType nodeType = GetNodeType(elementSchemaType, value7);
				ChildInfo childInfo = new ChildInfo(GetFieldName(value7.QualifiedName), nodeType);
				m_annotations.Add(childInfo, GetAnnotation(value7));
				if (!value7.SubstitutionGroup.IsEmpty)
				{
					multimap.Add(value7.SubstitutionGroup, childInfo);
				}
				string key = value7.QualifiedName.ToString();
				if (!m_rootElements.ContainsKey(key))
				{
					m_rootElements[key] = childInfo;
				}
			}
			foreach (XmlSchemaType value8 in schemaSet.GlobalTypes.Values)
			{
				if (value8 is XmlSchemaComplexType)
				{
					GetNodeType(value8, null);
				}
			}
			foreach (KeyValuePair<ChildInfo, XmlQualifiedName> refElement in m_refElements)
			{
				XmlQualifiedName value2 = refElement.Value;
				ChildInfo key2 = refElement.Key;
				ChildInfo[] array = CreateSubstitutions(multimap, value2).ToArray();
				if (array.Length != 0)
				{
					key2.AddRule(new SubstitutionGroupChildRule(array));
				}
			}
			foreach (XmlSchema item2 in collection)
			{
				foreach (XmlSchemaObject include in item2.Includes)
				{
					if (include is XmlSchemaRedefine schemaRedefine)
					{
						MergeRedefinedTypeAnnotations(schemaRedefine);
					}
				}
			}
			List<List<DomNodeType>> list = new List<List<DomNodeType>>();
			foreach (DomNodeType nodeType2 in GetNodeTypes())
			{
				int num = 0;
				DomNodeType domNodeType = nodeType2;
				while (domNodeType != null && domNodeType != DomNodeType.BaseOfAllTypes)
				{
					num++;
					domNodeType = domNodeType.BaseType;
				}
				int num2 = num - 2;
				if (num2 >= 0)
				{
					while (list.Count <= num2)
					{
						list.Add(new List<DomNodeType>());
					}
					list[num2].Add(nodeType2);
				}
			}
			foreach (List<DomNodeType> item3 in list)
			{
				foreach (DomNodeType item4 in item3)
				{
					if (item4.BaseType != null && item4.BaseType != DomNodeType.BaseOfAllTypes && m_annotations.TryGetValue(item4.BaseType, out var value3) && m_annotations.TryGetValue(item4, out var value4))
					{
						IEnumerable<XmlNode> enumerable = MergeInheritedTypeAnnotations(value3, value4);
						m_annotations[item4] = (enumerable as IList<XmlNode>) ?? enumerable.ToList();
					}
				}
			}
			OnSchemaSetLoaded(schemaSet);
			foreach (DomNodeType nodeType3 in GetNodeTypes())
			{
				foreach (XmlAttributeInfo item5 in nodeType3.Attributes.OfType<XmlAttributeInfo>())
				{
					if (((XmlAttributeType)item5.Type).XmlTypeCode == XmlTypeCode.Id)
					{
						nodeType3.SetIdAttribute(item5.Name);
					}
				}
			}
			foreach (KeyValuePair<NamedMetadata, IList<XmlNode>> annotation in m_annotations)
			{
				if (annotation.Value.Count > 0)
				{
					annotation.Key.SetTag((IEnumerable<XmlNode>)annotation.Value);
				}
			}
			ParseAnnotations(schemaSet, m_annotations);
			OnDomNodeTypesFrozen(schemaSet);
		}
		finally
		{
			m_annotations = null;
			m_typeNameSet = null;
			m_localElementSet = null;
		}
	}

	public static XmlNode FindElement(IEnumerable<XmlNode> xmlNodes, string elementName)
	{
		foreach (XmlNode xmlNode in xmlNodes)
		{
			if (xmlNode.LocalName == elementName)
			{
				return xmlNode;
			}
		}
		return null;
	}

	public static string FindAttribute(IEnumerable<XmlNode> xmlNodes, string elementName, string attributeName)
	{
		XmlNode xmlNode = FindElement(xmlNodes, elementName);
		if (xmlNode != null)
		{
			return FindAttribute(xmlNode, attributeName);
		}
		return null;
	}

	public static string FindAttribute(XmlNode xmlNode, string attributeName)
	{
		return xmlNode.Attributes[attributeName]?.Value;
	}

	public XmlSchemaTypeCollection GetTypeCollection(string targetNamespace)
	{
		m_typeCollections.TryGetValue(targetNamespace, out var value);
		return value;
	}

	public IEnumerable<XmlSchemaTypeCollection> GetTypeCollections()
	{
		return m_typeCollections.Values;
	}

	public XmlAttributeType GetAttributeType(string name)
	{
		m_attributeTypes.TryGetValue(name, out var value);
		return value;
	}

	public IEnumerable<AttributeType> GetAttributeTypes(string ns)
	{
		ns += ":";
		foreach (KeyValuePair<string, XmlAttributeType> kvp in m_attributeTypes)
		{
			if (kvp.Key.StartsWith(ns))
			{
				yield return kvp.Value;
			}
		}
	}

	public IEnumerable<AttributeType> GetAttributeTypes()
	{
		foreach (XmlAttributeType value in m_attributeTypes.Values)
		{
			yield return value;
		}
	}

	public DomNodeType GetNodeType(string name)
	{
		m_nodeTypes.TryGetValue(name, out var value);
		return value;
	}

	public IEnumerable<DomNodeType> GetNodeTypes(string ns)
	{
		ns += ":";
		foreach (KeyValuePair<string, DomNodeType> kvp in m_nodeTypes)
		{
			if (kvp.Key.StartsWith(ns))
			{
				yield return kvp.Value;
			}
		}
	}

	public IEnumerable<DomNodeType> GetNodeTypes(DomNodeType baseType)
	{
		if (baseType == null)
		{
			throw new ArgumentNullException("baseType");
		}
		foreach (DomNodeType type in m_nodeTypes.Values)
		{
			if (type != baseType && baseType.IsAssignableFrom(type))
			{
				yield return type;
			}
		}
	}

	public IEnumerable<DomNodeType> GetNodeTypes()
	{
		return m_nodeTypes.Values;
	}

	public void AddNodeType(string name, DomNodeType type)
	{
		m_nodeTypes[name] = type;
	}

	public bool RemoveNodeType(string name)
	{
		return m_nodeTypes.Remove(name);
	}

	public ChildInfo GetRootElement(string name)
	{
		m_rootElements.TryGetValue(name, out var value);
		return value;
	}

	public IEnumerable<ChildInfo> GetRootElements(string ns)
	{
		ns += ":";
		foreach (KeyValuePair<string, ChildInfo> kvp in m_rootElements)
		{
			if (kvp.Key.StartsWith(ns))
			{
				yield return kvp.Value;
			}
		}
	}

	public IEnumerable<ChildInfo> GetRootElements()
	{
		return m_rootElements.Values;
	}

	protected virtual void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
	}

	protected virtual void OnDomNodeTypesFrozen(XmlSchemaSet schemaSet)
	{
	}

	protected virtual void ParseAnnotations(XmlSchemaSet schemaSet, IDictionary<NamedMetadata, IList<XmlNode>> annotations)
	{
		foreach (XmlSchemaElement value2 in schemaSet.GlobalElements.Values)
		{
			ChildInfo rootElement = GetRootElement(value2.QualifiedName.ToString());
			if (!annotations.TryGetValue(rootElement.Type, out var value))
			{
				continue;
			}
			string text = FindAttribute(value, "idAttribute", "name");
			if (text == null)
			{
				continue;
			}
			foreach (DomNodeType nodeType in GetNodeTypes(value2.QualifiedName.Namespace))
			{
				nodeType.SetIdAttribute(text);
			}
		}
	}

	protected virtual string GetFieldName(XmlQualifiedName qualifiedName)
	{
		if (m_typeCollections.TryGetValue(qualifiedName.Namespace, out var value) && qualifiedName.Namespace == value.TargetNamespace)
		{
			return qualifiedName.Name;
		}
		return qualifiedName.ToString();
	}

	private IEnumerable<ChildInfo> CreateSubstitutions(Multimap<XmlQualifiedName, ChildInfo> substitutionGroups, XmlQualifiedName refName)
	{
		foreach (XmlQualifiedName group in substitutionGroups.Keys)
		{
			if (!(group == refName))
			{
				continue;
			}
			IEnumerable<ChildInfo> childInfos = substitutionGroups[group];
			foreach (ChildInfo childInfo in childInfos)
			{
				yield return childInfo;
				string ns = string.Empty;
				int index = childInfo.Type.Name.LastIndexOf(':');
				if (index >= 0)
				{
					ns = childInfo.Type.Name.Substring(0, index);
				}
				XmlQualifiedName qualifiedName = new XmlQualifiedName(childInfo.Name, ns);
				foreach (ChildInfo item in CreateSubstitutions(substitutionGroups, qualifiedName))
				{
					yield return item;
				}
			}
		}
	}

	private XmlAttributeType GetAttributeType(XmlSchemaSimpleType simpleType)
	{
		if (!m_attributeTypes.TryGetValue(simpleType.QualifiedName.ToString(), out var value))
		{
			bool flag = simpleType.Content is XmlSchemaSimpleTypeList;
			int result = 1;
			if (flag)
			{
				result = int.MaxValue;
			}
			List<AttributeRule> list = null;
			if (simpleType.Content is XmlSchemaSimpleTypeRestriction xmlSchemaSimpleTypeRestriction)
			{
				if (xmlSchemaSimpleTypeRestriction.BaseTypeName != null)
				{
					m_attributeTypes.TryGetValue(xmlSchemaSimpleTypeRestriction.BaseTypeName.ToString(), out var value2);
					if (value2 != null)
					{
						result = value2.Length;
					}
				}
				foreach (XmlSchemaFacet facet in xmlSchemaSimpleTypeRestriction.Facets)
				{
					int result3;
					if (facet is XmlSchemaLengthFacet xmlSchemaLengthFacet)
					{
						int.TryParse(xmlSchemaLengthFacet.Value, out result);
					}
					else if (facet is XmlSchemaMinLengthFacet xmlSchemaMinLengthFacet)
					{
						if (int.TryParse(xmlSchemaMinLengthFacet.Value, out var result2))
						{
							result = Math.Max(result, result2);
						}
					}
					else if (facet is XmlSchemaMaxLengthFacet xmlSchemaMaxLengthFacet && int.TryParse(xmlSchemaMaxLengthFacet.Value, out result3))
					{
						result = Math.Max(result, result3);
					}
				}
				list = GetRules(xmlSchemaSimpleTypeRestriction);
			}
			string text = simpleType.QualifiedName.ToString();
			Type type = simpleType.Datatype.ValueType;
			XmlTypeCode xmlTypeCode = simpleType.Datatype.TypeCode;
			if (xmlTypeCode == XmlTypeCode.Idref)
			{
				type = ((!type.IsArray) ? typeof(DomNode) : typeof(string[]));
			}
			switch (xmlTypeCode)
			{
			case XmlTypeCode.Integer:
				type = ((!type.IsArray) ? typeof(int) : typeof(int[]));
				xmlTypeCode = XmlTypeCode.Int;
				break;
			case XmlTypeCode.NonNegativeInteger:
				type = ((!type.IsArray) ? typeof(uint) : typeof(uint[]));
				xmlTypeCode = XmlTypeCode.UnsignedInt;
				break;
			}
			value = new XmlAttributeType(text, type, result, xmlTypeCode);
			m_annotations.Add(value, GetAnnotation(simpleType));
			if (list != null)
			{
				foreach (AttributeRule item in list)
				{
					value.AddRule(item);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				m_attributeTypes.Add(text, value);
			}
		}
		return value;
	}

	private DomNodeType GetNodeType(XmlSchemaType type, XmlSchemaElement element)
	{
		XmlSchemaComplexType xmlSchemaComplexType = type as XmlSchemaComplexType;
		DomNodeType domNodeType = null;
		if (xmlSchemaComplexType != null)
		{
			return GetNodeType(xmlSchemaComplexType, element);
		}
		XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
		return WrapSimpleType(simpleType);
	}

	private DomNodeType GetNodeType(XmlSchemaComplexType complexType, XmlSchemaElement element)
	{
		XmlQualifiedName xmlQualifiedName = complexType.QualifiedName;
		if (xmlQualifiedName.IsEmpty)
		{
			xmlQualifiedName = GetLocalTypeName(element);
		}
		string text = xmlQualifiedName.ToString();
		if (!m_nodeTypes.TryGetValue(text, out var value))
		{
			value = new DomNodeType(text);
			m_nodeTypes.Add(text, value);
			m_annotations.Add(value, GetAnnotation(complexType));
			DomNodeType domNodeType = null;
			XmlAttributeType xmlAttributeType = null;
			XmlSchemaComplexType baseType = GetBaseType(complexType);
			if (baseType != null)
			{
				domNodeType = GetNodeType(baseType, null);
			}
			if (complexType.BaseXmlSchemaType is XmlSchemaSimpleType simpleType)
			{
				xmlAttributeType = GetAttributeType(simpleType);
			}
			else if (complexType.IsMixed)
			{
				xmlAttributeType = s_mixedTextFieldSimpleType;
			}
			WalkParticle(complexType.ContentTypeParticle, value);
			if (xmlAttributeType != null)
			{
				XmlAttributeInfo attributeInfo = new XmlAttributeInfo(string.Empty, xmlAttributeType);
				value.Define(attributeInfo);
			}
			ICollection values = complexType.AttributeUses.Values;
			foreach (XmlSchemaAttribute item in values)
			{
				XmlAttributeType attributeType = GetAttributeType(item.AttributeSchemaType);
				string fieldName = GetFieldName(item.QualifiedName);
				XmlAttributeInfo xmlAttributeInfo = new XmlAttributeInfo(fieldName, attributeType);
				if (item.DefaultValue != null)
				{
					xmlAttributeInfo.DefaultValue = attributeType.Convert(item.DefaultValue);
				}
				m_annotations.Add(xmlAttributeInfo, GetAnnotation(item));
				value.Define(xmlAttributeInfo);
			}
			if (domNodeType != null)
			{
				value.BaseType = domNodeType;
			}
			value.IsAbstract = complexType.IsAbstract;
		}
		return value;
	}

	private void WalkParticle(XmlSchemaParticle particle, DomNodeType nodeType)
	{
		if (particle is XmlSchemaElement xmlSchemaElement)
		{
			XmlSchemaSimpleType xmlSchemaSimpleType = xmlSchemaElement.ElementSchemaType as XmlSchemaSimpleType;
			if (xmlSchemaSimpleType != null && xmlSchemaElement.MaxOccurs == 1m)
			{
				XmlAttributeType attributeType = GetAttributeType(xmlSchemaSimpleType);
				string fieldName = GetFieldName(xmlSchemaElement.QualifiedName);
				XmlAttributeInfo xmlAttributeInfo = new XmlAttributeInfo(fieldName, attributeType);
				nodeType.Define(xmlAttributeInfo);
				m_annotations.Add(xmlAttributeInfo, GetAnnotation(xmlSchemaElement));
				xmlAttributeInfo.IsElement = true;
				if (xmlSchemaElement.DefaultValue != null)
				{
					if (xmlSchemaElement.FixedValue != null)
					{
						throw new InvalidOperationException($"Schema element {xmlSchemaElement.QualifiedName} cannot have both a default value and a fixed value");
					}
					xmlAttributeInfo.DefaultValue = attributeType.Convert(xmlSchemaElement.DefaultValue);
				}
				else if (xmlSchemaElement.FixedValue != null)
				{
					xmlAttributeInfo.DefaultValue = attributeType.Convert(xmlSchemaElement.FixedValue);
				}
				return;
			}
			DomNodeType domNodeType = null;
			if (xmlSchemaSimpleType != null)
			{
				domNodeType = WrapSimpleType(xmlSchemaSimpleType);
				XmlAttributeType attributeType2 = GetAttributeType(xmlSchemaSimpleType);
				XmlAttributeInfo attributeInfo = new XmlAttributeInfo(string.Empty, attributeType2);
				domNodeType.Define(attributeInfo);
			}
			else if (xmlSchemaElement.ElementSchemaType is XmlSchemaComplexType complexType)
			{
				domNodeType = GetNodeType(complexType, xmlSchemaElement);
			}
			if (domNodeType != null)
			{
				int num;
				int num2;
				if (particle.Parent is XmlSchemaChoice)
				{
					XmlSchemaChoice xmlSchemaChoice = (XmlSchemaChoice)particle.Parent;
					num = (int)Math.Min(Math.Min(xmlSchemaElement.MinOccurs, xmlSchemaChoice.MinOccurs), 2147483647m);
					num2 = (int)Math.Min(Math.Max(xmlSchemaElement.MaxOccurs, xmlSchemaChoice.MaxOccurs), 2147483647m);
				}
				else if (particle.Parent is XmlSchemaSequence)
				{
					XmlSchemaSequence xmlSchemaSequence = (XmlSchemaSequence)particle.Parent;
					num = (int)Math.Min(Math.Min(xmlSchemaElement.MinOccurs, xmlSchemaSequence.MinOccurs), 2147483647m);
					num2 = (int)Math.Min(Math.Max(xmlSchemaElement.MaxOccurs, xmlSchemaSequence.MaxOccurs), 2147483647m);
				}
				else
				{
					num = (int)Math.Min(xmlSchemaElement.MinOccurs, 2147483647m);
					num2 = (int)Math.Min(xmlSchemaElement.MaxOccurs, 2147483647m);
				}
				ChildInfo childInfo = new ChildInfo(GetFieldName(xmlSchemaElement.QualifiedName), domNodeType, num2 > 1);
				if (num > 0 || num2 < int.MaxValue)
				{
					childInfo.AddRule(new ChildCountRule(num, num2));
				}
				if (!xmlSchemaElement.RefName.IsEmpty)
				{
					m_refElements.Add(childInfo, xmlSchemaElement.RefName);
				}
				nodeType.Define(childInfo);
				m_annotations.Add(childInfo, GetAnnotation(xmlSchemaElement));
			}
			return;
		}
		if (particle is XmlSchemaSequence xmlSchemaSequence2)
		{
			{
				foreach (XmlSchemaParticle item in xmlSchemaSequence2.Items)
				{
					WalkParticle(item, nodeType);
				}
				return;
			}
		}
		if (!(particle is XmlSchemaChoice xmlSchemaChoice2))
		{
			return;
		}
		foreach (XmlSchemaParticle item2 in xmlSchemaChoice2.Items)
		{
			WalkParticle(item2, nodeType);
		}
	}

	private DomNodeType WrapSimpleType(XmlSchemaSimpleType simpleType)
	{
		string text = simpleType.QualifiedName.ToString();
		if (!m_nodeTypes.TryGetValue(text, out var value))
		{
			value = new DomNodeType(text);
			m_nodeTypes.Add(text, value);
			m_annotations.Add(value, GetAnnotation(simpleType));
		}
		return value;
	}

	private XmlSchemaComplexType GetBaseType(XmlSchemaComplexType type)
	{
		XmlSchemaComplexType xmlSchemaComplexType = type;
		while (xmlSchemaComplexType != null && xmlSchemaComplexType.QualifiedName == type.QualifiedName)
		{
			xmlSchemaComplexType = xmlSchemaComplexType.BaseXmlSchemaType as XmlSchemaComplexType;
		}
		if (xmlSchemaComplexType != null && xmlSchemaComplexType.QualifiedName == s_anyTypeName)
		{
			xmlSchemaComplexType = null;
		}
		return xmlSchemaComplexType;
	}

	private XmlQualifiedName GetLocalTypeName(XmlSchemaElement element)
	{
		if (m_localElementSet.TryGetValue(element, out var value))
		{
			return value;
		}
		if (element.QualifiedName == null || element.RefName.Name == string.Empty)
		{
			XmlSchemaObject parent = element.Parent;
			string text = null;
			while (parent != null)
			{
				if (parent is XmlSchemaComplexType { Name: var name } xmlSchemaComplexType)
				{
					if (name == null)
					{
						XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)xmlSchemaComplexType.Parent;
						name = xmlSchemaElement.Name;
					}
					if (text == null)
					{
						text = element.Name;
					}
					text = name + "_" + text;
					if (!m_typeNameSet.Contains(text))
					{
						value = new XmlQualifiedName(text, element.QualifiedName.Namespace);
						m_localElementSet.Add(element, value);
						m_typeNameSet.Add(text);
						return value;
					}
				}
				parent = parent.Parent;
			}
		}
		return element.QualifiedName;
	}

	private List<AttributeRule> GetRules(XmlSchemaSimpleTypeRestriction restriction)
	{
		List<AttributeRule> list = new List<AttributeRule>();
		List<string> list2 = null;
		foreach (XmlSchemaFacet facet in restriction.Facets)
		{
			double result4;
			if (facet is XmlSchemaEnumerationFacet xmlSchemaEnumerationFacet)
			{
				if (list2 == null)
				{
					list2 = new List<string>();
				}
				list2.Add(xmlSchemaEnumerationFacet.Value);
			}
			else if (facet is XmlSchemaMinExclusiveFacet xmlSchemaMinExclusiveFacet)
			{
				if (double.TryParse(xmlSchemaMinExclusiveFacet.Value, out var result))
				{
					list.Add(new NumericMinRule(result, inclusive: false));
				}
			}
			else if (facet is XmlSchemaMinInclusiveFacet xmlSchemaMinInclusiveFacet)
			{
				if (double.TryParse(xmlSchemaMinInclusiveFacet.Value, out var result2))
				{
					list.Add(new NumericMinRule(result2, inclusive: true));
				}
			}
			else if (facet is XmlSchemaMaxExclusiveFacet xmlSchemaMaxExclusiveFacet)
			{
				if (double.TryParse(xmlSchemaMaxExclusiveFacet.Value, out var result3))
				{
					list.Add(new NumericMaxRule(result3, inclusive: false));
				}
			}
			else if (facet is XmlSchemaMaxInclusiveFacet xmlSchemaMaxInclusiveFacet && double.TryParse(xmlSchemaMaxInclusiveFacet.Value, out result4))
			{
				list.Add(new NumericMaxRule(result4, inclusive: true));
			}
		}
		if (list2 != null && list2.Count > 0)
		{
			list.Add(new StringEnumRule(list2.ToArray()));
		}
		return list;
	}

	private List<XmlNode> GetAnnotation(XmlSchemaAnnotated annotated)
	{
		List<XmlNode> list = new List<XmlNode>();
		XmlSchemaAnnotation annotation = annotated.Annotation;
		if (annotation != null)
		{
			foreach (XmlSchemaObject item in annotation.Items)
			{
				if (!(item is XmlSchemaAppInfo { Markup: var markup }))
				{
					continue;
				}
				foreach (XmlNode xmlNode in markup)
				{
					if (xmlNode.NodeType != XmlNodeType.Comment)
					{
						list.Add(xmlNode);
					}
				}
			}
		}
		return list;
	}

	private void MergeRedefinedTypeAnnotations(XmlSchemaRedefine schemaRedefine)
	{
		Dictionary<XmlQualifiedName, XmlSchemaComplexType> dictionary = new Dictionary<XmlQualifiedName, XmlSchemaComplexType>();
		foreach (XmlSchemaObject item in schemaRedefine.Schema.Items)
		{
			if (item is XmlSchemaComplexType xmlSchemaComplexType)
			{
				dictionary.Add(xmlSchemaComplexType.QualifiedName, xmlSchemaComplexType);
			}
		}
		foreach (XmlSchemaObject item2 in schemaRedefine.Items)
		{
			if (item2 is XmlSchemaComplexType { QualifiedName: var qualifiedName } xmlSchemaComplexType2)
			{
				XmlSchemaComplexType xmlSchemaComplexType3 = dictionary[qualifiedName];
				if (xmlSchemaComplexType3.IsAbstract != xmlSchemaComplexType2.IsAbstract)
				{
					throw new InvalidOperationException("Type redefinition changes abstractness. (" + xmlSchemaComplexType3.Name + ")");
				}
				string key = qualifiedName.ToString();
				if (m_nodeTypes.TryGetValue(key, out var value) && m_annotations.TryGetValue(value, out var value2))
				{
					IEnumerable<XmlNode> enumerable = MergeRedefinedTypeAnnotations(GetAnnotation(xmlSchemaComplexType3), value2);
					m_annotations[value] = (enumerable as IList<XmlNode>) ?? enumerable.ToList();
				}
			}
		}
	}

	protected virtual IEnumerable<XmlNode> MergeRedefinedTypeAnnotations(IEnumerable<XmlNode> originalTypeAnnotations, IEnumerable<XmlNode> redefineTypeAnnotations)
	{
		List<XmlNode> list = new List<XmlNode>(redefineTypeAnnotations);
		list.AddRange(originalTypeAnnotations);
		return list;
	}

	protected virtual IEnumerable<XmlNode> MergeInheritedTypeAnnotations(IEnumerable<XmlNode> baseAnnotations, IEnumerable<XmlNode> derivedAnnotations)
	{
		return derivedAnnotations;
	}
}
