using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.Granny;

internal class GrannyLod : IGrannyLod, IDisposable
{
	private int m_targetIndexCount;

	private float m_transitionArea;

	private float m_reduction;

	public virtual float Reduction
	{
		get
		{
			return m_reduction;
		}
		set
		{
			m_reduction = value;
		}
	}

	public virtual float TransitionArea
	{
		get
		{
			return m_transitionArea;
		}
		set
		{
			m_transitionArea = value;
		}
	}

	public virtual int TargetIndexCount
	{
		get
		{
			return m_targetIndexCount;
		}
		set
		{
			m_targetIndexCount = value;
		}
	}

	private void _007EGrannyLod()
	{
	}

	private void _0021GrannyLod()
	{
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyLod();
			return;
		}
		try
		{
			_0021GrannyLod();
		}
		finally
		{
			
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyLod()
	{
		Dispose(A_0: false);
	}
}
