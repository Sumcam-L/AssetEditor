using System;
using System.ComponentModel.Composition;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardPrintCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardPrintCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardPrintCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.FilePrint, this);
		m_commandService.RegisterCommand(CommandInfo.FilePageSetup, this);
		m_commandService.RegisterCommand(CommandInfo.FilePrintPreview, this);
	}

	public void ShowPrintDialog()
	{
		PrintDialog printDialog = new PrintDialog();
		PrintDocument printDocument = (printDialog.Document = GetPrintDocument());
		printDialog.AllowCurrentPage = true;
		printDialog.AllowSelection = true;
		printDialog.AllowSomePages = true;
		printDialog.UseEXDialog = true;
		if (printDialog.ShowDialog() == DialogResult.OK)
		{
			printDocument.Print();
		}
	}

	public void ShowPageSettingsDialog()
	{
		PageSetupDialog pageSetupDialog = new PageSetupDialog();
		PrintDocument printDocument = GetPrintDocument();
		pageSetupDialog.Document = printDocument;
		pageSetupDialog.ShowDialog();
	}

	public void ShowPrintPreviewDialog()
	{
		PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
		PrintDocument printDocument = GetPrintDocument();
		printPreviewDialog.Document = printDocument;
		printPreviewDialog.ShowDialog();
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.PageSetup:
			case StandardCommand.PrintPreview:
			case StandardCommand.Print:
			{
				IPrintableDocument activeContext = m_contextRegistry.GetActiveContext<IPrintableDocument>();
				result = activeContext != null;
				break;
			}
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.PageSetup:
				ShowPageSettingsDialog();
				break;
			case StandardCommand.PrintPreview:
				ShowPrintPreviewDialog();
				break;
			case StandardCommand.Print:
				ShowPrintDialog();
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private PrintDocument GetPrintDocument()
	{
		IPrintableDocument activeContext = m_contextRegistry.GetActiveContext<IPrintableDocument>();
		PrintDocument printDocument = activeContext.GetPrintDocument();
		if (printDocument == null)
		{
			throw new InvalidOperationException("Printable documents must produce a PrintDocument");
		}
		return printDocument;
	}
}
