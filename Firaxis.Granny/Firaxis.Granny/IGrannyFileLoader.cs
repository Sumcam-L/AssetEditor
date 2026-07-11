using System;
using Firaxis.CivTech;

namespace Firaxis.Granny;

public interface IGrannyFileLoader : IAssemblyInstance, IDisposable
{
	IGrannyFile LoadGrannyFile(string file);

	IGrannyFile CreateEmptyGrannyFile(string file);

	bool LoadStringDatabase(string file);
}
