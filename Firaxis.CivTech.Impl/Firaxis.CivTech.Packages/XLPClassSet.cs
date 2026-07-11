using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.Packages;

internal class XLPClassSet : IXLPClassSet
{
	private unsafe global::AssetObjects.XLPClassSet* m_pUnmanaged;

	private IList<IXLPClass> m_pmClasses;

	public virtual IEnumerable<IXLPClass> Items => m_pmClasses;

	public unsafe XLPClassSet(global::AssetObjects.XLPClassSet* pSet)
	{
		m_pUnmanaged = pSet;
		m_pmClasses = new List<IXLPClass>();
		base._002Ector();
	}

	public unsafe virtual void RemoveClass(IXLPClass cl)
	{
		if (!m_pmClasses.Remove(cl))
		{
			return;
		}
		global::AssetObjects.XLPClass* unmanaged = ((XLPClass)cl).GetUnmanaged();
		global::_003CModule_003E.AssetObjects_002EXLPClassSet_002ERemove(m_pUnmanaged, unmanaged);
		foreach (XLPClass pmClass in m_pmClasses)
		{
			pmClass.ReconcileUnmanaged(m_pUnmanaged);
		}
	}

	public unsafe virtual IXLPClass CreateClass(string name)
	{
		//IL_003b: Expected I, but got I8
		XLPClass xLPClass = new XLPClass(global::_003CModule_003E.AssetObjects_002EXLPClassSet_002EPush(m_pUnmanaged));
		xLPClass.Name = name;
		if (!global::_003CModule_003E._003FA0xb05bf479_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateClass_0040XLPClassSet_0040Packages_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIXLPClass_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && xLPClass == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09KHJJLMIK_0040pmManaged_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GC_0040BIPKJKPB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 48u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb05bf479_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateClass_0040XLPClassSet_0040Packages_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIXLPClass_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_pmClasses.Add(xLPClass);
		return xLPClass;
	}

	internal unsafe void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_001d: Expected I, but got I8
		m_pmClasses.Clear();
		if (bDisposing)
		{
			m_pmClasses = null;
			m_pUnmanaged = null;
		}
	}

	internal unsafe void AddReferences()
	{
		global::AssetObjects.XLPClassSet* pUnmanaged = m_pUnmanaged;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EXLPClassSet_002Ebegin(pUnmanaged, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E.iterator iterator2);
		global::_003CModule_003E.AssetObjects_002EXLPClassSet_002Eend(pUnmanaged, &iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
		{
			do
			{
				XLPClass item = new XLPClass(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E_002Eiterator_002E_002A(&iterator));
				m_pmClasses.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPClass_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
		}
	}
}
