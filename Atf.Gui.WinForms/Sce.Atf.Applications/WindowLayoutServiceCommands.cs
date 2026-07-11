using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Controls;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(WindowLayoutServiceCommandsBase))]
[Export(typeof(WindowLayoutServiceCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WindowLayoutServiceCommands : WindowLayoutServiceCommandsBase
{
	private class WindowLayoutServiceCommand
	{
		public string LayoutName { get; private set; }

		public CommandInfo CommandInfo { get; set; }

		public WindowLayoutServiceCommand(string layoutName)
		{
			LayoutName = layoutName;
		}
	}

	[Import(AllowDefault = true)]
	private CommandService m_commandService;

	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService;

	private readonly Dictionary<string, IEnumerable<Sce.Atf.Input.Keys>> m_dictCommandKeys = new Dictionary<string, IEnumerable<Sce.Atf.Input.Keys>>(StringComparer.CurrentCulture);

	private readonly Dictionary<string, WindowLayoutServiceCommand> m_dictCommands = new Dictionary<string, WindowLayoutServiceCommand>(StringComparer.CurrentCulture);

	[Import(AllowDefault = true)]
	public MainForm MainForm { get; set; }

	[Import(AllowDefault = true)]
	public ICommandService CommandService { get; set; }

	[ImportingConstructor]
	public WindowLayoutServiceCommands(IWindowLayoutService windowLayoutService)
		: base(windowLayoutService)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		if (CommandService != null)
		{
			CommandService.RegisterCommand(Command.SaveLayoutAs, StandardMenu.Window, StandardCommandGroup.UILayout, string.Format("{0}{1}{2}", WindowLayoutServiceCommandsBase.MenuName, "/", WindowLayoutServiceCommandsBase.MenuSaveLayoutAs), "Save layout as...".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.Menu, this);
			CommandService.RegisterCommand(Command.ManageLayouts, StandardMenu.Window, StandardCommandGroup.UILayout, string.Format("{0}{1}{2}", WindowLayoutServiceCommandsBase.MenuName, "/", WindowLayoutServiceCommandsBase.MenuManageLayouts), "Manage layouts...".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.Menu, this);
			CommandService.RegisterCommand(Command.ResetLayout, StandardMenu.Window, StandardCommandGroup.UILayout, string.Format("{0}{1}{2}", WindowLayoutServiceCommandsBase.MenuName, "/", WindowLayoutServiceCommandsBase.MenuResetLayout), "Reset Layouts...".Localize(), Sce.Atf.Input.Keys.None, null, CommandVisibility.Menu, this);
		}
	}

	public override bool CanDoCommand(object commandTag)
	{
		return base.CanDoCommand(commandTag) || commandTag is WindowLayoutServiceCommand;
	}

	public override void DoCommand(object commandTag)
	{
		base.DoCommand(commandTag);
		if (commandTag is WindowLayoutServiceCommand)
		{
			WindowLayoutServiceCommand windowLayoutServiceCommand = (WindowLayoutServiceCommand)commandTag;
			base.WindowLayoutService.CurrentLayout = windowLayoutServiceCommand.LayoutName;
		}
	}

	public override void UpdateCommand(object commandTag, CommandState state)
	{
		if (commandTag is WindowLayoutServiceCommand)
		{
			WindowLayoutServiceCommand windowLayoutServiceCommand = (WindowLayoutServiceCommand)commandTag;
			state.Check = base.WindowLayoutService.IsCurrent(windowLayoutServiceCommand.LayoutName);
		}
	}

	public override void ShowSaveLayoutAsDialog()
	{
		using WindowLayoutNewDialog windowLayoutNewDialog = new WindowLayoutNewDialog();
		TryUseMainFormIcon(MainForm, windowLayoutNewDialog);
		if (windowLayoutNewDialog.ShowDialog(MainForm) == DialogResult.OK)
		{
			SaveLayoutAs(windowLayoutNewDialog.LayoutName);
		}
	}

	public override void ShowManageLayoutsDialog()
	{
		using WindowLayoutManageDialog windowLayoutManageDialog = new WindowLayoutManageDialog();
		windowLayoutManageDialog.ScreenshotDirectory = base.LayoutScreenshotDirectory;
		windowLayoutManageDialog.LayoutNames = base.WindowLayoutService.Layouts;
		TryUseMainFormIcon(MainForm, windowLayoutManageDialog);
		windowLayoutManageDialog.ShowDialog(MainForm);
		foreach (KeyValuePair<string, string> renamedLayout in windowLayoutManageDialog.RenamedLayouts)
		{
			if (m_dictCommandKeys.TryGetValue(renamedLayout.Key, out var value))
			{
				m_dictCommandKeys.Remove(renamedLayout.Key);
				m_dictCommandKeys[renamedLayout.Value] = value;
			}
		}
		foreach (KeyValuePair<string, string> renamedLayout2 in windowLayoutManageDialog.RenamedLayouts)
		{
			base.WindowLayoutService.RenameLayout(renamedLayout2.Key, renamedLayout2.Value);
		}
		foreach (string deletedLayout in windowLayoutManageDialog.DeletedLayouts)
		{
			base.WindowLayoutService.RemoveLayout(deletedLayout);
		}
	}

	public override void ShowResetLayoutDialog()
	{
		ConfirmationDialog confirmationDialog = new ConfirmationDialog("Reset All Layouts".Localize("Reset all layouts to their default state?"), "Reset all layouts to their default state? (This will not delete saved layouts)".Localize());
		DialogResult dialogResult = confirmationDialog.ShowDialog();
		if (dialogResult == DialogResult.Yes)
		{
			DoResetLayout();
		}
	}

	public override void DoResetLayout()
	{
		base.WindowLayoutService.ResetLayout = true;
		m_commandService.ResetToolStrips();
		IList<ControlInfo> list = m_controlHostService.Controls.ToList();
		foreach (ControlInfo item in list)
		{
			m_controlHostService.UnregisterControl(item.Control);
		}
		foreach (ControlInfo item2 in list)
		{
			m_controlHostService.RegisterControl(item2.Control, item2, item2.Client);
		}
		base.WindowLayoutService.ResetLayout = false;
	}

	protected override void OnWindowLayoutServiceLayoutsChanging()
	{
		if (CommandService == null)
		{
			return;
		}
		foreach (KeyValuePair<string, WindowLayoutServiceCommand> dictCommand in m_dictCommands)
		{
			if (dictCommand.Value.CommandInfo.Shortcuts.Any())
			{
				m_dictCommandKeys[dictCommand.Key] = dictCommand.Value.CommandInfo.Shortcuts;
			}
			CommandService.UnregisterCommand(dictCommand.Value, this);
		}
		m_dictCommands.Clear();
	}

	protected override void OnWindowLayoutServiceLayoutsChanged()
	{
		if (CommandService == null)
		{
			return;
		}
		foreach (string layout in base.WindowLayoutService.Layouts)
		{
			WindowLayoutServiceCommand windowLayoutServiceCommand = new WindowLayoutServiceCommand(layout);
			Sce.Atf.Input.Keys shortcut = Sce.Atf.Input.Keys.None;
			if (m_dictCommandKeys.TryGetValue(layout, out var value))
			{
				shortcut = value.First();
			}
			windowLayoutServiceCommand.CommandInfo = CommandService.RegisterCommand(windowLayoutServiceCommand, StandardMenu.Window, StandardCommandGroup.UILayout, string.Format("{0}{1}{2}", WindowLayoutServiceCommandsBase.MenuName, "/", layout), layout, shortcut, null, CommandVisibility.Menu, this);
			m_dictCommands[layout] = windowLayoutServiceCommand;
			m_dictCommandKeys[layout] = windowLayoutServiceCommand.CommandInfo.Shortcuts;
		}
		foreach (string layout2 in base.WindowLayoutService.Layouts)
		{
			if (!m_dictCommandKeys.ContainsKey(layout2))
			{
				m_dictCommandKeys.Remove(layout2);
			}
		}
	}

	protected override Image GetApplicationScreenshot()
	{
		if (MainForm == null)
		{
			return null;
		}
		Image result = GdiUtil.CaptureWindow(MainForm.Handle);
		MainForm.Invalidate(invalidateChildren: true);
		return result;
	}

	private static void TryUseMainFormIcon(Form mainForm, Form dialog)
	{
		if (mainForm != null && mainForm.Icon != null)
		{
			dialog.Icon = mainForm.Icon;
		}
	}
}
