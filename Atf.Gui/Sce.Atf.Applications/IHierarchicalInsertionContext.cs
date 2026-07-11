namespace Sce.Atf.Applications;

public interface IHierarchicalInsertionContext
{
	bool CanInsert(object parent, object child);

	void Insert(object parent, object child);
}
