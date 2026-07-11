using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.ATF;
using Firaxis.CivTech.FireFX;
using Firaxis.Error;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

[Export(typeof(FireFXScriptCommands))]
[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FireFXScriptCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	private enum Command
	{
		Compile
	}

	private struct FireFXScriptCommandTag
	{
		public Command Command;

		public FireFXScriptCommandTag(Command command)
		{
			Command = command;
		}
	}

	private static FireFXScriptCommandTag CompileCommandTag = new FireFXScriptCommandTag(Command.Compile);

	private ICommandService CommandService { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IFireFXService FireFXService { get; set; }

	[ImportingConstructor]
	public FireFXScriptCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFireFXService fireFXSvc)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		FireFXService = fireFXSvc;
	}

	private bool CanProvideScriptResource(IDocument doc)
	{
		if (doc.Is<IFireFXScriptResource>())
		{
			return true;
		}
		FireFXInstanceAdapter fireFXInstanceAdapter = doc.As<FireFXInstanceAdapter>();
		if (fireFXInstanceAdapter != null)
		{
			return fireFXInstanceAdapter.ClassName == "FireFXScript";
		}
		return false;
	}

	private IFireFXScriptResource GetSriptResource(IDocument doc)
	{
		IFireFXScriptResource fireFXScriptResource = doc.As<IFireFXScriptResource>();
		if (fireFXScriptResource == null)
		{
			fireFXScriptResource = doc.As<IFireFXEditorContext>()?.ScriptResource;
		}
		return fireFXScriptResource;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is FireFXScriptCommandTag && ((FireFXScriptCommandTag)commandTag).Command == Command.Compile)
		{
			return CanProvideScriptResource(DocumentRegistry.ActiveDocument);
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is FireFXScriptCommandTag && ((FireFXScriptCommandTag)commandTag).Command == Command.Compile)
		{
			DoCompile(DocumentRegistry.ActiveDocument, GetSriptResource(DocumentRegistry.ActiveDocument));
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public virtual void Initialize()
	{
		CommandService.RegisterCommand(new CommandInfo(CompileCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Compile".Localize("Name of a command"), "Compile FireFX script and report any warnings and errors".Localize(), Keys.None, Resources.FireFXCompileIcon, CommandVisibility.All), this);
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object clicked)
	{
		if (context.As<ISelectionContext>() != null)
		{
			return new object[1] { CompileCommandTag };
		}
		return EmptyEnumerable<object>.Instance;
	}

	private void DoCompile(IDocument doc, IFireFXScriptResource script)
	{
		string fileName = Path.GetFileName(script.Uri.LocalPath);
		CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Info, "Compiling FireFX script: {0}", fileName);
		doc.As<ITransactionContext>().DoTransaction(delegate
		{
			ResultCode resultCode = FireFXService.CompileResource(script);
			if ((bool)resultCode)
			{
				CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Info, "Compile succeeded with no errors");
			}
			else
			{
				ReportCompileIssues(resultCode, script.Issues);
			}
		}, "Compile Script");
	}

	private void ReportCompileIssues(ResultCode compileResult, IList<CompileIssue> compileIssues)
	{
		CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Error, OutputMessageVerbosity.Verbose, compileResult.Message);
		int num = 0;
		int num2 = 0;
		string text = "succeeded";
		OutputMessageType type = OutputMessageType.Warning;
		foreach (CompileIssue compileIssue in compileIssues)
		{
			OutputMessageType type2 = OutputMessageType.Info;
			if (compileIssue.Type == CompileIssueType.Warning)
			{
				type2 = OutputMessageType.Warning;
				num++;
			}
			else if (compileIssue.Type == CompileIssueType.Error)
			{
				text = "failed";
				type = OutputMessageType.Error;
				type2 = OutputMessageType.Error;
				num2++;
			}
			CategorizedOutputs.WriteLine("FireFX", type2, "{0}({1}): {2}", compileIssue.File, compileIssue.LineNo, compileIssue.Message);
		}
		CategorizedOutputs.WriteLine("FireFX", type, "Compile {0} with {1} warnings and {2} errors. Overall={3}", text, num, num2, compileResult.Message);
	}
}
