namespace Firaxis.Granny;

public interface IGrannyBone
{
	string Name { get; }

	int ParentIndex { get; }

	IGrannyTransform LocalTransform { get; }

	float[] InverseWorldTransform { get; }

	float LODError { get; }
}
