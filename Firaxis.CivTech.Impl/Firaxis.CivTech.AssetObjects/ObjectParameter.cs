using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ObjectParameter : Parameter, IObjectParameter
{
	public unsafe virtual IEnumerable<string> AllowedClasses
	{
		get
		{
			List<string> list = new List<string>();
			global::AssetObjects.ObjectParameter* pkParameter = (global::AssetObjects.ObjectParameter*)m_pkParameter;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EObjectParameter_002Eclasses_begin(pkParameter, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			global::_003CModule_003E.AssetObjects_002EObjectParameter_002Eclasses_end(pkParameter, &const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2));
			}
			return list;
		}
	}

	public unsafe virtual bool IsNullAllowed
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EObjectParameter_002EIsNullAllowed((global::AssetObjects.ObjectParameter*)m_pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EObjectParameter_002ESetNullAllowed((global::AssetObjects.ObjectParameter*)m_pkParameter, value);
		}
	}

	public unsafe virtual InstanceType ObjectType => (InstanceType)global::_003CModule_003E.AssetObjects_002EObjectParameter_002EGetObjectType((global::AssetObjects.ObjectParameter*)m_pkParameter);

	public unsafe ObjectParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName, InstanceType eInstanceType)
	{
		global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)eInstanceType;
		base._002Ector((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AObjectParameter_002Cenum_0020AssetObjects_003A_003AInstanceType_003E(pkParameterSet, szName, &instanceType));
	}

	public unsafe ObjectParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AObjectParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe ObjectParameter(global::AssetObjects.ObjectParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	public unsafe virtual void ClearAllowedClasses()
	{
		global::_003CModule_003E.AssetObjects_002EObjectParameter_002EClearAllowedClasses((global::AssetObjects.ObjectParameter*)m_pkParameter);
	}

	public unsafe virtual void AllowClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EObjectParameter_002EAllowClass((global::AssetObjects.ObjectParameter*)m_pkParameter, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EObjectParameter_002EIsClassAllowed((global::AssetObjects.ObjectParameter*)m_pkParameter, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	private unsafe global::AssetObjects.ObjectParameter* GetParameter()
	{
		return (global::AssetObjects.ObjectParameter*)m_pkParameter;
	}
}
