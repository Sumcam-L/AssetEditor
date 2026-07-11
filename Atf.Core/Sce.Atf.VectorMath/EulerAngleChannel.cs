namespace Sce.Atf.VectorMath;

public static class EulerAngleChannel
{
	public static bool FreedomInX(this EulerAngleChannels EulerAngleChannels)
	{
		return (EulerAngleChannels & EulerAngleChannels.X) != 0;
	}

	public static bool FreedomInY(this EulerAngleChannels EulerAngleChannels)
	{
		return (EulerAngleChannels & EulerAngleChannels.Y) != 0;
	}

	public static bool FreedomInZ(this EulerAngleChannels EulerAngleChannels)
	{
		return (EulerAngleChannels & EulerAngleChannels.Z) != 0;
	}
}
