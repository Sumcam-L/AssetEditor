using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IFileDialogService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FileDialogService : IFileDialogService, IInitializable
{
	private string m_firstRunInitialDirectory;

	[Import(AllowDefault = true)]
	private ISettingsPathsProvider m_settingsPathProvider;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	public string InitialDirectory
	{
		get
		{
			if (m_firstRunInitialDirectory == null || !Directory.Exists(m_firstRunInitialDirectory))
			{
				m_firstRunInitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}
			if (string.IsNullOrEmpty(m_firstRunInitialDirectory))
			{
				m_firstRunInitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
			}
			return m_firstRunInitialDirectory;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
			{
				m_firstRunInitialDirectory = value;
			}
		}
	}

	public string ForcedInitialDirectory { get; set; }

	[Import(AllowDefault = true)]
	public IMainWindow MainWindow { get; set; }

	[Import(AllowDefault = true)]
	public Form MainForm { get; set; }

	private string RecentDirectoriesAsXml
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("FileDialogServiceSettings");
			xmlDocument.AppendChild(xmlElement);
			foreach (KeyValuePair<string, string> item in CustomFileDialog.FilterToLastUsedDirectory)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("lastUsedDir");
				xmlElement.PrependChild(xmlElement2);
				xmlElement2.SetAttribute("filter", item.Key);
				xmlElement2.SetAttribute("dir", item.Value);
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			foreach (XmlNode item in documentElement.GetElementsByTagName("lastUsedDir"))
			{
				string value2 = item.Attributes["filter"].Value;
				string value3 = item.Attributes["dir"].Value;
				CustomFileDialog.FilterToLastUsedDirectory[value2] = value3;
			}
		}
	}

	private bool HasRunBeforeOnUserAccount
	{
		get
		{
			if (m_settingsPathProvider != null)
			{
				return File.Exists(m_settingsPathProvider.SettingsPath);
			}
			return true;
		}
	}

	public FileDialogResult OpenFileName(ref string pathName, string filter)
	{
		CustomOpenFileDialog customOpenFileDialog = new CustomOpenFileDialog();
		customOpenFileDialog.Filter = filter;
		customOpenFileDialog.RestoreDirectory = true;
		customOpenFileDialog.ForcedInitialDirectory = GetInitialDirectory();
		DialogResult dialogResult = customOpenFileDialog.ShowDialog(GetDialogOwner());
		if (dialogResult == DialogResult.OK)
		{
			pathName = customOpenFileDialog.FileName;
		}
		return DialogResultToFileDialogResult(dialogResult);
	}

	public FileDialogResult OpenFileNames(ref string[] pathNames, string filter)
	{
		CustomOpenFileDialog customOpenFileDialog = new CustomOpenFileDialog();
		customOpenFileDialog.Filter = filter;
		customOpenFileDialog.RestoreDirectory = true;
		customOpenFileDialog.Multiselect = true;
		customOpenFileDialog.ForcedInitialDirectory = GetInitialDirectory();
		DialogResult dialogResult = customOpenFileDialog.ShowDialog(GetDialogOwner());
		if (dialogResult == DialogResult.OK)
		{
			pathNames = customOpenFileDialog.FileNames;
		}
		return DialogResultToFileDialogResult(dialogResult);
	}

	public FileDialogResult SaveFileName(ref string pathName, string filter)
	{
		CustomSaveFileDialog customSaveFileDialog = new CustomSaveFileDialog();
		customSaveFileDialog.Filter = filter;
		customSaveFileDialog.RestoreDirectory = true;
		pathName = Path.GetFileName(pathName);
		customSaveFileDialog.FileName = pathName;
		customSaveFileDialog.ForcedInitialDirectory = GetInitialDirectory();
		DialogResult dialogResult = customSaveFileDialog.ShowDialog(GetDialogOwner());
		if (dialogResult == DialogResult.OK)
		{
			pathName = customSaveFileDialog.FileName;
		}
		return DialogResultToFileDialogResult(dialogResult);
	}

	public FileDialogResult ConfirmFileClose(string message)
	{
		ConfirmationDialog confirmationDialog = new ConfirmationDialog("Close".Localize("Close file"), message);
		confirmationDialog.YesButtonText = "&Save".Localize("The '&' is optional and means that Alt+S is the keyboard shortcut on this button");
		confirmationDialog.NoButtonText = "&Discard".Localize("The '&' is optional and means that Alt+D is the keyboard shortcut on this button");
		DialogResult result = confirmationDialog.ShowDialog(GetDialogOwner());
		confirmationDialog.Dispose();
		return DialogResultToFileDialogResult(result);
	}

	public bool PathExists(string pathName)
	{
		return File.Exists(pathName);
	}

	void IInitializable.Initialize()
	{
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => RecentDirectoriesAsXml, "RecentDirectories", null, null);
			m_settingsService.RegisterSettings("Sce.Atf.Applications.FileDialogService", boundPropertyDescriptor);
		}
	}

	private IWin32Window GetDialogOwner()
	{
		if (MainWindow != null)
		{
			return MainWindow.DialogOwner;
		}
		if (MainForm != null)
		{
			return MainForm;
		}
		return Form.ActiveForm;
	}

	private string GetInitialDirectory()
	{
		string forcedInitialDirectory = ForcedInitialDirectory;
		if (!string.IsNullOrEmpty(forcedInitialDirectory))
		{
			return forcedInitialDirectory;
		}
		if (!HasRunBeforeOnUserAccount)
		{
			return InitialDirectory;
		}
		return null;
	}

	private FileDialogResult DialogResultToFileDialogResult(DialogResult result)
	{
		return result switch
		{
			DialogResult.Yes => FileDialogResult.Yes, 
			DialogResult.No => FileDialogResult.No, 
			DialogResult.OK => FileDialogResult.OK, 
			DialogResult.Cancel => FileDialogResult.Cancel, 
			_ => FileDialogResult.Cancel, 
		};
	}
}
