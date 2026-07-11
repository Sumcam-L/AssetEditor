using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BoolParameter : Parameter, IBoolParameter
{
	public unsafe virtual bool Default
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return ((IBoolValue)DefaultValue).ParameterValue;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002EBoolValue_002ESetValue(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BoolValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe BoolParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ABoolParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe BoolParameter(global::AssetObjects.BoolParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.BoolParameter* GetParameter()
	{
		return (global::AssetObjects.BoolParameter*)m_pkParameter;
	}
}
