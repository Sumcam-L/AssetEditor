using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPlaybackService : IAssemblyInstance, IDisposable
{
	ITracePlayer OpenTracePlayer(ICivTechService civ, string TracePath);

	string DisassembleTraceFile(string TracePath);
}
