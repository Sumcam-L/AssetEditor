using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class GeometryClass : ClassEntity, IGeometryClass
{
	private ParameterSet m_pmGroupParameters = null;

	public unsafe virtual DCCExportType ExportType
	{
		get
		{
			return (DCCExportType)global::_003CModule_003E.AssetObjects_002EGeometryClass_002EGetExportType((global::AssetObjects.GeometryClass*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeometryClass_002ESetExportType((global::AssetObjects.GeometryClass*)m_pkEntity, (global::AssetObjects.DCCExportType)value);
		}
	}

	public virtual IParameterSet GroupParameters => m_pmGroupParameters;

	public unsafe virtual bool EnableDegenerateCheck
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeometryClass_002EIsDegenerateCheckEnabled((global::AssetObjects.GeometryClass*)m_pkEntity);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeometryClass_002EEnableDegenerateCheck((global::AssetObjects.GeometryClass*)m_pkEntity, value);
		}
	}

	public unsafe virtual float MinimumTriangleArea
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeometryClass_002EGetMinimumTriangleArea((global::AssetObjects.GeometryClass*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeometryClass_002ESetMinimumTriangleArea((global::AssetObjects.GeometryClass*)m_pkEntity, value);
		}
	}

	public unsafe virtual uint MaxSkinnedBones
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeometryClass_002EGetMaxSkinnedBones((global::AssetObjects.GeometryClass*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeometryClass_002ESetMaxSkinnedBones((global::AssetObjects.GeometryClass*)m_pkEntity, value);
		}
	}

	public unsafe GeometryClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AGeometryClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe GeometryClass(global::AssetObjects.GeometryClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		m_pmGroupParameters = new ParameterSet(global::_003CModule_003E.AssetObjects_002EGeometryClass_002EGetGroupParams((global::AssetObjects.GeometryClass*)m_pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmGroupParameters.RemoveReferences();
		if (bDisposing)
		{
			m_pmGroupParameters = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
