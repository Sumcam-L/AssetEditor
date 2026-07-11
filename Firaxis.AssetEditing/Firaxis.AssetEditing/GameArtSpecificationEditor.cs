using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(GameArtSpecificationEditor))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class GameArtSpecificationEditor : IDocumentClient, IControlHostClient, IInitializable
{
	private readonly IControlHostService ControlHostService;

	private readonly ICivTechService CivTechService;

	private readonly IDocumentService DocumentService;

	private readonly IContextRegistry ContextRegistry;

	private readonly IDocumentRegistry DocumentRegistry;

	private readonly IFileDialogService DialogService;

	private readonly IProjectMapService ProjectMapService;

	private readonly GameArtSpecificationSchemaLoader SchemaLoader;

	public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfoEx("Game Art Specification".Localize(), new string[1] { ".Art.xml" }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, multiDocument: true, isHidden: false, "Other");

	public bool AskWhenClosingDirtyDocument => true;

	public DocumentClientInfo Info => DocumentClientInfo;

	[ImportingConstructor]
	public GameArtSpecificationEditor(IControlHostService controlHostService, ICivTechService civTechService, IDocumentService documentService, IContextRegistry contextRegistry, IDocumentRegistry documentRegistry, IFileDialogService dialogSvc, IProjectMapService projectMapService, GameArtSpecificationSchemaLoader schemaLoader)
	{
		ControlHostService = controlHostService;
		CivTechService = civTechService;
		DocumentService = documentService;
		ContextRegistry = contextRegistry;
		DocumentRegistry = documentRegistry;
		DialogService = dialogSvc;
		ProjectMapService = projectMapService;
		SchemaLoader = schemaLoader;
		Outputs.WriteLine(OutputMessageType.Info, "Starting up GameArtSpecificationEditor.");
	}

	public bool CanOpen(Uri uri)
	{
		string localPath = uri.LocalPath;
		string value = Info.Extensions[0].ToLower();
		return localPath.ToLower().EndsWith(value);
	}

	public IDocument Open(Uri uri)
	{
		var sw = Stopwatch.StartNew();
		string localPath = uri.LocalPath;
		Path.GetFileName(localPath);
		IGameArtSpecification gameArtSpecification = Context.EnsureCreated<CivTechContext>().CreateInstance<IGameArtSpecification>();
		bool flag = File.Exists(localPath) && (bool)gameArtSpecification.DeserializeFromFile(localPath);
		if (!flag)
		{
			gameArtSpecification.ID.Name = "New Game";
			gameArtSpecification.ID.ID = Guid.NewGuid().ToString();
		}
		DomNode domNode = new DomNode(GameArtSpecificationSchema.GameArtSpecificationType.Type, GameArtSpecificationSchema.GameArtSpecificationRootElement);
		domNode.InitializeExtensions();
		ControlInfo controlInfo = new ControlInfo(gameArtSpecification.ID.Name + ".Art.xml", localPath, StandardControlGroup.Center);
		controlInfo.IsDocument = true;
		GameArtSpecificationDocument document = InitializeDocument(domNode, uri, gameArtSpecification, !flag);
		InitializeExtensions(domNode, uri, gameArtSpecification, document, controlInfo, !flag);
		controlInfo.IsDirtyDocument = () => document.Dirty;
		controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
		ControlHostService.RegisterControl(domNode.As<GameArtSpecificationContext>().GUI, controlInfo, this);
		document.UpdateControlInfo();
		PaintTimingLog.Write("Opened document {0} in {1}ms", uri, sw.ElapsedMilliseconds);
		return domNode.As<GameArtSpecificationDocument>();
	}

	private void InitializeExtensions(DomNode node, Uri uri, IGameArtSpecification gameArtSpecification, GameArtSpecificationDocument document, ControlInfo controlInfo, bool isNewDocument)
	{
		InitializeAdapter(node, gameArtSpecification);
		InitializeContext(node, document, controlInfo);
	}

	private GameArtSpecificationDocument InitializeDocument(DomNode node, Uri uri, IGameArtSpecification gameArtSpecification, bool isNewDocument)
	{
		GameArtSpecificationDocument gameArtSpecificationDocument = node.As<GameArtSpecificationDocument>();
		gameArtSpecificationDocument.Uri = uri;
		gameArtSpecificationDocument.GameArtSpecification = gameArtSpecification;
		gameArtSpecificationDocument.CivTechService = CivTechService;
		gameArtSpecificationDocument.IsNewGameArt = isNewDocument;
		return gameArtSpecificationDocument;
	}

	private void InitializeAdapter(DomNode node, IGameArtSpecification gameArtSpecification)
	{
		node.As<GameArtSpecificationAdapter>().Initialize();
	}

	private GameArtSpecificationContext InitializeContext(DomNode node, GameArtSpecificationDocument document, ControlInfo controlInfo)
	{
		GameArtSpecificationContext gameArtSpecificationContext = node.As<GameArtSpecificationContext>();
		gameArtSpecificationContext.FileDialogService = DialogService;
		gameArtSpecificationContext.ProjectMapService = ProjectMapService;
		gameArtSpecificationContext.ControlInfo = controlInfo;
		gameArtSpecificationContext.Doc = document;
		gameArtSpecificationContext.GUI.Tag = document;
		return gameArtSpecificationContext;
	}

	public void Reload(IDocument document)
	{
	}

	public void Show(IDocument document)
	{
		GameArtSpecificationContext gameArtSpecificationContext = document.As<GameArtSpecificationContext>();
		ControlHostService.Show(gameArtSpecificationContext.GUI);
	}

	public bool Save(IDocument document, Uri uri)
	{
		bool flag = false;
		GameArtSpecificationDocument gameArtSpecificationDocument = document.As<GameArtSpecificationDocument>();
		if (gameArtSpecificationDocument != null)
		{
			if (gameArtSpecificationDocument.GameArtSpecification != null)
			{
				flag = gameArtSpecificationDocument.GameArtSpecification.SerializeIntoFile(uri.LocalPath);
			}
			if (flag)
			{
				gameArtSpecificationDocument.IsNewGameArt = false;
				gameArtSpecificationDocument.DomNode.As<GameArtSpecificationContext>().DisableGenerateIDButton();
				if (gameArtSpecificationDocument.GameArtSpecification.ID.Equals(ProjectMapService.PrimaryProject.PrimaryArtSpecification.ID))
				{
					MessageBoxes.Show("Please restart the AssetEditor for your Art.xml changes to take effect", "Restart for changes to take effect", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		return flag;
	}

	public void Close(IDocument document)
	{
		GameArtSpecificationContext gameArtSpecificationContext = document.As<GameArtSpecificationContext>();
		ControlHostService.UnregisterControl(gameArtSpecificationContext.GUI);
		gameArtSpecificationContext.ControlInfo = null;
		foreach (DomNode item in gameArtSpecificationContext.DomNode.Subtree)
		{
			foreach (EditingContext item2 in item.AsAll<EditingContext>())
			{
				ContextRegistry.RemoveContext(item2);
			}
		}
		DocumentRegistry.Remove(document);
	}

	void IInitializable.Initialize()
	{
		Info.InitialDirectory = ProjectMapService.PrimaryProject.Paths.GamePantry;
	}

	public void Activate(Control control)
	{
		if (control.Tag is GameArtSpecificationDocument gameArtSpecificationDocument)
		{
			DocumentRegistry.ActiveDocument = gameArtSpecificationDocument;
			EditingContext activeContext = gameArtSpecificationDocument.As<EditingContext>();
			ContextRegistry.ActiveContext = activeContext;
		}
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		bool flag = true;
		if (control.Tag is GameArtSpecificationDocument gameArtSpecificationDocument)
		{
			flag = DocumentService.Close(gameArtSpecificationDocument);
			if (flag)
			{
				ContextRegistry.RemoveContext(gameArtSpecificationDocument);
			}
		}
		return flag;
	}
}
