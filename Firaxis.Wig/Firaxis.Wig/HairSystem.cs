using System.Collections.Generic;

namespace Firaxis.Wig;

public class HairSystem
{
	public string name;

	public float[] position;

	public float[] rotation;

	public float[] scale;

	public int numberOfHairStrands;

	public List<HairStrand> HairStrandList;

	public HairSystem()
	{
		name = string.Empty;
		position = new float[3];
		rotation = new float[3];
		scale = new float[3];
		numberOfHairStrands = 0;
		HairStrandList = new List<HairStrand>();
	}
}
