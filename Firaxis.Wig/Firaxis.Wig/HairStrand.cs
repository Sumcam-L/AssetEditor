using System.Collections.Generic;

namespace Firaxis.Wig;

public class HairStrand
{
	public string name;

	public float[] position;

	public float[] rotation;

	public float[] scale;

	public float[] UV;

	public int numberOfBoneNames;

	public List<string> BoneNameList;

	public int numberOfControlPoints;

	public List<ControlPoint> ControlPointList;

	public HairStrand()
	{
		name = string.Empty;
		position = new float[3];
		rotation = new float[3];
		scale = new float[3];
		UV = new float[2];
		numberOfBoneNames = 0;
		numberOfControlPoints = 0;
		ControlPointList = new List<ControlPoint>();
		BoneNameList = new List<string>();
	}
}
