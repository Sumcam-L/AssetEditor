using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class Coord2DCollectionParameter : CollectionParameter, ICoord2DCollectionParameter
{
	public unsafe Coord2DCollectionParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Placeholder_003CAssetObjects_003A_003ACoord2DParameter_003E placeholder_003CAssetObjects_003A_003ACoord2DParameter_003E);
		base._002Ector(global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionParameter_002Cstruct_0020AssetObjects_003A_003APlaceholder_003Cclass_0020AssetObjects_003A_003ACoord2DParameter_003E_0020_003E(pkParameterSet, szName, &placeholder_003CAssetObjects_003A_003ACoord2DParameter_003E));
	}

	public unsafe Coord2DCollectionParameter(global::AssetObjects.CollectionParameter* pkParameter)
		: base(pkParameter)
	{
	}
}
