using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public abstract class NamedMetadata
{
	private readonly string m_name;

	private Dictionary<object, object> m_tags;

	public string Name => m_name;

	protected NamedMetadata(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_name = name;
	}

	public void SetTag(object key, object value)
	{
		if (m_tags == null)
		{
			m_tags = new Dictionary<object, object>();
		}
		m_tags[key] = value;
	}

	public void SetTag<T>(T value)
	{
		SetTag(typeof(T), value);
	}

	public object GetTagLocal(object key)
	{
		object value = null;
		if (m_tags != null)
		{
			m_tags.TryGetValue(key, out value);
		}
		return value;
	}

	public T GetTagLocal<T>()
	{
		if (!(GetTagLocal(typeof(T)) is T result))
		{
			return default(T);
		}
		return result;
	}

	public object GetTag(object key)
	{
		object obj = null;
		for (NamedMetadata namedMetadata = this; namedMetadata != null; namedMetadata = namedMetadata.GetParent())
		{
			obj = namedMetadata.GetTagLocal(key);
			if (obj != null)
			{
				break;
			}
		}
		return obj;
	}

	public T GetTag<T>()
	{
		if (!(GetTag(typeof(T)) is T result))
		{
			return default(T);
		}
		return result;
	}

	protected virtual NamedMetadata GetParent()
	{
		return null;
	}

	public override string ToString()
	{
		if (m_name != null)
		{
			return m_name;
		}
		return base.ToString();
	}
}
