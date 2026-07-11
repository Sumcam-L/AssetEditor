using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IFileDialogService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FileDialogService : IFileDialogService
{
	private string m_initialDirectory;

	public string InitialDirectory
	{
		get
		{
			if (m_initialDirectory == null)
			{
				m_initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}
			return m_initialDirectory;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
			{
				m_initialDirectory = value;
			}
		}
	}

	public string ForcedInitialDirectory { get; set; }

	public FileDialogResult OpenFileName(ref string pathName, string filter)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = filter;
		openFileDialog.RestoreDirectory = true;
		openFileDialog.InitialDirectory = m_initialDirectory;
		bool? flag = openFileDialog.ShowCommonDialogWorkaround();
		if (flag == true)
		{
			pathName = openFileDialog.FileName;
		}
		return ToFileDialogResult(flag);
	}

	public FileDialogResult OpenFileNames(ref string[] pathNames, string filter)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = filter;
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Multiselect = true;
		openFileDialog.InitialDirectory = m_initialDirectory;
		bool? flag = openFileDialog.ShowCommonDialogWorkaround();
		if (flag == true)
		{
			pathNames = openFileDialog.FileNames;
		}
		return ToFileDialogResult(flag);
	}

	public FileDialogResult SaveFileName(ref string pathName, string filter)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = filter;
		saveFileDialog.RestoreDirectory = true;
		saveFileDialog.FileName = pathName;
		saveFileDialog.InitialDirectory = m_initialDirectory;
		bool? flag = saveFileDialog.ShowDialog(Application.Current.MainWindow);
		if (flag == true)
		{
			pathName = saveFileDialog.FileName;
		}
		return ToFileDialogResult(flag);
	}

	public FileDialogResult ConfirmFileClose(string message)
	{
		ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel("Close".Localize("Close file"), message)
		{
			YesButtonText = "Save".Localize(),
			NoButtonText = "Discard".Localize()
		};
		confirmationDialogViewModel.ShowDialog();
		return ToFileDialogResult(confirmationDialogViewModel.Result);
	}

	public bool PathExists(string pathName)
	{
		return File.Exists(pathName);
	}

	private static FileDialogResult ToFileDialogResult(System.Windows.MessageBoxResult result)
	{
		return result switch
		{
			System.Windows.MessageBoxResult.Yes => FileDialogResult.Yes, 
			System.Windows.MessageBoxResult.No => FileDialogResult.No, 
			System.Windows.MessageBoxResult.OK => FileDialogResult.OK, 
			_ => FileDialogResult.Cancel, 
		};
	}

	private static FileDialogResult ToFileDialogResult(bool? result)
	{
		return (result == true) ? FileDialogResult.OK : FileDialogResult.Cancel;
	}
}
