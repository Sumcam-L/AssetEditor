using System;

namespace Firaxis.CivTech;

public interface IToolHostLoaderService
{
	string ToolHostDllPath { get; }

	IToolHostInterface ToolHostInterface { get; }

	event EventHandler<ToolHostEventArgs> Loaded;

	event EventHandler<ToolHostEventArgs> Unloaded;

	bool LoadToolHost();

	void UnloadToolHost();
}
