using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class CurveClass : ClassEntity, ICurveClass
{
	private unsafe global::AssetObjects.CurveClass* NativePtr => (global::AssetObjects.CurveClass*)m_pkEntity;

	public virtual IEnumerable<CurveSegmentType> AllowedCurveTypes
	{
		get
		{
			List<CurveSegmentType> list = new List<CurveSegmentType>();
			foreach (object value in Enum.GetValues(typeof(CurveSegmentType)))
			{
				CurveSegmentType curveSegmentType = (CurveSegmentType)value;
				if (IsCurveTypeAllowed(curveSegmentType))
				{
					list.Add(curveSegmentType);
				}
			}
			return list;
		}
	}

	public unsafe CurveClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ACurveClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe CurveClass(global::AssetObjects.CurveClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	public unsafe virtual void AllowCurveType(CurveSegmentType Type)
	{
		global::_003CModule_003E.AssetObjects_002ECurveClass_002EAllowCurveType((global::AssetObjects.CurveClass*)m_pkEntity, (global::AssetObjects.CurveSegmentType)Type);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsCurveTypeAllowed(CurveSegmentType Type)
	{
		return global::_003CModule_003E.AssetObjects_002ECurveClass_002EIsCurveTypeAllowed((global::AssetObjects.CurveClass*)m_pkEntity, (global::AssetObjects.CurveSegmentType)Type);
	}

	public unsafe virtual void ClearAllowedCurveTypes()
	{
		global::_003CModule_003E.AssetObjects_002ECurveClass_002EClearAllowedCurveTypes((global::AssetObjects.CurveClass*)m_pkEntity);
	}
}
