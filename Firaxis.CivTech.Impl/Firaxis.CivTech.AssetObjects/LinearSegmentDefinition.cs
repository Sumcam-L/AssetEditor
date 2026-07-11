using System;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class LinearSegmentDefinition : AbstractSegmentDefinition, ICurveSegmentDefinition<ILinearCurveSegment>
{
	private LinearSegment m_segment;

	private float m_startingPoint;

	private unsafe global::AssetObjects.LinearSegmentDefinition* m_nativeData;

	public virtual ILinearCurveSegment TypedCurve => m_segment;

	public override ICurveSegment Curve => m_segment;

	public unsafe override float StartingPoint
	{
		get
		{
			return m_startingPoint;
		}
		set
		{
			m_startingPoint = value;
			global::AssetObjects.LinearSegmentDefinition* nativeData = m_nativeData;
			if (nativeData != null)
			{
				global::_003CModule_003E.AssetObjects_002ESegmentDefinition_002ESetStartingPoint((SegmentDefinition*)nativeData, value);
			}
		}
	}

	public unsafe LinearSegmentDefinition(float startingPoint, float first, float last)
	{
		//IL_001c: Expected I, but got I8
		m_segment = new LinearSegment(first, last);
		m_startingPoint = startingPoint;
		m_nativeData = null;
		((object)this)._002Ector();
	}

	public unsafe LinearSegmentDefinition()
	{
		//IL_001e: Expected I, but got I8
		m_segment = new LinearSegment();
		m_startingPoint = 0f;
		m_nativeData = null;
		((object)this)._002Ector();
	}

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ELinearSegmentDefinition_002EGetTypeInfo();
	}

	public unsafe override TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ELinearSegmentDefinition_002EGetTypeInfo();
	}

	public unsafe override void AddToCurve(global::AssetObjects.Curve* Curve)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddToCurve_0040LinearSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAVCurve_00403_0040_0040Z_00404_NA && Curve == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HPADKKDG_0040Curve_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 201u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddToCurve_0040LinearSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAVCurve_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		float startingPoint = StartingPoint;
		float firstValue = m_segment.FirstValue;
		float lastValue = m_segment.LastValue;
		global::AssetObjects.LinearSegmentDefinition* nativeData = global::_003CModule_003E.AssetObjects_002ECurve_002EEmplaceBack_003Cclass_0020AssetObjects_003A_003ALinearSegmentDefinition_002Cfloat_0020_0026_002Cfloat_0020_0026_002Cfloat_0020_0026_003E(Curve, &startingPoint, &firstValue, &lastValue);
		SetNativeData((SegmentDefinition*)nativeData);
	}

	public unsafe override void CopyFrom(AbstractSegmentDefinition otherSegment)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040LinearSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVAbstractSegmentDefinition_0040345_0040_0040Z_00404_NA && otherSegment == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040IOFOBJBH_0040otherSegment_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 226u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040LinearSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVAbstractSegmentDefinition_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), otherSegment.GetInstanceTypeInfo()))
		{
			LinearSegmentDefinition linearSegmentDefinition = (LinearSegmentDefinition)otherSegment;
			StartingPoint = otherSegment.StartingPoint;
			m_segment.CopyFrom(linearSegmentDefinition.m_segment);
		}
	}

	public unsafe override void SetNativeData(SegmentDefinition* segment)
	{
		//IL_004e: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		//IL_0012: Expected I, but got I8
		//IL_003f: Expected I, but got I8
		if (segment != null)
		{
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*>)(*(ulong*)(*(ulong*)segment)))((nint)segment)))
			{
				m_nativeData = (global::AssetObjects.LinearSegmentDefinition*)segment;
				m_startingPoint = global::_003CModule_003E.AssetObjects_002ESegmentDefinition_002EGetStartingPoint(segment);
				m_segment.SetNativeData(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.ICurveSegment*>)(*(ulong*)(*(long*)segment + 32)))((nint)segment));
				return;
			}
		}
		m_nativeData = null;
		m_segment.SetNativeData(null);
	}
}
