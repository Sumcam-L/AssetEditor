using System;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

[Serializable]
public class PsdInvalidException : Exception
{
	public PsdInvalidException()
	{
	}

	public PsdInvalidException(string message)
		: base(message)
	{
	}
}
