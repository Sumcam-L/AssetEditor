using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class Coord2DCollectionValue : CollectionValue, ICoord2DCollectionValue
{
	public unsafe Coord2DCollectionValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		global::AssetObjects.ValueType valueType = (global::AssetObjects.ValueType)6;
		base._002Ector(global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionValue_002Cenum_0020AssetObjects_003A_003AValueType_003E(pkValueSet, szName, &valueType));
	}

	public unsafe Coord2DCollectionValue(global::AssetObjects.CollectionValue* pkValue)
		: base(pkValue)
	{
	}
}
