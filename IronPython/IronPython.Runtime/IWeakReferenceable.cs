namespace IronPython.Runtime;

public interface IWeakReferenceable
{
	WeakRefTracker GetWeakRef();

	bool SetWeakRef(WeakRefTracker value);

	void SetFinalizer(WeakRefTracker value);
}
