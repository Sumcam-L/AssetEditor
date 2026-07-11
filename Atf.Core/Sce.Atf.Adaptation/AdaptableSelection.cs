namespace Sce.Atf.Adaptation;

public class AdaptableSelection<T> : Selection<T> where T : class
{
	protected override U Convert<U>(T item)
	{
		return item.As<U>();
	}
}
