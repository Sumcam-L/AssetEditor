using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace msclr.interop;

internal class context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E : context_node_base, IDisposable
{
	private IntPtr _ip;

	public unsafe context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E(char** _to_object, string _from_object)
	{
		//IL_0021: Expected I8, but got I
		IntPtr ip = Marshal.StringToHGlobalUni(_from_object);
		_ip = ip;
		*(long*)_to_object = (nint)_ip.ToPointer();
	}

	private void _007Econtext_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		_0021context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E();
	}

	private void _0021context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		if (_ip != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(_ip);
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E();
			return;
		}
		try
		{
			_0021context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E();
		}
		finally
		{
			
		}
	}

	public virtual void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~context_node_003Cwchar_t_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		Dispose(A_0: false);
	}
}
