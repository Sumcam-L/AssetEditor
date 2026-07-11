using System.Runtime.CompilerServices;
using AssetObjects;
using Firaxis.CivTech.ReflectionHelperEx;
using Platform;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class CurveValue : Value, INativeReflection, ICurveValue
{
	private Curve m_segment = new Curve();

	public unsafe virtual ICurve ParameterValue
	{
		get
		{
			return m_segment;
		}
		set
		{
			//IL_003d: Expected I, but got I8
			//IL_002d: Expected I, but got I8
			global::AssetObjects.CurveValue* pkValue = (global::AssetObjects.CurveValue*)m_pkValue;
			if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ParameterValue_0040CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurve_0040456_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040MNHIABEI_0040nativePointer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 414u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ParameterValue_0040CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurve_0040456_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			m_segment.SetNativeData(null);
			global::AssetObjects.Curve* ptr = global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetValue(pkValue);
			global::_003CModule_003E.AssetObjects_002ECurve_002EClear(ptr);
			(m_segment = new Curve()).SetNativeData((global::AssetObjects.ICurveSegment*)ptr);
			Curve otherSegment = (Curve)value;
			m_segment.CopyFrom(otherSegment);
		}
	}

	public override ValueType ParameterType => ValueType.VT_CURVE;

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetTypeInfo();
	}

	public unsafe virtual TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetTypeInfo();
	}

	private unsafe global::AssetObjects.CurveValue* GetValue()
	{
		return (global::AssetObjects.CurveValue*)m_pkValue;
	}

	public virtual float GetValue(float x)
	{
		return m_segment.GetValue(x);
	}

	public unsafe CurveValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ACurveValue_003E(pkCollectionValue, szName))
	{
		//IL_0045: Expected I, but got I8
		global::AssetObjects.CurveValue* pkValue = (global::AssetObjects.CurveValue*)m_pkValue;
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVCollectionValue_00402_0040PEBD_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040GFAPCIAF_0040curveValue_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 403u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVCollectionValue_00402_0040PEBD_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_segment.SetNativeData((global::AssetObjects.ICurveSegment*)global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetValue(pkValue));
	}

	public unsafe CurveValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACurveValue_003E(pkValueSet, szName))
	{
		//IL_0045: Expected I, but got I8
		global::AssetObjects.CurveValue* pkValue = (global::AssetObjects.CurveValue*)m_pkValue;
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040PEBD_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040GFAPCIAF_0040curveValue_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 395u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040PEBD_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_segment.SetNativeData((global::AssetObjects.ICurveSegment*)global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetValue(pkValue));
	}

	public unsafe CurveValue(global::AssetObjects.CurveValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
		//IL_0038: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05MFEJDJP_0040value_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 387u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_segment.SetNativeData((global::AssetObjects.ICurveSegment*)global::_003CModule_003E.AssetObjects_002ECurveValue_002EGetValue(pkValue));
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_CURVE)
		{
			CurveValue curveValue = (CurveValue)otherValue;
			m_segment.CopyFrom(curveValue.m_segment);
		}
	}
}
