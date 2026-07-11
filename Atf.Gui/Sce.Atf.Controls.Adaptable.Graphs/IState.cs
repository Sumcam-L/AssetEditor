namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IState : IGraphNode
{
	StateType Type { get; }

	StateIndicators Indicators { get; }
}
