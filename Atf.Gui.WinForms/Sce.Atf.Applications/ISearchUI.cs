namespace Sce.Atf.Applications;

public interface ISearchUI : ISearchableContextUI
{
	void Bind(IQueryableContext queryableContext);
}
