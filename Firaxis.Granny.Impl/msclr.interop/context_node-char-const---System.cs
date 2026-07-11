using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using msclr.interop.details;

namespace msclr.interop;

internal class context_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E : context_node_base, IDisposable
{
	private unsafe sbyte* _ptr;

	public unsafe context_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E(sbyte** _to_object, string _from_object)
	{
		//IL_000e: Expected I, but got I8
		//IL_002d: Expected I8, but got I
		//IL_0042: Expected I, but got I8
		//IL_0046: Expected I, but got I8
		//IL_0055: Expected I8, but got I
		//IL_006c: Expected I, but got I8
		_ptr = null;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out char_buffer_003Cchar_003E char_buffer_003Cchar_003E2);
		if (_from_object == null)
		{
			*(long*)_to_object = 0L;
		}
		else
		{
			ulong num = global::_003CModule_003E.msclr_002Einterop_002Edetails_002EGetAnsiStringSize(_from_object);
			*(long*)(&char_buffer_003Cchar_003E2) = (nint)global::_003CModule_003E.new_005B_005D(num);
			try
			{
				if (*(long*)(&char_buffer_003Cchar_003E2) == 0L)
				{
					throw new InsufficientMemoryException();
				}
				global::_003CModule_003E.msclr_002Einterop_002Edetails_002EWriteAnsiString((sbyte*)(*(ulong*)(&char_buffer_003Cchar_003E2)), num, _from_object);
				sbyte* ptr = (sbyte*)(*(ulong*)(&char_buffer_003Cchar_003E2));
				*(long*)(&char_buffer_003Cchar_003E2) = 0L;
				_ptr = ptr;
				*(long*)_to_object = (nint)ptr;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<char_buffer_003Cchar_003E*, void>)(&global::_003CModule_003E.msclr_002Einterop_002Edetails_002Echar_buffer_003Cchar_003E_002E_007Bdtor_007D), &char_buffer_003Cchar_003E2);
				throw;
			}
			global::_003CModule_003E.delete_005B_005D(null);
		}
		try
		{
			return;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<char_buffer_003Cchar_003E*, void>)(&global::_003CModule_003E.msclr_002Einterop_002Edetails_002Echar_buffer_003Cchar_003E_002E_007Bdtor_007D), &char_buffer_003Cchar_003E2);
			throw;
		}
	}

	private unsafe void _007Econtext_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		global::_003CModule_003E.delete_005B_005D(_ptr);
	}

	private unsafe void _0021context_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		global::_003CModule_003E.delete_005B_005D(_ptr);
	}

	[HandleProcessCorruptedStateExceptions]
	protected unsafe virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			global::_003CModule_003E.delete_005B_005D(_ptr);
			return;
		}
		try
		{
			_0021context_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E();
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

	~context_node_003Cchar_0020const_0020_002A_002CSystem_003A_003AString_0020_005E_003E()
	{
		Dispose(A_0: false);
	}
}
