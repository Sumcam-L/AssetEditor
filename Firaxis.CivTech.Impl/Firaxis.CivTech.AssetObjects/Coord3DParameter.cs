using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class Coord3DParameter : Parameter, ICoord3DParameter
{
	public unsafe virtual float DefaultZ
	{
		get
		{
			return ((ICoord3DValue)DefaultValue).ParameterValue.z;
		}
		set
		{
			//IL_0014: Expected I, but got I8
			//IL_0021: Expected I, but got I8
			//IL_0033: Expected I, but got I8
			global::AssetObjects.Coord3DParameter* pkParameter = (global::AssetObjects.Coord3DParameter*)m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ECoord3DValue_002ESet(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetX(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetY(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)), value);
		}
	}

	public unsafe virtual float DefaultY
	{
		get
		{
			return ((ICoord3DValue)DefaultValue).ParameterValue.y;
		}
		set
		{
			//IL_0014: Expected I, but got I8
			//IL_0021: Expected I, but got I8
			//IL_0034: Expected I, but got I8
			global::AssetObjects.Coord3DParameter* pkParameter = (global::AssetObjects.Coord3DParameter*)m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ECoord3DValue_002ESet(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetX(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)), value, global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetZ(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)));
		}
	}

	public unsafe virtual float DefaultX
	{
		get
		{
			return ((ICoord3DValue)DefaultValue).ParameterValue.x;
		}
		set
		{
			//IL_0014: Expected I, but got I8
			//IL_0022: Expected I, but got I8
			//IL_0034: Expected I, but got I8
			global::AssetObjects.Coord3DParameter* pkParameter = (global::AssetObjects.Coord3DParameter*)m_pkParameter;
			global::_003CModule_003E.AssetObjects_002ECoord3DValue_002ESet(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value, global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetY(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetZ(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Coord3DValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter)));
		}
	}

	public unsafe Coord3DParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACoord3DParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe Coord3DParameter(global::AssetObjects.Coord3DParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.Coord3DParameter* GetParameter()
	{
		return (global::AssetObjects.Coord3DParameter*)m_pkParameter;
	}
}
