namespace Firaxis.Utility.Undo;

public interface IUndo
{
	void PerformUndo();

	void PerformRedo();

	void StoreUndo();

	void StoreRedo();
}
