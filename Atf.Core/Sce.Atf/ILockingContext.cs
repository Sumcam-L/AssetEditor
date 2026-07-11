namespace Sce.Atf;

public interface ILockingContext
{
	bool IsLocked(object item);

	bool CanSetLocked(object item);

	void SetLocked(object item, bool value);
}
