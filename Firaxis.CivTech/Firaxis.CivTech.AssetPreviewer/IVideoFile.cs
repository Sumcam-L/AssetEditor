using System;
using System.Drawing;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IVideoFile : IAssemblyInstance, IDisposable
{
	string FilePath { get; }

	void AddFrame(Bitmap bitmap);

	void SaveFile();
}
