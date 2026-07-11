using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ISpline
{
	IEnumerable<ISplineVertex> Vertices { get; }

	int VertexCount { get; }

	string Name { get; set; }

	string ClassName { get; }

	IValueSet CookParameters { get; }

	bool ClosedLoop { get; set; }

	ISplineVertex AppendVertex(float[] position);

	ISplineVertex InsertVertex(int index, float[] position);

	void RemoveVertex(int index);
}
