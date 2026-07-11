namespace Sce.Atf.Applications;

public interface ISearchableContext
{
	ISearchUI SearchUI { get; }

	IReplaceUI ReplaceUI { get; }

	IResultsUI ResultsUI { get; }
}
