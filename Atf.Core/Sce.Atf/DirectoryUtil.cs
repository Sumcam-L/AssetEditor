using System.Collections.Generic;
using System.IO;

namespace Sce.Atf;

public static class DirectoryUtil
{
	public static string[] GetFiles(string rootPath)
	{
		return GetFiles(rootPath, "*", SearchOption.AllDirectories);
	}

	public static string[] GetFiles(string rootPath, string searchPattern)
	{
		return GetFiles(rootPath, searchPattern, SearchOption.AllDirectories);
	}

	public static string[] GetFiles(string rootPath, string searchPattern, SearchOption option)
	{
		List<string> list = new List<string>();
		list.AddRange(Directory.GetFiles(rootPath, searchPattern, option));
		return list.ToArray();
	}

	public static IEnumerable<string> GetFilesIteratively(string rootPath)
	{
		return GetFilesIteratively(rootPath, "*.*");
	}

	public static IEnumerable<string> GetFilesIteratively(string rootPath, string searchPattern)
	{
		string[] files = Directory.GetFiles(rootPath, searchPattern);
		for (int i = 0; i < files.Length; i++)
		{
			yield return files[i];
		}
		string[] directories = Directory.GetDirectories(rootPath);
		foreach (string d in directories)
		{
			foreach (string item in GetFilesIteratively(d, searchPattern))
			{
				yield return item;
			}
		}
	}
}
