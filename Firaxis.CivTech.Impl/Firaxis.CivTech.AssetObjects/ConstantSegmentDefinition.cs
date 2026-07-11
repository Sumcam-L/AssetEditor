using System;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class ConstantSegmentDefinition : AbstractSegmentDefinition, ICurveSegmentDefinition<IConstantCurveSegment>
{
	private ConstantSegment m_segment;

	private float m_startingPoint;

	private unsafe global::AssetObjects.ConstantSegmentDefinition* m_nativeData;

	public virtual IConstantCurveSegment TypedCurve => m_segment;

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
			global::AssetObjects.ConstantSegmentDefinition* nativeData = m_nativeData;
			if (nativeData != null)
			{
				global::_003CModule_003E.AssetObjects_002ESegmentDefinition_002ESetStartingPoint((SegmentDefinition*)nativeData, value);
			}
		}
	}

	public unsafe ConstantSegmentDefinition(float startingPoint, float constantValue)
	{
		//IL_001b: Expected I, but got I8
		m_segment = new ConstantSegment(constantValue);
		m_startingPoint = startingPoint;
		m_nativeData = null;
		((object)this)._002Ector();
	}

	public unsafe ConstantSegmentDefinition()
	{
		//IL_001e: Expected I, but got I8
		m_segment = new ConstantSegment();
		m_startingPoint = 0f;
		m_nativeData = null;
		((object)this)._002Ector();
	}

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002EConstantSegmentDefinition_002EGetTypeInfo();
	}

	public unsafe override TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002EConstantSegmentDefinition_002EGetTypeInfo();
	}

	public unsafe override void AddToCurve(global::AssetObjects.Curve* Curve)
	{
		//IL_0023: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddToCurve_0040ConstantSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAVCurve_00403_0040_0040Z_00404_NA && Curve == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HPADKKDG_0040Curve_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 103u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddToCurve_0040ConstantSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAVCurve_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		float startingPoint = StartingPoint;
		float constantValue = m_segment.ConstantValue;
		global::AssetObjects.ConstantSegmentDefinition* nativeData = global::_003CModule_003E.AssetObjects_002ECurve_002EEmplaceBack_003Cclass_0020AssetObjects_003A_003AConstantSegmentDefinition_002Cfloat_0020_0026_002Cfloat_0020_0026_003E(Curve, &startingPoint, &constantValue);
		SetNativeData((SegmentDefinition*)nativeData);
	}

	public unsafe override void CopyFrom(AbstractSegmentDefinition otherSegment)
	{
		//IL_0023: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ConstantSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVAbstractSegmentDefinition_0040345_0040_0040Z_00404_NA && otherSegment == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040IOFOBJBH_0040otherSegment_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 127u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ConstantSegmentDefinition_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVAbstractSegmentDefinition_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), otherSegment.GetInstanceTypeInfo()))
		{
			ConstantSegmentDefinition obj = (ConstantSegmentDefinition)otherSegment;
			StartingPoint = otherSegment.StartingPoint;
			ConstantSegment segment = obj.m_segment;
			m_segment.ConstantValue = segment.ConstantValue;
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
				m_nativeData = (global::AssetObjects.ConstantSegmentDefinition*)segment;
				m_startingPoint = global::_003CModule_003E.AssetObjects_002ESegmentDefinition_002EGetStartingPoint(segment);
				m_segment.SetNativeData(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.ICurveSegment*>)(*(ulong*)(*(long*)segment + 32)))((nint)segment));
				return;
			}
		}
		m_nativeData = null;
		m_segment.SetNativeData(null);
	}
}
