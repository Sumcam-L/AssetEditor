using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;

namespace Firaxis.CivTech;

public class IOStringWrapper : IDisposable
{
	private unsafe char* m_convertedString;

	private unsafe char* m_string;

	private int m_length;

	public unsafe char* RawValue => m_string;

	public unsafe char* Value => m_convertedString;

	public unsafe IOStringWrapper(string @string)
	{
		//IL_00af: Expected I, but got I8
		m_string = (char*)Marshal.StringToHGlobalUni(@string).ToPointer();
		int num = (m_length = @string.Length);
		ulong num2 = 0uL;
		ulong num3 = (ulong)(num + 1);
		void* ptr = (m_convertedString = (char*)global::_003CModule_003E.new_005B_005D((num3 > long.MaxValue) ? ulong.MaxValue : (num3 * 2), (int)global::_003CModule_003E.Platform_002EGetMemBlockType(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GD_0040BHLCAKCL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 65, 23, 0));
		ulong num4 = (ulong)(m_length + 1);
		global::_003CModule_003E.Platform_002Ewchar_to_utf16((char*)ptr, num4, m_string, num4, &num2);
		if (!global::_003CModule_003E._003FA0xf1531103_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0IOStringWrapper_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && num2 != (ulong)m_length && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BM_0040GHGBFMGF_0040convertedLength_003F5_003F_0024DN_003F_0024DN_003F5m_length_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GD_0040BHLCAKCL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 68u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf1531103_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0IOStringWrapper_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	private void _007EIOStringWrapper()
	{
		_0021IOStringWrapper();
		GC.SuppressFinalize(this);
	}

	private unsafe void _0021IOStringWrapper()
	{
		//IL_0020: Expected I, but got I8
		//IL_0038: Expected I, but got I8
		char* ptr = m_string;
		if (ptr != null)
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			m_string = null;
		}
		char* convertedString = m_convertedString;
		if (convertedString != null)
		{
			global::_003CModule_003E.delete_005B_005D(convertedString);
			m_convertedString = null;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021IOStringWrapper();
			GC.SuppressFinalize(this);
			return;
		}
		try
		{
			_0021IOStringWrapper();
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

	~IOStringWrapper()
	{
		Dispose(A_0: false);
	}
}
