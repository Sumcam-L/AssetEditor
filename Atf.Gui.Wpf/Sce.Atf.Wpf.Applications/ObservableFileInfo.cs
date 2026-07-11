using System.IO;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

public class ObservableFileInfo : NotifyPropertyChangedBase
{
	private bool m_isChanged;

	private string m_fileName;

	private FileInfo m_fileInfo;

	public FileInfo FileInfo
	{
		get
		{
			return m_fileInfo;
		}
		private set
		{
			if (m_fileInfo != value)
			{
				m_fileInfo = value;
				RaisePropertyChanged("FileInfo");
			}
		}
	}

	public bool IsChanged
	{
		get
		{
			return m_isChanged;
		}
		private set
		{
			if (m_isChanged != value)
			{
				m_isChanged = value;
				RaisePropertyChanged("IsChanged");
			}
		}
	}

	public ObservableFileInfo(string file)
	{
		m_fileName = file;
		TakeSnapshot();
	}

	public void ChangeName(string newName)
	{
		m_fileName = newName;
		Refresh();
	}

	public void Refresh()
	{
		TakeSnapshot();
		MarkAsChanged();
	}

	private void MarkAsChanged()
	{
		IsChanged = true;
		IsChanged = false;
	}

	private void TakeSnapshot()
	{
		FileInfo = new FileInfo(m_fileName);
	}
}
