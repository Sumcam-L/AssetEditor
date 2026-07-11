using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface ICodecLister : IAssemblyInstance, IDisposable
{
	IEnumerable<string> AvailableVideoCodecs { get; }

	long GetCodecFCCHandler(string codecName);
}
