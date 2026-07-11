namespace IronPython.Runtime.Binding;

internal struct FastBindResult<T> where T : class
{
	public readonly T Target;

	public readonly bool ShouldCache;

	public FastBindResult(T target, bool shouldCache)
	{
		Target = target;
		ShouldCache = shouldCache;
	}

	public FastBindResult(T target)
	{
		Target = target;
		ShouldCache = false;
	}
}
