using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Firaxis.ATF;

[Export(typeof(IWorkspaceChangeMediator))]
[Export(typeof(WorkspaceChangeMediator))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WorkspaceChangeMediator : IWorkspaceChangeMediator
{
	[Import(AllowDefault = true)]
	private IPreviewerDocumentService _previewerDocService;

	[Import(AllowDefault = true)]
	private ITunerQueueService _tunerQueue;

	public void AddChangesToQueue(IEnumerable<Uri> uris)
	{
		_tunerQueue?.AddFilesToQueue(uris);
		_previewerDocService?.ApplyWorkspaceChanges(uris);
	}

	public void AddChangeToQueue(Uri uri)
	{
		AddChangesToQueue(new Uri[1] { uri });
	}
}
