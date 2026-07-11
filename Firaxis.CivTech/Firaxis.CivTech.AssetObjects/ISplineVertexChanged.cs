namespace Firaxis.CivTech.AssetObjects;

public interface ISplineVertexChanged : IEntityChangedEvent
{
	string SplineName { get; set; }

	int VertexIndex { get; set; }

	float[] Position { get; set; }
}
