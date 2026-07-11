namespace Firaxis.CivTech.AssetObjects;

public interface ITextInputFilter : IEntityFilterDefinition
{
	string FilterText { get; set; }
}
