using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Sce.Atf;

public class FileSystemResourceFolder : IResourceFolder
{
	private string m_name;

	private readonly string m_path;

	private readonly IResourceFolder m_parent;

	public virtual IList<IResourceFolder> Folders
	{
		get
		{
			string[] array = null;
			try
			{
				array = Directory.GetDirectories(m_path);
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (ArgumentNullException)
			{
			}
			catch (ArgumentException)
			{
			}
			catch (PathTooLongException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (IOException)
			{
			}
			finally
			{
				if (array == null)
				{
					array = new string[0];
				}
			}
			List<IResourceFolder> list = new List<IResourceFolder>(array.Length);
			string[] array2 = array;
			foreach (string path in array2)
			{
				list.Add(new FileSystemResourceFolder(path, this));
			}
			return new ReadOnlyCollection<IResourceFolder>(list);
		}
	}

	public virtual IList<Uri> ResourceUris
	{
		get
		{
			string[] array = null;
			try
			{
				array = Directory.GetFiles(m_path);
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (ArgumentNullException)
			{
			}
			catch (ArgumentException)
			{
			}
			catch (PathTooLongException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (IOException)
			{
			}
			finally
			{
				if (array == null)
				{
					array = new string[0];
				}
			}
			List<Uri> list = new List<Uri>(array.Length);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!PathUtil.GetLastElement(text).StartsWith("~"))
				{
					list.Add(new Uri(text));
				}
			}
			return new ReadOnlyCollection<Uri>(list);
		}
	}

	public IResourceFolder Parent => m_parent;

	public bool ReadOnlyName => true;

	public virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public string Path => m_path;

	public FileSystemResourceFolder(string path)
		: this(path, null)
	{
	}

	private FileSystemResourceFolder(string path, IResourceFolder parent)
	{
		m_path = path;
		m_parent = parent;
		m_name = PathUtil.GetLastElement(path);
	}

	public virtual IResourceFolder CreateFolder()
	{
		return null;
	}
}
