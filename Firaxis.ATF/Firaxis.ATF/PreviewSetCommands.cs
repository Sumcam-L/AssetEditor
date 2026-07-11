using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(PreviewSetCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewSetCommands : ICommandClient, IInitializable, IPartImportsSatisfiedNotification
{
	private enum Command
	{
		Save,
		Load,
		Apply
	}

	private struct PreviewSetCommandTag
	{
		public Command Command;

		public PreviewSetCommandTag(Command command)
		{
			Command = command;
		}
	}

	public class EntityPathPair : IEquatable<EntityPathPair>, IEquatable<EntityID>
	{
		public string EntityName { get; set; }

		public InstanceType EntityType { get; set; }

		public string Path { get; set; }

		public EntityPathPair()
		{
			EntityName = string.Empty;
			EntityType = InstanceType.IT_INVALID;
			Path = string.Empty;
		}

		public EntityPathPair(EntityID id, string path)
		{
			EntityName = id.Name;
			EntityType = id.Type;
			Path = path;
		}

		private EntityID GetEntityID()
		{
			return new EntityID(EntityName, EntityType);
		}

		public bool Equals(EntityPathPair other)
		{
			EntityID entityID = GetEntityID();
			EntityID entityID2 = other.GetEntityID();
			return entityID.Equals(entityID2);
		}

		public bool Equals(EntityID other)
		{
			return GetEntityID().Equals(other);
		}
	}

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	private readonly List<EntityPathPair> m_openedPreviewSets = new List<EntityPathPair>();

	private readonly string m_openedPreviewSetsFile;

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	protected IFileDialogService FileDialogService { get; private set; }

	private AssetBrowserFileCommands FileCommands { get; set; }

	private IPreviewSetService PreviewSetService { get; set; }

	[ImportingConstructor]
	public PreviewSetCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService, AssetBrowserFileCommands fileCommands, IPreviewSetService previewSetSvc)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		FileDialogService = fileDialogService;
		FileCommands = fileCommands;
		PreviewSetService = previewSetSvc;
		m_openedPreviewSetsFile = GetOpenedPreviewSetsPath();
		fileCommands.DocumentClosing += FileCommands_DocumentClosing;
		fileCommands.DocumentOpened += FileCommands_DocumentOpened;
		if (File.Exists(m_openedPreviewSetsFile))
		{
			m_openedPreviewSets = LoadOpenedPreviewSets();
		}
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("previewSetService", this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		bool result = false;
		bool flag = commandTag is PreviewSetCommandTag;
		bool flag2 = DocumentRegistry.ActiveDocument != null;
		bool flag3 = DocumentRegistry.ActiveDocument is IPreviewableDocument;
		if (flag)
		{
			PreviewSetCommandTag previewSetCommandTag = (PreviewSetCommandTag)commandTag;
			if (previewSetCommandTag.Command == Command.Load || previewSetCommandTag.Command == Command.Apply)
			{
				result = flag;
			}
			else if (previewSetCommandTag.Command == Command.Save)
			{
				result = flag && flag2 && flag3;
			}
		}
		return result;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is PreviewSetCommandTag previewSetCommandTag)
		{
			_ = DocumentRegistry.ActiveDocument;
			if (previewSetCommandTag.Command == Command.Save)
			{
				SavePreviewSet();
			}
			else if (previewSetCommandTag.Command == Command.Load)
			{
				LoadPreviewSet();
			}
			else if (previewSetCommandTag.Command == Command.Apply)
			{
				ApplyPreviewSetToCurrentAsset();
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public void ApplyPreviewSetToCurrentAsset()
	{
		string pathName = string.Empty;
		FileDialogService.ForcedInitialDirectory = null;
		if (FileDialogService.OpenFileName(ref pathName, "Preview Set File (*.xml)|*.xml") == FileDialogResult.OK)
		{
			ApplyPreviewSetToCurrentAsset(pathName);
		}
	}

	public PreviewSetData GetPreviewSetData()
	{
		return PreviewSetService.GeneratePreviewSetData();
	}

	public void LoadPreviewSet()
	{
		string pathName = string.Empty;
		FileDialogService.ForcedInitialDirectory = null;
		if (FileDialogService.OpenFileName(ref pathName, "Preview Set File (*.xml)|*.xml") == FileDialogResult.OK)
		{
			LoadPreviewSet(pathName);
		}
	}

	public void LoadPreviewSet(PreviewSetData previewSetData)
	{
		if (previewSetData != null && !string.IsNullOrEmpty(previewSetData.PrimaryAssetName))
		{
			if (!FileCommands.IsDocumentOpen(previewSetData.PrimaryAssetType, previewSetData.PrimaryAssetName))
			{
				FileCommands.OpenExistingDocument(previewSetData.PrimaryAssetType, previewSetData.PrimaryAssetName);
			}
			new EntityID(previewSetData.PrimaryAssetName, previewSetData.PrimaryAssetType);
			PreviewSetService.ApplyPreviewSetData(previewSetData);
		}
	}

	public void OnImportsSatisfied()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_mainWindow.Closing += RemoveEvents;
	}

	public void SavePreviewSet()
	{
		string pathName = string.Empty;
		FileDialogService.SaveFileName(ref pathName, "Preview Set File (*.xml)|*.xml");
		SavePreviewSet(pathName);
	}

	private void ApplyPreviewSet(EntityID id)
	{
		EntityPathPair entityPathPair = m_openedPreviewSets.FirstOrDefault((EntityPathPair set) => set.Equals(id));
		if (entityPathPair != null)
		{
			string path = entityPathPair.Path;
			if (File.Exists(path))
			{
				LoadPreviewSet(path);
			}
		}
	}

	private void ApplyPreviewSetToCurrentAsset(string filePath)
	{
		string errorMessage;
		PreviewSetData previewSetData = PreviewSetData.LoadFromFile(filePath, out errorMessage);
		if (previewSetData != null)
		{
			bool flag = DocumentRegistry.ActiveDocument != null;
			bool flag2 = DocumentRegistry.ActiveDocument is IPreviewableDocument;
			IEntityDocument entityDocument = DocumentRegistry.ActiveDocument.As<IEntityDocument>();
			if (entityDocument != null && flag && flag2)
			{
				previewSetData.PrimaryAssetName = entityDocument.InstanceEntity.Name;
				previewSetData.OpenedAssets[0].AssetName = entityDocument.InstanceEntity.Name;
				EntityID id = new EntityID(previewSetData.PrimaryAssetName, previewSetData.PrimaryAssetType);
				UpdateOpenedPreviewSet(id, filePath);
				PreviewSetService.ApplyPreviewSetData(previewSetData);
			}
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, errorMessage);
			MessageBox.Show(errorMessage);
		}
	}

	private void FileCommands_DocumentClosing(object sender, DocumentClosingEventArgs e)
	{
		if (e.Document is IInstanceEntityDocument instanceEntityDocument)
		{
			EntityID id = new EntityID(instanceEntityDocument.InstanceEntity);
			RemovePreviewSet(id);
		}
	}

	private void FileCommands_DocumentOpened(object sender, DocumentEventArgs e)
	{
		if (e.Document is IInstanceEntityDocument instanceEntityDocument)
		{
			EntityID id = new EntityID(instanceEntityDocument.InstanceEntity);
			ApplyPreviewSet(id);
		}
	}

	private string GetOpenedPreviewSetsPath()
	{
		AssemblyName name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
		string name2 = name.Name;
		Version version = name.Version;
		string text = version.Major + "." + version.Minor;
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string text2 = Path.Combine(folderPath, name2, text, "Opened Preview Sets", "Opened Preview Sets.xml");
		string directoryName = Path.GetDirectoryName(text2);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		return text2;
	}

	private List<EntityPathPair> LoadOpenedPreviewSets()
	{
		List<EntityPathPair> result = null;
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<EntityPathPair>));
			using FileStream stream = File.Open(m_openedPreviewSetsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			result = xmlSerializer.Deserialize(stream) as List<EntityPathPair>;
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Unable to load data regarding the previous preview sets.");
			Outputs.WriteLine(OutputMessageType.Error, "Reason: {0}", ex.Message);
			result = new List<EntityPathPair>();
		}
		return result;
	}

	private void LoadPreviewSet(string filePath)
	{
		string errorMessage;
		PreviewSetData previewSetData = PreviewSetData.LoadFromFile(filePath, out errorMessage);
		if (previewSetData != null)
		{
			if (!FileCommands.IsDocumentOpen(previewSetData.PrimaryAssetType, previewSetData.PrimaryAssetName))
			{
				FileCommands.OpenExistingDocument(previewSetData.PrimaryAssetType, previewSetData.PrimaryAssetName);
			}
			EntityID id = new EntityID(previewSetData.PrimaryAssetName, previewSetData.PrimaryAssetType);
			UpdateOpenedPreviewSet(id, filePath);
			PreviewSetService.ApplyPreviewSetData(previewSetData);
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, errorMessage);
			MessageBox.Show(errorMessage);
		}
	}

	private void RegisterClientCommands()
	{
		Sce.Atf.Input.Keys shortcut = Sce.Atf.Input.Keys.S | Sce.Atf.Input.Keys.Shift | Sce.Atf.Input.Keys.Alt;
		Sce.Atf.Input.Keys shortcut2 = Sce.Atf.Input.Keys.O | Sce.Atf.Input.Keys.Shift | Sce.Atf.Input.Keys.Alt;
		CommandService.RegisterCommand(new CommandInfo(new PreviewSetCommandTag(Command.Save), StandardMenu.Window, StandardCommandGroup.WindowLayout, "Save Preview Set".Localize("Name of a command"), "Saves the current preview configuration".Localize(), shortcut, string.Empty, CommandVisibility.Menu), this);
		CommandService.RegisterCommand(new CommandInfo(new PreviewSetCommandTag(Command.Load), StandardMenu.Window, StandardCommandGroup.WindowLayout, "Load Preview Set".Localize("Name of a command"), "Loads a preview configuration".Localize(), shortcut2, string.Empty, CommandVisibility.Menu), this);
		CommandService.RegisterCommand(new CommandInfo(new PreviewSetCommandTag(Command.Apply), StandardMenu.Window, StandardCommandGroup.WindowLayout, "Apply Preview Set to Current".Localize("Name of a command"), "Loads a preview configuration and applies to the current Asset".Localize(), Sce.Atf.Input.Keys.None, string.Empty, CommandVisibility.Menu), this);
	}

	private void RemoveEvents(object sender, CancelEventArgs e)
	{
		FileCommands.DocumentClosing -= FileCommands_DocumentClosing;
		FileCommands.DocumentOpened -= FileCommands_DocumentOpened;
		m_mainWindow.Closing -= RemoveEvents;
	}

	private void RemovePreviewSet(EntityID id)
	{
		if (RemovePreviewSetInternal(id))
		{
			SaveOpenedPreviewSets();
		}
	}

	private bool RemovePreviewSetInternal(EntityID id)
	{
		bool result = false;
		for (int i = 0; i < m_openedPreviewSets.Count; i++)
		{
			if (m_openedPreviewSets[i].Equals(id))
			{
				result = true;
				m_openedPreviewSets.RemoveAt(i);
				i--;
			}
		}
		return result;
	}

	private void SaveOpenedPreviewSets()
	{
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<EntityPathPair>));
			using FileStream stream = File.Open(m_openedPreviewSetsFile, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlSerializer.Serialize(stream, m_openedPreviewSets);
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Unable to save data regarding opened preview sets.");
			Outputs.WriteLine(OutputMessageType.Error, "Reason: {0}", ex.Message);
		}
	}

	private void SavePreviewSet(string filePath)
	{
		PreviewSetData previewSetData = PreviewSetService.GeneratePreviewSetData();
		string text;
		if (previewSetData == null)
		{
			text = "There is no active preview document to save to a preview set.  Please select an asset and try again.";
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error saving");
			return;
		}
		previewSetData.SaveToFile(filePath, overwrite: true, out text);
		if (string.IsNullOrEmpty(text))
		{
			EntityID id = new EntityID(previewSetData.PrimaryAssetName, previewSetData.PrimaryAssetType);
			UpdateOpenedPreviewSet(id, filePath);
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error saving");
		}
	}

	private void UpdateOpenedPreviewSet(EntityID id, string filePath)
	{
		RemovePreviewSetInternal(id);
		EntityPathPair item = new EntityPathPair(id, filePath);
		m_openedPreviewSets.Add(item);
		SaveOpenedPreviewSets();
	}
}
