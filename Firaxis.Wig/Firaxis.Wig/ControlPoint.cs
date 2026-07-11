namespace Firaxis.Wig;

public class ControlPoint
{
	public float[] position;

	public int[] boneIndices;

	public float[] boneWeights;

	public ControlPoint()
	{
		position = new float[3];
		boneIndices = new int[8];
		boneWeights = new float[8];
	}
}
