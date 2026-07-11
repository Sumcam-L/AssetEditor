using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEnumParameter : IParameter
{
	IList<string> GetEnumerations();
}
