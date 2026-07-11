namespace Sce.Atf;

public static class Global<T> where T : class, new()
{
	public static readonly T Instance = new T();
}
