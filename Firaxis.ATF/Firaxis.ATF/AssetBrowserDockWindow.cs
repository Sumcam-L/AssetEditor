using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Firaxis.AssetBrowser;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.MVVMBase.Controls;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(IProjectChangeWatcher))]
[Export(typeof(AssetBrowserDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetBrowserDockWindow : IInitializable, IControlHostClient, IProjectChangeWatcher, IDisposable
{
	private bool disposedValue;

	[Import(AllowDefault = false)]
	private IControlHostService ControlHostService { get; set; }

	[Import(AllowDefault = false)]
	private ISettingsService SettingsService { get; set; }

	[Import(AllowDefault = true)]
	private ISynchronizeInvoke UIInvoker { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private ICommandService CommandService { get; set; }

	private WpfContentHost BrowserControl { get; set; }

	private AssetBrowserViewModel AssetBrowser { get; set; }

	private AssetBrowserFileCommands BrowserCommands { get; set; }

	private ICivTechService CivTechService { get; set; }

	private WpfSkinService SkinService { get; set; }

	private ControlInfo AssetBrowserViewInfo { get; set; }

	[Import(AllowDefault = true)]
	private IMainWindow MainWindow { get; set; }

	private System.Threading.Timer GCTimer { get; set; }

	private int GCPeriod { get; set; } = 60000;

	private string SavedBrowserDataPath { get; }

	[ImportMany]
	public Lazy<IAssetBrowserCommandDefinition>[] BrowserEntityCommands { get; private set; }

	[ImportingConstructor]
	public AssetBrowserDockWindow(IThemeService themeSvc, IDocumentRegistry docReg, ICommandService cmdSvc, AssetBrowserFileCommands assetBrowserCommands, ICivTechService civTechService, WpfSkinService skinService)
	{
		DocumentRegistry = docReg;
		CommandService = cmdSvc;
		BrowserCommands = assetBrowserCommands;
		CivTechService = civTechService;
		SkinService = skinService;
		SavedBrowserDataPath = civTechService.GetBrowserDataPath();
		CreateDirectory(Path.GetDirectoryName(SavedBrowserDataPath));
	}

	private void CreateDirectory(string logDirectory)
	{
		if (!Directory.Exists(logDirectory))
		{
			Directory.CreateDirectory(logDirectory);
		}
	}

	public virtual void Initialize()
	{
		BugSubmitter.SilentAssert(ControlHostService != null, "MEF Catalog must have a component that provides an IControlHostService export. AssetBrowserDockWindow disabled.");
		if (ControlHostService == null)
		{
			return;
		}
		AssetBrowserViewInfo = new ControlInfo("Asset Browser", "Asset browsing view", StandardControlGroup.Bottom);
		AssetBrowserViewInfo.ControlVisibility = ControlInitialVisibility.InitiallyVisible;
		AssetBrowserViewInfo.MenuText = "Asset Browser";
		AssetBrowserViewInfo.MenuGroupOverride = StandardCommandGroup.UILayout;
		Messenger.RegisterByType<OpenEntityID>(OpenBrowserEntities);
		AssetBrowser = new AssetBrowserViewModel(CivTechService, CivTechRegistry.EntityFilteringService, null, allowMultipleSelection: true, startFocused: false);
		foreach (IAssetBrowserCommandDefinition item in from item in BrowserEntityCommands
			where item != null
			select item.Value)
		{
			AssetBrowser.RegisterEntityCommand(item.Name, item.Content, item.Command);
		}
		AssetBrowser.LoadState();
		BrowserControl = new WpfContentHost(AssetBrowser);
		ControlHostService.RegisterControl(BrowserControl, AssetBrowserViewInfo, this);
		InitializeGCTimer();
	}

	private void OpenBrowserEntities(OpenEntityID openEntityId)
	{
		BrowserCommands.OpenExistingDocument(openEntityId.ID.Type, openEntityId.ID.Name);
	}

	public virtual void Activate(Control control)
	{
	}

	public virtual void Deactivate(Control control)
	{
	}

	public virtual bool Close(Control control)
	{
		return true;
	}

	public void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		if (BrowserControl != null && AssetBrowserViewInfo != null)
		{
			statusMessagePrinter("Updating asset browser...");
			if (UIInvoker != null && UIInvoker.InvokeRequired)
			{
				UIInvoker.Invoke(new Action(HandleProjectChange_Impl), null);
			}
			else
			{
				HandleProjectChange_Impl();
			}
		}
	}

	private void HandleProjectChange_Impl()
	{
		using (WaitHandle waitHandle = new Mutex())
		{
			GCTimer.Dispose(waitHandle);
			waitHandle.WaitOne();
		}
		GCTimer = null;
		AssetBrowser.SaveState();
		BrowserControl.Hide();
		ControlHostService.UnregisterControl(BrowserControl);
		BrowserControl.Dispose();
		AssetBrowser = new AssetBrowserViewModel(CivTechService, CivTechRegistry.EntityFilteringService, null, allowMultipleSelection: true, startFocused: false);
		foreach (IAssetBrowserCommandDefinition item in from item in BrowserEntityCommands
			where item != null
			select item.Value)
		{
			AssetBrowser.RegisterEntityCommand(item.Name, item.Content, item.Command);
		}
		AssetBrowser.LoadState();
		BrowserControl = new WpfContentHost(AssetBrowser);
		ControlHostService.RegisterControl(BrowserControl, AssetBrowserViewInfo, this);
		InitializeGCTimer();
	}

	private void InitializeGCTimer()
	{
		GCTimer = new System.Threading.Timer(GarbageCollect);
		GCTimer.Change(GCPeriod, GCPeriod);
	}

	private void GarbageCollect(object context)
	{
		AssetBrowser?.ClearUnselectedEntities();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (GCTimer != null)
			{
				using (WaitHandle waitHandle = new Mutex())
				{
					GCTimer.Dispose(waitHandle);
					waitHandle.WaitOne();
				}
				GCTimer = null;
			}
			if (BrowserControl != null)
			{
				BrowserControl.Hide();
				BrowserControl.Dispose();
				BrowserControl = null;
			}
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
