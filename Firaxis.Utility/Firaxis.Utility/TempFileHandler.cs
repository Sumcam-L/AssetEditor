using System;
using System.IO;

namespace Firaxis.Utility;

public class TempFileHandler : IDisposable
{
	public string FilePath { get; private set; }

	public TempFileHandler()
	{
		FilePath = Path.GetTempFileName();
	}

	public TempFileHandler(string fileName)
	{
		FilePath = Path.Combine(Path.GetTempPath(), fileName);
	}

	public void Dispose()
	{
		if (File.Exists(FilePath))
		{
			File.Delete(FilePath);
		}
	}
}
