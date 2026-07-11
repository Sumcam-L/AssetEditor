namespace Sce.Atf.Rendering;

public interface IAnimData
{
	int NumKeys { get; set; }

	int KeyStride { get; set; }

	float TimeOffset { get; set; }

	float Duration { get; set; }

	float[] KeyValues { get; set; }

	float[] KeyTimes { get; set; }

	float[] Tangents { get; set; }

	string InterpolationType { get; set; }
}
