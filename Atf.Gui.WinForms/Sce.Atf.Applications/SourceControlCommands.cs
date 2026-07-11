using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(SourceControlCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SourceControlCommands : SourceControlCommandsBase
{
	private enum SourceControlCommandGroup
	{
		OnOff
	}

	[ImportingConstructor]
	public SourceControlCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IDocumentService documentService)
		: base(commandService, documentRegistry, documentService)
	{
		documentService.DocumentSaving += OnDocumentSaving;
	}

	protected virtual void OnDocumentSaving(object sender, DocumentEventArgs e)
	{
		SyncFileIfOutOfDate(e.Document, delegate(SourceControlResultCodeEventArgs res)
		{
			if ((bool)res.SourceControlResult)
			{
				TestCheckedIn(sender);
			}
		});
	}
}
