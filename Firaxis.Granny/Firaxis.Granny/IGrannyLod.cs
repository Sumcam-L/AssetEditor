namespace Firaxis.Granny;

public interface IGrannyLod
{
	int TargetIndexCount { get; set; }

	float TransitionArea { get; set; }

	float Reduction { get; set; }
}
