using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class EnvironmentLightClass : ClassEntity, IEnvironmentLightClass
{
	private EnvironmentMapImportOptions m_pmImportOptions = null;

	public virtual IEnvironmentMapImportOptions ImportOptions => m_pmImportOptions;

	public unsafe EnvironmentLightClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AEnvironmentLightClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe EnvironmentLightClass(global::AssetObjects.EnvironmentLightClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		m_pmImportOptions = new EnvironmentMapImportOptions(global::_003CModule_003E.AssetObjects_002EEnvironmentLightClass_002EGetImportOptions((global::AssetObjects.EnvironmentLightClass*)m_pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmImportOptions.RemoveReferences();
		if (bDisposing)
		{
			m_pmImportOptions = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
