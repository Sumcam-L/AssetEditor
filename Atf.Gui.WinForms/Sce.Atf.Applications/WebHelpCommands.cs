using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(WebHelpCommands))]
public class WebHelpCommands : ICommandClient, IInitializable
{
	protected enum Commands
	{
		OpenHelpPage
	}

	private WebHelp m_webHelp;

	public string Url { get; set; }

	[Import]
	protected CommandService CommandService { get; set; }

	[Import]
	protected Form MainForm { get; set; }

	[ImportingConstructor]
	public WebHelpCommands()
	{
	}

	public WebHelpCommands(string url)
	{
		Url = url;
	}

	public virtual void Initialize()
	{
		CommandInfo commandInfo = new CommandInfo(Commands.OpenHelpPage, StandardMenu.Help, StandardCommandGroup.HelpAbout, "Online Help".Localize(), "Opens an online help page for this app".Localize(), Sce.Atf.Input.Keys.F1, null, CommandVisibility.ApplicationMenu);
		commandInfo.EnableCheckCanDoEvent(this);
		CommandService.RegisterCommand(commandInfo, this);
		CommandService.CommandControls commandControls = CommandService.GetCommandControls(commandInfo);
		commandControls.MenuItem.Click += MenuItemOnClick;
		m_webHelp = MainForm.AddHelp(Url);
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return !string.IsNullOrEmpty(Url);
	}

	public virtual void DoCommand(object commandTag)
	{
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void MenuItemOnClick(object sender, EventArgs eventArgs)
	{
		m_webHelp.OpenUrl();
	}
}
