namespace Sce.Atf;

public interface IReplacingQueryPattern
{
	bool Matches(IQueryMatch itemToMatch);

	void Replace(IQueryMatch itemToReplace, object replaceWith);
}
