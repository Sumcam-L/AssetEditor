using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IInitializable))]
[Export(typeof(IAssetPreviewer))]
[Export(typeof(IAudioPreviewer))]
[Export(typeof(IKnobManager))]
[Export(typeof(IPreviewDisplay))]
[Export(typeof(IPreviewerWidgetHost))]
[Export(typeof(IPreviewerControlHost))]
[Export(typeof(IPreviewerWindowService))]
[Export(typeof(PreviewerDockWindow2))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerDockWindow2 : IInitializable, IControlHostClient, IPreviewerControlHost, IAssetPreviewer, IAssemblyInstance, IDisposable, IPreviewDisplay, IKnobManager, IPreviewerWidgetHost, IPreviewerWindowService, IProjectChangeWatcher, IAudioPreviewer
{
	private readonly Control AssetPreviewerWrapper;

	private readonly Control AssetPreviewerControl;

	private readonly ToolStrip WidgetToolbar;

	private readonly ToolStripDropDownButton WidgetSelectorBtn;

	private IList<ToolStripMenuItem> m_widgetEditorMenuItems = new List<ToolStripMenuItem>();

	private IList<ToolStripButton> m_widgetStateItems = new List<ToolStripButton>();

	private string m_lastActiveWidgetStateName;

	private Image m_noWidgetImage;

	private IAssetPreviewer AssetPreviewer;

	private IAudioPreviewer AudioPreviewer;

	private readonly IControlHostService ControlHostService;

	private readonly ICivTechService CivTechService;

	private readonly IXLPRegistry XLPRegistry;

	private IPreviewDisplay PreviewDisplay { get; set; }

	private IPreviewWindow PreviewWindow { get; set; }

	private LowLevelKeyHandler LowLevelKeyHandler { get; set; }

	public Control PreviewerControl => AssetPreviewerControl;

	IKnobManager IAssetPreviewer.KnobManager => AssetPreviewer.KnobManager;

	uint IAssetPreviewer.FrameNumber => AssetPreviewer.FrameNumber;

	event KnobGroupChangedEventHandler IKnobManager.KnobGroupChanged
	{
		add
		{
			BugSubmitter.Assert(AssetPreviewer?.KnobManager != null, "Adding event handler before KnobManager is setup @summary Adding KnobGroupChanged handler before KnobManager is setup @assign bwhitman");
			AssetPreviewer.KnobManager.KnobGroupChanged += value;
		}
		remove
		{
			BugSubmitter.Assert(AssetPreviewer?.KnobManager != null, "Removing event handler before KnobManager is setup @summary Removing KnobGroupChanged handler before KnobManager is setup @assign bwhitman");
			AssetPreviewer.KnobManager.KnobGroupChanged -= value;
		}
	}

	event KnobGroupClearedEventHandler IKnobManager.KnobGroupCleared
	{
		add
		{
			BugSubmitter.Assert(AssetPreviewer?.KnobManager != null, "Adding event handler before KnobManager is setup @summary Adding KnobGroupCleared handler before KnobManager is setup @assign bwhitman");
			AssetPreviewer.KnobManager.KnobGroupCleared += value;
		}
		remove
		{
			BugSubmitter.Assert(AssetPreviewer?.KnobManager != null, "Removing event handler before KnobManager is setup @summary Removing KnobGroupCleared handler before KnobManager is setup @assign bwhitman");
			AssetPreviewer.KnobManager.KnobGroupCleared -= value;
		}
	}

	event LogEventHandler IAssetPreviewer.Logger
	{
		add
		{
			AssetPreviewer.Logger += value;
		}
		remove
		{
			AssetPreviewer.Logger -= value;
		}
	}

	event EventHandler IAssetPreviewer.KnobChangesComplete
	{
		add
		{
			AssetPreviewer.KnobChangesComplete += value;
		}
		remove
		{
			AssetPreviewer.KnobChangesComplete -= value;
		}
	}

	[ImportingConstructor]
	public PreviewerDockWindow2(ICivTechService civTechSvc, IXLPRegistry xlpReg, IControlHostService ctlHostSvc)
	{
		CivTechService = civTechSvc;
		XLPRegistry = xlpReg;
		ControlHostService = ctlHostSvc;
		AssetPreviewerWrapper = new Control();
		AssetPreviewerControl = new Control();
		WidgetToolbar = new ToolStrip();
		WidgetSelectorBtn = new ToolStripDropDownButton();
		IntPtr handle = AssetPreviewerWrapper.Handle;
		handle = AssetPreviewerControl.Handle;
		handle = WidgetToolbar.Handle;
		AssetPreviewerControl.Dock = DockStyle.None;
		AssetPreviewerControl.Size = new Size(1, 1);
		WidgetSelectorBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
		WidgetSelectorBtn.Image = (m_noWidgetImage = ResourceUtil.GetImage24(Firaxis.AssetEditing.Resources.NoneWidgetIcon));
		WidgetSelectorBtn.ImageTransparentColor = Color.Magenta;
		WidgetSelectorBtn.Name = "WidgetSelectorBtn";
		WidgetToolbar.Dock = DockStyle.Top;
		WidgetToolbar.Items.AddRange(new ToolStripItem[2]
		{
			WidgetSelectorBtn,
			new ToolStripSeparator()
		});
		AssetPreviewerWrapper.Controls.Add(WidgetToolbar);
		AssetPreviewerWrapper.Controls.Add(AssetPreviewerControl);
		AssetPreviewerWrapper.SizeChanged += AssetPreviewerWrapper_SizeChanged;
		CreatePreviewers();
	}

	public void Startup()
	{
		((IAssetPreviewer)this).Startup(CivTechService, XLPRegistry);
		((IAudioPreviewer)this).Startup(CivTechService, true);
	}

	public void Shutdown()
	{
		((IAudioPreviewer)this).Shutdown(true);
		((IAssetPreviewer)this).Shutdown();
	}

	public void MakeActiveDisplay()
	{
		PreviewDisplay?.MakeActiveDisplay();
	}

	public void BindWindow(IPreviewWindow pWindow)
	{
		PreviewWindow = pWindow;
		PreviewDisplay?.BindWindow(pWindow);
	}

	public void UnbindWindow()
	{
		PreviewDisplay?.UnbindWindow();
		PreviewWindow = null;
	}

	public void CaptureScreenshot(string imgPath)
	{
		PreviewDisplay?.CaptureScreenshot(imgPath);
	}

	public void OnResized(int nWidth, int nHeight)
	{
		PreviewDisplay?.OnResized(nWidth, nHeight);
	}

	IKnobSet IKnobManager.GetKnobSet(string groupName)
	{
		return AssetPreviewer?.KnobManager?.GetKnobSet(groupName);
	}

	public void Activate(Control control)
	{
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	public virtual void SetActiveWidget(IWidgetEditor widget)
	{
		if (WidgetToolbar.InvokeRequired)
		{
			WidgetToolbar.BeginInvoke((Action)delegate
			{
				SetActiveWidget(widget);
			});
			return;
		}
		foreach (object dropDownItem in WidgetSelectorBtn.DropDownItems)
		{
			ToolStripMenuItem toolStripMenuItem = dropDownItem as ToolStripMenuItem;
			if (toolStripMenuItem.Tag == widget)
			{
				WidgetSelectorBtn.Image = toolStripMenuItem.Image;
				UpdateWidgetStates((IWidgetEditor)toolStripMenuItem.Tag);
				break;
			}
		}
	}

	public virtual void AddWidget(IWidgetEditor widget, string name, Image img, Action onClick)
	{
		ToolStripMenuItem menuItem = new ToolStripMenuItem(name, img);
		menuItem.Tag = widget;
		menuItem.Click += delegate
		{
			onClick();
			WidgetSelectorBtn.Image = menuItem.Image;
			UpdateWidgetStates((IWidgetEditor)menuItem.Tag);
		};
		WidgetSelectorBtn.DropDownItems.Add(menuItem);
		if (WidgetSelectorBtn.DropDownItems.Count == 1)
		{
			SetActiveWidget(widget);
		}
	}

	public virtual void RemoveWidget(IWidgetEditor widget)
	{
		foreach (ToolStripItem dropDownItem in WidgetSelectorBtn.DropDownItems)
		{
			if (dropDownItem.Tag == widget)
			{
				if (dropDownItem.Image == WidgetSelectorBtn.Image)
				{
					WidgetSelectorBtn.Image = m_noWidgetImage;
					WidgetSelectorBtn.ImageTransparentColor = Color.Magenta;
					UpdateWidgetStates(null);
				}
				WidgetSelectorBtn.DropDownItems.Remove(dropDownItem);
				break;
			}
		}
	}

	public void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		statusMessagePrinter?.Invoke("Restarting previewer...");
		if (AssetPreviewer != null)
		{
			Shutdown();
			DestroyPreviewers();
		}
		CreatePreviewers();
		Startup();
	}

	public void Initialize()
	{
		ControlHostService.RegisterControl(AssetPreviewerWrapper, "Asset Previewer", "Asset previewing utility view", StandardControlGroup.Right, null, this);
		ControlHostService.Show(AssetPreviewerWrapper);
		Startup();
	}

	private void AssetPreviewerWrapper_SizeChanged(object sender, EventArgs e)
	{
		Size size = AssetPreviewerWrapper.Size;
		if (size.Width > 0 && size.Height > WidgetToolbar.Height)
		{
			AssetPreviewerControl.Top = WidgetToolbar.Bottom;
			AssetPreviewerControl.Width = size.Width;
			AssetPreviewerControl.Height = size.Height - WidgetToolbar.Height;
		}
	}

	private void AssetPreviewControl_SizeChanged(object sender, EventArgs e)
	{
		if (AssetPreviewerControl.Size.Width > 0 && AssetPreviewerControl.Size.Height > 0)
		{
			PreviewDisplay?.OnResized(AssetPreviewerControl.Size.Width, AssetPreviewerControl.Size.Height);
		}
	}

	protected virtual void HookKeyboardInput()
	{
		if (LowLevelKeyHandler == null)
		{
			LowLevelKeyHandler = new LowLevelKeyHandler(new List<Keys>
			{
				Keys.LMenu,
				Keys.RMenu
			}, sendUp: true);
			LowLevelKeyHandler.KeyDown += LowLevelKeyHandler_KeyDown;
			LowLevelKeyHandler.KeyUp += LowLevelKeyHandler_KeyUp;
		}
	}

	protected virtual void UnhookKeyboardInput()
	{
		if (LowLevelKeyHandler != null)
		{
			LowLevelKeyHandler.KeyDown -= LowLevelKeyHandler_KeyDown;
			LowLevelKeyHandler.KeyUp -= LowLevelKeyHandler_KeyUp;
			LowLevelKeyHandler.UnhookKeyboardInput();
			LowLevelKeyHandler = null;
		}
	}

	protected virtual void AssetPreviewControl_MouseEnter(object sender, EventArgs e)
	{
		if (!AssetPreviewerControl.Focused)
		{
			AssetPreviewerControl.Focus();
		}
		HookKeyboardInput();
	}

	protected virtual void AssetPreviewControl_MouseLeave(object sender, EventArgs e)
	{
		UnhookKeyboardInput();
	}

	protected virtual void LowLevelKeyHandler_KeyUp(object sender, KeyEventArgs e)
	{
		bool flag = e.KeyValue == 164 || e.KeyValue == 165;
		SendKeyEvent(flag ? 18 : e.KeyValue, keyDown: false);
	}

	protected virtual void LowLevelKeyHandler_KeyDown(object sender, KeyEventArgs e)
	{
		bool flag = e.KeyValue == 164 || e.KeyValue == 165;
		SendKeyEvent(flag ? 18 : e.KeyValue, keyDown: true);
	}

	protected virtual void AssetPreviewControl_KeyUp(object sender, KeyEventArgs e)
	{
		SendKeyEvent(e.KeyValue, keyDown: false);
	}

	protected virtual void AssetPreviewControl_KeyDown(object sender, KeyEventArgs e)
	{
		SendKeyEvent(e.KeyValue, keyDown: true);
	}

	protected virtual void AssetPreviewControl_MouseWheel(object sender, MouseEventArgs e)
	{
		PreviewWindow?.OnMouseWheel(e.Delta);
	}

	protected virtual void AssetPreviewControl_MouseUp(object sender, MouseEventArgs e)
	{
		PreviewWindow?.OnMouseUp(e.Button);
	}

	protected virtual void AssetPreviewControl_MouseMove(object sender, MouseEventArgs e)
	{
		PreviewWindow?.OnMouseMove(e.X, e.Y);
	}

	protected virtual void AssetPreviewControl_MouseDown(object sender, MouseEventArgs e)
	{
		PreviewWindow?.OnMouseDown(e.Button);
	}

	protected virtual void AssetPreviewControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		PreviewWindow?.OnMouseDoubleClick(e.Button);
	}

	void IAssetPreviewer.Startup(ICivTechService civTechSvc, IXLPRegistry xlpRegistry)
	{
		AssetPreviewer.Startup(civTechSvc, xlpRegistry);
		PreviewDisplay = AssetPreviewer.CreateDisplay(AssetPreviewerControl.Handle);
		RegisterEventHandlers();
	}

	void IAssetPreviewer.Shutdown()
	{
		RemoveEventHandlers();
		if (PreviewDisplay != null)
		{
			AssetPreviewer.DestroyDisplay(PreviewDisplay);
			PreviewDisplay?.Dispose();
			PreviewDisplay = null;
		}
		AssetPreviewer.Shutdown();
	}

	void IAssetPreviewer.FlushMessages()
	{
		AssetPreviewer.FlushMessages();
	}

	void IAssetPreviewer.SetTargetFPS(float targetFPS)
	{
		AssetPreviewer.SetTargetFPS(targetFPS);
	}

	void IAssetPreviewer.SetThrottleMode(bool shouldThrottle)
	{
		AssetPreviewer.SetThrottleMode(shouldThrottle);
	}

	ICachedAsset IAssetPreviewer.CacheAsset(IInstanceEntity entity)
	{
		return AssetPreviewer.CacheAsset(entity);
	}

	void IAssetPreviewer.ReloadEntity(InstanceType enttype, string entname)
	{
		AssetPreviewer.ReloadEntity(enttype, entname);
	}

	IPreviewWindow IAssetPreviewer.OpenWindow(IntPtr hWindow, IInstanceSet pmInstanceSet)
	{
		return AssetPreviewer.OpenWindow(hWindow, pmInstanceSet);
	}

	void IAssetPreviewer.CloseWindow(IPreviewWindow window)
	{
		AssetPreviewer.CloseWindow(window);
	}

	IPreviewDisplay IAssetPreviewer.CreateDisplay(IntPtr hWindow)
	{
		return AssetPreviewer.CreateDisplay(hWindow);
	}

	void IAssetPreviewer.DestroyDisplay(IPreviewDisplay pmDisplay)
	{
		AssetPreviewer.DestroyDisplay(pmDisplay);
	}

	IEnumerable<string> IAssetPreviewer.GetAllowedLightRigClasses(string moduleName)
	{
		return AssetPreviewer.GetAllowedLightRigClasses(moduleName);
	}

	bool IAssetPreviewer.DoesModuleSupportsLighting(string moduleName)
	{
		return AssetPreviewer.DoesModuleSupportsLighting(moduleName);
	}

	bool IAssetPreviewer.DoesSupportsModule(string moduleName)
	{
		return AssetPreviewer.DoesSupportsModule(moduleName);
	}

	IEnumerable<IPreviewerSlotInfo> IAssetPreviewer.GetSlotsInfo(string moduleName)
	{
		return AssetPreviewer.GetSlotsInfo(moduleName);
	}

	void IAudioPreviewer.Startup(ICivTechService civTech, bool bColdStart)
	{
		AudioPreviewer.Startup(civTech, bColdStart);
	}

	void IAudioPreviewer.Shutdown(bool bStopWwise)
	{
		AudioPreviewer.Shutdown(bStopWwise);
	}

	void IAudioPreviewer.ReloadSoundBanks()
	{
		AudioPreviewer.ReloadSoundBanks();
	}

	int IAudioPreviewer.GetBankID(string bankName)
	{
		return AudioPreviewer.GetBankID(bankName);
	}

	int IAudioPreviewer.GetNumBankEventNames(int iBankID)
	{
		return AudioPreviewer.GetNumBankEventNames(iBankID);
	}

	string IAudioPreviewer.GetBankEventName(int iBankID, int iEventIndex)
	{
		return AudioPreviewer.GetBankEventName(iBankID, iEventIndex);
	}

	bool IAudioPreviewer.GetBankCategory(string sBankName, int iCategoryToCheck)
	{
		return AudioPreviewer.GetBankCategory(sBankName, iCategoryToCheck);
	}

	void IAudioPreviewer.PlaySoundEvent(string sEventName)
	{
		AudioPreviewer.PlaySoundEvent(sEventName);
	}

	void IAudioPreviewer.StopAllSounds()
	{
		AudioPreviewer.StopAllSounds();
	}

	int IAudioPreviewer.GetNumSoundBanks()
	{
		return AudioPreviewer.GetNumSoundBanks();
	}

	string IAudioPreviewer.GetSoundBankName(int iBank)
	{
		return AudioPreviewer.GetSoundBankName(iBank);
	}

	uint IAudioPreviewer.GetNumSwitchGroups()
	{
		return AudioPreviewer.GetNumSwitchGroups();
	}

	uint IAudioPreviewer.GetNumSwitchSettings(string sGroupName)
	{
		return AudioPreviewer.GetNumSwitchSettings(sGroupName);
	}

	string IAudioPreviewer.GetSwitchGroupName(uint uSwitch)
	{
		return AudioPreviewer.GetSwitchGroupName(uSwitch);
	}

	string IAudioPreviewer.GetSwitchSettingName(string sGroupName, uint uSwitch)
	{
		return AudioPreviewer.GetSwitchSettingName(sGroupName, uSwitch);
	}

	void IAudioPreviewer.SetPlaybackSwitch(string sGroupName, string sSwitchName)
	{
		AudioPreviewer.SetPlaybackSwitch(sGroupName, sSwitchName);
	}

	void IAudioPreviewer.UnloadProjectData()
	{
		AudioPreviewer.UnloadProjectData();
	}

	void IAudioPreviewer.LoadProjectData()
	{
		AudioPreviewer.LoadProjectData();
	}

	void IDisposable.Dispose()
	{
		Shutdown();
		DestroyPreviewers();
		WidgetToolbar.Dispose();
		AssetPreviewerControl.Dispose();
		AssetPreviewerWrapper.SizeChanged -= AssetPreviewerWrapper_SizeChanged;
		AssetPreviewerWrapper.Dispose();
	}

	private void CreatePreviewers()
	{
		AssetPreviewer = Context.EnsureCreated<CivTechContext>().CreateInstance<IAssetPreviewer>(new object[1] { AssetPreviewerControl });
		AudioPreviewer = Context.EnsureCreated<CivTechContext>().CreateInstance<IAudioPreviewer>();
	}

	private void DestroyPreviewers()
	{
		AssetPreviewer.Dispose();
		AssetPreviewer = null;
		AudioPreviewer.Dispose();
		AudioPreviewer = null;
	}

	private int RemapWinFormsToAppHostKeys(int keyValue)
	{
		int num = 1;
		int num2 = 27;
		int num3 = 37;
		int num4 = 53;
		int num5 = 83;
		int num6 = 104;
		int num7 = 106;
		int num8 = 107;
		int num9 = 108;
		int num10 = 0;
		int result = 0;
		if (keyValue == 16)
		{
			result = num7;
		}
		else if (keyValue == 17)
		{
			result = num8;
		}
		else if (keyValue == 18)
		{
			result = num9;
		}
		else if (keyValue == 8)
		{
			result = num6;
		}
		else if (keyValue >= 65 && keyValue <= 90)
		{
			num10 = 65 - num;
			result = keyValue - num10;
		}
		else if (keyValue >= 48 && keyValue <= 57)
		{
			num10 = 48 - num2;
			result = keyValue - num10;
		}
		else if (keyValue >= 96 && keyValue <= 105)
		{
			num10 = 96 - num3;
			result = keyValue - num10;
		}
		else if (keyValue >= 112 && keyValue <= 135)
		{
			num10 = 112 - num4;
			result = keyValue - num10;
		}
		else if (keyValue >= 37 && keyValue <= 40)
		{
			num10 = 37 - num5;
			result = keyValue - num10;
		}
		return result;
	}

	private void RegisterEventHandlers()
	{
		RemoveEventHandlers();
		AssetPreviewerControl.MouseDoubleClick += AssetPreviewControl_MouseDoubleClick;
		AssetPreviewerControl.MouseDown += AssetPreviewControl_MouseDown;
		AssetPreviewerControl.MouseMove += AssetPreviewControl_MouseMove;
		AssetPreviewerControl.MouseUp += AssetPreviewControl_MouseUp;
		AssetPreviewerControl.MouseWheel += AssetPreviewControl_MouseWheel;
		AssetPreviewerControl.KeyDown += AssetPreviewControl_KeyDown;
		AssetPreviewerControl.KeyUp += AssetPreviewControl_KeyUp;
		AssetPreviewerControl.MouseEnter += AssetPreviewControl_MouseEnter;
		AssetPreviewerControl.MouseLeave += AssetPreviewControl_MouseLeave;
		AssetPreviewerControl.SizeChanged += AssetPreviewControl_SizeChanged;
	}

	private void RemoveEventHandlers()
	{
		AssetPreviewerControl.MouseDoubleClick -= AssetPreviewControl_MouseDoubleClick;
		AssetPreviewerControl.MouseDown -= AssetPreviewControl_MouseDown;
		AssetPreviewerControl.MouseMove -= AssetPreviewControl_MouseMove;
		AssetPreviewerControl.MouseUp -= AssetPreviewControl_MouseUp;
		AssetPreviewerControl.MouseWheel -= AssetPreviewControl_MouseWheel;
		AssetPreviewerControl.KeyDown -= AssetPreviewControl_KeyDown;
		AssetPreviewerControl.KeyUp -= AssetPreviewControl_KeyUp;
		AssetPreviewerControl.MouseEnter -= AssetPreviewControl_MouseEnter;
		AssetPreviewerControl.MouseLeave -= AssetPreviewControl_MouseLeave;
		AssetPreviewerControl.SizeChanged -= AssetPreviewControl_SizeChanged;
	}

	private void UpdateWidgetStates(IWidgetEditor activeWidget)
	{
		foreach (ToolStripButton widgetStateItem in m_widgetStateItems)
		{
			WidgetToolbar.Items.Remove(widgetStateItem);
		}
		m_widgetStateItems.Clear();
		foreach (IWidgetState item in activeWidget?.States ?? Enumerable.Empty<IWidgetState>())
		{
			ToolStripButton btn = new ToolStripButton();
			btn.Name = item.Name;
			btn.Image = item.Image;
			btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
			btn.Text = item.Text;
			btn.ToolTipText = item.Text;
			btn.CheckOnClick = true;
			btn.Tag = item;
			btn.Click += delegate
			{
				btn.CheckState = CheckState.Checked;
				m_lastActiveWidgetStateName = ((IWidgetState)btn.Tag).Name;
				foreach (ToolStripButton widgetStateItem2 in m_widgetStateItems)
				{
					if (widgetStateItem2 != btn)
					{
						widgetStateItem2.CheckState = CheckState.Unchecked;
					}
				}
				((IWidgetState)btn.Tag).Activate();
			};
			m_widgetStateItems.Add(btn);
			WidgetToolbar.Items.Add(btn);
		}
		if (!m_widgetStateItems.Any())
		{
			return;
		}
		bool flag = false;
		foreach (ToolStripButton widgetStateItem3 in m_widgetStateItems)
		{
			if (((IWidgetState)widgetStateItem3.Tag).Name.Equals(m_lastActiveWidgetStateName))
			{
				widgetStateItem3.CheckState = CheckState.Checked;
				((IWidgetState)widgetStateItem3.Tag).Activate();
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ToolStripButton toolStripButton = m_widgetStateItems.First();
			toolStripButton.CheckState = CheckState.Checked;
			((IWidgetState)toolStripButton.Tag).Activate();
		}
	}

	private void SendKeyEvent(int keyValue, bool keyDown)
	{
		int num = RemapWinFormsToAppHostKeys(keyValue);
		if (keyDown)
		{
			PreviewWindow?.OnKeyDown(num);
		}
		else
		{
			PreviewWindow?.OnKeyUp(num);
		}
	}
}
