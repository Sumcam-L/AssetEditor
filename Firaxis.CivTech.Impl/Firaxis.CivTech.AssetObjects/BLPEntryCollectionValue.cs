using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BLPEntryCollectionValue : CollectionValue, IBLPEntryCollectionValue
{
	public unsafe BLPEntryCollectionValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		global::AssetObjects.ValueType valueType = (global::AssetObjects.ValueType)8;
		base._002Ector(global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionValue_002Cenum_0020AssetObjects_003A_003AValueType_003E(pkValueSet, szName, &valueType));
	}

	public unsafe BLPEntryCollectionValue(global::AssetObjects.CollectionValue* pkValue)
		: base(pkValue)
	{
	}
}
