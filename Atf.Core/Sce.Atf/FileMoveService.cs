using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace Sce.Atf;

[Export(typeof(FileMoveService))]
[Export(typeof(IFileMoveService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FileMoveService : IFileMoveService
{
	private abstract class FileOperation
	{
		public abstract void Rollback();
	}

	private class DirectoryMoveOperation : FileOperation
	{
		private readonly string m_sourcePath;

		private readonly string m_destinationPath;

		public DirectoryMoveOperation(string sourcePath, string destinationPath)
		{
			m_sourcePath = sourcePath;
			m_destinationPath = destinationPath;
		}

		public override void Rollback()
		{
			if (!Directory.Exists(m_sourcePath))
			{
				Directory.Move(m_destinationPath, m_sourcePath);
			}
			Directory.Delete(m_destinationPath);
		}
	}

	private class FileMoveOperation : FileOperation
	{
		private readonly string m_sourcePath;

		private readonly string m_destinationPath;

		public FileMoveOperation(string sourcePath, string destinationPath)
		{
			m_sourcePath = sourcePath;
			m_destinationPath = destinationPath;
		}

		public override void Rollback()
		{
			if (!File.Exists(m_sourcePath))
			{
				File.Move(m_destinationPath, m_sourcePath);
			}
			File.Delete(m_destinationPath);
		}
	}

	private List<FileOperation> m_fileOperations;

	public void AtomicMove(IEnumerable<FileMoveInfo> moves)
	{
		string tempPath = Path.GetTempPath();
		tempPath = Path.Combine(tempPath, "{88056160-E42B-4133-93FF-58A02F9DFDBE}");
		m_fileOperations = new List<FileOperation>();
		try
		{
			int num = 1;
			foreach (FileMoveInfo move in moves)
			{
				string sourcePath = move.SourcePath;
				string text = move.DestinationPath;
				if (string.Equals(sourcePath, text, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}
				if (move.Type == FileMoveType.Delete)
				{
					text = Path.Combine(tempPath, num++.ToString());
					Directory.CreateDirectory(text);
				}
				if ((File.GetAttributes(sourcePath) & FileAttributes.Directory) != 0)
				{
					MoveDirectory(sourcePath, text, move);
					continue;
				}
				if (move.Type == FileMoveType.Delete)
				{
					text = Path.Combine(text, Path.GetFileName(sourcePath));
				}
				MoveFile(sourcePath, text, move);
			}
		}
		catch (IOException)
		{
			for (int num2 = m_fileOperations.Count - 1; num2 >= 0; num2--)
			{
				m_fileOperations[num2].Rollback();
			}
			throw;
		}
		finally
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(tempPath);
			if (directoryInfo.Exists)
			{
				ClearDirectory(directoryInfo);
			}
			m_fileOperations = null;
		}
	}

	private void MoveDirectory(string srcPath, string dstPath, FileMoveInfo move)
	{
		if (!Directory.Exists(srcPath))
		{
			throw new IOException("Can't find source directory".Localize());
		}
		if (!Directory.Exists(dstPath))
		{
			Directory.CreateDirectory(dstPath);
			m_fileOperations.Add(new DirectoryMoveOperation(srcPath, dstPath));
		}
		string[] files = Directory.GetFiles(srcPath);
		foreach (string text in files)
		{
			string path = text.Substring(srcPath.Length + 1);
			MoveFile(text, Path.Combine(dstPath, path), move);
		}
		string[] directories = Directory.GetDirectories(srcPath);
		foreach (string text2 in directories)
		{
			string path2 = text2.Substring(srcPath.Length + 1);
			MoveDirectory(text2, Path.Combine(dstPath, path2), move);
		}
		if (move.Type == FileMoveType.Delete || move.Type == FileMoveType.Move)
		{
			Directory.Delete(srcPath);
		}
	}

	private void MoveFile(string srcPath, string dstPath, FileMoveInfo move)
	{
		if (!File.Exists(srcPath))
		{
			throw new IOException("Can't find source file".Localize());
		}
		if (File.Exists(dstPath))
		{
			if (move.Type == FileMoveType.Delete)
			{
				File.SetAttributes(dstPath, FileAttributes.Normal);
				File.Delete(dstPath);
			}
			else if (!move.AllowOverwrites)
			{
				throw new IOException("Can't overwrite existing file".Localize());
			}
		}
		switch (move.Type)
		{
		case FileMoveType.Move:
			File.Move(srcPath, dstPath);
			break;
		case FileMoveType.Copy:
			File.Copy(srcPath, dstPath, move.AllowOverwrites);
			break;
		case FileMoveType.Delete:
			File.Move(srcPath, dstPath);
			break;
		default:
			throw new InvalidOperationException("illegal FileMoveType value");
		}
		m_fileOperations.Add(new FileMoveOperation(srcPath, dstPath));
	}

	private static void ClearDirectory(DirectoryInfo info)
	{
		try
		{
			FileInfo[] files = info.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				fileInfo.IsReadOnly = false;
				fileInfo.Delete();
			}
			DirectoryInfo[] directories = info.GetDirectories();
			foreach (DirectoryInfo info2 in directories)
			{
				ClearDirectory(info2);
			}
			info.Delete();
		}
		catch (IOException)
		{
		}
		catch (AccessViolationException)
		{
		}
	}
}
