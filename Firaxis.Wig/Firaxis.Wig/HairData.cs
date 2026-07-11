using System.Collections.Generic;

namespace Firaxis.Wig;

public class HairData
{
	public string name;

	public float[] position;

	public float[] rotation;

	public float[] scale;

	public int numberOfHairSystems;

	public List<HairSystem> HairSystemList;

	public HairData()
	{
		name = string.Empty;
		position = new float[3];
		rotation = new float[3];
		scale = new float[3];
		numberOfHairSystems = 0;
		HairSystemList = new List<HairSystem>();
	}
}
