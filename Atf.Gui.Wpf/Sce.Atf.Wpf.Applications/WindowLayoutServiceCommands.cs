using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Sce.Atf.Applications;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(WindowLayoutServiceCommandsBase))]
[Export(typeof(WindowLayoutServiceCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WindowLayoutServiceCommands : WindowLayoutServiceCommandsBase
{
	private class WindowLayoutServiceCommand
	{
		public string LayoutName { get; private set; }

		public ICommandItem Command { get; set; }

		public WindowLayoutServiceCommand(string layoutName)
		{
			LayoutName = layoutName;
		}
	}

	private readonly Dictionary<string, IEnumerable<Keys>> m_dictCommandKeys = new Dictionary<string, IEnumerable<Keys>>(StringComparer.CurrentCulture);

	private readonly Dictionary<string, ICommandItem> m_dictCommands = new Dictionary<string, ICommandItem>(StringComparer.CurrentCulture);

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
			CommandService.RegisterCommand(Command.SaveLayoutAs, StandardMenu.Window, Group.WindowLayoutServiceCommandsBase, WindowLayoutServiceCommandsBase.MenuName + "/" + WindowLayoutServiceCommandsBase.MenuSaveLayoutAs, "Saves the layout to a new configuration".Localize(), Keys.None, null, CommandVisibility.Menu, this);
			CommandService.RegisterCommand(Command.ManageLayouts, StandardMenu.Window, Group.WindowLayoutServiceCommandsBase, WindowLayoutServiceCommandsBase.MenuName + "/" + WindowLayoutServiceCommandsBase.MenuManageLayouts, "Manages the current layouts".Localize(), Keys.None, null, CommandVisibility.Menu, this);
			CommandService.RegisterCommand(Command.ResetLayout, StandardMenu.Window, Group.WindowLayoutServiceCommandsBase, WindowLayoutServiceCommandsBase.MenuName + "/" + WindowLayoutServiceCommandsBase.MenuResetLayout, "Resets the layout to default".Localize(), Keys.None, null, CommandVisibility.Menu, this);
		}
	}

	public override bool CanDoCommand(object tag)
	{
		return tag != null && (base.CanDoCommand(tag) || tag is WindowLayoutServiceCommand);
	}

	public override void DoCommand(object tag)
	{
		if (tag != null)
		{
			base.DoCommand(tag);
			if (tag is WindowLayoutServiceCommand)
			{
				WindowLayoutServiceCommand windowLayoutServiceCommand = (WindowLayoutServiceCommand)tag;
				base.WindowLayoutService.CurrentLayout = windowLayoutServiceCommand.LayoutName;
			}
		}
	}

	public override void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public override void ShowSaveLayoutAsDialog()
	{
		WindowLayoutNewViewModel windowLayoutNewViewModel = new WindowLayoutNewViewModel(base.WindowLayoutService.Layouts);
		WindowLayoutNewDialog dialog = new WindowLayoutNewDialog(windowLayoutNewViewModel);
		if (dialog.ShowParentedDialog() == true)
		{
			SaveLayoutAs(windowLayoutNewViewModel.LayoutName);
		}
	}

	public override void ShowManageLayoutsDialog()
	{
		List<Pair<string, Keys>> list = new List<Pair<string, Keys>>();
		foreach (string layout in base.WindowLayoutService.Layouts)
		{
			Keys second = Keys.None;
			if (m_dictCommandKeys.TryGetValue(layout, out var value))
			{
				second = value.FirstOrDefault();
			}
			list.Add(new Pair<string, Keys>(layout, second));
		}
		ManageWindowLayoutsDialogViewModel manageWindowLayoutsDialogViewModel = new ManageWindowLayoutsDialogViewModel(list);
		manageWindowLayoutsDialogViewModel.ScreenshotDirectory = base.LayoutScreenshotDirectory;
		WindowLayoutManageDialog dialog = new WindowLayoutManageDialog(manageWindowLayoutsDialogViewModel);
		if (dialog.ShowParentedDialog() != true)
		{
			return;
		}
		foreach (KeyValuePair<string, string> renamedLayout in manageWindowLayoutsDialogViewModel.RenamedLayouts)
		{
			if (m_dictCommandKeys.TryGetValue(renamedLayout.Key, out var value2))
			{
				m_dictCommandKeys.Remove(renamedLayout.Key);
				m_dictCommandKeys[renamedLayout.Value] = value2;
			}
		}
		foreach (KeyValuePair<string, string> renamedLayout2 in manageWindowLayoutsDialogViewModel.RenamedLayouts)
		{
			base.WindowLayoutService.RenameLayout(renamedLayout2.Key, renamedLayout2.Value);
		}
		foreach (string deletedLayout in manageWindowLayoutsDialogViewModel.DeletedLayouts)
		{
			base.WindowLayoutService.RemoveLayout(deletedLayout);
		}
	}

	public override void ShowResetLayoutDialog()
	{
	}

	public override void DoResetLayout()
	{
	}

	protected override Image GetApplicationScreenshot()
	{
		BitmapSource bitmapsource = ImageUtil.CaptureWindow(Application.Current.MainWindow);
		return ImageUtil.BitmapFromSource(bitmapsource);
	}

	protected override void OnWindowLayoutServiceLayoutsChanging()
	{
		if (CommandService == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ICommandItem> dictCommand in m_dictCommands)
		{
			m_dictCommandKeys[dictCommand.Key] = dictCommand.Value.Shortcuts;
			CommandService.UnregisterCommand(dictCommand.Value.CommandTag, this);
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
			Keys shortcut = Keys.None;
			if (m_dictCommandKeys.TryGetValue(layout, out var value))
			{
				shortcut = value.FirstOrDefault();
			}
			ICommandItem commandItem = (windowLayoutServiceCommand.Command = CommandService.RegisterCommand(windowLayoutServiceCommand, StandardMenu.Window, StandardCommandGroup.WindowLayoutItems, WindowLayoutServiceCommandsBase.MenuName + "/" + layout, null, shortcut, null, CommandVisibility.Menu, this).GetCommandItem());
			m_dictCommands[layout] = commandItem;
			m_dictCommandKeys[layout] = commandItem.Shortcuts;
		}
		foreach (string layout2 in base.WindowLayoutService.Layouts)
		{
			if (!m_dictCommandKeys.ContainsKey(layout2))
			{
				m_dictCommandKeys.Remove(layout2);
			}
		}
	}
}
