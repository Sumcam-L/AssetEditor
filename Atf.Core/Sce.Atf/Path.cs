using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf;

public class Path<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<Path<T>>
{
	private readonly T[] m_path;

	public T First
	{
		get
		{
			return m_path[0];
		}
		set
		{
			m_path[0] = value;
		}
	}

	public T Last
	{
		get
		{
			return m_path[m_path.Length - 1];
		}
		set
		{
			m_path[m_path.Length - 1] = value;
		}
	}

	public T this[int index]
	{
		get
		{
			return m_path[index];
		}
		set
		{
			m_path[index] = value;
		}
	}

	public int Count => m_path.Length;

	public bool IsReadOnly => false;

	public Path(T last)
	{
		m_path = new T[1];
		m_path[0] = last;
	}

	public Path(IEnumerable<T> path)
	{
		m_path = path.ToArray();
	}

	public Path(ICollection<T> path)
	{
		m_path = new T[path.Count];
		path.CopyTo(m_path, 0);
	}

	public Path<T> Prefix(int length)
	{
		CheckSubPathLength(length);
		T[] array = new T[length];
		Array.Copy(m_path, 0, array, 0, length);
		return new Path<T>(array);
	}

	public Path<T> Suffix(int length)
	{
		CheckSubPathLength(length);
		T[] array = new T[length];
		int sourceIndex = m_path.Length - length;
		Array.Copy(m_path, sourceIndex, array, 0, length);
		return new Path<T>(array);
	}

	public Path<U> Convert<U>() where U : class
	{
		U[] array = new U[m_path.Length];
		for (int i = 0; i < m_path.Length; i++)
		{
			array[i] = Convert<U>(m_path[i]);
		}
		return new Path<U>(array);
	}

	protected virtual U Convert<U>(T item) where U : class
	{
		return item as U;
	}

	public bool Equals(Path<T> other)
	{
		if (object.Equals(other, null))
		{
			return false;
		}
		if (m_path.Length != other.m_path.Length)
		{
			return false;
		}
		for (int i = 0; i < m_path.Length; i++)
		{
			ref readonly T reference = ref m_path[i];
			object obj = other.m_path[i];
			if (!reference.Equals(obj))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Path<T> path = obj as Path<T>;
		if (path != null)
		{
			return Equals(path);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		T[] path = m_path;
		for (int i = 0; i < path.Length; i++)
		{
			T val = path[i];
			num ^= val.GetHashCode();
		}
		return num;
	}

	public static bool operator ==(Path<T> o1, Path<T> o2)
	{
		if (object.Equals(o1, null))
		{
			return object.Equals(o2, null);
		}
		return o1.Equals(o2);
	}

	public static bool operator !=(Path<T> o1, Path<T> o2)
	{
		if (object.Equals(o1, null))
		{
			return !object.Equals(o2, null);
		}
		return !o1.Equals(o2);
	}

	public static Path<T> operator +(T lhs, Path<T> rhs)
	{
		if (rhs == null)
		{
			return new Path<T>(lhs);
		}
		T[] array = new T[1 + rhs.Count];
		array[0] = lhs;
		Array.Copy(rhs.m_path, 0, array, 1, rhs.Count);
		return new Path<T>(array);
	}

	public static Path<T> operator +(Path<T> lhs, T rhs)
	{
		if (lhs == null)
		{
			return new Path<T>(rhs);
		}
		T[] array = new T[lhs.Count + 1];
		Array.Copy(lhs.m_path, 0, array, 0, lhs.Count);
		array[array.Length - 1] = rhs;
		return new Path<T>(array);
	}

	public static Path<T> operator +(Path<T> lhs, Path<T> rhs)
	{
		if (lhs == null)
		{
			return rhs;
		}
		if (rhs == null)
		{
			return lhs;
		}
		T[] array = new T[lhs.Count + rhs.Count];
		Array.Copy(lhs.m_path, 0, array, 0, lhs.Count);
		Array.Copy(rhs.m_path, 0, array, lhs.Count, rhs.Count);
		return new Path<T>(array);
	}

	public static IEnumerable<T> GetLastItems(IEnumerable<Path<T>> paths)
	{
		foreach (Path<T> path in paths)
		{
			yield return path.Last;
		}
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < m_path.Length; i++)
		{
			if (m_path[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public void Add(T item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		T[] path = m_path;
		for (int i = 0; i < path.Length; i++)
		{
			T val = path[i];
			if (val.Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_path.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)m_path).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_path.GetEnumerator();
	}

	private Path(T[] path)
	{
		m_path = path;
	}

	private void CheckSubPathLength(int length)
	{
		if (length < 1)
		{
			throw new InvalidOperationException("Length must be > 0");
		}
		if (length > m_path.Length)
		{
			throw new InvalidOperationException("Length greater than path length");
		}
	}
}
