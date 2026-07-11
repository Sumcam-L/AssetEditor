using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sce.Atf;

public static class PathUtil
{
	public static readonly char[] DirectorySeparatorChars = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private const int FILE_ATTRIBUTE_DIRECTORY = 16;

	private const int MAX_PATH = 260;

	private static readonly char[] Slashes = new char[2] { '/', '\\' };

	public static string GetRelativePath(string absolutePath, string basePath)
	{
		basePath = basePath.TrimEnd(Slashes);
		absolutePath = absolutePath.TrimEnd(Slashes);
		StringBuilder stringBuilder = new StringBuilder(260);
		if (PathRelativePathTo(stringBuilder, basePath, 16, absolutePath, 16))
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public static string GetAbsolutePath(string path, string basePath)
	{
		string src = Path.Combine(basePath, path);
		StringBuilder stringBuilder = new StringBuilder(260);
		if (PathCanonicalize(stringBuilder, src))
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public static string GetCulturePath(string defaultPath)
	{
		string twoLetterISOLanguageName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
		string name = Thread.CurrentThread.CurrentUICulture.Name;
		string fileName = Path.GetFileName(defaultPath);
		string directoryName = Path.GetDirectoryName(defaultPath);
		string path = Path.Combine(directoryName, name);
		string text = Path.Combine(path, fileName);
		if (File.Exists(text))
		{
			return text;
		}
		if (twoLetterISOLanguageName != name)
		{
			path = Path.Combine(directoryName, twoLetterISOLanguageName);
			text = Path.Combine(path, fileName);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return defaultPath;
	}

	public static bool IsValidPath(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return false;
		}
		if (filePath.Length != filePath.Trim().Length)
		{
			return false;
		}
		if (filePath.Length > 260)
		{
			return false;
		}
		if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
		{
			return false;
		}
		if (filePath.EndsWith("."))
		{
			return false;
		}
		int num = filePath.IndexOf(':');
		if (num >= 0)
		{
			if (filePath.Length > num + 1 && filePath[num + 1] != '\\' && filePath[num + 1] != '/')
			{
				return false;
			}
			if (filePath.LastIndexOf(':') != num)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsRelative(string path)
	{
		return PathIsRelative(path);
	}

	public static bool IsAbsolute(string path)
	{
		return !PathIsRelative(path);
	}

	public static string GetCanonicalPath(Uri uri)
	{
		string absolutePath = uri.AbsolutePath;
		absolutePath = Uri.UnescapeDataString(absolutePath);
		return GetCanonicalPath(absolutePath);
	}

	public static string GetCanonicalPath(string path)
	{
		path = Path.GetFullPath(path);
		string lpDeviceName = path.Substring(0, 2);
		StringBuilder stringBuilder = new StringBuilder(256);
		Kernel32.QueryDosDeviceW(lpDeviceName, stringBuilder, 256u);
		string text = stringBuilder.ToString();
		if (text.StartsWith("\\??\\"))
		{
			path = text.Substring("\\??\\".Length) + path.Substring(2);
		}
		return path;
	}

	public static string GetCompactedPath(string path, int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		PathCompactPathEx(stringBuilder, path, length, 0);
		return stringBuilder.ToString();
	}

	public static string GetLastElement(string path)
	{
		int num = path.Length - 1;
		while (num >= 0 && (path[num] == Path.DirectorySeparatorChar || path[num] == Path.AltDirectorySeparatorChar))
		{
			num--;
		}
		int num2;
		for (num2 = num - 1; num2 >= 0; num2--)
		{
			if (path[num2] == Path.DirectorySeparatorChar || path[num2] == Path.AltDirectorySeparatorChar)
			{
				num2++;
				break;
			}
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num <= num2)
		{
			return string.Empty;
		}
		return path.Substring(num2, num - num2 + 1);
	}

	public static string[] GetPathElements(string path)
	{
		return path.Split(DirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
	}

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
	private static extern bool PathRelativePathTo(StringBuilder path, string from, int attrFrom, string to, int attrTo);

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
	private static extern bool PathCanonicalize(StringBuilder path, string src);

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
	private static extern bool PathIsRelative(string path);

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
	private static extern bool PathCompactPathEx([Out] StringBuilder resultPath, string originalPath, int maxResultChars, int dwFlags);
}
