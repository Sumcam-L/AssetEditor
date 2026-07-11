using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class FloatParameter : Parameter, IFloatParameter
{
	public unsafe virtual float Default
	{
		get
		{
			return ((IFloatValue)DefaultValue).ParameterValue;
		}
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002EFloatValue_002ESetValue(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.FloatValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe virtual float Max => global::_003CModule_003E.AssetObjects_002EFloatParameter_002EGetMax((global::AssetObjects.FloatParameter*)m_pkParameter);

	public unsafe virtual float Min => global::_003CModule_003E.AssetObjects_002EFloatParameter_002EGetMin((global::AssetObjects.FloatParameter*)m_pkParameter);

	public unsafe FloatParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AFloatParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe FloatParameter(global::AssetObjects.FloatParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	public unsafe virtual void SetRange(float fMin, float fMax)
	{
		global::_003CModule_003E.AssetObjects_002EFloatParameter_002ESetRanges((global::AssetObjects.FloatParameter*)m_pkParameter, fMin, fMax);
	}

	private unsafe global::AssetObjects.FloatParameter* GetParameter()
	{
		return (global::AssetObjects.FloatParameter*)m_pkParameter;
	}
}
