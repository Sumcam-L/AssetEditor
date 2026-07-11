using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class DomNodeType : NamedMetadata, IEquatable<DomNodeType>
{
	private class Definitions
	{
		public readonly List<AttributeInfo> Attributes = new List<AttributeInfo>();

		public readonly List<ChildInfo> Children = new List<ChildInfo>();

		public readonly List<ExtensionInfo> Extensions = new List<ExtensionInfo>();
	}

	private class ExtensionAdapterCreator : IAdapterCreator
	{
		private readonly ExtensionInfo m_extensionInfo;

		public ExtensionAdapterCreator(ExtensionInfo extensionInfo)
		{
			m_extensionInfo = extensionInfo;
		}

		public bool CanAdapt(object adaptee, Type type)
		{
			return adaptee is DomNode && type != null && type.IsAssignableFrom(m_extensionInfo.Type);
		}

		public object GetAdapter(object adaptee, Type type)
		{
			if (adaptee is DomNode domNode && type.IsAssignableFrom(m_extensionInfo.Type))
			{
				IAdapter adapter = (IAdapter)domNode.GetExtension(m_extensionInfo);
				if (adapter.Adaptee == null)
				{
					adapter.Adaptee = domNode;
				}
				return adapter;
			}
			return null;
		}
	}

	private class StringIndex
	{
		private struct StringInfo : IComparable<StringInfo>
		{
			public readonly string String;

			public readonly int Index;

			public bool Duplicate;

			public StringInfo(string s, int index)
			{
				String = s;
				Index = index;
				Duplicate = false;
			}

			public int CompareTo(StringInfo other)
			{
				return String.CompareTo(other.String);
			}
		}

		public static readonly StringIndex Empty = new StringIndex(EmptyArray<FieldMetadata>.Instance);

		private readonly StringInfo[] m_strings;

		public StringIndex(IList<FieldMetadata> fields)
		{
			m_strings = new StringInfo[fields.Count];
			int num = 0;
			foreach (FieldMetadata field in fields)
			{
				m_strings[num] = new StringInfo(field.Name, num);
				num++;
			}
			Array.Sort(m_strings);
			if (m_strings.Length < 2)
			{
				return;
			}
			string value = m_strings[0].String;
			for (num = 1; num < m_strings.Length; num++)
			{
				string text = m_strings[num].String;
				if (text.Equals(value))
				{
					m_strings[num].Duplicate = true;
				}
				value = text;
			}
		}

		public int FindIndex(string searchString)
		{
			int num = 0;
			int num2 = m_strings.Length;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = m_strings[num3].String.CompareTo(searchString);
				if (num4 < 0)
				{
					num = num3 + 1;
					continue;
				}
				if (num4 > 0)
				{
					num2 = num3;
					continue;
				}
				if (m_strings[num3].Duplicate)
				{
					throw new InvalidOperationException("FieldMetaData named '" + searchString + "' was not unique on its DomNodeType");
				}
				return m_strings[num3].Index;
			}
			return -1;
		}
	}

	internal int FieldCount;

	internal int FirstChildIndex;

	internal int FirstExtensionIndex;

	internal bool IsFrozen;

	private DomNodeType m_baseType;

	private AttributeInfo[] m_attributes;

	private StringIndex m_attributeIndex;

	private ChildInfo[] m_children;

	private StringIndex m_childIndex;

	private ExtensionInfo[] m_extensions;

	private StringIndex m_extensionIndex;

	private AttributeInfo m_idAttribute;

	private Definitions m_definitions = new Definitions();

	private List<IAdapterCreator> m_adapterCreators;

	private Dictionary<Type, IEnumerable<IAdapterCreator>> m_adapterCreatorCache = new Dictionary<Type, IEnumerable<IAdapterCreator>>();

	private bool m_isAbstract;

	private static readonly DomNodeType s_baseOfAllTypes;

	public DomNodeType BaseType
	{
		get
		{
			return m_baseType;
		}
		set
		{
			if (m_attributes != null || m_children != null || m_extensions != null)
			{
				throw new InvalidOperationException("Can't change base type once any fields are frozen");
			}
			SetBaseType(value);
		}
	}

	public static DomNodeType BaseOfAllTypes => s_baseOfAllTypes;

	public IEnumerable<DomNodeType> Lineage
	{
		get
		{
			for (DomNodeType type = this; type != null; type = type.BaseType)
			{
				yield return type;
			}
		}
	}

	public IEnumerable<AttributeInfo> Attributes
	{
		get
		{
			if (m_attributes == null)
			{
				FreezeAttributes();
			}
			return m_attributes;
		}
	}

	public AttributeInfo IdAttribute
	{
		get
		{
			for (DomNodeType domNodeType = this; domNodeType != null; domNodeType = domNodeType.m_baseType)
			{
				if (domNodeType.m_idAttribute != null)
				{
					return domNodeType.m_idAttribute;
				}
			}
			return null;
		}
	}

	public IEnumerable<ChildInfo> Children
	{
		get
		{
			if (m_children == null)
			{
				FreezeChildren();
			}
			return m_children;
		}
	}

	public IEnumerable<ExtensionInfo> Extensions
	{
		get
		{
			if (m_extensions == null)
			{
				FreezeExtensions();
			}
			return m_extensions;
		}
	}

	public bool IsAbstract
	{
		get
		{
			return m_isAbstract;
		}
		set
		{
			m_isAbstract = value;
		}
	}

	public DomNodeType(string name)
		: base(name)
	{
		m_baseType = s_baseOfAllTypes;
	}

	public DomNodeType(string name, DomNodeType baseType, IEnumerable<AttributeInfo> attributes, IEnumerable<ChildInfo> children, IEnumerable<ExtensionInfo> extensions)
		: base(name)
	{
		SetBaseType(baseType);
		if (attributes != null)
		{
			m_definitions.Attributes.AddRange(attributes);
		}
		if (children != null)
		{
			m_definitions.Children.AddRange(children);
		}
		if (extensions != null)
		{
			m_definitions.Extensions.AddRange(extensions);
		}
	}

	public DomNodeType(string name, DomNodeType baseType, params FieldMetadata[] metadata)
		: base(name)
	{
		SetBaseType(baseType);
		foreach (FieldMetadata fieldMetadata in metadata)
		{
			if (fieldMetadata is AttributeInfo)
			{
				m_definitions.Attributes.Add((AttributeInfo)fieldMetadata);
			}
			else if (fieldMetadata is ChildInfo)
			{
				m_definitions.Children.Add((ChildInfo)fieldMetadata);
			}
			else if (fieldMetadata is ExtensionInfo)
			{
				m_definitions.Extensions.Add((ExtensionInfo)fieldMetadata);
			}
		}
	}

	public void Define(AttributeInfo attributeInfo)
	{
		if (m_attributes != null)
		{
			throw new InvalidOperationException("Attributes frozen");
		}
		m_definitions.Attributes.Add(attributeInfo);
	}

	public void Define(ChildInfo childInfo)
	{
		if (m_children != null)
		{
			throw new InvalidOperationException("Children frozen");
		}
		m_definitions.Children.Add(childInfo);
	}

	public void Define(ExtensionInfo extensionInfo)
	{
		if (m_extensions != null)
		{
			throw new InvalidOperationException("Extensions frozen");
		}
		m_definitions.Extensions.Add(extensionInfo);
	}

	public void AddAdapterCreator(IAdapterCreator creator)
	{
		if (m_extensions == null)
		{
			FreezeExtensions();
		}
		AddCreator(creator);
	}

	private void SetBaseType(DomNodeType baseType)
	{
		if (baseType == null)
		{
			baseType = s_baseOfAllTypes;
		}
		m_baseType = baseType;
	}

	internal AttributeInfo GetAttributeInfo(int index)
	{
		if (m_attributes == null)
		{
			FreezeAttributes();
		}
		if (index < m_attributes.Length)
		{
			return m_attributes[index];
		}
		return null;
	}

	public void SetIdAttribute(string name)
	{
		m_idAttribute = GetAttributeInfo(name);
	}

	public void SetIdAttribute(AttributeInfo idAttribute)
	{
		if (!IsValid(idAttribute))
		{
			throw new InvalidOperationException("invalid attribute info");
		}
		m_idAttribute = idAttribute;
	}

	internal ChildInfo GetChildInfo(int index)
	{
		if (m_children == null)
		{
			FreezeChildren();
		}
		if (index < m_children.Length)
		{
			return m_children[index];
		}
		return null;
	}

	internal ExtensionInfo GetExtensionInfo(int index)
	{
		if (m_extensions == null)
		{
			FreezeExtensions();
		}
		if (index < m_extensions.Length)
		{
			return m_extensions[index];
		}
		return null;
	}

	public bool IsAssignableFrom(DomNodeType type)
	{
		for (DomNodeType domNodeType = type; domNodeType != null; domNodeType = domNodeType.BaseType)
		{
			if (this == domNodeType)
			{
				return true;
			}
		}
		return false;
	}

	public AttributeInfo GetAttributeInfo(string name)
	{
		if (m_attributes == null)
		{
			FreezeAttributes();
		}
		int num = m_attributeIndex.FindIndex(name);
		return (num >= 0) ? m_attributes[num] : null;
	}

	public ChildInfo GetChildInfo(string name)
	{
		if (m_children == null)
		{
			FreezeChildren();
		}
		int num = m_childIndex.FindIndex(name);
		return (num >= 0) ? m_children[num] : null;
	}

	public ChildInfo GetDescendantInfo(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		ChildInfo childInfo = null;
		DomNodeType domNodeType = this;
		string[] array = path.Split(':');
		string[] array2 = array;
		foreach (string name in array2)
		{
			childInfo = domNodeType.GetChildInfo(name);
			if (childInfo == null)
			{
				break;
			}
			domNodeType = childInfo.Type;
		}
		return childInfo;
	}

	public ExtensionInfo GetExtensionInfo(string name)
	{
		if (m_extensions == null)
		{
			FreezeExtensions();
		}
		int num = m_extensionIndex.FindIndex(name);
		return (num >= 0) ? m_extensions[num] : null;
	}

	public override string ToString()
	{
		return base.Name;
	}

	public bool IsValid(AttributeInfo attributeInfo)
	{
		if (m_attributes == null)
		{
			FreezeAttributes();
		}
		return TryGetDataIndex(attributeInfo) >= 0;
	}

	internal int GetDataIndex(AttributeInfo attributeInfo)
	{
		int num = TryGetDataIndex(attributeInfo);
		if (num < 0)
		{
			throw new InvalidOperationException($"attributeInfo \"{attributeInfo.Name}\" doesn't belong to node type \"{base.Name}\"");
		}
		return num;
	}

	private int TryGetDataIndex(AttributeInfo attributeInfo)
	{
		if (attributeInfo == null)
		{
			throw new ArgumentNullException("attributeInfo");
		}
		int index = attributeInfo.Index;
		if (index < 0 || index >= m_attributes.Length)
		{
			return -1;
		}
		if (m_attributes[index].DefiningType == attributeInfo.DefiningType)
		{
			return index;
		}
		if (m_attributes[index].DefiningType.Name == attributeInfo.DefiningType.Name)
		{
			return index;
		}
		return -1;
	}

	public bool IsValid(ChildInfo childInfo)
	{
		if (m_children == null)
		{
			FreezeChildren();
		}
		return TryGetDataIndex(childInfo) >= 0;
	}

	internal int GetDataIndex(ChildInfo childInfo)
	{
		int num = TryGetDataIndex(childInfo);
		if (num < 0)
		{
			throw new InvalidOperationException("childInfo doesn't belong to node type");
		}
		return num;
	}

	internal int TryGetDataIndex(ChildInfo childInfo)
	{
		if (childInfo == null)
		{
			throw new ArgumentNullException("childInfo");
		}
		int index = childInfo.Index;
		if (index < m_children.Length && m_children[index].DefiningType == childInfo.DefiningType)
		{
			return index + FirstChildIndex;
		}
		return -1;
	}

	public bool IsValid(ExtensionInfo extensionInfo)
	{
		if (m_extensions == null)
		{
			FreezeExtensions();
		}
		return TryGetDataIndex(extensionInfo) >= 0;
	}

	internal int GetDataIndex(ExtensionInfo extensionInfo)
	{
		int num = TryGetDataIndex(extensionInfo);
		if (num < 0)
		{
			throw new InvalidOperationException("extensionInfo doesn't belong to node type");
		}
		return num;
	}

	internal int TryGetDataIndex(ExtensionInfo extensionInfo)
	{
		if (extensionInfo == null)
		{
			throw new ArgumentNullException("extensionInfo");
		}
		int index = extensionInfo.Index;
		if (index < m_extensions.Length && m_extensions[index].DefiningType == extensionInfo.DefiningType)
		{
			return index + FirstExtensionIndex;
		}
		return -1;
	}

	protected override NamedMetadata GetParent()
	{
		return m_baseType;
	}

	internal void Freeze()
	{
		if (m_attributes == null)
		{
			FreezeAttributes();
		}
		if (m_children == null)
		{
			FreezeChildren();
		}
		if (m_extensions == null)
		{
			FreezeExtensions();
		}
		IsFrozen = true;
		m_definitions = null;
		FirstChildIndex = m_attributes.Length;
		FirstExtensionIndex = FirstChildIndex + m_children.Length;
		FieldCount = FirstExtensionIndex + m_extensions.Length;
	}

	private void FreezeAttributes()
	{
		List<AttributeInfo> list = new List<AttributeInfo>();
		if (m_baseType != null)
		{
			list.AddRange(m_baseType.Attributes);
		}
		foreach (AttributeInfo attribute in m_definitions.Attributes)
		{
			attribute.OwningType = this;
			AttributeInfo attributeInfo = m_baseType.GetAttributeInfo(attribute.Name);
			if (attributeInfo != null)
			{
				attribute.Index = attributeInfo.Index;
				attribute.DefiningType = attributeInfo.DefiningType;
				list[attributeInfo.Index] = attribute;
			}
			else
			{
				attribute.Index = list.Count;
				attribute.DefiningType = this;
				list.Add(attribute);
			}
		}
		if (list.Count > 0)
		{
			m_attributes = list.ToArray();
			m_attributeIndex = new StringIndex(m_attributes);
		}
		else
		{
			m_attributes = EmptyArray<AttributeInfo>.Instance;
			m_attributeIndex = StringIndex.Empty;
		}
	}

	private void FreezeChildren()
	{
		List<ChildInfo> list = new List<ChildInfo>();
		if (m_baseType != null)
		{
			list.AddRange(m_baseType.Children);
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ChildInfo child in m_definitions.Children)
		{
			child.OwningType = this;
			ChildInfo childInfo = m_baseType.GetChildInfo(child.Name);
			if (childInfo != null)
			{
				child.Index = childInfo.Index;
				child.DefiningType = childInfo.DefiningType;
				list[childInfo.Index] = child;
			}
			else if (hashSet.Add(child.Name))
			{
				child.Index = list.Count;
				child.DefiningType = this;
				list.Add(child);
				hashSet.Add(child.Name);
			}
		}
		if (list.Count > 0)
		{
			m_children = list.ToArray();
			m_childIndex = new StringIndex(m_children);
		}
		else
		{
			m_children = EmptyArray<ChildInfo>.Instance;
			m_childIndex = StringIndex.Empty;
		}
	}

	private void FreezeExtensions()
	{
		List<ExtensionInfo> list = new List<ExtensionInfo>();
		if (m_baseType != null)
		{
			list.AddRange(m_baseType.Extensions);
		}
		foreach (ExtensionInfo extension in m_definitions.Extensions)
		{
			extension.OwningType = this;
			ExtensionInfo extensionInfo = null;
			if (m_baseType != null)
			{
				extensionInfo = m_baseType.GetExtensionInfo(extension.Name);
			}
			if (extensionInfo != null)
			{
				extension.Index = extensionInfo.Index;
				extension.DefiningType = extensionInfo.DefiningType;
				list[extensionInfo.Index] = extension;
			}
			else
			{
				extension.Index = list.Count;
				extension.DefiningType = this;
				list.Add(extension);
			}
		}
		if (list.Count > 0)
		{
			m_extensions = list.ToArray();
			m_extensionIndex = new StringIndex(m_extensions);
		}
		else
		{
			m_extensions = EmptyArray<ExtensionInfo>.Instance;
			m_extensionIndex = StringIndex.Empty;
		}
		ExtensionInfo[] extensions = m_extensions;
		foreach (ExtensionInfo extensionInfo2 in extensions)
		{
			if (extensionInfo2.DefiningType == this && typeof(IAdapter).IsAssignableFrom(extensionInfo2.Type))
			{
				AddCreator(new ExtensionAdapterCreator(extensionInfo2));
			}
		}
	}

	private void AddCreator(IAdapterCreator creator)
	{
		if (m_adapterCreators == null)
		{
			m_adapterCreators = new List<IAdapterCreator>();
		}
		m_adapterCreators.Add(creator);
	}

	internal static object GetAdapter(DomNode node, Type type)
	{
		IEnumerable<IAdapterCreator> adapterCreators = GetAdapterCreators(node, type);
		using (IEnumerator<IAdapterCreator> enumerator = adapterCreators.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IAdapterCreator current = enumerator.Current;
				return current.GetAdapter(node, type);
			}
		}
		return null;
	}

	internal static IEnumerable<object> GetAdapters(DomNode node, Type type)
	{
		IEnumerable<IAdapterCreator> adapterCreators = GetAdapterCreators(node, type);
		foreach (IAdapterCreator adapterCreator in adapterCreators)
		{
			yield return adapterCreator.GetAdapter(node, type);
		}
	}

	private static IEnumerable<IAdapterCreator> GetAdapterCreators(DomNode node, Type type)
	{
		DomNodeType domNodeType = node.Type;
		IEnumerable<IAdapterCreator> value = null;
		IDictionary<Type, IEnumerable<IAdapterCreator>> adapterCreatorCache = domNodeType.m_adapterCreatorCache;
		lock (adapterCreatorCache)
		{
			if (!adapterCreatorCache.TryGetValue(type, out value))
			{
				List<IAdapterCreator> list = new List<IAdapterCreator>();
				while (domNodeType != null)
				{
					if (domNodeType.m_adapterCreators != null)
					{
						foreach (IAdapterCreator adapterCreator in domNodeType.m_adapterCreators)
						{
							if (adapterCreator.CanAdapt(node, type))
							{
								list.Add(adapterCreator);
							}
						}
					}
					domNodeType = domNodeType.BaseType;
				}
				value = ((list.Count != 0) ? list.ToArray() : Enumerable.Empty<IAdapterCreator>());
				adapterCreatorCache.Add(type, value);
			}
		}
		return value;
	}

	public override int GetHashCode()
	{
		return base.Name.GetHashCode();
	}

	public bool Equals(DomNodeType other)
	{
		if (BaseType == null && other.BaseType == null)
		{
			return other.Name.Equals(base.Name);
		}
		return other.BaseType.Equals(BaseType) && other.Name.Equals(base.Name);
	}

	public override bool Equals(object obj)
	{
		if (obj is DomNodeType other)
		{
			return Equals(other);
		}
		return false;
	}

	static DomNodeType()
	{
		s_baseOfAllTypes = new DomNodeType("Sce.Atf.Dom.Object");
		s_baseOfAllTypes.m_baseType = null;
	}
}
