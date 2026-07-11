using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Applications.WebServices;

[Export(typeof(IInitializable))]
[Export(typeof(VersionUpdateService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class VersionUpdateService : ICommandClient, IInitializable, IPartImportsSatisfiedNotification
{
	private enum Command
	{
		HelpCheckForUpdate
	}

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow = null;

	private VersionCheck m_updateCheck;

	private bool m_checkForUpdateAtStartup = true;

	private bool m_assemblyMappingFound;

	private bool m_notifyUserIfNoUpdate;

	[DefaultValue(true)]
	public bool CheckForUpdateAtStartup
	{
		get
		{
			return m_checkForUpdateAtStartup;
		}
		set
		{
			m_checkForUpdateAtStartup = value;
		}
	}

	protected ICommandService CommandService { get; set; }

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		m_mainWindow.Loaded += mainWindow_Loaded;
	}

	void IInitializable.Initialize()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(ProjectMappingAttribute));
		m_assemblyMappingFound = projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0;
		if (m_assemblyMappingFound)
		{
			m_updateCheck = new VersionCheck();
			m_updateCheck.CheckComplete += updateCheck_CheckComplete;
			if (m_commandService != null)
			{
				m_commandService.RegisterCommand(Command.HelpCheckForUpdate, StandardMenu.Help, StandardCommandGroup.HelpUpdate, "Check for update...".Localize(), "Check for product update".Localize(), Keys.None, null, CommandVisibility.Menu, this);
			}
			RegisterSettings();
		}
	}

	public bool CanDoCommand(object tag)
	{
		return Command.HelpCheckForUpdate.Equals(tag);
	}

	public void DoCommand(object tag)
	{
		if (Command.HelpCheckForUpdate.Equals(tag))
		{
			m_notifyUserIfNoUpdate = true;
			m_updateCheck.Check(async: false);
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}

	protected virtual void RegisterSettings()
	{
		PropertyDescriptor[] properties = new PropertyDescriptor[1]
		{
			new BoundPropertyDescriptor(this, () => CheckForUpdateAtStartup, "Check for update at startup".Localize(), null, "Check for product update at startup".Localize())
		};
		m_settingsService.RegisterSettings(new Guid("{612B1EE5-D591-4707-91CB-E9F5890DDEB2}").ToString(), properties);
		m_settingsService.RegisterUserSettings("Application".Localize(), properties);
	}

	private void mainWindow_Loaded(object sender, EventArgs e)
	{
		if (m_assemblyMappingFound && m_checkForUpdateAtStartup)
		{
			m_updateCheck.Check(async: true);
		}
	}

	private void updateCheck_CheckComplete(string val, bool error)
	{
		if (!error && !string.IsNullOrEmpty(val))
		{
			string message = "There is a newer version of this program available." + Environment.NewLine + string.Format("Your version is {0}".Localize(), m_updateCheck.AppVersion) + "." + Environment.NewLine + string.Format("The most recent version is {0}".Localize(), m_updateCheck.ServerVersion) + "." + Environment.NewLine + Environment.NewLine + "Would you like to download the latest version?".Localize();
			MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Update".Localize(), message, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
			if (messageBoxDialog.ShowDialog() == true)
			{
				try
				{
					Process process = new Process();
					process.StartInfo.FileName = val;
					process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					process.Start();
					process.Dispose();
				}
				catch (Exception ex)
				{
					WpfMessageBox.Show("Cannot open url:".Localize() + val + ".\n" + "Error".Localize() + ":" + ex.Message, "Error".Localize(), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Hand);
				}
			}
		}
		else if (m_notifyUserIfNoUpdate)
		{
			if (error)
			{
				WpfMessageBox.Show(val, "Error".Localize(), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
			}
			else
			{
				WpfMessageBox.Show("This software is up to date.".Localize(), "Updater".Localize(), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
			}
		}
	}
}
