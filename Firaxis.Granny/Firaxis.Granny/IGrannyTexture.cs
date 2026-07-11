using System;

namespace Firaxis.Granny;

public interface IGrannyTexture : IDisposable
{
	string FromFileName { get; set; }
}
