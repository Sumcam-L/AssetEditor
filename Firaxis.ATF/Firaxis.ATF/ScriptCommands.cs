using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(ScriptCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ScriptCommands : ICommandClient, IInitializable
{
	private enum Command
	{
		RunScript
	}

	private struct RunScriptCommandTag
	{
		public Command Command;

		public RunScriptCommandTag(Command command)
		{
			Command = command;
		}
	}

	private readonly IList<IDisposable> PreviewUpdateSuspendsions = new List<IDisposable>();

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	protected IFileDialogService FileDialogService { get; private set; }

	protected ScriptingService ScriptingService { get; private set; }

	[ImportingConstructor]
	public ScriptCommands(ICommandService commandService, ScriptingService scriptingSvc, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService)
	{
		CommandService = commandService;
		ScriptingService = scriptingSvc;
		DocumentRegistry = documentRegistry;
		FileDialogService = fileDialogService;
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return commandTag is RunScriptCommandTag;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is RunScriptCommandTag)
		{
			RunScript();
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public void RunScript()
	{
		string pathName = string.Empty;
		FileDialogService.ForcedInitialDirectory = null;
		if (FileDialogService.OpenFileName(ref pathName, "Python Script (*.py)|*.py") == FileDialogResult.Cancel)
		{
			return;
		}
		BugSubmitter.SilentAssert(PreviewUpdateSuspendsions.Count == 0, "Preview update suspensions for {0} documents were orphaned before running script \"{1}\" @summary Preview update suspensions orphaned @assign bwhitman", PreviewUpdateSuspendsions.Count, pathName);
		PreviewUpdateSuspendsions.Clear();
		try
		{
			using (new WaitCursor())
			{
				DocumentRegistry.Documents.Where((IDocument wd) => wd.Is<IEntityPreviewComponent>()).ForEach(delegate(IDocument doc)
				{
					IEntityPreviewComponent entityPreviewComponent = doc.As<IEntityPreviewComponent>();
					PreviewUpdateSuspendsions.Add(entityPreviewComponent.SuspendPreviewerUpdates());
				});
				DocumentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
				ScriptingService.ExecuteFile(pathName);
				DocumentRegistry.DocumentAdded -= DocumentRegistry_DocumentAdded;
			}
		}
		catch (System.Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
		finally
		{
			PreviewUpdateSuspendsions.Clear();
			DocumentRegistry.Documents.Where((IDocument wd) => wd.Is<IEntityPreviewComponent>() && wd.Is<IInstanceEntityAdapter>()).ForEach(delegate(IDocument doc)
			{
				IEntityPreviewComponent entityPreviewComponent = doc.As<IEntityPreviewComponent>();
				IInstanceEntityAdapter instanceEntityAdapter = doc.As<IInstanceEntityAdapter>();
				entityPreviewComponent.EntityChanges.CreateEntityChangedEvent(instanceEntityAdapter.InstanceType, instanceEntityAdapter.Name);
			});
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		IEntityPreviewComponent entityPreviewComponent = e.Item.As<IEntityPreviewComponent>();
		if (entityPreviewComponent != null)
		{
			PreviewUpdateSuspendsions.Add(entityPreviewComponent.SuspendPreviewerUpdates());
		}
	}

	private void RegisterClientCommands()
	{
		Keys shortcut = Keys.R | Keys.Shift | Keys.Control;
		CommandService.RegisterCommand(new CommandInfo(new RunScriptCommandTag(Command.RunScript), StandardMenu.File, StandardCommandGroup.FileOther, "Run Script".Localize("Name of a command"), "Runs a python script in the tool".Localize(), shortcut, string.Empty, CommandVisibility.Menu), this);
	}
}
