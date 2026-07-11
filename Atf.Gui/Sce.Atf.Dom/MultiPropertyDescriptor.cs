using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Dom;

public class MultiPropertyDescriptor : PropertyDescriptor
{
	private readonly string m_key;

	private readonly Dictionary<object, System.ComponentModel.PropertyDescriptor> m_descriptorMap = new Dictionary<object, System.ComponentModel.PropertyDescriptor>();

	public Func<IEnumerable<object>> GetSelectionFunc { private get; set; }

	public MultiPropertyDescriptor(System.ComponentModel.PropertyDescriptor masterDescriptor)
		: base(masterDescriptor.Name, masterDescriptor.PropertyType, masterDescriptor.Category, masterDescriptor.Description, masterDescriptor.IsReadOnly, masterDescriptor.GetEditor(typeof(object)), masterDescriptor.Converter, masterDescriptor.Attributes.Cast<Attribute>().ToArray())
	{
		m_key = masterDescriptor.GetPropertyDescriptorKey();
	}

	public override bool CanResetValue(object component)
	{
		return FindDescriptor(component)?.CanResetValue(component) ?? false;
	}

	public bool CanResetValues()
	{
		foreach (object item in GetSelectionFunc())
		{
			if (CanResetValue(item))
			{
				return true;
			}
		}
		return false;
	}

	public override void ResetValue(object component)
	{
		FindDescriptor(component)?.ResetValue(component);
	}

	public void ResetValues()
	{
		foreach (object item in GetSelectionFunc())
		{
			if (CanResetValue(item))
			{
				ResetValue(item);
			}
		}
	}

	public override void SetValue(object component, object value)
	{
		FindDescriptor(component)?.SetValue(component, value);
	}

	public void SetValues(object value)
	{
		foreach (object item in GetSelectionFunc())
		{
			SetValue(item, value);
		}
	}

	public override object GetValue(object component)
	{
		return FindDescriptor(component)?.GetValue(component);
	}

	public IEnumerable<object> GetValues()
	{
		foreach (object component in GetSelectionFunc())
		{
			yield return GetValue(component);
		}
	}

	private System.ComponentModel.PropertyDescriptor FindDescriptor(object component)
	{
		if (component == null)
		{
			return null;
		}
		if (m_descriptorMap.TryGetValue(component, out var value))
		{
			return value;
		}
		value = PropertyUtils.FindPropertyDescriptor(component, m_key);
		m_descriptorMap.Add(component, value);
		return value;
	}
}
