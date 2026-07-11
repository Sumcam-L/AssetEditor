using System;

namespace Sce.Atf;

public class Cached<T>
{
	private bool m_dirty = true;

	private T m_value;

	private readonly Func<T> m_computeFunc;

	public T Value
	{
		get
		{
			if (m_dirty)
			{
				m_value = m_computeFunc();
				m_dirty = false;
			}
			return m_value;
		}
	}

	public Cached(Func<T> computeFunc)
	{
		m_dirty = true;
		m_computeFunc = computeFunc;
	}

	public Cached(Func<T> computeFunc, T currentValue)
	{
		m_dirty = false;
		m_computeFunc = computeFunc;
		m_value = currentValue;
	}

	public void Invalidate()
	{
		m_dirty = true;
	}
}
