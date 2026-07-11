using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IStateGraphWriter : IStateGraphContainer, IAssemblyInstance, IDisposable
{
	bool Save(string sFilename);
}
