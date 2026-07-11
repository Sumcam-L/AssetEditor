using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IStateGraphReader : IAssemblyInstance, IDisposable
{
	bool Load(string sFilename, IStateGraphContainer container);
}
