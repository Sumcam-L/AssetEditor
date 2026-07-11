using System;
using AssetObjects;
using Firaxis.CivTech.ReflectionHelperEx;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class LinearSegment : INativeReflection, ILinearCurveSegment
{
	private float m_first;

	private float m_last;

	private unsafe global::AssetObjects.LinearSegment* m_segment;

	public unsafe virtual float LastValue
	{
		get
		{
			return m_last;
		}
		set
		{
			m_last = value;
			global::AssetObjects.LinearSegment* segment = m_segment;
			if (segment != null)
			{
				global::_003CModule_003E.AssetObjects_002ELinearSegment_002ESetLast(segment, value);
			}
		}
	}

	public unsafe virtual float FirstValue
	{
		get
		{
			return m_first;
		}
		set
		{
			m_first = value;
			global::AssetObjects.LinearSegment* segment = m_segment;
			if (segment != null)
			{
				global::_003CModule_003E.AssetObjects_002ELinearSegment_002ESetFirst(segment, value);
			}
		}
	}

	public LinearSegment(float first, float last)
	{
		m_first = first;
		m_last = last;
		base._002Ector();
	}

	public LinearSegment()
	{
		m_first = 0f;
		m_last = 1f;
		base._002Ector();
	}

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ELinearSegment_002EGetTypeInfo();
	}

	public unsafe virtual TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ELinearSegment_002EGetTypeInfo();
	}

	public unsafe virtual float GetValue(float X)
	{
		//IL_0018: Expected I, but got I8
		global::AssetObjects.LinearSegment* segment = m_segment;
		if (segment != null)
		{
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, float, float>)(*(ulong*)(*(long*)segment + 16)))((nint)segment, X);
		}
		return 0f;
	}

	public unsafe void SetNativeData(global::AssetObjects.ICurveSegment* segment)
	{
		//IL_0047: Expected I, but got I8
		//IL_0012: Expected I, but got I8
		if (segment != null)
		{
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*>)(*(ulong*)(*(ulong*)segment)))((nint)segment)))
			{
				m_segment = (global::AssetObjects.LinearSegment*)segment;
				m_first = global::_003CModule_003E.AssetObjects_002ELinearSegment_002EGetFirst((global::AssetObjects.LinearSegment*)segment);
				m_last = global::_003CModule_003E.AssetObjects_002ELinearSegment_002EGetLast(m_segment);
				return;
			}
		}
		m_segment = null;
	}

	public void CopyFrom(LinearSegment otherSegment)
	{
		FirstValue = otherSegment.FirstValue;
		LastValue = otherSegment.LastValue;
	}
}
