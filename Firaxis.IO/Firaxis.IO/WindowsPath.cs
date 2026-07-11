using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Firaxis.IO;

public class WindowsPath : IPath, ICloneable, IEquatable<string>, IEquatable<WindowsPath>
{
	private string _path = null;

	private static Regex ms_PathRegex = new Regex("^([a-zA-Z]:[\\\\/])([^:*\\\\/?<>\"|]+[\\\\/])*([^:*?\\\\/<>\"|])+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public bool Exists => File.Exists(_path) || Directory.Exists(_path);

	public string DirectoryName
	{
		get
		{
			if (IsDirectory)
			{
				return _path;
			}
			return Path.GetDirectoryName(_path);
		}
	}

	public WindowsPath ParentDirectory => (!string.IsNullOrEmpty(_path)) ? new WindowsPath(Path.GetDirectoryName(_path)) : null;

	public string Extension => Path.GetExtension(_path);

	public string FileName => Path.GetFileName(_path);

	public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(_path);

	public string FullPath => Path.GetFullPath(_path);

	public string PathRoot => Path.GetPathRoot(_path);

	public bool HasExtension => Path.HasExtension(_path);

	public bool IsDirectory
	{
		get
		{
			if (!Exists)
			{
				return !HasExtension;
			}
			return HasAttributes(FileAttributes.Directory);
		}
	}

	public bool IsFile => !IsDirectory;

	public bool PathRooted => Path.IsPathRooted(_path);

	public bool Archive
	{
		get
		{
			return HasAttributes(FileAttributes.Archive);
		}
		set
		{
			UpdateAttributes(FileAttributes.Archive, value);
		}
	}

	public bool Compressed
	{
		get
		{
			return HasAttributes(FileAttributes.Compressed);
		}
		set
		{
			UpdateAttributes(FileAttributes.Compressed, value);
		}
	}

	public bool Device
	{
		get
		{
			return HasAttributes(FileAttributes.Device);
		}
		set
		{
			UpdateAttributes(FileAttributes.Device, value);
		}
	}

	public bool Encrypted
	{
		get
		{
			return HasAttributes(FileAttributes.Encrypted);
		}
		set
		{
			UpdateAttributes(FileAttributes.Encrypted, value);
		}
	}

	public bool Hidden
	{
		get
		{
			return HasAttributes(FileAttributes.Hidden);
		}
		set
		{
			UpdateAttributes(FileAttributes.Hidden, value);
		}
	}

	public bool Normal
	{
		get
		{
			return HasAttributes(FileAttributes.Normal);
		}
		set
		{
			UpdateAttributes(FileAttributes.Normal, value);
		}
	}

	public bool NotContentIndexed
	{
		get
		{
			return HasAttributes(FileAttributes.NotContentIndexed);
		}
		set
		{
			UpdateAttributes(FileAttributes.NotContentIndexed, value);
		}
	}

	public bool Offline
	{
		get
		{
			return HasAttributes(FileAttributes.Offline);
		}
		set
		{
			UpdateAttributes(FileAttributes.Offline, value);
		}
	}

	public bool ReadOnly
	{
		get
		{
			return HasAttributes(FileAttributes.ReadOnly);
		}
		set
		{
			UpdateAttributes(FileAttributes.ReadOnly, value);
		}
	}

	public bool ReparsePoint
	{
		get
		{
			return HasAttributes(FileAttributes.ReparsePoint);
		}
		set
		{
			UpdateAttributes(FileAttributes.ReparsePoint, value);
		}
	}

	public bool SparseFile
	{
		get
		{
			return HasAttributes(FileAttributes.SparseFile);
		}
		set
		{
			UpdateAttributes(FileAttributes.SparseFile, value);
		}
	}

	public bool System
	{
		get
		{
			return HasAttributes(FileAttributes.System);
		}
		set
		{
			UpdateAttributes(FileAttributes.System, value);
		}
	}

	public bool Temporary
	{
		get
		{
			return HasAttributes(FileAttributes.Temporary);
		}
		set
		{
			UpdateAttributes(FileAttributes.Temporary, value);
		}
	}

	private WindowsPath()
	{
	}

	public WindowsPath(string path)
	{
		SetPath(path);
	}

	public static bool IsWellFormed(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		MatchCollection matchCollection = ms_PathRegex.Matches(s);
		foreach (Match item in matchCollection)
		{
			if (item.Value == s)
			{
				return true;
			}
		}
		return false;
	}

	public static WindowsPath Combine(WindowsPath path1, WindowsPath path2)
	{
		return Combine(path1.ToString(), path2.ToString());
	}

	public static WindowsPath Combine(WindowsPath path1, string path2)
	{
		return Combine(path1.ToString(), path2);
	}

	public static WindowsPath Combine(string path1, WindowsPath path2)
	{
		return Combine(path1, path2.ToString());
	}

	public static WindowsPath Combine(string path1, string path2)
	{
		return new WindowsPath(Path.Combine(path1, path2));
	}

	public static string SanitizePath(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		string fileName = Path.GetFileName(path);
		if ((!string.IsNullOrEmpty(directoryName) && directoryName.IndexOfAny(Path.GetInvalidPathChars()) != -1) || (!string.IsNullOrEmpty(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1))
		{
			throw new IOException("The path contains illegal characters.");
		}
		if (path.Length >= 260)
		{
			throw new PathTooLongException($"The path couldn't be sanitized because it was too long:\r\n\t{path}");
		}
		return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLower();
	}

	public void SetPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("An empty string cannot be a Windows path.");
		}
		_path = SanitizePath(path);
	}

