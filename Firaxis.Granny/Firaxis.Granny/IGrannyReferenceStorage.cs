namespace Firaxis.Granny;

public interface IGrannyReferenceStorage
{
	bool StoredReferences { get; }

	bool StoreReferences();

	bool RestoreReferences();
}
