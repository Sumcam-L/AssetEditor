using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BLPEntryCollectionParameter : CollectionParameter, IBLPEntryCollectionParameter
{
	public unsafe BLPEntryCollectionParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
	{
		bool flag = true;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Placeholder_003CAssetObjects_003A_003ABLPEntryParameter_003E placeholder_003CAssetObjects_003A_003ABLPEntryParameter_003E);
		base._002Ector(global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionParameter_002Cstruct_0020AssetObjects_003A_003APlaceholder_003Cclass_0020AssetObjects_003A_003ABLPEntryParameter_003E_002Cbool_003E(pkParameterSet, szName, &placeholder_003CAssetObjects_003A_003ABLPEntryParameter_003E, &flag));
	}

	public unsafe BLPEntryCollectionParameter(global::AssetObjects.CollectionParameter* pkParameter)
		: base(pkParameter)
	{
	}
}
