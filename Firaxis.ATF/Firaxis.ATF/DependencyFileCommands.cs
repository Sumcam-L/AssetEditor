using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(DependencyFileCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DependencyFileCommands : IDependencyFileCommands, ICommandClient, IInitializable
{
	private enum DependencyCommands
	{
		OpenDependency,
		OpenDependant
	}

	private readonly IDocumentService m_documentService;

	private DependencyInfoControl Control;

	private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

	public static CommandInfo OpenDependency = new CommandInfo(DependencyCommands.OpenDependency, StandardMenu.Edit, StandardCommandGroup.FileOther, "Entity".Localize(), "Entity".Localize("Entity"), Sce.Atf.Input.Keys.None, Resources.OpenEntityIcon, CommandVisibility.ControlDefault);

	public static CommandInfo OpenDependant = new CommandInfo(DependencyCommands.OpenDependant, StandardMenu.Edit, StandardCommandGroup.FileOther, "Entity".Localize(), "Entity".Localize("Entity"), Sce.Atf.Input.Keys.None, Resources.OpenEntityIcon, CommandVisibility.ControlDefault);

	public IEnumerable<CommandInfo> Commands { get; } = new CommandInfo[2] { OpenDependency, OpenDependant };

	[Import(AllowDefault = true)]
	private ICommandService CommandService { get; set; }

	public DependencyFileCommands(ICommandService commandService, IDocumentService docService, IEnumerable<Lazy<IDocumentClient>> docClients, DependencyInfoControl depInfoControl)
	{
		Control = depInfoControl;
		CommandService = commandService;
		m_documentService = docService;
		m_documentClients = docClients;
		Control.dependantTree.MouseDoubleClick += dependant_MouseDoubleClick;
		Control.dependencyTree.MouseDoubleClick += dependencyTree_MouseDoubleClick;
	}

	public bool CanDoCommand(object commandTag)
	{
		return true;
	}

	public void DoCommand(object commandTag)
	{
		Uri result = null;
		switch ((DependencyCommands)commandTag)
		{
		case DependencyCommands.OpenDependency:
			if (Uri.TryCreate(Control.dependencyTree.SelectedNode.Tag.ToString(), UriKind.RelativeOrAbsolute, out result))
			{
				OpenExistingDocument(result);
			}
			break;
		case DependencyCommands.OpenDependant:
			if (Uri.TryCreate(Control.dependantTree.SelectedNode.Tag.ToString(), UriKind.RelativeOrAbsolute, out result))
			{
				OpenExistingDocument(result);
			}
			break;
		}
	}

	public void OpenExistingDocument(Uri docUri)
	{
		foreach (IDocumentClient value in m_documentClients.GetValues())
		{
			if (value.CanOpen(docUri))
			{
				m_documentService.OpenExistingDocument(value, docUri);
				break;
			}
		}
	}

	public void Initialize()
	{
		CommandService.RegisterCommand(OpenDependency, this);
		CommandService.RegisterCommand(OpenDependant, this);
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void dependencyTree_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (sender is TreeView { SelectedNode: not null })
		{
			DoCommand(DependencyCommands.OpenDependency);
		}
	}

	private void dependant_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (sender is TreeView { SelectedNode: not null })
		{
			DoCommand(DependencyCommands.OpenDependant);
		}
	}
}
