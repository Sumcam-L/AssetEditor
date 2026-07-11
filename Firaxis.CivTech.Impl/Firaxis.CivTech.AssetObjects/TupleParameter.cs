using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TupleParameter : Parameter, ITupleParameter
{
	public unsafe virtual string ClassName
	{
		get
		{
			sbyte* value = global::_003CModule_003E.AssetObjects_002ETupleParameter_002EGetClassName((global::AssetObjects.TupleParameter*)m_pkParameter);
			IntPtr ptr = new IntPtr(value);
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002ETupleParameter_002ESetClassName((global::AssetObjects.TupleParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe TupleParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ATupleParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe TupleParameter(global::AssetObjects.TupleParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.TupleParameter* GetParameter()
	{
		return (global::AssetObjects.TupleParameter*)m_pkParameter;
	}
}
