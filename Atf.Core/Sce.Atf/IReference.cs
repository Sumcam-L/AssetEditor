namespace Sce.Atf;

public interface IReference<T>
{
	T Target { get; set; }

	bool CanReference(T item);
}
