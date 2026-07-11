namespace Sce.Atf.Adaptation;

public class AdaptableActiveCollection<T> : ActiveCollection<T> where T : class
{
	public AdaptableActiveCollection()
	{
	}

	public AdaptableActiveCollection(int maximumCount)
		: base(maximumCount)
	{
	}

	protected override U Convert<U>(T item)
	{
		return item.As<U>();
	}
}
