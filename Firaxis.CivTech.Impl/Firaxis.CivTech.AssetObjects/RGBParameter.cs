using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class RGBParameter : Parameter, IRGBParameter
{
	public unsafe virtual float DefaultB
	{
		get
		{
			return (int)((IRGBValue)DefaultValue).ParameterValue.B;
		}
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ERGBValue_002ESetB(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.RGBValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe virtual float DefaultG
	{
		get
		{
			return (int)((IRGBValue)DefaultValue).ParameterValue.G;
		}
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ERGBValue_002ESetG(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.RGBValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe virtual float DefaultR
	{
		get
		{
			return (int)((IRGBValue)DefaultValue).ParameterValue.R;
		}
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ERGBValue_002ESetR(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.RGBValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe RGBParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ARGBParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe RGBParameter(global::AssetObjects.RGBParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.RGBParameter* GetParameter()
	{
		return (global::AssetObjects.RGBParameter*)m_pkParameter;
	}
}
