using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ObjectCollectionParameter : CollectionParameter, IObjectCollectionParameter
{
	public unsafe ObjectCollectionParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName, InstanceType eInstanceType)
	{
		global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)eInstanceType;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Placeholder_003CAssetObjects_003A_003AObjectParameter_003E placeholder_003CAssetObjects_003A_003AObjectParameter_003E);
		base._002Ector(global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACollectionParameter_002Cstruct_0020AssetObjects_003A_003APlaceholder_003Cclass_0020AssetObjects_003A_003AObjectParameter_003E_002Cenum_0020AssetObjects_003A_003AInstanceType_003E(pkParameterSet, szName, &placeholder_003CAssetObjects_003A_003AObjectParameter_003E, &instanceType));
	}

	public unsafe ObjectCollectionParameter(global::AssetObjects.CollectionParameter* pkParameter)
		: base(pkParameter)
	{
	}
}
