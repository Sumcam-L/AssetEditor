using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ObjectCollectionValue : CollectionValue, IObjectCollectionValue
{
	public unsafe ObjectCollectionValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName, InstanceType eInstanceType)
	{
		global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)eInstanceType;
		base._002Ector(global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionValue_002Cenum_0020AssetObjects_003A_003AInstanceType_003E(pkValueSet, szName, &instanceType));
	}

	public unsafe ObjectCollectionValue(global::AssetObjects.CollectionValue* pkValue)
		: base(pkValue)
	{
	}
}
