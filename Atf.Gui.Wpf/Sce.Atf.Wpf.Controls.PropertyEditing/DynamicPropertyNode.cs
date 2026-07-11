using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class DynamicPropertyNode : PropertyNode
{
	private PropertyDescriptor[] m_masterGroups = EmptyArray<PropertyDescriptor>.Instance;

	private GroupEnables[] m_groupEnableAttributes = EmptyArray<GroupEnables>.Instance;

	private PropertyDescriptor[] m_dependencyGroups = EmptyArray<PropertyDescriptor>.Instance;

	private bool m_groupDisable;

	public PropertyDescriptor[] MasterGroups
	{
		get
		{
			PropertyDescriptor[] array = new PropertyDescriptor[m_masterGroups.Length];
			m_masterGroups.CopyTo(array, 0);
			return array;
		}
	}

	public PropertyDescriptor[] DependencyGroups
	{
		get
		{
			PropertyDescriptor[] array = new PropertyDescriptor[m_dependencyGroups.Length];
			m_dependencyGroups.CopyTo(array, 0);
			return array;
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			return base.IsReadOnly || m_groupDisable;
		}
		set
		{
			base.IsReadOnly = value;
		}
	}

	public event EventHandler MasterGroupChanged;

	public event EventHandler DependencyGroupChanged;

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		PropertyDescriptorCollection propertyDescriptorCollection = null;
		GroupEnabledAttribute groupEnabledAttribute = base.Descriptor.Attributes.OfType<GroupEnabledAttribute>().FirstOrDefault();
		if (groupEnabledAttribute != null)
		{
			propertyDescriptorCollection = new PropertyDescriptorCollection(PropertyUtils.GetProperties(base.Instances.Cast<object>()).ToArray());
			m_groupEnableAttributes = groupEnabledAttribute.GroupEnables;
			List<GroupEnables> list = new List<GroupEnables>();
			List<PropertyDescriptor> list2 = new List<PropertyDescriptor>();
			GroupEnables[] groupEnables = groupEnabledAttribute.GroupEnables;
			foreach (GroupEnables groupEnables2 in groupEnables)
			{
				PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[groupEnables2.GroupName];
				if (propertyDescriptor != null)
				{
					list.Add(groupEnables2);
					list2.Add(propertyDescriptor);
				}
			}
			m_groupEnableAttributes = list.ToArray();
			m_masterGroups = list2.ToArray();
			SetGroupEnabledState();
		}
		DependencyAttribute dependencyAttribute = base.Descriptor.Attributes.OfType<DependencyAttribute>().FirstOrDefault();
		if (dependencyAttribute == null)
		{
			return;
		}
		if (propertyDescriptorCollection == null)
		{
			propertyDescriptorCollection = new PropertyDescriptorCollection(PropertyUtils.GetProperties(base.Instances.Cast<object>()).ToArray());
		}
		List<PropertyDescriptor> list3 = new List<PropertyDescriptor>();
		string[] dependencyDescriptors = dependencyAttribute.DependencyDescriptors;
		foreach (string name in dependencyDescriptors)
		{
			PropertyDescriptor propertyDescriptor2 = propertyDescriptorCollection[name];
			if (propertyDescriptor2 != null)
			{
				list3.Add(propertyDescriptor2);
			}
		}
		m_dependencyGroups = list3.ToArray();
	}

	protected override void SubscribeValueChanged(object instance)
	{
		base.SubscribeValueChanged(instance);
		PropertyDescriptor[] masterGroups = m_masterGroups;
		foreach (PropertyDescriptor pd in masterGroups)
		{
			ValueChangedEventManager.AddListener(instance, this, pd);
		}
		PropertyDescriptor[] dependencyGroups = m_dependencyGroups;
		foreach (PropertyDescriptor pd2 in dependencyGroups)
		{
			ValueChangedEventManager.AddListener(instance, this, pd2);
		}
	}

	protected override void UnsubscribeValueChanged(object instance)
	{
		base.UnsubscribeValueChanged(instance);
		PropertyDescriptor[] masterGroups = m_masterGroups;
		foreach (PropertyDescriptor pd in masterGroups)
		{
			ValueChangedEventManager.RemoveListener(instance, this, pd);
		}
		PropertyDescriptor[] dependencyGroups = m_dependencyGroups;
		foreach (PropertyDescriptor pd2 in dependencyGroups)
		{
			ValueChangedEventManager.RemoveListener(instance, this, pd2);
		}
	}

	protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		bool flag = base.OnReceiveWeakEvent(managerType, sender, e);
		if (!flag && managerType == typeof(ValueChangedEventManager))
		{
			ValueChangedEventArgs e2 = (ValueChangedEventArgs)e;
			if (m_masterGroups.Contains(e2.PropertyDescriptor))
			{
				OnMasterGroupPropertyValueChanged();
			}
			else if (m_dependencyGroups.Contains(e2.PropertyDescriptor))
			{
				OnDependecyGroupPropertyValueChanged();
			}
			flag = true;
		}
		return flag;
	}

	private void OnMasterGroupPropertyValueChanged()
	{
		SetGroupEnabledState();
		OnMasterGroupChanged();
		Refresh();
	}

	private void OnDependecyGroupPropertyValueChanged()
	{
		OnDependencyGroupChanged();
		Refresh();
	}

	protected virtual void OnMasterGroupChanged()
	{
		this.MasterGroupChanged.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnDependencyGroupChanged()
	{
		this.DependencyGroupChanged.Raise(this, EventArgs.Empty);
	}

	private void SetGroupEnabledState()
	{
		if (base.IsReadOnly)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < m_groupEnableAttributes.Length; i++)
		{
			GroupEnables attribute = m_groupEnableAttributes[i];
			PropertyDescriptor propertyDescriptor = m_masterGroups[i];
			if (propertyDescriptor != null)
			{
				object valueFromDescriptor = GetValueFromDescriptor(propertyDescriptor);
				if (valueFromDescriptor == null)
				{
					flag = true;
					break;
				}
				if (!GetGroupValues(propertyDescriptor, attribute).Contains(valueFromDescriptor))
				{
					flag = true;
					break;
				}
			}
		}
		if (m_groupDisable != flag)
		{
			m_groupDisable = flag;
			OnReadOnlyStateChanged();
		}
	}

	private IEnumerable<object> GetGroupValues(PropertyDescriptor masterGroup, GroupEnables attribute)
	{
		if (attribute.Values == null)
		{
			TypeConverter converter = masterGroup.Converter;
			if (converter != null && converter.CanConvertFrom(typeof(string)))
			{
				attribute.Values = attribute.StringValues.Select(converter.ConvertFromString).ToArray();
			}
			else
			{
				attribute.Values = attribute.StringValues;
			}
		}
		return attribute.Values;
	}

	private object GetValueFromDescriptor(PropertyDescriptor descriptor)
	{
		object obj = null;
		foreach (object instance in base.Instances)
		{
			object value = descriptor.GetValue(instance);
			if (obj == null)
			{
				obj = value;
			}
			if (obj != null && value == null)
			{
				return null;
			}
			if (value != null && !value.Equals(obj))
			{
				return null;
			}
		}
		return obj;
	}
}