	public bool IsWithin(WindowsPath directory)
	{
		if (!PathRooted || !directory.PathRooted)
		{
			throw new ArgumentException("Both paths must be rooted.");
		}
		return _path.StartsWith(directory._path + Path.DirectorySeparatorChar);
	}

	public DirectoryInfo CreateDirectory()
	{
		string directoryName = DirectoryName;
		if (directoryName != null && !Directory.Exists(directoryName))
		{
			return Directory.CreateDirectory(directoryName);
		}
		return null;
	}

	public FileStream CreateFile()
	{
		if (IsFile && !Exists)
		{
			return File.Create(_path);
		}
		return null;
	}

	public bool HasAttributes(FileAttributes attrs)
	{
		try
		{
			return (File.GetAttributes(_path) & attrs) == attrs;
		}
		catch
		{
			return false;
		}
	}

	public void SetAttributes(FileAttributes attrs)
	{
		FileAttributes fileAttributes = File.GetAttributes(_path) | attrs;
		File.SetAttributes(_path, fileAttributes);
	}

	public void ClearAttributes(FileAttributes attrs)
	{
		FileAttributes fileAttributes = File.GetAttributes(_path) & ~attrs;
		File.SetAttributes(_path, fileAttributes);
	}

	private void UpdateAttributes(FileAttributes attrs, bool bValue)
	{
		if (bValue)
		{
			SetAttributes(attrs);
		}
		else
		{
			ClearAttributes(attrs);
		}
	}

	public override string ToString()
	{
		return _path ?? string.Empty;
	}

	public object Clone()
	{
		return new WindowsPath(_path);
	}

	public override int GetHashCode()
	{
		return _path.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is WindowsPath)
		{
			return Equals((WindowsPath)obj);
		}
		if (obj is string)
		{
			return Equals((string)obj);
		}
		return base.Equals(obj);
	}

	public bool Equals(WindowsPath other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return string.Equals(_path, other._path, StringComparison.OrdinalIgnoreCase);
	}

	public static bool Equals(WindowsPath path1, WindowsPath path2)
	{
		return path1?.Equals(path2) ?? ((object)path2 == null);
	}

	public static bool operator ==(WindowsPath path1, WindowsPath path2)
	{
		return Equals(path1, path2);
	}

	public static bool operator !=(WindowsPath path1, WindowsPath path2)
	{
		return !Equals(path1, path2);
	}

	public bool Equals(string other)
	{
		if (other == null)
		{
			return false;
		}
		try
		{
			return string.Equals(_path, SanitizePath(other), StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	public static bool Equals(WindowsPath path1, string path2)
	{
		return path1?.Equals(path2) ?? (path2 == null);
	}

	public static bool Equals(string path1, WindowsPath path2)
	{
		return Equals(path2, path1);
	}
}
