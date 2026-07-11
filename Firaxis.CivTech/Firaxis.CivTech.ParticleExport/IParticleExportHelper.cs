using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.ParticleExport;

public interface IParticleExportHelper : IAssemblyInstance, IDisposable
{
	IEnumerable<KeyValuePair<ParticleAssetType, string>> GetParticleEffectAssets(string filename);
}
