using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(RecentDocumentCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class RecentDocumentCommands : ICommandClient, IInitializable
{
	public enum Command
	{
		Pin,
		EmptyMru
	}

	public const int DefaultRecentDocumentCount = 8;

	public const int MaxRecentDocumentCount = 32;

	private readonly IDocumentService m_documentService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_documentClients;

	private string[] m_extensionFilter;

	private readonly PinnableActiveCollection<RecentDocumentInfo> m_recentDocuments = new PinnableActiveCollection<RecentDocumentInfo>(8);

	private readonly List<RecentDocumentInfo> m_registeredRecentDocs = new List<RecentDocumentInfo>(8);

	private string m_activeDocument;

	private CommandInfo m_emptyMruCommandInfo = null;

	public IEnumerable<RecentDocumentInfo> RecentDocuments => m_recentDocuments;

	public string[] RecentDocumentExtensions
	{
		get
		{
			return m_extensionFilter;
		}
		set
		{
			m_extensionFilter = value;
		}
	}

	[DefaultValue(8)]
	public int RecentDocumentCount
	{
		get
		{
			return m_recentDocuments.MaximumCount;
		}
		set
		{
			if (value < 1 || value > 32)
			{
				throw new ArgumentException("Must be between 1 and 32".Localize());
			}
			m_recentDocuments.MaximumCount = value;
		}
	}

	public string RecentDocumentsAsCsv
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("DocumentServiceSettings");
			xmlDocument.AppendChild(xmlElement);
			foreach (RecentDocumentInfo item in m_recentDocuments.MostRecentOrder)
			{
				if (item != null)
				{
					XmlElement xmlElement2 = xmlDocument.CreateElement("info");
					xmlElement.PrependChild(xmlElement2);
					xmlElement2.SetAttribute("uri", item.Uri.LocalPath);
					xmlElement2.SetAttribute("pinned", item.Pinned.ToString());
				}
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			RecentDocumentInfo[] array = m_recentDocuments.ToArray();
			RecentDocumentInfo[] array2 = array;
			foreach (RecentDocumentInfo info in array2)
			{
				RemoveDocument(info);
			}
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement xmlElement;
			try
			{
				xmlDocument.LoadXml(value);
				xmlElement = xmlDocument.DocumentElement;
			}
			catch (XmlException)
			{
				xmlElement = null;
			}
			foreach (XmlNode item in xmlElement.GetElementsByTagName("info"))
			{
				string value2 = item.Attributes["uri"].Value;
				if (File.Exists(value2))
				{
					Uri uri = new Uri(value2, UriKind.RelativeOrAbsolute);
					bool result = false;
					XmlAttribute xmlAttribute = item.Attributes["pinned"];
					if (xmlAttribute != null)
					{
						bool.TryParse(xmlAttribute.Value, out result);
					}
					AddDocument(uri, null, result);
				}
			}
			UpdateRecentFilesMenuItems();
		}
	}

	public virtual int MaxPathLength => -1;

	protected ICommandService CommandService { get; private set; }

	protected IEnumerable<CommandInfo> CommandInfos { get; private set; }

	[ImportingConstructor]
	public RecentDocumentCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IDocumentService documentService)
	{
		CommandService = commandService;
		documentRegistry.DocumentAdded += documentRegistry_DocumentAdded;
		m_documentService = documentService;
		documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
		documentRegistry.DocumentRemoved += documentRegistry_DocumentRemoved;
		m_recentDocuments.ItemRemoved += documentInfo_ItemRemoved;
	}

	public virtual void Initialize()
	{
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => RecentDocumentCount, "Recent Files Count".Localize("Number of recent files to display in File Menu"), null, "Number of recent files to display in File Menu".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => RecentDocumentsAsCsv, "RecentDocuments", null, null);
			m_settingsService.RegisterSettings("Sce.Atf.Applications.RecentDocumentCommands", boundPropertyDescriptor, boundPropertyDescriptor2);
			m_settingsService.RegisterUserSettings("Documents".Localize(), boundPropertyDescriptor);
		}
		if (CommandService != null)
		{
			CommandInfo info = new CommandInfo(Command.Pin, StandardMenu.File, null, "Pin file".Localize("Pin active file to the recent files list"), "Pin active file to the recent files list".Localize(), Keys.None, Resources.PinGreenImage, CommandVisibility.Menu);
			CommandService.RegisterCommand(info, this);
			m_emptyMruCommandInfo = new CommandInfo(Command.EmptyMru, StandardMenu.File, StandardCommandGroup.FileRecentlyUsed, "Recent Files".Localize() + "/(" + "empty".Localize() + ")", "No entries in recent files list".Localize(), Keys.None);
			CommandService.RegisterCommand(m_emptyMruCommandInfo, this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is RecentDocumentInfo)
		{
			return true;
		}
		if (commandTag is Command && (Command)commandTag == Command.Pin && !string.IsNullOrEmpty(m_activeDocument) && File.Exists(m_activeDocument))
		{
			return true;
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		bool flag = false;
		if (CommandService != null && CommandService is CommandServiceBase)
		{
			CommandServiceBase commandServiceBase = CommandService as CommandServiceBase;
			flag = commandServiceBase.IconClicked;
		}
		if (commandTag is RecentDocumentInfo && !flag)
		{
			RecentDocumentInfo recentDocumentInfo = (RecentDocumentInfo)commandTag;
			IDocument document = null;
			IDocumentClient documentClient = FindClientFromUri(recentDocumentInfo.Uri);
			if (documentClient != null && documentClient.CanOpen(recentDocumentInfo.Uri))
			{
				document = m_documentService.OpenExistingDocument(documentClient, recentDocumentInfo.Uri);
			}
			if (document == null && !Debugger.IsAttached)
			{
				RemoveDocument(recentDocumentInfo);
			}
		}
		else
		{
			RecentDocumentInfo recentDocumentInfo2 = ((!(commandTag is RecentDocumentInfo)) ? GetActiveRecentDocumentInfo() : ((RecentDocumentInfo)commandTag));
			if (recentDocumentInfo2 != null)
			{
				recentDocumentInfo2.Pinned = !recentDocumentInfo2.Pinned;
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
		bool flag = false;
		bool flag2 = true;
		if (commandTag is RecentDocumentInfo recentDocumentInfo)
		{
			flag = true;
			state.Text = recentDocumentInfo.Uri.LocalPath;
			if (!recentDocumentInfo.Pinned)
			{
				flag2 = false;
			}
		}
		else if (commandTag is Command command)
		{
			switch (command)
			{
			case Command.Pin:
			{
				RecentDocumentInfo activeRecentDocumentInfo = GetActiveRecentDocumentInfo();
				string text = "Pin active document".Localize();
				if (activeRecentDocumentInfo != null)
				{
					flag2 = !activeRecentDocumentInfo.Pinned;
					string text2 = activeRecentDocumentInfo.Uri.AbsolutePath;
					if (MaxPathLength > 0 && text2.Length > MaxPathLength)
					{
						text2 = text2.Substring(text2.Length - MaxPathLength);
						while (!text2.StartsWith("/"))
						{
							text2 = text2.Substring(1);
						}
						text2 = "..." + text2;
					}
					text = string.Format(activeRecentDocumentInfo.Pinned ? "Unpin {0}".Localize("{0} will be replaced with a file name") : "Pin {0}".Localize("{0} will be replaced with a file name"), text2);
				}
				state.Text = text;
				break;
			}
			case Command.EmptyMru:
				return;
			}
		}
		if (!(CommandService is CommandServiceBase commandServiceBase))
		{
			return;
		}
		CommandInfo commandInfo = commandServiceBase.GetCommandInfo(commandTag);
		if (commandInfo != null)
		{
			if (flag && commandServiceBase.MouseIsOverCommandIcon == commandInfo)
			{
				flag2 = !flag2;
			}
			string text3 = (flag2 ? Resources.PinGreenImage : Resources.PinGreyImage);
			if (commandInfo.ImageName != text3)
			{
				commandInfo.ImageName = text3;
				commandServiceBase.RefreshImage(commandInfo);
			}
		}
	}

	private RecentDocumentInfo GetActiveRecentDocumentInfo()
	{
		IEnumerable<RecentDocumentInfo> enumerable = m_recentDocuments.Where((RecentDocumentInfo info) => info.Uri.AbsolutePath == m_activeDocument);
		using (IEnumerator<RecentDocumentInfo> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	private void documentInfo_ItemRemoved(object sender, ItemRemovedEventArgs<RecentDocumentInfo> e)
	{
		if (CommandService != null && m_registeredRecentDocs.Contains(e.Item))
		{
			CommandService.UnregisterCommand(e.Item, this);
			m_registeredRecentDocs.Remove(e.Item);
		}
	}

	private void AddDocument(Uri uri, string type, bool pinned)
	{
		if (type == null)
		{
			IDocumentClient documentClient = FindClientFromUri(uri);
			type = ((documentClient != null) ? documentClient.Info.FileType : string.Empty);
		}
		m_recentDocuments.ActiveItem = new RecentDocumentInfo(uri, type, pinned);
	}

	private IDocumentClient FindClientFromUri(Uri uri)
	{
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			if (lazy.Value.CanOpen(uri))
			{
				return lazy.Value;
			}
		}
		return null;
	}

	private void documentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		IDocument item = e.Item;
		if (File.Exists(item.Uri.LocalPath) && CanAdd(item.Uri))
		{
			bool pinned = m_recentDocuments.GetPinnedState(item.Uri) ?? false;
			AddDocument(item.Uri, item.Type, pinned);
			UpdateRecentFilesMenuItems();
		}
	}

	private void documentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		IDocument item = e.Item;
		if (File.Exists(item.Uri.LocalPath) && CanAdd(item.Uri))
		{
			bool pinned = m_recentDocuments.GetPinnedState(item.Uri) ?? false;
			if (!m_recentDocuments.Contains(new RecentDocumentInfo(item.Uri, item.Type, pinned)))
			{
				AddDocument(item.Uri, item.Type, pinned);
				UpdateRecentFilesMenuItems();
			}
		}
	}

	private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		IDocument activeDocument = ((IDocumentRegistry)sender).ActiveDocument;
		if (activeDocument != null)
		{
			m_activeDocument = activeDocument.Uri.AbsolutePath;
		}
		else
		{
			m_activeDocument = string.Empty;
		}
		UpdateRecentFilesMenuItems();
	}

	protected virtual void UpdateRecentFilesMenuItems()
	{
		if (CommandService == null)
		{
			return;
		}
		List<CommandInfo> list = new List<CommandInfo>();
		foreach (RecentDocumentInfo recentDocument in m_recentDocuments)
		{
			if (m_registeredRecentDocs.Contains(recentDocument))
			{
				CommandService.UnregisterCommand(recentDocument, this);
				m_registeredRecentDocs.Remove(recentDocument);
			}
		}
		if (m_recentDocuments.Count > 0 && m_emptyMruCommandInfo != null)
		{
			CommandService.UnregisterCommand(m_emptyMruCommandInfo.CommandTag, this);
			m_emptyMruCommandInfo = null;
		}
		foreach (RecentDocumentInfo item in m_recentDocuments.MostRecentOrder)
		{
			if (!m_registeredRecentDocs.Contains(item))
			{
				string localPath = item.Uri.LocalPath;
				localPath = localPath.Replace("/", "-");
				localPath = localPath.Replace("\\", "-");
				CommandInfo commandInfo = new CommandInfo(item, StandardMenu.File, StandardCommandGroup.FileRecentlyUsed, "Recent Files".Localize() + "/" + localPath, "Open a recently used file".Localize(), Keys.None);
				commandInfo.ImageName = (item.Pinned ? Resources.PinGreenImage : Resources.PinGreyImage);
				commandInfo.ShortcutsEditable = false;
				CommandService.RegisterCommand(commandInfo, this);
				list.Add(commandInfo);
				m_registeredRecentDocs.Add(item);
			}
		}
		CommandInfos = list;
	}

	private void RemoveDocument(RecentDocumentInfo info)
	{
		m_recentDocuments.Remove(info);
	}

	private bool CanAdd(Uri fileUri)
	{
		bool result = true;
		if (m_extensionFilter != null)
		{
			result = false;
			string absolutePath = fileUri.AbsolutePath;
			string[] extensionFilter = m_extensionFilter;
			foreach (string value in extensionFilter)
			{
				if (absolutePath.EndsWith(value, ignoreCase: true, CultureInfo.InvariantCulture))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
