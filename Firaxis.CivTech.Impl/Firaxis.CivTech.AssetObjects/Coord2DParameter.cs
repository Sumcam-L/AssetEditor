using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class Coord2DParameter : Parameter, ICoord2DParameter
{
	public unsafe virtual float DefaultY
	{
		get
		{
			return ((ICoord2DValue)DefaultValue).ParameterValue.Y;
		}
		set
		{
			//IL_0014: Expected I, but got I8
			//IL_0021: Expected I, but got I8
			global::AssetObjects.Coord2DParameter* pkParameter = (global::AssetObjects.Coord2DParameter*)m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ECoord2DValue_002ESet(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord2DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), global::_003CModule_003E.AssetObjects_002ECoord2DValue_002EGetX(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord2DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)), value);
		}
	}

	public unsafe virtual float DefaultX
	{
		get
		{
			return ((ICoord2DValue)DefaultValue).ParameterValue.X;
		}
		set
		{
			//IL_0014: Expected I, but got I8
			//IL_0022: Expected I, but got I8
			global::AssetObjects.Coord2DParameter* pkParameter = (global::AssetObjects.Coord2DParameter*)m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ECoord2DValue_002ESet(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord2DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value, global::_003CModule_003E.AssetObjects_002ECoord2DValue_002EGetY(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord2DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)));
		}
	}

	public unsafe Coord2DParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACoord2DParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe Coord2DParameter(global::AssetObjects.Coord2DParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.Coord2DParameter* GetParameter()
	{
		return (global::AssetObjects.Coord2DParameter*)m_pkParameter;
	}
}
