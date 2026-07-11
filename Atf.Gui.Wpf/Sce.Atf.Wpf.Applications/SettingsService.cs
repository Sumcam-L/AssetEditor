using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(ISettingsService))]
[Export(typeof(SettingsServiceBase))]
[Export(typeof(SettingsService))]
[Export(typeof(ISettingsPathsProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SettingsService : SettingsServiceBase, IPartImportsSatisfiedNotification
{
	private class TreeView : Tree<object>, ITreeView, IItemView
	{
		public object Root => this;

		public TreeView(object root)
			: base(root)
		{
		}

		public IEnumerable<object> GetChildren(object parent)
		{
			foreach (Tree<object> child in ((Tree<object>)parent).Children)
			{
				yield return child;
			}
		}

		public void GetInfo(object item, ItemInfo info)
		{
			object value = ((Tree<object>)item).Value;
			if (value is string)
			{
				info.Label = (string)value;
				info.AllowSelect = false;
				return;
			}
			UserSettingsInfo userSettingsInfo = value as UserSettingsInfo;
			info.Label = userSettingsInfo.Name;
			info.AllowLabelEdit = false;
			info.IsLeaf = true;
		}
	}

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow = null;

	private readonly TreeView m_userSettings = new TreeView(string.Empty);

	protected override ITreeView UserSettings => m_userSettings;

	internal ITreeView UserSettingsInternal => m_userSettings;

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_mainWindow.Loading += mainWindow_Loaded;
		m_mainWindow.Closed += mainWindow_Closed;
		string directoryName = Path.GetDirectoryName(base.SettingsPath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
	}

	public override void PresentUserSettings(string pathName)
	{
		SettingsDialogViewModel viewModel = new SettingsDialogViewModel(this, pathName);
		SettingsDialog dialog = new SettingsDialog(viewModel);
		if (dialog.ShowParentedDialog() == true)
		{
			SaveSettings();
		}
	}

	protected override void PresentLoadSaveSettings()
	{
		new SettingsLoadSaveDialog(new SettingsLoadSaveViewModel(this)).ShowParentedDialog();
	}

	internal List<PropertyDescriptor> GetPropertiesInternal(Tree<object> tree)
	{
		return GetProperties(tree);
	}

	internal Path<object> GetSettingsPathInternal(string pathName)
	{
		return GetSettingsPath(pathName);
	}

	internal void SerializeInternal(Stream stream)
	{
		Serialize(stream);
	}

	internal bool DeserializeInternal(Stream stream)
	{
		return Deserialize(stream);
	}

	private void mainWindow_Loaded(object sender, EventArgs e)
	{
		Initialize();
	}

	private void mainWindow_Closed(object sender, EventArgs e)
	{
		SaveSettings();
	}
}
