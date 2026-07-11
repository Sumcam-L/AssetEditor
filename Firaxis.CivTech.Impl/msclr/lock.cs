using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace msclr;

internal class @lock : IDisposable
{
	private object m_object;

	private bool m_locked;

    public @lock(object _object)
    {
        this.m_object = _object ?? throw new ArgumentNullException(nameof(_object));
        this.acquire(-1);
    }

    private void _007Elock()
	{
		release();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool is_locked()
	{
		return m_locked;
	}

    public override string ToString()
    {
        return (!m_locked) ? _detail_class._safe_false : _detail_class._safe_true;
    }

    public void acquire(TimeSpan _timeout)
	{
		if (!m_locked)
		{
			Monitor.TryEnter(m_object, _timeout, ref m_locked);
			if (!m_locked)
			{
				throw Marshal.GetExceptionForHR(-2147024638);
			}
		}
	}

	public void acquire()
	{
		if (!m_locked)
		{
			Monitor.TryEnter(m_object, -1, ref m_locked);
			if (!m_locked)
			{
				throw Marshal.GetExceptionForHR(-2147024638);
			}
		}
	}

	public void acquire(int _timeout)
	{
		if (!m_locked)
		{
			Monitor.TryEnter(m_object, _timeout, ref m_locked);
			if (!m_locked)
			{
				throw Marshal.GetExceptionForHR(-2147024638);
			}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool try_acquire(TimeSpan _timeout)
	{
		if (!m_locked)
		{
			Monitor.TryEnter(m_object, _timeout, ref m_locked);
			if (!m_locked)
			{
				return false;
			}
		}
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool try_acquire(int _timeout)
	{
		if (!m_locked)
		{
			Monitor.TryEnter(m_object, _timeout, ref m_locked);
			if (!m_locked)
			{
				return false;
			}
		}
		return true;
	}

	public void release()
	{
		if (m_locked)
		{
			Monitor.Exit(m_object);
			m_locked = false;
		}
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
        if (A_0)
        {
            release();
        }
    }

    public void Dispose()
    {
        Dispose(A_0: true);
        GC.SuppressFinalize(this);
    }
}
