using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications.VersionControl;

namespace Sce.Atf.Wpf.Applications;

[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(SourceControlCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SourceControlCommands : SourceControlCommandsBase
{
	[ImportingConstructor]
	public SourceControlCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IDocumentService documentService)
		: base(commandService, documentRegistry, documentService)
	{
	}

	protected override bool DoReconcile(bool doing)
	{
		if (base.SourceControlService == null || SourceControlContext == null)
		{
			return false;
		}
		if (!doing)
		{
			return SourceControlContext.Resources.Any();
		}
		List<Uri> list = SourceControlContext.Resources.Select((IResource resource) => resource.Uri).ToList();
		List<Uri> list2 = new List<Uri>();
		List<Uri> list3 = new List<Uri>();
		foreach (Uri modifiedFile in base.SourceControlService.GetModifiedFiles(list))
		{
			if (base.SourceControlService.GetStatus(modifiedFile) != SourceControlStatus.CheckedOut)
			{
				list2.Add(modifiedFile);
			}
		}
		foreach (Uri item in list)
		{
			if (!list2.Contains(item) && base.SourceControlService.GetStatus(item) == SourceControlStatus.NotControlled)
			{
				list3.Add(item);
			}
		}
		ReconcileViewModel viewModel = new ReconcileViewModel(base.SourceControlService, list2, list3);
		DialogUtils.ShowDialogWithViewModel<ReconcileDialog>(viewModel);
		return true;
	}

	protected override void ShowCheckInDialog(IList<IResource> toCheckIns)
	{
		CheckInViewModel viewModel = new CheckInViewModel(base.SourceControlService, toCheckIns);
		DialogUtils.ShowDialogWithViewModel<CheckInDialog>(viewModel);
	}
}
