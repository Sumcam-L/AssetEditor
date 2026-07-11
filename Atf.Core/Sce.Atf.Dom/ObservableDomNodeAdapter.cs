using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sce.Atf.Dom;

public abstract class ObservableDomNodeAdapter : DomNodeAdapter, INotifyPropertyChanged
{
	private static readonly Multimap<Type, DomNodeType> s_registeredTypes = new Multimap<Type, DomNodeType>();

	private Dictionary<ChildInfo, Dictionary<Type, IObservableCollection>> m_childListsCache;

	private bool m_hasTags;

	public event PropertyChangedEventHandler PropertyChanged
	{
		add
		{
			if (this.m_propertyChanged == null && m_hasTags)
			{
				Subscibe();
			}
			m_propertyChanged += value;
		}
		remove
		{
			m_propertyChanged -= value;
			if (this.m_propertyChanged == null)
			{
				Unsubscibe();
			}
		}
	}

	private event PropertyChangedEventHandler m_propertyChanged;

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Type type = GetType();
		lock (s_registeredTypes)
		{
			if (!s_registeredTypes.Find(type).Contains(base.DomNode.Type))
			{
				AddAttributeTags(type, base.DomNode.Type);
				AddChildTags(type, base.DomNode.Type);
				s_registeredTypes.Add(type, base.DomNode.Type);
			}
		}
		m_hasTags = HasTags();
	}

	public IObservableCollection<T> GetObservableChildList<T>(ChildInfo childInfo) where T : class
	{
		if (m_childListsCache == null)
		{
			m_childListsCache = new Dictionary<ChildInfo, Dictionary<Type, IObservableCollection>>();
		}
		IObservableCollection value2;
		lock (m_childListsCache)
		{
			if (!m_childListsCache.TryGetValue(childInfo, out var value))
			{
				value = new Dictionary<Type, IObservableCollection>();
				m_childListsCache.Add(childInfo, value);
			}
			if (!value.TryGetValue(typeof(T), out value2))
			{
				value2 = new ObservableDomNodeListAdapter<T>(base.DomNode, childInfo);
				value.Add(typeof(T), value2);
			}
		}
		return (IObservableCollection<T>)value2;
	}

	private bool HasTags()
	{
		return base.DomNode.Type.Attributes.Any((AttributeInfo attributeInfo) => attributeInfo.GetTag<PropertyChangedEventArgsCollection>() != null);
	}

	private static void AddAttributeTags(Type adapterType, DomNodeType nodeType)
	{
		PropertyInfo[] properties = adapterType.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(ObservableDomPropertyAttribute), inherit: true);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				continue;
			}
			PropertyChangedEventArgs item = new PropertyChangedEventArgs(propertyInfo.Name);
			object[] array = customAttributes;
			foreach (object obj in array)
			{
				string attributeName = ((ObservableDomPropertyAttribute)obj).AttributeName;
				DomNodeType domNodeType = nodeType;
				AttributeInfo attributeInfo = domNodeType.GetAttributeInfo(attributeName);
				Requires.NotNull(attributeInfo, "Unrecognized attribute name in ObservableDomPropertyAttribute");
				while (domNodeType != DomNodeType.BaseOfAllTypes && attributeInfo != null)
				{
					PropertyChangedEventArgsCollection propertyChangedEventArgsCollection = attributeInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
					if (propertyChangedEventArgsCollection == null)
					{
						propertyChangedEventArgsCollection = new PropertyChangedEventArgsCollection();
						attributeInfo.SetTag(propertyChangedEventArgsCollection);
					}
					propertyChangedEventArgsCollection.Add(item);
					domNodeType = domNodeType.BaseType;
					attributeInfo = domNodeType.GetAttributeInfo(attributeName);
				}
			}
		}
	}

	private static void AddChildTags(Type adapterType, DomNodeType nodeType)
	{
		PropertyInfo[] properties = adapterType.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(ObservableDomChildAttribute), inherit: true);
			if (customAttributes.Length == 0)
			{
				continue;
			}
			PropertyChangedEventArgs item = new PropertyChangedEventArgs(propertyInfo.Name);
			object[] array = customAttributes;
			foreach (object obj in array)
			{
				string childName = ((ObservableDomChildAttribute)obj).ChildName;
				DomNodeType domNodeType = nodeType;
				ChildInfo childInfo = domNodeType.GetChildInfo(childName);
				Requires.NotNull(childInfo, "Unrecognized childInfo in ObservableDomPropertyAttribute");
				while (domNodeType != DomNodeType.BaseOfAllTypes && childInfo != null)
				{
					PropertyChangedEventArgsCollection propertyChangedEventArgsCollection = childInfo.GetTag<PropertyChangedEventArgsCollection>();
					if (propertyChangedEventArgsCollection == null)
					{
						propertyChangedEventArgsCollection = new PropertyChangedEventArgsCollection();
						childInfo.SetTag(propertyChangedEventArgsCollection);
					}
					propertyChangedEventArgsCollection.Add(item);
					domNodeType = domNodeType.BaseType;
					childInfo = domNodeType.GetChildInfo(childName);
				}
			}
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.DomNode != base.DomNode)
		{
			return;
		}
		PropertyChangedEventArgsCollection tagLocal = e.AttributeInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
		if (tagLocal == null)
		{
			return;
		}
		foreach (PropertyChangedEventArgs item in tagLocal)
		{
			OnPropertyChanged(item);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Parent != base.DomNode)
		{
			return;
		}
		PropertyChangedEventArgsCollection tagLocal = e.ChildInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
		if (tagLocal == null)
		{
			return;
		}
		foreach (PropertyChangedEventArgs item in tagLocal)
		{
			OnPropertyChanged(item);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Parent != base.DomNode)
		{
			return;
		}
		PropertyChangedEventArgsCollection tagLocal = e.ChildInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
		if (tagLocal == null)
		{
			return;
		}
		foreach (PropertyChangedEventArgs item in tagLocal)
		{
			OnPropertyChanged(item);
		}
	}

	private void Subscibe()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void Unsubscibe()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.m_propertyChanged?.Invoke(this, e);
	}
}
