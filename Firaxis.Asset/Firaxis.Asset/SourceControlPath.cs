using System;
using System.IO;
using Firaxis.IO;

namespace Firaxis.Asset;

public class SourceControlPath : IPath, ICloneable, IEquatable<string>, IEquatable<SourceControlPath>
{
	private string _path = null;

	private SourceControlPath()
	{
	}

	public SourceControlPath(string path)
	{
		SetPath(path);
	}

	public static string SanitizePath(string path)
	{
		if (!SourceControl.IsSourceControlPath(path))
		{
			throw new ArgumentException("The path is not a valid source control path.");
		}
		if (Path.GetDirectoryName(path).IndexOfAny(Path.GetInvalidPathChars()) != -1 || Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new IOException("The path contains illegal characters.");
		}
		return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLower();
	}

	public void SetPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("An empty string cannot be a source control path.");
		}
		_path = SanitizePath(path);
	}

	public override string ToString()
	{
		return _path;
	}

	public object Clone()
	{
		return new SourceControlPath(_path);
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
		if (obj is SourceControlPath)
		{
			return Equals((SourceControlPath)obj);
		}
		if (obj is string)
		{
			return Equals((string)obj);
		}
		return base.Equals(obj);
	}

	public bool Equals(SourceControlPath other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return string.Equals(_path, other._path, StringComparison.OrdinalIgnoreCase);
	}

	public static bool Equals(SourceControlPath path1, SourceControlPath path2)
	{
		return path1?.Equals(path2) ?? ((object)path2 == null);
	}

	public static bool operator ==(SourceControlPath path1, SourceControlPath path2)
	{
		return Equals(path1, path2);
	}

	public static bool operator !=(SourceControlPath path1, SourceControlPath path2)
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

	public static bool Equals(SourceControlPath path1, string path2)
	{
		return path1?.Equals(path2) ?? (path2 == null);
	}

	public static bool Equals(string path1, SourceControlPath path2)
	{
		return Equals(path2, path1);
	}
}
