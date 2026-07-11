using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public abstract class CommandServiceBase : ICommandService, ICommandClient, IInitializable
{
	public enum ImageSizes
	{
		Size16x16,
		Size24x24,
		Size32x32
	}

	protected class CommandComparer : IComparer<CommandInfo>
	{
		public int Compare(CommandInfo x, CommandInfo y)
		{
			return CommandServiceBase.Compare(x, y);
		}
	}

	[Import(AllowDefault = true)]
	protected IStatusService m_statusService;

	[Import(AllowDefault = true)]
	protected ISettingsService m_settingsService;

	protected ImageSizes m_imageSize = ImageSizes.Size24x24;

	protected List<MenuInfo> m_menus = new List<MenuInfo>();

	protected List<CommandInfo> m_commands = new List<CommandInfo>();

	protected Dictionary<object, CommandInfo> m_commandsById = new Dictionary<object, CommandInfo>();

	protected Multimap<object, ICommandClient> m_commandClients = new Multimap<object, ICommandClient>();

	protected Dictionary<Keys, object> m_shortcuts = new Dictionary<Keys, object>();

	protected Dictionary<Keys, string> m_reservedKeys = new Dictionary<Keys, string>();

	protected ICommandClient m_activeClient;

	private bool m_contextMenuAutoCompact = true;

	protected static char[] s_pathDelimiters;

	private static readonly HashSet<object> m_beginningTags;

	private static readonly HashSet<object> m_endingTags;

	private static readonly HashSet<object> m_defaultSortByMenuLabel;

	[DefaultValue(ImageSizes.Size24x24)]
	public ImageSizes UserSelectedImageSize
	{
		get
		{
			return m_imageSize;
		}
		set
		{
			if (m_imageSize != value)
			{
				m_imageSize = value;
				OnImageSizeChanged();
			}
		}
	}

	public string CommandShortcuts
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Shortcuts");
			xmlDocument.AppendChild(xmlElement);
			foreach (CommandInfo command in m_commands)
			{
				if (IsUnregistered(command) || command.ShortcutsAreDefault)
				{
					continue;
				}
				string commandPath = GetCommandPath(command);
				int num = 0;
				foreach (Keys shortcut in command.Shortcuts)
				{
					XmlElement xmlElement2 = xmlDocument.CreateElement("shortcut");
					xmlElement2.SetAttribute("name", commandPath);
					xmlElement2.SetAttribute("value", shortcut.ToString());
					xmlElement.AppendChild(xmlElement2);
					num++;
				}
				if (num < 1)
				{
					XmlElement xmlElement3 = xmlDocument.CreateElement("shortcut");
					xmlElement3.SetAttribute("name", commandPath);
					xmlElement3.SetAttribute("value", Keys.None.ToString());
					xmlElement.AppendChild(xmlElement3);
				}
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("shortcut");
			if (xmlNodeList == null || xmlNodeList.Count == 0)
			{
				return;
			}
			Dictionary<string, CommandInfo> dictionary = new Dictionary<string, CommandInfo>(m_commands.Count);
			foreach (CommandInfo command in m_commands)
			{
				if (!IsUnregistered(command))
				{
					string commandPath = GetCommandPath(command);
					dictionary.Add(commandPath, command);
				}
			}
			Dictionary<CommandInfo, CommandInfo> dictionary2 = new Dictionary<CommandInfo, CommandInfo>(m_commands.Count);
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("name");
				string attribute2 = item.GetAttribute("value");
				if (!dictionary.ContainsKey(attribute))
				{
					continue;
				}
				CommandInfo commandInfo = dictionary[attribute];
				if (!dictionary2.ContainsKey(commandInfo))
				{
					List<Keys> list = new List<Keys>(commandInfo.Shortcuts);
					foreach (Keys item2 in list)
					{
						EraseShortcut(item2);
					}
					dictionary2.Add(commandInfo, commandInfo);
				}
				Keys keys = (Keys)Enum.Parse(typeof(Keys), attribute2);
				keys = KeysUtil.NumPadToNum(keys);
				SetShortcut(keys, dictionary[attribute]);
			}
		}
	}

	public bool ContextMenuAutoCompact
	{
		get
		{
			return m_contextMenuAutoCompact;
		}
		set
		{
			m_contextMenuAutoCompact = value;
		}
	}

	public bool IconClicked { get; set; }

	public virtual CommandInfo MouseIsOverCommandIcon { get; private set; }

	public event EventHandler ContextMenuClosed;

	public event EventHandler<KeyEventArgs> ProcessingKey;

	public CommandServiceBase()
	{
		RegisterMenuInfo(new MenuInfo(StandardMenu.File, "File".Localize("this is the name of a menu"), "File Commands".Localize()));
		RegisterMenuInfo(new MenuInfo(StandardMenu.Edit, "Edit".Localize("this is the name of a menu"), "Editing Commands".Localize()));
		RegisterMenuInfo(new MenuInfo(StandardMenu.View, "View".Localize("this is the name of a menu"), "View Commands".Localize("'View' is a noun. This text is a description of the View menu")));
		RegisterMenuInfo(new MenuInfo(StandardMenu.Modify, "Modify".Localize("this is the name of a menu"), "Modify Commands".Localize()));
		RegisterMenuInfo(new MenuInfo(StandardMenu.Format, "Format".Localize("this is the name of a menu"), "Formatting Commands".Localize()));
		RegisterMenuInfo(new MenuInfo(StandardMenu.Window, "Window".Localize("this is the name of a menu"), "Window Management Commands".Localize()));
		RegisterMenuInfo(new MenuInfo(StandardMenu.Help, "Help".Localize("this is the name of a menu"), "Help Commands".Localize()));
	}

	public virtual void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => CommandShortcuts, "Keyboard Shortcuts".Localize(), null, null));
			PropertyDescriptor[] properties = new PropertyDescriptor[1]
			{
				new BoundPropertyDescriptor(this, () => UserSelectedImageSize, "Command Icon Size".Localize(), null, "Size of icons on Toolbar buttons".Localize())
			};
			m_settingsService.RegisterSettings(this, properties);
			SettingsServices.RegisterUserSettings(m_settingsService, "Application".Localize(), properties);
		}
		this.RegisterCommand(CommandId.EditKeyboard, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "Keyboard Shortcuts".Localize() + " ...", "Customize keyboard shortcuts".Localize(), this);
	}

	public void RegisterMenu(MenuInfo menuInfo)
	{
		RegisterMenuInfo(menuInfo);
	}

	public virtual void RegisterCommand(CommandInfo info, ICommandClient client)
	{
		if (client == null)
		{
			throw new InvalidOperationException("Command has no client");
		}
		CommandInfo commandInfo = GetCommandInfo(info.CommandTag);
		if (commandInfo == null)
		{
			if (!CommandIsUnique(info.MenuTag, info.MenuText))
			{
				throw new InvalidOperationException($"Duplicate menu/command combination. CommandTag: {info.CommandTag}, MenuTag: {info.GroupTag}, GroupTag: {info.MenuTag}, MenuText: {info.MenuText}");
			}
			RegisterCommandInfo(info);
			IncrementMenuCommandCount(info.MenuTag);
		}
		m_commandClients.Add(info.CommandTag, client);
	}

	public virtual void UnregisterCommand(object commandTag, ICommandClient client)
	{
		if (client == null)
		{
			m_commandClients.Remove(commandTag);
		}
		else
		{
			m_commandClients.Remove(commandTag, client);
		}
		CommandInfo commandInfo = GetCommandInfo(commandTag);
		if (commandInfo == null)
		{
			return;
		}
		UnregisterCommandInfo(commandInfo);
		if (commandInfo.MenuTag != null)
		{
			MenuInfo menuInfo = GetMenuInfo(commandInfo.MenuTag);
			if (menuInfo != null)
			{
				DecrementMenuCommandCount(menuInfo);
			}
		}
	}

	public abstract void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint);

	protected virtual void OnContextMenuClosed(object sender, EventArgs evtArgs)
	{
		this.ContextMenuClosed?.Raise(sender, evtArgs);
	}

	public void SetActiveClient(ICommandClient client)
	{
		List<object> list = new List<object>(m_commandClients.Keys);
		List<object> list2 = new List<object>();
		if (client == null && m_activeClient != null)
		{
			foreach (object item in list)
			{
				if (m_commandClients.ContainsKeyValue(item, m_activeClient))
				{
					m_commandClients.AddFirst(item, m_activeClient);
					list2.Add(item);
				}
			}
		}
		m_activeClient = client;
		if (m_activeClient != null)
		{
			foreach (object item2 in list)
			{
				if (m_commandClients.ContainsKeyValue(item2, client))
				{
					m_commandClients.Add(item2, client);
					list2.Add(item2);
				}
			}
		}
		foreach (object item3 in list2)
		{
			UpdateCommand(item3);
		}
	}

	protected virtual void UpdateCommand(object commandTag)
	{
	}

	public void ReserveKey(Keys key, string reason)
	{
		if (key == Keys.None)
		{
			throw new ArgumentException("key");
		}
		if (reason == null)
		{
			throw new ArgumentNullException("reason");
		}
		key = KeysUtil.NumPadToNum(key);
		if (m_reservedKeys.ContainsKey(key))
		{
			m_reservedKeys[key] = reason;
			return;
		}
		m_reservedKeys[key] = reason;
		EraseShortcut(key);
	}

	public virtual bool ProcessKey(Keys key)
	{
		KeyEventArgs e = new KeyEventArgs(key);
		this.ProcessingKey.Raise(this, e);
		if (e.Handled)
		{
			return true;
		}
		Keys keys = KeysUtil.NumPadToNum(key);
		if (keys == Keys.None)
		{
			return false;
		}
		if (!m_shortcuts.TryGetValue(keys, out var value))
		{
			return false;
		}
		ICommandClient commandClient = GetClient(value);
		if (commandClient == null)
		{
			commandClient = m_activeClient;
		}
		if (commandClient == null || !commandClient.CanDoCommand(value))
		{
			return false;
		}
		commandClient.DoCommand(value);
		return true;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
	}

	protected virtual void OnImageSizeChanged()
	{
	}

	public MenuInfo GetMenuInfo(object menuTag)
	{
		MenuInfo result = null;
		foreach (MenuInfo menu in m_menus)
		{
			if (menu.MenuTag.Equals(menuTag))
			{
				result = menu;
				break;
			}
		}
		return result;
	}

	private MenuInfo GetContextMenuInfo(object menuTag)
	{
		MenuInfo result = null;
		if (menuTag != null)
		{
			string text = menuTag.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				result = new MenuInfo(menuTag, text, string.Empty);
			}
		}
		return result;
	}

	public CommandInfo GetCommandInfo(object commandTag)
	{
		CommandInfo value = null;
		if (commandTag != null)
		{
			m_commandsById.TryGetValue(commandTag, out value);
		}
		int hashCode = commandTag.GetHashCode();
		foreach (KeyValuePair<object, CommandInfo> item in m_commandsById)
		{
			int hashCode2 = item.Value.GetHashCode();
			int hashCode3 = item.Key.GetHashCode();
		}
		return value;
	}

	public IEnumerable<CommandInfo> GetCommandInfos()
	{
		return m_commands;
	}

	public ICommandClient GetClient(object commandTag)
	{
		m_commandClients.TryGetLast(commandTag, out var result);
		return result;
	}

	public ICommandClient GetClientOrActiveClient(object commandTag)
	{
		return GetClient(commandTag) ?? m_activeClient;
	}

	protected virtual void RegisterMenuInfo(MenuInfo info)
	{
		MenuInfo menuInfo = GetMenuInfo(info.MenuTag);
		if (menuInfo != null)
		{
			throw new InvalidOperationException(string.Concat("Menu object '", info.MenuTag, "' was already added"));
		}
		if (info.MenuTag is StandardMenu)
		{
			m_menus.Add(info);
		}
		else
		{
			m_menus.Insert(m_menus.Count - 2, info);
		}
	}

	protected virtual void RegisterCommandInfo(CommandInfo info)
	{
		string menuText = info.MenuText;
		if (string.IsNullOrEmpty(menuText))
		{
			throw new ArgumentException("menuText is null or empty");
		}
		int num = 1;
		if (menuText[0] != '@')
		{
			num += menuText.LastIndexOfAny(s_pathDelimiters);
		}
		string displayedMenuText = menuText.Substring(num, menuText.Length - num);
		info.DisplayedMenuText = displayedMenuText;
		m_commands.Add(info);
		m_commandsById[info.CommandTag] = info;
		foreach (Keys shortcut in info.Shortcuts)
		{
			SetShortcut(shortcut, info);
		}
	}

	protected virtual void UnregisterCommandInfo(CommandInfo info)
	{
		m_commandsById.Remove(info.CommandTag);
		m_commands.Remove(info);
	}

	protected void SetShortcut(Keys shortcut, CommandInfo info)
	{
		shortcut = KeysUtil.NumPadToNum(shortcut);
		if (m_reservedKeys.ContainsKey(shortcut))
		{
			Outputs.WriteLine(OutputMessageType.Warning, "cannot assign " + KeysUtil.KeysToString(shortcut, digitOnly: true) + " to " + GetCommandPath(info) + " it is reserved for " + m_reservedKeys[shortcut]);
			info.RemoveShortcut(shortcut);
			EraseShortcut(shortcut);
			return;
		}
		info.AddShortcut(shortcut);
		if (shortcut == Keys.None)
		{
			return;
		}
		if (m_shortcuts.ContainsKey(shortcut) && m_shortcuts[shortcut] != info.CommandTag)
		{
			object key = m_shortcuts[shortcut];
			if (m_commandsById.ContainsKey(key))
			{
				CommandInfo commandInfo = m_commandsById[key];
				commandInfo.RemoveShortcut(shortcut);
			}
		}
		m_shortcuts[shortcut] = info.CommandTag;
	}

	private void EraseShortcut(Keys shortcut)
	{
		if (m_shortcuts.ContainsKey(shortcut))
		{
			object key = m_shortcuts[shortcut];
			if (m_commandsById.ContainsKey(key))
			{
				CommandInfo commandInfo = m_commandsById[key];
				commandInfo.RemoveShortcut(shortcut);
				m_shortcuts.Remove(shortcut);
			}
		}
	}

	private bool CommandIsUnique(object menuTag, string menuText)
	{
		foreach (CommandInfo command in m_commands)
		{
			if (IsUnregistered(command) || !TagsEqual(command.MenuTag, menuTag) || !(command.MenuText == menuText))
			{
				continue;
			}
			return false;
		}
		return true;
	}

	protected virtual MenuInfo IncrementMenuCommandCount(object menuTag)
	{
		MenuInfo menuInfo = null;
		if (menuTag != null)
		{
			menuInfo = GetMenuInfo(menuTag);
			if (menuInfo != null)
			{
				menuInfo.Commands++;
			}
		}
		return menuInfo;
	}

	protected virtual void DecrementMenuCommandCount(MenuInfo menuInfo)
	{
		menuInfo.Commands--;
	}

	public string GetCommandPath(CommandInfo commandInfo)
	{
		string text = commandInfo.MenuText;
		MenuInfo menuInfo = GetMenuInfo(commandInfo.MenuTag);
		if (menuInfo == null)
		{
			menuInfo = GetContextMenuInfo(commandInfo.MenuTag);
		}
		if (menuInfo != null)
		{
			text = menuInfo.MenuText + "/" + text;
		}
		return text;
	}

	protected bool IsUnregistered(CommandInfo info)
	{
		return GetClient(info.CommandTag) == null;
	}

	private static int Compare(CommandInfo x, CommandInfo y)
	{
		int num = CompareTags(x.MenuTag, y.MenuTag);
		if (num == 0)
		{
			num = CompareTags(x.GroupTag, y.GroupTag);
		}
		if (num == 0)
		{
			num = CompareTags(x.CommandTag, y.CommandTag);
		}
		if (num == 0)
		{
			num = ((x.GroupTag == null || !m_defaultSortByMenuLabel.Contains(x.GroupTag)) ? (x.Index - y.Index) : CompareTags(x.DisplayedMenuText, y.DisplayedMenuText));
		}
		return num;
	}

	private static int CompareTags(object tag1, object tag2)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (tag1 != null)
		{
			flag = m_beginningTags.Contains(tag1);
			flag3 = m_endingTags.Contains(tag1);
		}
		if (tag2 != null)
		{
			flag2 = m_beginningTags.Contains(tag2);
			flag4 = m_endingTags.Contains(tag2);
		}
		if (flag && !flag2)
		{
			return -1;
		}
		if (flag2 && !flag)
		{
			return 1;
		}
		if (flag3 && !flag4)
		{
			return 1;
		}
		if (flag4 && !flag3)
		{
			return -1;
		}
		if (tag1 is Enum && tag2 is Enum)
		{
			return ((int)tag1).CompareTo((int)tag2);
		}
		if (tag1 is Enum)
		{
			return -1;
		}
		if (tag2 is Enum)
		{
			return 1;
		}
		if (tag1 is string && tag2 is string)
		{
			return StringUtil.CompareNaturalOrder((string)tag1, (string)tag2);
		}
		IComparable comparable = tag1 as IComparable;
		IComparable comparable2 = tag2 as IComparable;
		if (comparable != null)
		{
			int num = comparable.CompareTo(tag2);
			if (num != 0)
			{
				return num;
			}
		}
		if (comparable2 != null)
		{
			int num2 = comparable2.CompareTo(tag1);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return 0;
	}

	protected static bool TagsEqual(object tag1, object tag2)
	{
		return tag1?.Equals(tag2) ?? (tag2 == null);
	}

	public virtual void RefreshImages()
	{
	}

	public virtual void RefreshImage(CommandInfo commandInfo)
	{
	}

	static CommandServiceBase()
	{
		s_pathDelimiters = new char[2] { '/', '\\' };
		m_beginningTags = new HashSet<object>();
		m_endingTags = new HashSet<object>();
		m_defaultSortByMenuLabel = new HashSet<object>();
		m_endingTags.Add(StandardCommand.FileClose);
		m_beginningTags.Add(StandardCommand.FileSave);
		m_beginningTags.Add(StandardCommand.FileSaveAs);
		m_beginningTags.Add(StandardCommand.FileSaveAll);
		m_beginningTags.Add(StandardCommand.EditUndo);
		m_beginningTags.Add(StandardCommand.EditRedo);
		m_beginningTags.Add(StandardCommand.EditCut);
		m_beginningTags.Add(StandardCommand.EditCopy);
		m_beginningTags.Add(StandardCommand.EditPaste);
		m_endingTags.Add(StandardCommand.EditDelete);
		m_beginningTags.Add(StandardCommand.EditSelectAll);
		m_beginningTags.Add(StandardCommand.EditDeselectAll);
		m_beginningTags.Add(StandardCommand.EditInvertSelection);
		m_beginningTags.Add(StandardCommand.EditGroup);
		m_beginningTags.Add(StandardCommand.EditUngroup);
		m_beginningTags.Add(StandardCommand.EditLock);
		m_beginningTags.Add(StandardCommand.EditUnlock);
		m_beginningTags.Add(StandardCommand.ViewZoomIn);
		m_beginningTags.Add(StandardCommand.ViewZoomOut);
		m_beginningTags.Add(StandardCommand.ViewZoomExtents);
		m_beginningTags.Add(StandardCommand.WindowSplitHoriz);
		m_beginningTags.Add(StandardCommand.WindowSplitVert);
		m_beginningTags.Add(StandardCommand.WindowRemoveSplit);
		m_endingTags.Add(StandardCommand.HelpAbout);
		m_beginningTags.Add(StandardCommandGroup.FileNew);
		m_beginningTags.Add(StandardCommandGroup.FileSave);
		m_beginningTags.Add(StandardCommandGroup.FileOther);
		m_endingTags.Add(StandardCommandGroup.FileRecentlyUsed);
		m_endingTags.Add(StandardCommandGroup.FileExit);
		m_beginningTags.Add(StandardCommandGroup.EditUndo);
		m_beginningTags.Add(StandardCommandGroup.EditCut);
		m_beginningTags.Add(StandardCommandGroup.EditSelectAll);
		m_beginningTags.Add(StandardCommandGroup.EditGroup);
		m_beginningTags.Add(StandardCommandGroup.EditOther);
		m_endingTags.Add(StandardCommandGroup.EditPreferences);
		m_beginningTags.Add(StandardCommandGroup.ViewZoomIn);
		m_beginningTags.Add(StandardCommandGroup.ViewControls);
		m_beginningTags.Add(StandardCommandGroup.WindowLayout);
		m_beginningTags.Add(StandardCommandGroup.WindowSplit);
		m_endingTags.Add(StandardCommandGroup.WindowDocuments);
		m_endingTags.Add(StandardCommandGroup.HelpAbout);
		m_beginningTags.Add(CommandId.FileRecentlyUsed1);
		m_beginningTags.Add(CommandId.FileRecentlyUsed2);
		m_beginningTags.Add(CommandId.FileRecentlyUsed3);
		m_beginningTags.Add(CommandId.FileRecentlyUsed4);
		m_endingTags.Add(StandardCommand.FileExit);
		m_endingTags.Add(CommandId.EditPreferences);
		m_endingTags.Add(CommandId.EditDocumentPreferences);
		m_defaultSortByMenuLabel.Add(StandardCommandGroup.WindowDocuments);
	}

	public void SetStatusService(IStatusService statusService)
	{
		if (m_statusService == null)
		{
			m_statusService = statusService;
		}
	}
}
