using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.Properties;
using Firaxis.Utility;
using Microsoft.Win32;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(TunerCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TunerCommands : ICommandClient, IInitializable
{
	private enum Command
	{
		Enabled,
		HotLoad,
		StartGame
	}

	private struct TunerCommandTag
	{
		public Command Command;

		public TunerCommandTag(Command command)
		{
			Command = command;
		}
	}

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	private string GameExecPath;

	private string _lastStateMessage = string.Empty;

	private CommandInfo m_tunerConnectionEnableCmd;

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	protected ITunerService TunerService { get; private set; }

	protected ITunerQueueService TunerQueue { get; private set; }

	protected IHotLoadService HotLoadService { get; private set; }

	private ICivTechService CivTechService { get; set; }

	public string EnabledIconName { get; set; }

	public string DisabledIconName { get; set; }

	[ImportingConstructor]
	public TunerCommands(ICommandService commandService, IDocumentRegistry documentRegistry, ITunerService tunerService, ITunerQueueService tunerQueue, IHotLoadService hotLoadService, ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			CommandService = commandService;
			DocumentRegistry = documentRegistry;
			TunerService = tunerService;
			TunerQueue = tunerQueue;
			HotLoadService = hotLoadService;
			CivTechService = civTechSvc;
			EnabledIconName = Resources.EnableTunerIcon;
			DisabledIconName = Resources.DisableTunerIcon;
		}
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("tunerService", this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (!(commandTag is TunerCommandTag tunerCommandTag))
		{
			return false;
		}
		if (tunerCommandTag.Command == Command.Enabled && TunerService != null && !HotLoadService.IsHotLoading)
		{
			return true;
		}
		if (tunerCommandTag.Command == Command.StartGame)
		{
			if (FindGameExePath())
			{
				return true;
			}
			return false;
		}
		if (tunerCommandTag.Command == Command.HotLoad && DocumentRegistry.ActiveDocument != null && TunerService.Enabled && TunerService.IsConnected)
		{
			return !HotLoadService.IsHotLoading;
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (!(commandTag is TunerCommandTag tunerCommandTag))
		{
			return;
		}
		if (tunerCommandTag.Command == Command.Enabled)
		{
			TunerService.Enabled = !TunerService.IsConnected || !TunerService.Enabled;
		}
		else if (tunerCommandTag.Command == Command.HotLoad)
		{
			if (CivTechService.PrimaryProject.DependencyRegistry.GetDependentTree(DocumentRegistry.ActiveDocument.Uri).Dependents.Any() || DocumentRegistry.ActiveDocument.Is<ICookable>())
			{
				TunerQueue.AddDocumentToQueue(DocumentRegistry.ActiveDocument);
			}
			else
			{
				MessageBoxes.Show($"{DocumentRegistry.ActiveDocument.Uri} is not referenced by any cookable parent.\n\nItem can not be hot loaded!", "HotLoad Failed", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		else if (tunerCommandTag.Command == Command.StartGame)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
				{
					string text = registryKey?.GetValue("SteamPath") as string;
					if (!string.IsNullOrWhiteSpace(text))
					{
						string text2 = Path.Combine(text, "Steam.exe");
						if (File.Exists(text2))
						{
							Process.Start(text2, "-applaunch 289070 -Tuner");
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Failed to start Civilization VI: {0}", ex.Message);
				MessageBoxes.Show("Failed to start Civilization VI.\n\n" + ex.Message, "Start Civ 6 Failed", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
		if (commandTag is TunerCommandTag && ((TunerCommandTag)commandTag).Command == Command.Enabled && TunerService != null)
		{
			string text = ((TunerService.Enabled && TunerService.IsConnected) ? "Disable tuner connection".Localize() : "Enable tuner connection".Localize());
			if (text != _lastStateMessage)
			{
				commandState.Text = text;
				m_tunerConnectionEnableCmd.GetButton().ToolTipText = commandState.Text;
				m_tunerConnectionEnableCmd.GetButton().Image = ((TunerService.Enabled && TunerService.IsConnected) ? ResourceUtil.GetImage24(EnabledIconName) : ResourceUtil.GetImage24(DisabledIconName));
			}
			_lastStateMessage = text;
		}
	}

	private void RegisterClientCommands()
	{
		Keys shortcut = Keys.H | Keys.Shift | Keys.Control;
		m_tunerConnectionEnableCmd = CommandService.RegisterCommand(new TunerCommandTag(Command.Enabled), StandardMenu.File, StandardCommandGroup.FileOther, "Tuner Enabled".Localize("Name of a command"), "Enables or disables tuner connection".Localize(), Keys.None, EnabledIconName, CommandVisibility.All, this);
		CommandService.RegisterCommand(new CommandInfo(new TunerCommandTag(Command.HotLoad), StandardMenu.File, StandardCommandGroup.FileOther, "Hot Load".Localize("Name of a command"), "Requests the game hot load an existing document".Localize(), shortcut, Resources.HotloadIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(new TunerCommandTag(Command.StartGame), StandardMenu.File, StandardCommandGroup.FileOther, "Start Civ 6".Localize("Name of a command"), "Starts up Civilization 6".Localize(), Keys.None, Resources.Civ6Game, CommandVisibility.All), this);
	}

	private bool FindGameExePath()
	{
		if (Firaxis.CivTech.Properties.Resources.ModTools)
		{
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
			{
				string text = registryKey?.GetValue("SteamPath") as string;
				if (!string.IsNullOrWhiteSpace(text) && File.Exists(Path.Combine(text, "Steam.exe")))
				{
					return true;
				}
			}
			return false;
		}
		RegistryKey toolsRegistryKey = CivTechService.AssetCloudSettings.GetToolsRegistryKey("Civ6", "ContentTools");
		if (toolsRegistryKey != null)
		{
			string[] valueNames = toolsRegistryKey.GetValueNames();
			for (int i = 0; i < valueNames.Length; i++)
			{
				GameExecPath = toolsRegistryKey.GetValue(valueNames[i]) as string;
				if (!string.IsNullOrEmpty(GameExecPath))
					{
						GameExecPath = GameExecPath.Trim('"');
						if (File.Exists(GameExecPath) && valueNames[i] == "Civ6")
						{
							return true;
						}
					}
			}
		}
		return false;
	}

	}
