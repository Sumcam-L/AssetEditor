using Firaxis.Utility;

namespace Firaxis.CivTech.AssetObjects;

public interface IDynamicEnum
{
	string EnumerationProperty { get; set; }

	Receipt SetEnumeratorInstance(object inst);
}
