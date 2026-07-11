using System.Collections;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ContentExporters;

public class ImportOperationResultCollection : IEnumerable<ImportOperationResult>, IEnumerable, IDictionary<IImportedEntity, ImportOperationResult>, ICollection<KeyValuePair<IImportedEntity, ImportOperationResult>>, IEnumerable<KeyValuePair<IImportedEntity, ImportOperationResult>>
{
	private readonly IDictionary<IImportedEntity, ImportOperationResult> m_dictionary;

	public ICollection<IImportedEntity> Keys => m_dictionary.Keys;

	public ICollection<ImportOperationResult> Values => m_dictionary.Values;

	public ImportOperationResult this[IImportedEntity key]
	{
		get
		{
			return m_dictionary[key];
		}
		set
		{
			m_dictionary[key] = value;
		}
	}

	public int Count => m_dictionary.Count;

	public bool IsReadOnly => m_dictionary.IsReadOnly;

	public ImportOperationResultCollection(IEnumerable<IImportedEntity> entities)
	{
		m_dictionary = new Dictionary<IImportedEntity, ImportOperationResult>();
		foreach (IImportedEntity entity in entities)
		{
			Add(new ImportOperationResult(entity));
		}
	}

	public void Add(ImportOperationResult item)
	{
		Add(item.Entity, item);
	}

	public void Add(IImportedEntity key, ImportOperationResult value)
	{
		if (!m_dictionary.ContainsKey(key))
		{
			m_dictionary.Add(key, value);
		}
	}

	public bool ContainsKey(IImportedEntity key)
	{
		return m_dictionary.ContainsKey(key);
	}

	public bool Remove(IImportedEntity key)
	{
		return m_dictionary.Remove(key);
	}

	public bool TryGetValue(IImportedEntity key, out ImportOperationResult value)
	{
		return m_dictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<IImportedEntity, ImportOperationResult> item)
	{
		if (!m_dictionary.ContainsKey(item.Key))
		{
			m_dictionary.Add(item);
		}
	}

	public bool Contains(KeyValuePair<IImportedEntity, ImportOperationResult> item)
	{
		return m_dictionary.Contains(item);
	}

	public void CopyTo(KeyValuePair<IImportedEntity, ImportOperationResult>[] array, int arrayIndex)
	{
		m_dictionary.CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<IImportedEntity, ImportOperationResult> item)
	{
		return m_dictionary.Remove(item);
	}

	IEnumerator<KeyValuePair<IImportedEntity, ImportOperationResult>> IEnumerable<KeyValuePair<IImportedEntity, ImportOperationResult>>.GetEnumerator()
	{
		return m_dictionary.GetEnumerator();
	}

	public void Clear()
	{
		m_dictionary.Clear();
	}

	IEnumerator<ImportOperationResult> IEnumerable<ImportOperationResult>.GetEnumerator()
	{
		return m_dictionary.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_dictionary.Values.GetEnumerator();
	}
}
