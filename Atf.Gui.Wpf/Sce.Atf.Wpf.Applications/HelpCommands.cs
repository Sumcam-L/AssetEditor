using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(HelpCommands))]
[Export(typeof(ICommandClient))]
[Export(typeof(IContextMenuCommandProvider))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HelpCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	public enum Commands
	{
		Help,
		HelpReleaseNotes,
		HelpAbout
	}

	public enum Groups
	{
		Help
	}

	private class ContextMenuHelpTag
	{
		public int Index { get; set; }
	}

	private const int kMaxContextHelpKeys = 10;

	[Import]
	private ICommandService m_commandService = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	private bool m_enableContextHelpUserSetting = true;

	private bool m_showContextHelp = true;

	private string[] m_lastContextMenuKeys;

	private string m_applicationName;

	private static readonly CommandInfo s_helpCommand = new CommandInfo(Commands.Help, StandardMenu.Help, Groups.Help, "_Contents".Localize("the '_' means that 'C' is a shortcut key"), "Help Contents".Localize(), Sce.Atf.Input.Keys.F1, null, CommandVisibility.Menu);

	private static readonly CommandInfo s_releaseNotesCommand = new CommandInfo(Commands.HelpReleaseNotes, StandardMenu.Help, Groups.Help, "_Release Notes".Localize("the '_' means that 'R' is a shortcut key"), "Release Notes".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.Menu);

	private static readonly CommandInfo s_helpAboutCommand = new CommandInfo(Commands.HelpAbout, StandardMenu.Help, Groups.Help, "_About".Localize("the '_' means that 'A' is a shortcut key"), "Shows Information About Application".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.Menu);

	private readonly List<CommandInfo> m_contextMenuHelpCommands = new List<CommandInfo>();

	public string HelpFilePath { get; set; }

	public string ReleaseNotesFilePath { get; set; }

	public bool EnableContextHelpUserSetting
	{
		get
		{
			return m_enableContextHelpUserSetting;
		}
		set
		{
			m_enableContextHelpUserSetting = value;
		}
	}

	public bool ShowContextHelp
	{
		get
		{
			return m_showContextHelp;
		}
		set
		{
			m_showContextHelp = value;
		}
	}

	public string ApplicationName
	{
		get
		{
			return m_applicationName;
		}
		set
		{
			m_applicationName = value;
			ICommandItem commandItem = s_helpAboutCommand.GetCommandItem();
			commandItem.Text = "_About".Localize() + " " + ApplicationName;
			commandItem.Description = commandItem.Text;
		}
	}

	public void Initialize()
	{
		m_commandService.RegisterCommand(s_helpCommand, this);
		m_commandService.RegisterCommand(s_helpAboutCommand, this);
		if (!string.IsNullOrEmpty(ReleaseNotesFilePath))
		{
			m_commandService.RegisterCommand(s_releaseNotesCommand, this);
		}
		for (int i = 0; i < 10; i++)
		{
			ContextMenuHelpTag commandTag = new ContextMenuHelpTag
			{
				Index = i
			};
			CommandInfo item = m_commandService.RegisterCommand(commandTag, null, Groups.Help, "Help/Help".Localize() + " " + (i + 1), "Help".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.None, this);
			m_contextMenuHelpCommands.Add(item);
		}
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help.chm");
		if (File.Exists(text))
		{
			HelpFilePath = text;
		}
		if (m_settingsService != null && EnableContextHelpUserSetting)
		{
			m_settingsService.RegisterUserSettings("Help".Localize(), new BoundPropertyDescriptor(this, () => ShowContextHelp, "Show Context Help".Localize(), "Help".Localize(), "Uncheck this to hide help commands in context menus".Localize()));
		}
	}

	public void ShowHelp()
	{
		if (HelpFilePath != null)
		{
			Help.ShowHelp(null, HelpFilePath);
		}
	}

	public void ShowReleaseNotes()
	{
		if (ReleaseNotesFilePath != null)
		{
			Help.ShowHelp(null, ReleaseNotesFilePath);
		}
	}

	public void ShowHelp(HelpNavigator helpNavigator)
	{
		Help.ShowHelp(null, HelpFilePath, helpNavigator);
	}

	public void ShowHelp(string keyword)
	{
		ShowHelp(HelpNavigator.KeywordIndex, keyword);
	}

	public void ShowHelp(HelpNavigator helpNavigator, string keyword)
	{
		if (keyword == null)
		{
			ShowHelp(helpNavigator);
		}
		else if (HelpFilePath != null)
		{
			Help.ShowHelp(null, HelpFilePath, helpNavigator, keyword);
		}
	}

	public virtual void ShowHelpAbout()
	{
		DialogUtils.ShowDialogWithViewModel<AboutDialog, AboutDialogViewModel>();
	}

	public bool CanDoCommand(object tag)
	{
		if (tag is Commands)
		{
			switch ((Commands)tag)
			{
			case Commands.Help:
				return HelpFilePath != null;
			case Commands.HelpReleaseNotes:
				return ReleaseNotesFilePath != null;
			case Commands.HelpAbout:
				return true;
			}
		}
		else if (ShowContextHelp && tag is ContextMenuHelpTag)
		{
			return m_lastContextMenuKeys != null && HelpFilePath != null && ((ContextMenuHelpTag)tag).Index < m_lastContextMenuKeys.Length;
		}
		return false;
	}

	public void DoCommand(object tag)
	{
		if (tag is Commands)
		{
			switch ((Commands)tag)
			{
			case Commands.Help:
				ShowHelp(HelpNavigator.TableOfContents);
				break;
			case Commands.HelpReleaseNotes:
				ShowReleaseNotes();
				break;
			case Commands.HelpAbout:
				ShowHelpAbout();
				break;
			}
		}
		else if (tag is ContextMenuHelpTag)
		{
			int index = ((ContextMenuHelpTag)tag).Index;
			if (index < m_lastContextMenuKeys.Length)
			{
				string keyword = m_lastContextMenuKeys[index];
				ShowHelp(keyword);
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public IEnumerable<object> GetCommands(object context, object target)
	{
		if (!ShowContextHelp || HelpFilePath == null)
		{
			yield break;
		}
		m_lastContextMenuKeys = TryGetHelpKeys(target);
		if (m_lastContextMenuKeys == null)
		{
			m_lastContextMenuKeys = TryGetHelpKeys(context);
		}
		if (m_lastContextMenuKeys != null)
		{
			for (int i = 0; i < m_lastContextMenuKeys.Length; i++)
			{
				ICommandItem command = m_contextMenuHelpCommands[i].GetCommandItem();
				command.Text = m_lastContextMenuKeys[i];
				yield return m_contextMenuHelpCommands[i].CommandTag;
			}
		}
	}

	private string[] TryGetHelpKeys(object target)
	{
		if (target is IAdaptable adaptable)
		{
			IHelpContext helpContext = adaptable.As<IHelpContext>();
			if (helpContext != null)
			{
				return helpContext.GetHelpKeys();
			}
		}
		return null;
	}
}
