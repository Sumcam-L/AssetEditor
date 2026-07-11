using System;
using System.ComponentModel.Composition;
using System.Windows;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(AudioLoader))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AudioLoader : ICommandClient, IInitializable, IDisposable
{
	private enum Command
	{
		ReloadSoundBanks
	}

	private readonly IAudioPreviewer _audioPreviewer;

	public ICommandService CommandService { get; set; }

	public bool CanDoCommand(object commandTag)
	{
		return true;
	}

	public void Dispose()
	{
	}

	public void DoCommand(object commandTag)
	{
		if (_audioPreviewer != null)
		{
			_audioPreviewer.ReloadSoundBanks();
		}
		else
		{
			MessageBox.Show("音频预览器未初始化，无法重新加载音频库。");
		}
	}

	public void Initialize()
	{
		RegisterClientCommands();
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void RegisterClientCommands()
	{
		Keys shortcut = Keys.None;
		CommandService.RegisterCommand(new CommandInfo(Command.ReloadSoundBanks, StandardMenu.File, StandardCommandGroup.FileOther, "Reload Sound Banks".Localize("Name of a command"), "Reloads all currently loaded sound banks.".Localize(), shortcut, "", CommandVisibility.Menu), this);
	}

	[ImportingConstructor]
	public AudioLoader(ICivTechService civTechService, ICommandService commandService, IAudioPreviewer audioPreviewer)
	{
		CommandService = commandService;
		_audioPreviewer = audioPreviewer;
	}
}
