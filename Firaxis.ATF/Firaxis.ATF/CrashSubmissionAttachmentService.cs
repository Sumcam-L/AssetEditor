using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(CrashSubmissionAttachmentService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CrashSubmissionAttachmentService : IInitializable
{
	[Import(AllowDefault = true)]
	private IDocumentRegistry m_docRegistry;

	private ICrashSubmissionService CrashSubmissionService { get; set; }

	[ImportingConstructor]
	public CrashSubmissionAttachmentService(ICrashSubmissionService crashSubmissionSvc)
	{
		CrashSubmissionService = crashSubmissionSvc;
	}

	private void DocRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		CrashSubmissionService.RemoveAttachment(e.Item.Uri);
	}

	private void DocRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		CrashSubmissionService.AddAttachment(e.Item.Uri);
	}

	public void Initialize()
	{
		RegisterDocumentRegistryHandlers();
	}

	private void RegisterDocumentRegistryHandlers()
	{
		if (m_docRegistry != null)
		{
			m_docRegistry.DocumentAdded += DocRegistry_DocumentAdded;
			m_docRegistry.DocumentRemoved += DocRegistry_DocumentRemoved;
		}
	}
}
