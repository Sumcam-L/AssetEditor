using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Firaxis.CivTech.AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.Packages;

internal class XLPClass : IXLPClass
{
	private unsafe global::AssetObjects.XLPClass* m_unmanaged;

	private string m_name;

	private string m_errorEntityName;

	private string m_cookModuleName;

	private ISet<string> m_allowedClassNames;

	public virtual IEnumerable<string> AllowedEntityClasses => m_allowedClassNames;

	public unsafe virtual Firaxis.CivTech.AssetObjects.InstanceType InstanceType
	{
		get
		{
			return (Firaxis.CivTech.AssetObjects.InstanceType)global::_003CModule_003E.AssetObjects_002EXLPClass_002EGetInstanceEntityType(m_unmanaged);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EXLPClass_002ESetInstanceEntityType(m_unmanaged, (global::AssetObjects.InstanceType)value);
		}
	}

	public unsafe virtual string CookModuleName
	{
		get
		{
			return m_cookModuleName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_errorEntityName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EXLPClass_002ESetCookModuleName(m_unmanaged, standardStringWrapper.Value);
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

	public unsafe virtual string ErrorEntityName
	{
		get
		{
			return m_errorEntityName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_errorEntityName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EXLPClass_002ESetErrorAssetName(m_unmanaged, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_name = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EXLPClass_002ESetName(m_unmanaged, standardStringWrapper.Value);
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

	public unsafe XLPClass(global::AssetObjects.XLPClass* pUnmanaged)
	{
		//IL_003b: Expected I, but got I8
		m_unmanaged = pUnmanaged;
		m_allowedClassNames = new SortedSet<string>();
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV1AssetObjects_0040_0040_0040Z_00404_NA && pUnmanaged == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08DNJAGBJH_0040xlpClass_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040IEFLGCOA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 16u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV1AssetObjects_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		ResolveReferences();
	}

	public unsafe virtual void ClearAllowedEntityClasses()
	{
		m_allowedClassNames.Clear();
		global::_003CModule_003E.AssetObjects_002EXLPClass_002EClearAllowedClasses(m_unmanaged);
	}

	public unsafe virtual void AllowEntityClass(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (m_allowedClassNames.Add(name))
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EXLPClass_002EAllowClass(m_unmanaged, standardStringWrapper.Value);
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

	public unsafe virtual void DisallowEntityClass(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (m_allowedClassNames.Remove(name))
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EXLPClass_002EDisallowClass(m_unmanaged, standardStringWrapper.Value);
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

	internal unsafe global::AssetObjects.XLPClass* GetUnmanaged()
	{
		return m_unmanaged;
	}

	internal unsafe void ReconcileUnmanaged(global::AssetObjects.XLPClassSet* xlpClassSet)
	{
		//IL_0025: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003FReconcileUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVXLPClassSet_0040AssetObjects_0040_0040_0040Z_00404_NA && xlpClassSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040KIECHCJP_0040xlpClassSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040IEFLGCOA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 33u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003FReconcileUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVXLPClassSet_0040AssetObjects_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(Name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.XLPClass* ptr = (m_unmanaged = global::_003CModule_003E.AssetObjects_002EXLPClassSet_002EFindByName(xlpClassSet, standardStringWrapper.Value));
			if (!global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F9_003F_003FReconcileUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVXLPClassSet_0040AssetObjects_0040_0040_0040Z_00404_NA && ptr == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040NFIPDIFL_0040Unable_003F5to_003F5reconcile_003F5XLP_003F5Class_003F5wi_0040), __arglist(standardStringWrapper.Value));
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040EEKPMDEA_0040m_unmanaged_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040IEFLGCOA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 37u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F9_003F_003FReconcileUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVXLPClassSet_0040AssetObjects_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	internal unsafe void SetUnmanaged(global::AssetObjects.XLPClass* xlpClass)
	{
		//IL_0023: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV2AssetObjects_0040_0040_0040Z_00404_NA && xlpClass == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08DNJAGBJH_0040xlpClass_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040IEFLGCOA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 24u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x8b715b82_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetUnmanaged_0040XLPClass_0040Packages_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV2AssetObjects_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_unmanaged = xlpClass;
		ResolveReferences();
	}

	private unsafe void ResolveReferences()
	{
		m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLPClass_002EGetName(m_unmanaged));
		m_errorEntityName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLPClass_002EGetErrorEntityName(m_unmanaged));
		m_cookModuleName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLPClass_002EGetCookModuleName(m_unmanaged));
		m_allowedClassNames.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EXLPClass_002Eclasses_begin(m_unmanaged, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EXLPClass_002Eclasses_end(m_unmanaged, &const_iterator2)))
		{
			do
			{
				string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
				m_allowedClassNames.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EXLPClass_002Eclasses_end(m_unmanaged, &const_iterator2)));
		}
	}
}
