using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech;

public class StandardStringWrapper : IDisposable
{
	private unsafe sbyte* m_string = null;

	public unsafe sbyte* Value => m_string;

	public unsafe StandardStringWrapper(string @string)
	{
		//IL_0008: Expected I, but got I8
		if (@string != null)
		{
			m_string = (sbyte*)Marshal.StringToHGlobalAnsi(@string).ToPointer();
		}
	}

	public unsafe StandardStringWrapper()
	{
	}//IL_0008: Expected I, but got I8


	private void _007EStandardStringWrapper()
	{
		_0021StandardStringWrapper();
		GC.SuppressFinalize(this);
	}

	private unsafe void _0021StandardStringWrapper()
	{
		//IL_0020: Expected I, but got I8
		sbyte* ptr = m_string;
		if (ptr != null)
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			m_string = null;
		}
	}

	public unsafe void AssignValue(string @string)
	{
		//IL_0020: Expected I, but got I8
		sbyte* ptr = m_string;
		if (ptr != null)
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			m_string = null;
		}
		if (@string != null)
		{
			m_string = (sbyte*)Marshal.StringToHGlobalAnsi(@string).ToPointer();
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021StandardStringWrapper();
			GC.SuppressFinalize(this);
			return;
		}
		try
		{
			_0021StandardStringWrapper();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~StandardStringWrapper()
	{
		Dispose(A_0: false);
	}
}
