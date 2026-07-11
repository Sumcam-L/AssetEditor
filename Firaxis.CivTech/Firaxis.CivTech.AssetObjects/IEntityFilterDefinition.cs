namespace Firaxis.CivTech.AssetObjects;

public interface IEntityFilterDefinition
{
	string FilterName { get; }

	IEntityFilter CreateFilter();

	IEntityFilterDefinition DeepCopy();
}
