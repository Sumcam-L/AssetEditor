namespace Sce.Atf.Applications;

public interface IInstancingContext
{
	bool CanCopy();

	object Copy();

	bool CanInsert(object dataObject);

	void Insert(object dataObject);

	bool CanDelete();

	void Delete();
}
