using System;
using System.Collections;
using System.ComponentModel;

namespace Firaxis.Controls;

public class DictionaryPropertyDescriptor : PropertyDescriptor
{
	private IDictionary m_Dictionary;

	private object m_Key;

	public override Type ComponentType => null;

	public override bool IsReadOnly => false;

	public override Type PropertyType => m_Dictionary[m_Key].GetType();

	public DictionaryPropertyDescriptor(IDictionary dictionary, object key)
		: this(dictionary, key, null)
	{
	}

	public DictionaryPropertyDescriptor(IDictionary dictionary, object key, Attribute[] attributes)
		: base(key.ToString(), attributes)
	{
		m_Dictionary = dictionary;
		m_Key = key;
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override object GetValue(object component)
	{
		return m_Dictionary[m_Key];
	}

	public override void ResetValue(object component)
	{
	}

	public override void SetValue(object component, object value)
	{
		m_Dictionary[m_Key] = value;
	}

	public override bool ShouldSerializeValue(object component)
	{
		return false;
	}
}
