using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class MaterialClass : ClassEntity, IMaterialClass
{
	private IMaterialValidationOptions m_pmValidationOptions = null;

	public virtual IMaterialValidationOptions ValidationOptions => m_pmValidationOptions;

	public unsafe MaterialClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AMaterialClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe MaterialClass(global::AssetObjects.MaterialClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		m_pmValidationOptions = new MaterialValidationOptions(global::_003CModule_003E.AssetObjects_002EMaterialClass_002EGetValidationOptions((global::AssetObjects.MaterialClass*)m_pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		((MaterialValidationOptions)m_pmValidationOptions).RemoveReferences();
		if (bDisposing)
		{
			m_pmValidationOptions = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
