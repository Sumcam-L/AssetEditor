namespace Firaxis.Granny;

public interface IGrannyTransform
{
	ETransformFlags Flags { get; }

	float[] Position { get; }

	float[] Orientation { get; }

	float[] ScaleShear { get; }
}
