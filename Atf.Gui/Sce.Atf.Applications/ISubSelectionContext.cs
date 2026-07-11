namespace Sce.Atf.Applications;

public interface ISubSelectionContext : ISelectionContext
{
	ISelectionContext SubSelectionContext { get; }
}
