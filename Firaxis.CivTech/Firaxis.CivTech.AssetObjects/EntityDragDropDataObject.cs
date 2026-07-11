namespace Firaxis.CivTech.AssetObjects;

public class EntityDragDropDataObject
{
	public readonly EntityID ID;

	public readonly string ClassName;

	public EntityDragDropDataObject(string name, InstanceType type, string className)
	{
		ID = new EntityID(name, type);
		ClassName = className;
	}

	public EntityDragDropDataObject(EntityID id, string className)
	{
		ID = id;
		ClassName = className;
	}
}
