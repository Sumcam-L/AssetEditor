using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class IntParameter : Parameter, IIntParameter
{
	public unsafe virtual int Default
	{
		get
		{
			return ((IIntValue)DefaultValue).ParameterValue;
		}
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002EIntValue_002ESetValue(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.IntValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe virtual int Max => global::_003CModule_003E.AssetObjects_002EIntParameter_002EGetMax((global::AssetObjects.IntParameter*)m_pkParameter);

	public unsafe virtual int Min => global::_003CModule_003E.AssetObjects_002EIntParameter_002EGetMin((global::AssetObjects.IntParameter*)m_pkParameter);

	public unsafe IntParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AIntParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe IntParameter(global::AssetObjects.IntParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	public unsafe virtual void SetRange(int iMin, int iMax)
	{
		global::_003CModule_003E.AssetObjects_002EIntParameter_002ESetRanges((global::AssetObjects.IntParameter*)m_pkParameter, iMin, iMax);
	}

	private unsafe global::AssetObjects.IntParameter* GetParameter()
	{
		return (global::AssetObjects.IntParameter*)m_pkParameter;
	}
}
