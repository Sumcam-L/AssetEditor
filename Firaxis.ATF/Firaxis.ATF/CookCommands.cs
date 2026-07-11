using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(CookCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CookCommands : ICommandClient, IInitializable
{
	private enum Command
	{
		Cook
	}

	private struct CookCommandTag
	{
		public Command Command;

		public CookCommandTag(Command command)
		{
			Command = command;
		}
	}

	private string m_cookIconName;

	private ICookService CookerService { get; set; }

	private AssetBrowserFileCommands FileCommands { get; set; }

	protected ICommandService CommandService { get; private set; }

	public string CookIconName
	{
		get
		{
			return m_cookIconName;
		}
		set
		{
			m_cookIconName = value;
		}
	}

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	protected IFileDialogService FileDialogService { get; private set; }

	[ImportingConstructor]
	public CookCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService, ICookService cookerService, AssetBrowserFileCommands fileCommands)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			CommandService = commandService;
			DocumentRegistry = documentRegistry;
			FileDialogService = fileDialogService;
			CookerService = cookerService;
			FileCommands = fileCommands;
		}
	}

	private void Cook(IDocument document)
	{
		if (!document.Dirty || (MessageBox.Show(document.Type + " document " + document.Uri.LocalPath + " is dirty and must be saved to be cooked. Would you like to save now?", "File has changed", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) != DialogResult.No && FileCommands.Save(document)))
		{
			CookResult cookResult = CookerService.Cook(document);
			if ((bool)cookResult.Result)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Cooking {0} was successful.", document.Uri.LocalPath);
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Cooking {0} failed. Response: {1}", document.Uri.LocalPath, cookResult.Result.Message);
			}
		}
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is CookCommandTag && DocumentRegistry.ActiveDocument != null && !DocumentRegistry.ActiveDocument.IsReadOnly)
		{
			return DocumentRegistry.ActiveDocument is ICookable;
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is CookCommandTag cookCommandTag)
		{
			IDocument activeDocument = DocumentRegistry.ActiveDocument;
			if (cookCommandTag.Command == Command.Cook)
			{
				Cook(activeDocument);
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void RegisterClientCommands()
	{
		Sce.Atf.Input.Keys shortcut = Sce.Atf.Input.Keys.C | Sce.Atf.Input.Keys.Shift | Sce.Atf.Input.Keys.Control;
		CommandService.RegisterCommand(new CommandInfo(new CookCommandTag(Command.Cook), StandardMenu.File, StandardCommandGroup.FileSave, "Cook".Localize("Name of a command"), "Cooks an existing document".Localize(), shortcut, CookIconName, (!string.IsNullOrEmpty(CookIconName)) ? CommandVisibility.All : CommandVisibility.Menu), this);
	}
}
