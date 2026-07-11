using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Models;

internal sealed class SettingsLoadSaveViewModel : DialogViewModelBase
{
	private SettingsAction m_action;

	private static readonly PropertyChangedEventArgs ActionArgs = ObservableUtil.CreateArgs((FindFileDialogViewModel x) => x.Action);

	private readonly SettingsService m_settingsService;

	private readonly SaveFileDialog m_saveDialog;

	private readonly OpenFileDialog m_openDialog;

	public SettingsAction Action
	{
		get
		{
			return m_action;
		}
		set
		{
			m_action = value;
			OnPropertyChanged(ActionArgs);
		}
	}

	public SettingsLoadSaveViewModel(SettingsService settings)
	{
		base.Title = "Load and Save Settings".Localize();
		if (settings == null)
		{
			throw new ArgumentNullException();
		}
		m_settingsService = settings;
		m_saveDialog = new SaveFileDialog();
		m_saveDialog.OverwritePrompt = true;
		m_saveDialog.Title = "Export settings";
		m_saveDialog.Filter = "Setting file(*.xml)|*.xml";
		m_openDialog = new OpenFileDialog();
		m_openDialog.Title = "Import settings";
		m_openDialog.CheckFileExists = true;
		m_openDialog.CheckPathExists = true;
		m_openDialog.Multiselect = false;
		m_openDialog.Filter = "Setting file(*.xml)|*.xml";
		Action = SettingsAction.Save;
	}

	protected override void OnCloseDialog(CloseDialogEventArgs args)
	{
		if (args.DialogResult == true)
		{
			if (Action == SettingsAction.Save)
			{
				if (m_saveDialog.ShowCommonDialogWorkaround() != true)
				{
					return;
				}
				SaveSettings(m_saveDialog.FileName);
			}
			else if (Action == SettingsAction.Load)
			{
				bool? flag = m_openDialog.ShowCommonDialogWorkaround();
				bool flag2 = true;
				if (flag == true != flag2 || !flag.HasValue || !LoadSettings(m_openDialog.FileName))
				{
					return;
				}
			}
		}
		RaiseCloseDialog(args);
	}

	private void SaveSettings(string fullFileName)
	{
		Stream stream = null;
		try
		{
			stream = File.Create(fullFileName);
			m_settingsService.SerializeInternal(stream);
		}
		finally
		{
			stream?.Close();
		}
	}

	private bool LoadSettings(string fullFileName)
	{
		bool flag = false;
		Stream stream = null;
		try
		{
			stream = File.OpenRead(fullFileName);
			flag = m_settingsService.DeserializeInternal(stream);
			if (flag)
			{
				m_settingsService.SaveSettings();
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
		finally
		{
			stream?.Close();
		}
		return flag;
	}
}
