namespace Firaxis.CivTech.AssetObjects;

public class OpenEntityID
{
	public EntityID ID { get; private set; }

	public OpenEntityID(EntityID id)
	{
		ID = id;
	}
}
