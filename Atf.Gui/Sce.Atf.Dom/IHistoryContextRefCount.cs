namespace Sce.Atf.Dom;

internal interface IHistoryContextRefCount
{
	void Suspend();

	void Resume();
}
