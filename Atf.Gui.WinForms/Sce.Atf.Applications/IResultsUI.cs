namespace Sce.Atf.Applications;

public interface IResultsUI : ISearchableContextUI
{
	void Bind(IQueryableResultContext queryResultContext);
}
