using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(DocumentReloadService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DocumentReloadService : IInitializable
{
	[ImportMany]
	private Lazy<IDocumentClient>[] m_documentClients;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IFileWatcherService FileWatchService { get; set; }

	[ImportingConstructor]
	public DocumentReloadService(IFileWatcherService fileWatcher, IDocumentRegistry docRegistry)
	{
		FileWatchService = fileWatcher;
		DocumentRegistry = docRegistry;
	}

	public void Initialize()
	{
		FileWatchService.FileChanged += FileWatchService_FileChanged;
	}

	public void ReloadDocument(IDocument doc, bool forceReload = false)
	{
		if (doc != null)
		{
			IDocumentClient documentClient = GetDocumentClient(doc.Uri.LocalPath);
			if (documentClient != null && (forceReload || PromptForReload(doc.Uri.LocalPath)))
			{
				documentClient.Reload(doc);
				doc.As<IHistoryContext>()?.Clear();
			}
		}
	}

	private void FileWatchService_FileChanged(object sender, FileSystemEventArgs e)
	{
		IDocument document = DocumentRegistry.Documents.FirstOrDefault((IDocument doc) => doc.Uri.LocalPath == e.FullPath);
		if (document != null)
		{
			ReloadDocument(document);
		}
	}

	private IDocumentClient GetDocumentClient(string pathName)
	{
		return m_documentClients.Select((Lazy<IDocumentClient> lazy) => lazy.Value).GetFirstClientForPath(pathName);
	}

	private bool PromptForReload(string pathName)
	{
		string text = "File " + pathName + " has changed on disk. Would you like to reload the document?";
		return MessageBox.Show(m_mainForm, text, "File Changed", MessageBoxButtons.YesNo) == DialogResult.Yes;
	}
}
