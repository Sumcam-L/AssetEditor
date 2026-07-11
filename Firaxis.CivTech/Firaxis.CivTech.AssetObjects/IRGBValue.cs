using System.Drawing;

namespace Firaxis.CivTech.AssetObjects;

public interface IRGBValue : IValue
{
	Color ParameterValue { get; set; }
}
