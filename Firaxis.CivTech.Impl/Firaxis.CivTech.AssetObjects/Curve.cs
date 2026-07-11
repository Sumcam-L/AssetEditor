using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.ReflectionHelperEx;
using Platform;
using Reflection;
using std;

namespace Firaxis.CivTech.AssetObjects;

public class Curve : INativeReflection, ICurve
{
	private List<ICurveSegmentDefinition> m_segmentDefinitions = new List<ICurveSegmentDefinition>();

	private unsafe global::AssetObjects.Curve* m_segment = null;

	public virtual bool IsEmpty
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_segmentDefinitions.Count == 0;
		}
	}

	public virtual IEnumerable<ICurveSegmentDefinition> CurveSegments => m_segmentDefinitions;

	public unsafe static TypeInfo* GetTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ECurve_002EGetTypeInfo();
	}

	public unsafe virtual TypeInfo* GetInstanceTypeInfo()
	{
		return global::_003CModule_003E.AssetObjects_002ECurve_002EGetTypeInfo();
	}

	public unsafe virtual float GetValue(float X)
	{
		//IL_0018: Expected I, but got I8
		global::AssetObjects.Curve* segment = m_segment;
		if (segment != null)
		{
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, float, float>)(*(ulong*)(*(long*)segment + 16)))((nint)segment, X);
		}
		return 0f;
	}

	public unsafe virtual void AddCurveSegment(ICurveSegmentDefinition Curve)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCurveSegment_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurveSegmentDefinition_0040345_0040_0040Z_00404_NA && Curve == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HPADKKDG_0040Curve_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 287u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCurveSegment_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurveSegmentDefinition_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_segmentDefinitions.Add(Curve);
		m_segmentDefinitions.Sort();
		if (Curve == m_segmentDefinitions[m_segmentDefinitions.Count - 1])
		{
			((AbstractSegmentDefinition)Curve).AddToCurve(m_segment);
		}
		else
		{
			RebuildNativeData();
		}
	}

	public unsafe virtual void RemoveCurveSegment(ICurveSegmentDefinition Curve)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCurveSegment_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurveSegmentDefinition_0040345_0040_0040Z_00404_NA && Curve == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HPADKKDG_0040Curve_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 310u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCurveSegment_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICurveSegmentDefinition_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_segmentDefinitions.Count == 0)
		{
			return;
		}
		if (Curve == m_segmentDefinitions[m_segmentDefinitions.Count - 1])
		{
			int count = m_segmentDefinitions.Count;
			m_segmentDefinitions.RemoveAt(count - 1);
			global::AssetObjects.Curve* segment = m_segment;
			if (segment != null)
			{
				global::_003CModule_003E.AssetObjects_002ECurve_002EPopBack(segment);
			}
		}
		else if (m_segmentDefinitions.Remove(Curve))
		{
			RebuildNativeData();
		}
	}

	public unsafe virtual void ClearCurveSegments()
	{
		RemoveChildNativeBacking();
		m_segmentDefinitions.Clear();
		global::AssetObjects.Curve* segment = m_segment;
		if (segment != null)
		{
			global::_003CModule_003E.AssetObjects_002ECurve_002EClear(segment);
		}
	}

	public unsafe void RebuildNativeData()
	{
		if (m_segment == null)
		{
			return;
		}
		RemoveChildNativeBacking();
		global::_003CModule_003E.AssetObjects_002ECurve_002EClear(m_segment);
		List<ICurveSegmentDefinition>.Enumerator enumerator = m_segmentDefinitions.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AbstractSegmentDefinition)enumerator.Current).AddToCurve(m_segment);
			}
			while (enumerator.MoveNext());
		}
	}

	public unsafe void SetNativeData(global::AssetObjects.ICurveSegment* segment)
	{
		//IL_00d2: Expected I, but got I8
		//IL_001b: Expected I, but got I8
		//IL_0081: Expected I, but got I8
		RemoveChildNativeBacking();
		if (segment != null)
		{
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsExactType(GetInstanceTypeInfo(), ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*>)(*(ulong*)(*(ulong*)segment)))((nint)segment)))
			{
				m_segmentDefinitions.Clear();
				m_segment = (global::AssetObjects.Curve*)segment;
				if (global::_003CModule_003E.AssetObjects_002ECurve_002ESize((global::AssetObjects.Curve*)segment) == 0)
				{
					return;
				}
				System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E obj);
				global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_002E_007Bctor_007D(&obj);
				try
				{
					global::_003CModule_003E.AssetObjects_002ECurve_002EGetSegments_003Cclass_0020std_003A_003Avector_003Cclass_0020AssetObjects_003A_003ASegmentDefinition_0020_002A_002Cclass_0020std_003A_003Aallocator_003Cclass_0020AssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E(m_segment, &obj);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E obj2);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_002Ebegin(&obj, &obj2);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E obj3);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_002Eend(&obj, &obj3);
					if (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E_002E_0021_003D((_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E*)(&obj2), (_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E*)(&obj3)))
					{
						do
						{
							SegmentDefinition* ptr = (SegmentDefinition*)(*(ulong*)global::_003CModule_003E.std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E_002E_002A(&obj2));
							AbstractSegmentDefinition abstractSegmentDefinition = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x1ba5dfb6_002EConstructManagedSegment(ptr);
							if (abstractSegmentDefinition != null)
							{
								abstractSegmentDefinition.SetNativeData(ptr);
								m_segmentDefinitions.Add(abstractSegmentDefinition);
							}
							global::_003CModule_003E.std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E_002E_002B_002B(&obj2);
						}
						while (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E_002E_0021_003D((_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E*)(&obj2), (_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_0020_003E*)(&obj3)));
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
				global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ASegmentDefinition_0020_002A_003E_0020_003E_002E_007Bdtor_007D(&obj);
				return;
			}
		}
		m_segment = null;
	}

	public unsafe void CopyFrom(Curve otherSegment)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPE_0024AAV2345_0040_0040Z_00404_NA && otherSegment == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040IOFOBJBH_0040otherSegment_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040EGJCOAAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 368u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1ba5dfb6_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040Curve_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPE_0024AAV2345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		RemoveChildNativeBacking();
		m_segmentDefinitions.Clear();
		List<ICurveSegmentDefinition>.Enumerator enumerator = otherSegment.m_segmentDefinitions.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				AbstractSegmentDefinition item = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x1ba5dfb6_002ECopyManagedSegment((AbstractSegmentDefinition)enumerator.Current);
				m_segmentDefinitions.Add(item);
			}
			while (enumerator.MoveNext());
		}
		RebuildNativeData();
	}

	private unsafe void RemoveChildNativeBacking()
	{
		//IL_0028: Expected I, but got I8
		List<ICurveSegmentDefinition>.Enumerator enumerator = m_segmentDefinitions.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AbstractSegmentDefinition)enumerator.Current).SetNativeData(null);
			}
			while (enumerator.MoveNext());
		}
	}
}
