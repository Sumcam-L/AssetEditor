using System;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(DanglingAssetReportCommands))]
public class DanglingAssetReportCommands : ICommandClient, IInitializable
{
	private enum Command
	{
		GenerateReport
	}

	private struct ReportCommandTag
	{
		public Command Command;

		public ReportCommandTag(Command command)
		{
			Command = command;
		}
	}

	private string _reportPath;

	private readonly IDanglingAssetReportService _assetReportService;

	private readonly ISettingsService _settingsService;

	private readonly ISplashScreenService _splashScreenService;

	private readonly IMessageBoxService _messageBoxes;

	public string ReportPath
	{
		get
		{
			return _reportPath;
		}
		set
		{
			_reportPath = value;
		}
	}

	[ImportingConstructor]
	public DanglingAssetReportCommands(ICommandService commandService, IDanglingAssetReportService assetReportService, ISettingsService settingsService, ISplashScreenService splashScreenService, IMessageBoxService messageBoxes)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			_assetReportService = assetReportService;
			_settingsService = settingsService;
			_splashScreenService = splashScreenService;
			_messageBoxes = messageBoxes;
			_reportPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			_reportPath = Path.Combine(_reportPath, "My Games", "AssetCloud", "dangling_entity_report.txt");
			RegisterClientCommands(commandService);
		}
	}

	private void RegisterClientCommands(ICommandService commandService)
	{
		Keys shortcut = Keys.R | Keys.Shift | Keys.Control;
		commandService.RegisterCommand(new CommandInfo(new ReportCommandTag(Command.GenerateReport), StandardMenu.File, StandardCommandGroup.FileOther, "Generate Dangling XLP Report".Localize("Name of a command"), "Creates a report that contains a list of XLP entries that are not referenced by any ArtDef.".Localize(), shortcut, "", CommandVisibility.Menu), this);
	}

	public void Initialize()
	{
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => ReportPath, "Dangling Asset Report Location".Localize(), "Reports".Localize(), "Location where the Dangling Entity report goes.".Localize());
		_settingsService.RegisterSettings("Application".Localize(), boundPropertyDescriptor);
		_settingsService.RegisterUserSettings("Application".Localize(), boundPropertyDescriptor);
	}

	public bool CanDoCommand(object commandTag)
	{
		return commandTag is ReportCommandTag;
	}

	public void DoCommand(object commandTag)
	{
		if (((ReportCommandTag)commandTag).Command == Command.GenerateReport)
		{
			_splashScreenService.ShowSplashScreen(delegate
			{
				string reportMessage = _assetReportService.GenerateDanglingAssetReport();
				WriteReport(reportMessage);
			}, "Dangling Assets", "Generating dangling assets report...");
			string message = "Report generated at " + _reportPath + ".";
			_messageBoxes.Show(message, "Report Ready", MessageBoxButton.OK, MessageBoxImage.None);
		}
	}

	private void WriteReport(string reportMessage)
	{
		using StreamWriter streamWriter = File.CreateText(_reportPath);
		streamWriter.Write(reportMessage);
		streamWriter.Flush();
		streamWriter.Close();
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}
}
