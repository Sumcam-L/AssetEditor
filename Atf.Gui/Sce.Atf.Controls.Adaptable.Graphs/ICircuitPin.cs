namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitPin : IEdgeRoute
{
	string Name { get; }

	string TypeName { get; }

	int Index { get; }
}
