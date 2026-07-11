using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class EnumParameter : Parameter, IManualEnumParameter
{
	public unsafe EnumParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AEnumParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe EnumParameter(global::AssetObjects.EnumParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	public unsafe virtual void NewEnumeration(string pmEnumeration)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmEnumeration).ToPointer();
		global::_003CModule_003E.AssetObjects_002EEnumParameter_002ENewEnumeration((global::AssetObjects.EnumParameter*)m_pkParameter, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual IList<string> GetEnumerations()
	{
		List<string> list = new List<string>();
		global::AssetObjects.EnumParameter* pkParameter = (global::AssetObjects.EnumParameter*)m_pkParameter;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EEnumParameter_002Eenum_begin(pkParameter, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnumParameter_002Eenum_end(pkParameter, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				list.Add(Marshal.PtrToStringAnsi(ptr));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnumParameter_003A_003AEnumeration_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnumParameter_002Eenum_end(pkParameter, &iterator2)));
		}
		return list;
	}

	public unsafe virtual void ClearEnumerations()
	{
		global::_003CModule_003E.AssetObjects_002EEnumParameter_002EClearEnumerations((global::AssetObjects.EnumParameter*)m_pkParameter);
	}

	private unsafe global::AssetObjects.EnumParameter* GetParameter()
	{
		return (global::AssetObjects.EnumParameter*)m_pkParameter;
	}
}
