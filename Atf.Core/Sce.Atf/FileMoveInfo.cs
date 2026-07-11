using System;
using System.IO;

namespace Sce.Atf;

public class FileMoveInfo
{
	public readonly FileMoveType Type;

	public readonly string SourcePath;

	public readonly string DestinationPath;

	public bool m_allowOverwrites;

	public bool AllowOverwrites
	{
		get
		{
			return m_allowOverwrites;
		}
		set
		{
			m_allowOverwrites = value;
		}
	}

	public FileMoveInfo(FileMoveType type, string sourcePath, string destinationPath)
		: this(type, sourcePath, destinationPath, allowOverwrites: false)
	{
	}

	public FileMoveInfo(FileMoveType type, string sourcePath, string destinationPath, bool allowOverwrites)
	{
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (destinationPath == null && type != FileMoveType.Delete)
		{
			throw new ArgumentNullException("destinationPath");
		}
		if (sourcePath != null && destinationPath != null && string.IsNullOrEmpty(Path.GetFileName(sourcePath)) != string.IsNullOrEmpty(Path.GetFileName(destinationPath)))
		{
			throw new InvalidOperationException("source and destination paths must both be directories or files");
		}
		Type = type;
		SourcePath = sourcePath;
		DestinationPath = destinationPath;
		m_allowOverwrites = allowOverwrites;
	}
}
