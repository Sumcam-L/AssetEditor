using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TupleClass : ClassEntity, ITupleClass
{
	private unsafe global::AssetObjects.TupleClass* NativePtr => (global::AssetObjects.TupleClass*)m_pkEntity;

	public unsafe TupleClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ATupleClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe TupleClass(global::AssetObjects.TupleClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}
