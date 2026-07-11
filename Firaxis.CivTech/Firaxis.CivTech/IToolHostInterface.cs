using System;

namespace Firaxis.CivTech;

public interface IToolHostInterface : IAssemblyInstance, IDisposable
{
	string DllPath { get; }

	bool IsLoaded { get; }
}
