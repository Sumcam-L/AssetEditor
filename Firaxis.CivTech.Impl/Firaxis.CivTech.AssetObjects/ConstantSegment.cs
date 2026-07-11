using System;
using AssetObjects;
using Firaxis.CivTech.ReflectionHelperEx;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class ConstantSegment : INativeReflection, IConstantCurveSegment
{
	private float m_constantValue;

	private unsafe global::AssetObjects.ConstantSegment* m_segment;

	public unsafe virtual float ConstantValue
	{
		get
		{
			return m_constantValue;
		}
		set
		{
			m_constantValue = value;
			global::AssetObjects.ConstantSegment* segment = m_segment;
			if (segment != null)
			{
				global::_003CModule_003E.AssetObjects_002EConstantSegment_002ESetValue(segment, value);
			}
		}
	}

	public unsafe ConstantSegment(float value)
	{
		//IL_000f: Expected I, but got I8
		m_constantValue = value;
		m_segment = null;
		base._002Ector();
	}

	public unsafe ConstantSegment()
	{
		//IL_0013: Expected I, but got I8
		m_constantValue = 0f;
		m_segment = null;
		base._002Ector();
	}

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002EConstantSegment_002EGetTypeInfo();
	}

	public unsafe virtual TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002EConstantSegment_002EGetTypeInfo();
	}

	public unsafe virtual float GetValue(float X)
	{
		//IL_0018: Expected I, but got I8
		global::AssetObjects.ConstantSegment* segment = m_segment;
		if (segment != null)
		{
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, float, float>)(*(ulong*)(*(long*)segment + 16)))((nint)segment, X);
		}
		return 0f;
	}

	public unsafe void SetNativeData(global::AssetObjects.ICurveSegment* segment)
	{
		//IL_0036: Expected I, but got I8
		//IL_0012: Expected I, but got I8
		if (segment != null)
		{
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*>)(*(ulong*)(*(ulong*)segment)))((nint)segment)))
			{
				m_segment = (global::AssetObjects.ConstantSegment*)segment;
				m_constantValue = global::_003CModule_003E.AssetObjects_002EConstantSegment_002EGetValue((global::AssetObjects.ConstantSegment*)segment);
				return;
			}
		}
		m_segment = null;
	}

	public void CopyFrom(ConstantSegment otherSegment)
	{
		ConstantValue = otherSegment.ConstantValue;
	}
}
