using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class InstanceDataFile : IInstanceDataFile, IDisposable
{
	private unsafe global::AssetObjects.InstanceDataFile* m_dataFile;

	private bool m_ownsPointer;

	public unsafe virtual string RelativePath
	{
		get
		{
			return new string(global::_003CModule_003E.AssetObjects_002EInstanceDataFile_002EGetRelativePath(m_dataFile));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EInstanceDataFile_002ESetRelativePath(m_dataFile, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe virtual string ID
	{
		get
		{
			return new string(global::_003CModule_003E.AssetObjects_002EInstanceDataFile_002EGetID(m_dataFile));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EInstanceDataFile_002ESetID(m_dataFile, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe InstanceDataFile(string ID, string RelativePath)
	{
		//IL_0049: Expected I, but got I8
		//IL_0060: Expected I, but got I8
		//IL_002e: Expected I, but got I8
		//IL_00b0: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.InstanceDataFile* ptr = (global::AssetObjects.InstanceDataFile*)global::_003CModule_003E.@new(56uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HM_0040LDLBGIPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 23, 0);
		global::AssetObjects.InstanceDataFile* dataFile;
		try
		{
			if (ptr != null)
			{
				global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)ptr);
				try
				{
					global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)((ulong)(nint)ptr + 24uL));
					try
					{
						*(long*)((ulong)(nint)ptr + 48uL) = 0L;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), (void*)((ulong)(nint)ptr + 24uL));
						throw;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), ptr);
					throw;
				}
				dataFile = ptr;
			}
			else
			{
				dataFile = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HM_0040LDLBGIPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 23, 0);
			throw;
		}
		m_dataFile = dataFile;
		m_ownsPointer = true;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x59f66340_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_00400_0040Z_00404_NA && m_dataFile == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040EJLMKIGF_0040m_dataFile_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HM_0040LDLBGIPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 20u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x59f66340_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_00400_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	public unsafe InstanceDataFile(global::AssetObjects.InstanceDataFile* pkDataFile)
	{
		//IL_003c: Expected I, but got I8
		m_dataFile = pkDataFile;
		m_ownsPointer = false;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x59f66340_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && m_dataFile == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040EJLMKIGF_0040m_dataFile_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HM_0040LDLBGIPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 13u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x59f66340_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	private void _007EInstanceDataFile()
	{
		RemoveReferences();
	}

	private void _0021InstanceDataFile()
	{
		RemoveReferences();
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0044: Expected I, but got I8
		//IL_001e: Expected I, but got I8
		if (m_ownsPointer)
		{
			global::AssetObjects.InstanceDataFile* dataFile = m_dataFile;
			if (dataFile != null)
			{
				global::AssetObjects.InstanceDataFile* ptr = dataFile;
				try
				{
					global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)((ulong)(nint)ptr + 24uL));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), ptr);
					throw;
				}
				global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)ptr);
				global::_003CModule_003E.delete(ptr, 56uL);
			}
		}
		m_dataFile = null;
	}

	internal unsafe global::AssetObjects.InstanceDataFile* GetAssetObject()
	{
		return m_dataFile;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EInstanceDataFile();
			return;
		}
		try
		{
			_0021InstanceDataFile();
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

	~InstanceDataFile()
	{
		Dispose(A_0: false);
	}
}
