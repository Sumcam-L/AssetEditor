namespace Firaxis.CivTech.AssetObjects;

public interface IParticleEffectAdded : IEntityChangedEvent
{
	string ParticleEffectName { get; set; }
}
