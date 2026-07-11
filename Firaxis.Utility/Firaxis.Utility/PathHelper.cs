using System.IO;
using System.Linq;

namespace Firaxis.Utility;

public static class PathHelper
{
	private static char[] m_invalidPathCharacters;

	private static char[] m_invalidFileNameCharacters;

	private static char[] InvalidPathCharacters
	{
		get
		{
			if (m_invalidPathCharacters == null)
			{
				m_invalidPathCharacters = Path.GetInvalidPathChars();
			}
			return m_invalidPathCharacters;
		}
	}

	private static char[] InvalidFileNameCharacters
	{
		get
		{
			if (m_invalidFileNameCharacters == null)
			{
				m_invalidFileNameCharacters = Path.GetInvalidFileNameChars();
			}
			return m_invalidFileNameCharacters;
		}
	}

	public static bool IsPathOnNetwork(string filePath)
	{
		string pathRoot = Path.GetPathRoot(filePath);
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			if (driveInfo.Name == pathRoot)
			{
				if (driveInfo.DriveType == DriveType.Network)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public static bool IsValidPath(string filePath)
	{
		return !filePath.Any((char c) => InvalidPathCharacters.Contains(c));
	}
}
