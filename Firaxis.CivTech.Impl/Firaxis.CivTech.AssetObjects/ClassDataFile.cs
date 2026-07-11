using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ClassDataFile : IClassDataFile
{
	private unsafe global::AssetObjects.ClassDataFile* m_classDataFile;

	public unsafe virtual bool IsGenerated
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EClassDataFile_002EIsGenerated(m_classDataFile);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EClassDataFile_002ESetIsGenerated(m_classDataFile, value);
		}
	}

	public unsafe virtual string Extension
	{
		get
		{
			return new string(global::_003CModule_003E.AssetObjects_002EClassDataFile_002EGetExtension(m_classDataFile));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EClassDataFile_002ESetExtension(m_classDataFile, standardStringWrapper.Value);
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
			return new string(global::_003CModule_003E.AssetObjects_002EClassDataFile_002EGetID(m_classDataFile));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EClassDataFile_002ESetID(m_classDataFile, standardStringWrapper.Value);
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

	public unsafe ClassDataFile(global::AssetObjects.ClassDataFile* pkClassDataFile)
	{
		//IL_0030: Expected I, but got I8
		m_classDataFile = pkClassDataFile;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x85ec39dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ClassDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && pkClassDataFile == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PJCAHEO_0040classDataFile_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040EBLEOODC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 11u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x85ec39dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ClassDataFile_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_classDataFile = null;
	}

	internal unsafe global::AssetObjects.ClassDataFile* GetUnmanaged()
	{
		return m_classDataFile;
	}
}
