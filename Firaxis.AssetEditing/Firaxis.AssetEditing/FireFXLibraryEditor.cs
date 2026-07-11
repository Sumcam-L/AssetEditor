using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(FireFXLibraryEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FireFXLibraryEditor : IInitializable, IDocumentClient, IControlHostClient, IDisposable
{
	public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfoEx("FireFX Library".Localize(), ".FireFX", Resources.NewFileIcon, Resources.FireFXCompileIcon, multiDocument: true, isHidden: false, "Other");

	private IControlHostService ControlHostService { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IContextRegistry ContextRegistry { get; set; }

	private IDocumentService DocumentService { get; set; }

	public bool AskWhenClosingDirtyDocument => true;

	public DocumentClientInfo Info { get; private set; } = DocumentClientInfo;

	[ImportingConstructor]
	public FireFXLibraryEditor(IControlHostService controlHostService, IDocumentRegistry documentRegistry, IContextRegistry contextRegistry, IDocumentService documentService)
	{
		ControlHostService = controlHostService;
		DocumentRegistry = documentRegistry;
		ContextRegistry = contextRegistry;
		DocumentService = documentService;
	}

	public void Initialize()
	{
	}

	public bool CanOpen(Uri uri)
	{
		return DocumentClientInfo.IsCompatibleUri(uri);
	}

	public void Close(IDocument document)
	{
		FireFXLibraryContext fireFXLibraryContext = document.As<FireFXLibraryContext>();
		ControlHostService.UnregisterControl(fireFXLibraryContext.Control);
		fireFXLibraryContext.ControlInfo = null;
		foreach (DomNode item in fireFXLibraryContext.DomNode.Subtree)
		{
			foreach (EditingContext item2 in item.AsAll<EditingContext>())
			{
				ContextRegistry.RemoveContext(item2);
			}
		}
		DocumentRegistry.Remove(document);
	}

	public IDocument Open(Uri uri)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Opening FireFX file \"{0}\"", uri.LocalPath);
		string scriptText = LoadTextIfExists(uri);
		IDocument document = CreateDocument(uri, scriptText);
		RegisterDocumentControl(document);
		Outputs.WriteLine(OutputMessageType.Info, "Opened FireFX file \"{0}\"", uri.LocalPath);
		return document;
	}

	public void Reload(IDocument document)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Reloading FireFX file \"{0}\"", document.Uri.LocalPath);
		document.As<FireFXLibraryDocument>().Text = LoadTextIfExists(document.Uri);
		Outputs.WriteLine(OutputMessageType.Info, "Reloaded FireFX file \"{0}\"", document.Uri.LocalPath);
	}

	public bool Save(IDocument document, Uri uri)
	{
		FireFXLibraryDocument fireFXLibraryDocument = document.As<FireFXLibraryDocument>();
		using (StreamWriter streamWriter = new StreamWriter(uri.LocalPath, append: false, Encoding.ASCII))
		{
			streamWriter.Write(fireFXLibraryDocument.Text);
		}
		return true;
	}

	public void Show(IDocument document)
	{
		FireFXLibraryContext fireFXLibraryContext = document.As<FireFXLibraryContext>();
		ControlHostService.Show(fireFXLibraryContext.Control);
	}

	public void Activate(Control control)
	{
		if (control.Tag is FireFXLibraryDocument fireFXLibraryDocument)
		{
			DocumentRegistry.ActiveDocument = fireFXLibraryDocument;
			ContextRegistry.ActiveContext = fireFXLibraryDocument.As<FireFXLibraryContext>();
		}
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		if (!(control.Tag is FireFXLibraryDocument fireFXLibraryDocument))
		{
			return true;
		}
		if (!DocumentService.Close(fireFXLibraryDocument))
		{
			return false;
		}
		ContextRegistry.RemoveContext(fireFXLibraryDocument);
		return true;
	}

	public void Dispose()
	{
	}

	private IDocument CreateDocument(Uri uri, string scriptText)
	{
		string localPath = uri.LocalPath;
		string fileName = Path.GetFileName(localPath);
		DomNode domNode = new DomNode(FireFXLibrarySchema.FireFXLibraryType.Type);
		domNode.InitializeExtensions();
		FireFXLibraryContext fireFXLibraryContext = domNode.As<FireFXLibraryContext>();
		fireFXLibraryContext.ControlInfo = new ControlInfo(fileName, localPath, StandardControlGroup.Center, ResourceUtil.GetIcon(Resources.OpenSourceFileIcon), null)
		{
			IsDocument = true
		};
		FireFXLibraryDocument fireFXLibraryDocument = domNode.As<FireFXLibraryDocument>();
		fireFXLibraryDocument.Uri = uri;
		fireFXLibraryDocument.Text = scriptText;
		fireFXLibraryContext.Doc = fireFXLibraryDocument;
		fireFXLibraryContext.Control.Tag = fireFXLibraryDocument;
		return fireFXLibraryDocument;
	}

	private string LoadTextIfExists(Uri uri)
	{
		if (!File.Exists(uri.LocalPath))
		{
			return string.Empty;
		}
		using StreamReader streamReader = new StreamReader(uri.LocalPath);
		return streamReader.ReadToEnd();
	}

	private void RegisterDocumentControl(IDocument doc)
	{
		FireFXLibraryContext fireFXLibraryContext = doc.As<FireFXLibraryContext>();
		ControlHostService.RegisterControl(fireFXLibraryContext.Control, fireFXLibraryContext.ControlInfo, this);
	}
}
