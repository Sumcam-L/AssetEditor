using System.Drawing;

namespace Firaxis.CivTech.AssetObjects;

public interface ICoord2DValue : IValue
{
	PointF ParameterValue { get; set; }
}
