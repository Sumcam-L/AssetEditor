namespace Firaxis.Granny;

public interface IGrannyTransformTrack
{
	string Name { get; }

	int Flags { get; }

	IGrannyCurve PositionCurve { get; }

	IGrannyCurve OrientationCurve { get; }

	IGrannyCurve ScaleShearCurve { get; }
}
