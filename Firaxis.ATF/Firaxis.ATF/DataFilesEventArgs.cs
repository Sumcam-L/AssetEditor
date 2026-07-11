using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public class DataFilesEventArgs : EventArgs
{
	public readonly IEnumerable<DataFileInfo> DataFileInfos;

	public DataFilesEventArgs(IEnumerable<DataFileInfo> dataFiles)
	{
		DataFileInfos = dataFiles;
	}
}
