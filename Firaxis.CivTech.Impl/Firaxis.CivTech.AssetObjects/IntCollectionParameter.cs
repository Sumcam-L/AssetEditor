using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class IntCollectionParameter : CollectionParameter, IIntCollectionParameter
{
	public unsafe IntCollectionParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Placeholder_003CAssetObjects_003A_003AIntParameter_003E placeholder_003CAssetObjects_003A_003AIntParameter_003E);
		base._002Ector(global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionParameter_002Cstruct_0020AssetObjects_003A_003APlaceholder_003Cclass_0020AssetObjects_003A_003AIntParameter_003E_0020_003E(pkParameterSet, szName, &placeholder_003CAssetObjects_003A_003AIntParameter_003E));
	}

	public unsafe IntCollectionParameter(global::AssetObjects.CollectionParameter* pkParameter)
		: base(pkParameter)
	{
	}
}
