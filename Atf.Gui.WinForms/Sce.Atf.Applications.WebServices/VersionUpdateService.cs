using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.WebServices;

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
	private ICommandService m_commandService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	private VersionCheck m_updateCheck;

	private IWin32Window m_dialogOwner;

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

	protected Form MainForm
	{
		get
		{
			return m_mainForm;
		}
		set
		{
			m_mainForm = value;
		}
	}

	protected ICommandService CommandService
	{
		get
		{
			return m_commandService;
		}
		set
		{
			m_commandService = value;
		}
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_dialogOwner = m_mainWindow.DialogOwner;
		m_mainWindow.Loaded += mainWindow_Loaded;
	}

	void IInitializable.Initialize()
	{
		Assembly element = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(element, typeof(ProjectMappingAttribute));
		m_assemblyMappingFound = projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0;
		if (m_assemblyMappingFound)
		{
			m_updateCheck = new VersionCheck();
			m_updateCheck.CheckComplete += updateCheck_CheckComplete;
			m_commandService.RegisterCommand(new CommandInfo(Command.HelpCheckForUpdate, StandardMenu.Help, StandardCommandGroup.HelpUpdate, "Check for update...".Localize(), "Check for product update".Localize()), this);
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
			string newLine = Environment.NewLine;
			DialogResult dialogResult = MessageBox.Show(m_dialogOwner, "There is a newer version of this program available.".Localize() + newLine + string.Format("Your version is {0}".Localize(), m_updateCheck.AppVersion) + "." + newLine + string.Format("The most recent version is {0}".Localize(), m_updateCheck.ServerVersion) + "." + newLine + newLine + "Would you like to download the latest version?".Localize(), "Update".Localize("this is the title of a dialog box"), MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
			if (dialogResult == DialogResult.Yes)
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
					MessageBox.Show(m_dialogOwner, "Cannot open url:".Localize() + val + ".\n" + "Error".Localize() + ":" + ex.Message, "Error".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		else if (m_notifyUserIfNoUpdate)
		{
			if (error)
			{
				MessageBox.Show(m_dialogOwner, val, "Error".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				MessageBox.Show(m_dialogOwner, "This software is up to date.".Localize(), "Updater".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}
	}
}
