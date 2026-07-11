using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Sce.Atf.Applications;

public abstract class WindowLayoutServiceCommandsBase : IInitializable, ICommandClient
{
	protected enum Command
	{
		SaveLayoutAs,
		ManageLayouts,
		ResetLayout
	}

	protected enum Group
	{
		WindowLayoutServiceCommandsBase
	}

	protected const string MenuSeparator = "/";

	protected static readonly string MenuName = "Layouts".Localize();

	protected static readonly string MenuSaveLayoutAs = "Save Layout As...".Localize();

	protected static readonly string MenuManageLayouts = "Manage Layouts...".Localize();

	protected static readonly string MenuResetLayout = "Reset Layouts...".Localize();

	protected const string LayoutDirectoryName = "Layouts";

	public const string ScreenshotExtension = ".jpg";

	[Import(AllowDefault = true)]
	public ISettingsPathsProvider SettingsPathsProvider { get; set; }

	public IWindowLayoutService WindowLayoutService { get; private set; }

	public DirectoryInfo LayoutScreenshotDirectory { get; private set; }

	protected WindowLayoutServiceCommandsBase(IWindowLayoutService windowLayoutService)
	{
		WindowLayoutService = windowLayoutService;
		WindowLayoutService.LayoutsChanging += WindowLayoutServiceLayoutsChanging;
		WindowLayoutService.LayoutsChanged += WindowLayoutServiceLayoutsChanged;
	}

	public virtual void Initialize()
	{
		if (SettingsPathsProvider == null)
		{
			return;
		}
		try
		{
			string path = Path.GetDirectoryName(SettingsPathsProvider.SettingsPath) + Path.DirectorySeparatorChar + "Layouts";
			LayoutScreenshotDirectory = (Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path));
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "{0}: Exception setting layout screenshot directory: {1}", this, ex.Message);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return commandTag is Command;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is Command)
		{
			switch ((Command)commandTag)
			{
			case Command.SaveLayoutAs:
				ShowSaveLayoutAsDialog();
				break;
			case Command.ManageLayouts:
				ShowManageLayoutsDialog();
				break;
			case Command.ResetLayout:
				ShowResetLayoutDialog();
				break;
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public virtual bool SaveLayoutAs(string layoutName)
	{
		if (string.IsNullOrEmpty(layoutName))
		{
			return false;
		}
		WindowLayoutService.RemoveLayout(layoutName);
		WindowLayoutService.CurrentLayout = layoutName;
		SaveLayoutScreenshot(layoutName, LayoutScreenshotDirectory, this);
		return true;
	}

	public abstract void ShowSaveLayoutAsDialog();

	public abstract void ShowManageLayoutsDialog();

	public abstract void ShowResetLayoutDialog();

	public abstract void DoResetLayout();

	protected abstract Image GetApplicationScreenshot();

	protected abstract void OnWindowLayoutServiceLayoutsChanging();

	protected abstract void OnWindowLayoutServiceLayoutsChanged();

	private void WindowLayoutServiceLayoutsChanging(object sender, EventArgs e)
	{
		OnWindowLayoutServiceLayoutsChanging();
	}

	private void WindowLayoutServiceLayoutsChanged(object sender, EventArgs e)
	{
		OnWindowLayoutServiceLayoutsChanged();
	}

	private static void SaveLayoutScreenshot(string name, DirectoryInfo dir, WindowLayoutServiceCommandsBase wlscb)
	{
		if (string.IsNullOrEmpty(name) || dir == null || wlscb == null)
		{
			return;
		}
		try
		{
			if (!Directory.Exists(dir.FullName))
			{
				return;
			}
			string path = Path.Combine(dir.FullName + Path.DirectorySeparatorChar, name + ".jpg");
			using Image image = wlscb.GetApplicationScreenshot();
			using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Write);
			image.Save(stream, ImageFormat.Jpeg);
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "{0}: Exception saving layout screenshot: {1}", wlscb, ex.Message);
		}
	}
}
