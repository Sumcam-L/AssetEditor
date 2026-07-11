using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationClass : ClassEntity, IAnimationClass
{
	public unsafe virtual DCCExportType ExportType
	{
		get
		{
			return (DCCExportType)global::_003CModule_003E.AssetObjects_002EAnimationClass_002EGetExportType((global::AssetObjects.AnimationClass*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EAnimationClass_002ESetExportType((global::AssetObjects.AnimationClass*)m_pkEntity, (global::AssetObjects.DCCExportType)value);
		}
	}

	public unsafe AnimationClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AAnimationClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe AnimationClass(global::AssetObjects.AnimationClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}
