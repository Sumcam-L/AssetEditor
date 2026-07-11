using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(CommandService))]
[Export(typeof(ICommandService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CommandService : CommandServiceBase, IPartImportsSatisfiedNotification
{
	[Export(typeof(IToolBar))]
	[Export(typeof(Sce.Atf.Wpf.IMenu))]
	internal class MenuToolBarModel : IToolBar, Sce.Atf.Wpf.IMenu
	{
		public MenuInfo MenuInfo { get; private set; }

		public ComposablePart ComposablePart { get; set; }

		public object Tag => MenuInfo.MenuTag;

		public object MenuTag => MenuInfo.MenuTag;

		public string Text => MenuInfo.MenuText;

		public string Description => MenuInfo.Description;

		public MenuToolBarModel(MenuInfo menuInfo)
		{
			MenuInfo = menuInfo;
		}
	}

	[Import]
	private IComposer m_composer = null;

	private readonly Dictionary<object, CommandItem> m_commandsLookup = new Dictionary<object, CommandItem>();

	private readonly EventHandler m_cachedRequerySuggestedHandler;

	public CommandService()
	{
		m_cachedRequerySuggestedHandler = CommandManager_RequerySuggested;
	}

	public override void Initialize()
	{
		CommandManager.RequerySuggested += m_cachedRequerySuggestedHandler;
	}

	public ICommandItem GetCommand(object commandTag)
	{
		m_commandsLookup.TryGetValue(commandTag, out var value);
		return value;
	}

	protected sealed override void RegisterMenuInfo(MenuInfo info)
	{
		base.RegisterMenuInfo(info);
		if (m_composer != null)
		{
			ExportMenuModel(info);
		}
	}

	protected override void RegisterCommandInfo(CommandInfo info)
	{
		object obj = null;
		if (info.ImageKey == null && !string.IsNullOrEmpty(info.ImageName))
		{
			obj = ResourceUtil.GetKeyFromImageName(info.ImageName);
			info.ImageKey = obj;
		}
		base.RegisterCommandInfo(info);
		info.CommandService = this;
		if (!m_commandsLookup.TryGetValue(info.CommandTag, out var value))
		{
			value = new CommandItem(info, CanExecuteCommand, ExecuteCommand);
			m_commandsLookup.Add(value.CommandTag, value);
			if (m_composer != null)
			{
				value.ComposablePart = m_composer.AddPart(value);
			}
		}
	}

	public override void UnregisterCommand(object commandTag, ICommandClient client)
	{
		base.UnregisterCommand(commandTag, client);
		if (!m_commandClients.TryGetFirst(commandTag, out client))
		{
			ICommandItem commandItem = m_commandsLookup[commandTag];
			m_commandsLookup.Remove(commandTag);
			if (commandItem is CommandItem { ComposablePart: not null } commandItem2)
			{
				m_composer.RemovePart(commandItem2.ComposablePart);
				commandItem2.ComposablePart = null;
			}
		}
	}

	public override void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint)
	{
		throw new NotSupportedException("Use IContextMenuService");
	}

	public void OnImportsSatisfied()
	{
		foreach (MenuInfo menu in m_menus)
		{
			ExportMenuModel(menu);
		}
	}

	private bool CanExecuteCommand(ICommandItem command)
	{
		if (command != null)
		{
			ICommandClient client = GetClient(command.CommandTag);
			if (client != null)
			{
				return client.CanDoCommand(command.CommandTag);
			}
		}
		return false;
	}

	private void ExecuteCommand(ICommandItem command)
	{
		if (command != null)
		{
			GetClient(command.CommandTag)?.DoCommand(command.CommandTag);
		}
	}

	private void ExportMenuModel(MenuInfo menuInfo)
	{
		MenuToolBarModel menuToolBarModel = new MenuToolBarModel(menuInfo);
		menuToolBarModel.ComposablePart = m_composer.AddPart(menuToolBarModel);
	}

	private void CommandManager_RequerySuggested(object sender, EventArgs e)
	{
		foreach (CommandItem value in m_commandsLookup.Values)
		{
			LegacyUpdateCommand(value);
		}
	}

	private void LegacyUpdateCommand(ICommandItem item)
	{
		ICommandClient clientOrActiveClient = GetClientOrActiveClient(item.CommandTag);
		if (clientOrActiveClient != null)
		{
			CommandState commandState = new CommandState
			{
				Text = item.Text,
				Check = item.IsChecked
			};
			clientOrActiveClient.UpdateCommand(item.CommandTag, commandState);
			item.Text = commandState.Text.Trim();
			item.IsChecked = commandState.Check;
		}
	}
}
