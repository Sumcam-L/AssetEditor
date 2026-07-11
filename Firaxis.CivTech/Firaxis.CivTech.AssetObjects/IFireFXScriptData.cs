using Firaxis.CivTech.FireFX;

namespace Firaxis.CivTech.AssetObjects;

public interface IFireFXScriptData : IFireFXInstanceData
{
	string Script { get; set; }

	IFireFXEffect CompiledScript { get; }
}
