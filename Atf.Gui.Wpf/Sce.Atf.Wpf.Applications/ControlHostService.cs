using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Docking;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IControlHostService))]
[Export(typeof(IInitializable))]
[Export(typeof(IDockStateProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ControlHostService : IControlHostService, ICommandClient, IInitializable, IDockStateProvider
{
	private string m_cachedDockPanelState;

	private readonly IMainWindow m_mainWindow;

	private IControlHostClient m_activeClient;

	private List<ControlInfo> m_registeredContents = new List<ControlInfo>();

	private IDockContent m_activeDockControl;

	private Sce.Atf.Wpf.Docking.DockPanel m_dockPanel;

	private bool m_stateApplied;

	private bool m_mainWindowLoaded;

	private bool m_closed;

	private HashSet<object> m_contentToShowOnMainWindowLoad = new HashSet<object>();

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	[Import(AllowDefault = true)]
	private IMainWindowContentSite m_dockPanelSite = null;

	public Control DockPanel => m_dockPanel;

	object IDockStateProvider.DockState
	{
		get
		{
			return DockPanelState;
		}
		set
		{
			DockPanelState = (string)value;
		}
	}

	public IControlHostClient ActiveClient => m_activeClient;

	public IEnumerable<IControlInfo> Contents
	{
		get
		{
			foreach (ControlInfo registeredContent in m_registeredContents)
			{
				yield return registeredContent;
			}
		}
	}

	public string DockPanelState
	{
		get
		{
			if (!m_closed)
			{
				m_cachedDockPanelState = GetDockPanelState();
			}
			return m_cachedDockPanelState;
		}
		set
		{
			m_cachedDockPanelState = value;
			SetDockPanelState(m_cachedDockPanelState);
		}
	}

	public event EventHandler DockStateChanged;

	[ImportingConstructor]
	public ControlHostService(IMainWindow mainWindow)
	{
		m_dockPanel = new Sce.Atf.Wpf.Docking.DockPanel();
		m_mainWindow = mainWindow;
		if (this.DockStateChanged != null)
		{
		}
	}

	void IDockStateProvider.ResetDockState()
	{
		ControlInfo[] array = Enumerable.ToArray(m_registeredContents);
		ControlInfo[] array2 = array;
		foreach (ControlInfo controlInfo in array2)
		{
			UnregisterContent(controlInfo.Content);
		}
		ControlInfo[] array3 = array;
		foreach (ControlInfo controlInfo2 in array3)
		{
			this.RegisterControl(controlInfo2.Content, controlInfo2.Name, controlInfo2.Description, controlInfo2.Group, controlInfo2.Id, controlInfo2.Client);
		}
	}

	public void Initialize()
	{
		m_mainWindow.Closing += m_mainWindow_Closing;
		m_mainWindow.Loaded += m_mainWindow_Loaded;
		if (m_dockPanelSite != null)
		{
			m_dockPanelSite.MainContent = m_dockPanel;
		}
		ShowDefaultContents();
		if (m_settingsService != null)
		{
			m_settingsService.Reloaded += m_settingsService_Reloaded;
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => DockPanelState, "DockPanelState", null, null));
		}
	}

	public IControlInfo RegisterControl(ControlDef def, object control, IControlHostClient client)
	{
		Requires.NotNull(def, "def");
		Requires.NotNull(control, "control");
		Requires.NotNullOrEmpty(def.Id, "def.Id");
		Requires.NotNull(client, "client");
		if (m_registeredContents.Any((ControlInfo x) => x.Id == def.Id))
		{
			throw new ArgumentException("Content with id " + def.Id + " already registered");
		}
		IDockContent dockContent = m_dockPanel.RegisterContent(control, def.Id, ControlGroupToDockTo(def.Group));
		dockContent.IsFocusedChanged += DockContent_IsFocusedChanged;
		ControlInfo controlInfo = new ControlInfo(def.Name, def.Description, def.Id, def.Group, def.ImageSourceKey, dockContent, client);
		m_registeredContents.Add(controlInfo);
		if (m_commandService != null)
		{
			controlInfo.Command = m_commandService.RegisterCommand(dockContent, StandardMenu.Window, StandardCommandGroup.WindowDocuments, controlInfo.Name, "Activate Control".Localize(), Keys.None, null, CommandVisibility.Menu, this).GetCommandItem();
		}
		ActivateClient(client, activating: true);
		return controlInfo;
	}

	public void UnregisterContent(object content)
	{
		ControlInfo controlInfo = FindControlInfo(content);
		if (controlInfo != null)
		{
			if (m_commandService != null)
			{
				m_commandService.UnregisterCommand(controlInfo.Command.CommandTag, this);
			}
			m_dockPanel.UnregisterContent(controlInfo.DockContent);
			controlInfo.DockContent.IsFocusedChanged -= DockContent_IsFocusedChanged;
			m_registeredContents.Remove(controlInfo);
			if (m_activeDockControl == controlInfo.Content)
			{
				m_activeDockControl = null;
			}
		}
	}

	public void Show(object content)
	{
		ControlInfo controlInfo = FindControlInfo(content);
		if (controlInfo != null)
		{
			if (m_mainWindowLoaded)
			{
				m_dockPanel.ShowContent(controlInfo.DockContent);
			}
			else
			{
				m_contentToShowOnMainWindowLoad.Add(content);
			}
		}
	}

	public bool CanDoCommand(object tag)
	{
		return tag is IDockContent;
	}

	public void DoCommand(object tag)
	{
		if (!(tag is IDockContent content))
		{
			return;
		}
		if (m_dockPanel.IsContentVisible(content))
		{
			m_dockPanel.HideContent(content);
			return;
		}
		m_dockPanel.ShowContent(content);
		ControlInfo controlInfo = m_registeredContents.FirstOrDefault((ControlInfo x) => x.Command.CommandTag == tag);
		if (controlInfo != null)
		{
			controlInfo.Command.IsChecked = true;
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public void ShowDefaultContents()
	{
		ActivateClient(null, activating: true);
	}

	public bool Close(bool mainWindowClosing)
	{
		ControlInfo[] array = m_registeredContents.ToArray();
		foreach (ControlInfo controlInfo in array)
		{
			if (!controlInfo.Client.Close(controlInfo.Content, mainWindowClosing))
			{
				return false;
			}
		}
		return true;
	}

	private void m_settingsService_Reloaded(object sender, EventArgs e)
	{
		SetDockPanelState(m_cachedDockPanelState);
		m_stateApplied = true;
	}

	private void m_mainWindow_Loaded(object sender, EventArgs e)
	{
		m_mainWindowLoaded = true;
		if (!m_stateApplied)
		{
			SetDockPanelState(null);
		}
		foreach (object item in m_contentToShowOnMainWindowLoad)
		{
			Show(item);
		}
		m_contentToShowOnMainWindowLoad.Clear();
	}

	private void m_mainWindow_Closing(object sender, CancelEventArgs e)
	{
		if (!e.Cancel)
		{
			m_cachedDockPanelState = GetDockPanelState();
			m_closed = Close(mainWindowClosing: true);
			e.Cancel = !m_closed;
		}
	}

	private void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
	{
		IDockContent activeContent = m_dockPanel.GetActiveContent();
		if (activeContent != m_activeDockControl)
		{
			if (m_activeDockControl != null)
			{
				DeactivateClient(m_activeDockControl.Content);
			}
			if (activeContent != null)
			{
				ActivateClient(activeContent.Content);
			}
			m_activeDockControl = activeContent;
		}
	}

	private ControlInfo FindControlInfo(object content)
	{
		return m_registeredContents.FirstOrDefault((ControlInfo x) => x.Content == content);
	}

	private void ActivateClient(object content)
	{
		IControlInfo controlInfo = FindControlInfo(content);
		if (controlInfo != null)
		{
			m_activeClient = controlInfo.Client;
			m_activeClient.Activate(content);
		}
	}

	private void DeactivateClient(object content)
	{
		IControlInfo controlInfo = FindControlInfo(content);
		if (controlInfo != null)
		{
			controlInfo.Client.Deactivate(content);
			m_activeClient = null;
		}
	}

	private void ActivateClient(IControlHostClient client, bool activating)
	{
		foreach (ControlInfo registeredContent in m_registeredContents)
		{
			if (client == registeredContent.Client)
			{
				m_dockPanel.ShowContent(registeredContent.DockContent);
			}
		}
	}

	private void SetDockPanelState(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			m_dockPanel.ApplyLayout(null);
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		if (string.Compare(value, 0, "<?xml", 0, 5) != 0)
		{
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
		}
		streamWriter.Write(value);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		XmlReader reader = XmlReader.Create(memoryStream);
		try
		{
			m_dockPanel.ApplyLayout(reader);
		}
		catch
		{
			Outputs.WriteLine(OutputMessageType.Error, "Could not load window layout".Localize());
		}
	}

	private string GetDockPanelState()
	{
		string result = null;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
			m_dockPanel.SaveLayout(xmlWriter);
			xmlWriter.Flush();
			memoryStream.Seek(0L, SeekOrigin.Begin);
			StreamReader streamReader = new StreamReader(memoryStream);
			result = streamReader.ReadToEnd();
		}
		return result;
	}

	private static DockTo ControlGroupToDockTo(StandardControlGroup standardControlGroup)
	{
		DockTo result = DockTo.Top;
		switch (standardControlGroup)
		{
		case StandardControlGroup.Bottom:
			result = DockTo.Bottom;
			break;
		case StandardControlGroup.Center:
			result = DockTo.Center;
			break;
		case StandardControlGroup.CenterPermanent:
			result = DockTo.Center;
			break;
		case StandardControlGroup.Floating:
			result = DockTo.Center;
			break;
		case StandardControlGroup.Left:
			result = DockTo.Left;
			break;
		case StandardControlGroup.Right:
			result = DockTo.Right;
			break;
		case StandardControlGroup.Top:
			result = DockTo.Top;
			break;
		}
		return result;
	}
}
