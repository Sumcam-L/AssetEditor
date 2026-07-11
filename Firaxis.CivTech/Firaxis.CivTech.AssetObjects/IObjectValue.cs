namespace Firaxis.CivTech.AssetObjects;

public interface IObjectValue : IValue
{
	string GetBoundObjectName();

	InstanceType GetBoundObjectType();

	void BindObject(string name, InstanceType type);
}
