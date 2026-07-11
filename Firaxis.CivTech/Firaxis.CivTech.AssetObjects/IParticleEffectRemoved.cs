namespace Firaxis.CivTech.AssetObjects;

public interface IParticleEffectRemoved : IEntityChangedEvent
{
	string ParticleEffectName { get; set; }
}
