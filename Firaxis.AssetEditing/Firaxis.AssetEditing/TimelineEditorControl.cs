using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetEditing;

public class TimelineEditorControl : UserControl
{
	private struct SubControlInfo
	{
		public string Name;

		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;
	}

	private EditingContext m_behaviorContext;

	private IBehaviorProviderAdapter m_behaviorAdapter;

	private TimelineTreeControl m_timelineTreeControl;

	private TriggerListControl m_triggerListControl;

	private PropertyGridView m_propertyGrid;

	private DockPanel m_dockPanel;

	private IThemeService m_themeService;

	private ICommandService m_commandService;

	private IDictionary<Control, Firaxis.ATF.DockContent> m_dockContent;

	private readonly string kdcTimeline = "Timeline";

	private readonly string kdcTriggers = "Triggers";

	private readonly string kdcProperties = "Properties";

	public string EditorLayoutState
	{
		get
		{
			return GetAssetEditorLayoutState();
		}
		set
		{
			SetAssetEditorLayoutState(value);
		}
	}

	public TimelineEditorControl(string layoutState, ICommandService cmdSvc, IThemeService themeSvc, IAnimationKnobService animKnobSvc, ITimelinePlaybackService tlPlyBckSvc, ITimelineTrackCommands timeTrkCmds, StandardEditCommands stdEditCmds)
	{
		m_commandService = cmdSvc;
		m_themeService = themeSvc;
		m_themeService.ThemeChanged += ThemeService_ThemeChanged;
		m_dockContent = new Dictionary<Control, Firaxis.ATF.DockContent>();
		m_dockPanel = new DockPanel();
		m_dockPanel.Theme = new VS2005Theme();
		m_dockPanel.Dock = DockStyle.Fill;
		m_dockPanel.ShowDocumentIcon = false;
		m_dockPanel.LargeDocumentIcon = false;
		m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
		base.Controls.Add(m_dockPanel);
		m_timelineTreeControl = new TimelineTreeControl(m_commandService, animKnobSvc, tlPlyBckSvc, timeTrkCmds, stdEditCmds);
		m_timelineTreeControl.Dock = DockStyle.Fill;
		AddDockContextImpl(m_timelineTreeControl, kdcTimeline, "Timeline".Localize(), string.Empty, DockState.Document).Show(m_dockPanel, DockState.Document);
		m_triggerListControl = new TriggerListControl();
		m_triggerListControl.Dock = DockStyle.Fill;
		AddDockContextImpl(m_triggerListControl, kdcTriggers, "Triggers".Localize(), string.Empty, DockState.DockRight).Show(m_dockPanel, DockState.DockRight);
		m_propertyGrid = new PropertyGridView();
		m_propertyGrid.Dock = DockStyle.Fill;
		m_propertyGrid.PropertySorting = PropertySorting.Categorized;
		AddDockContextImpl(m_propertyGrid, kdcProperties, "Properties".Localize(), string.Empty, DockState.DockRight).Show(m_dockPanel, DockState.DockRight);
		base.VisibleChanged += TimelineEditorControl_VisibleChanged;
	}

	private void TimelineEditorControl_VisibleChanged(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			SkinService.ApplyActiveSkin(this);
		}
	}

	private IEnumerable<SubControlInfo> DetachControlInfos()
	{
		IList<SubControlInfo> list = new List<SubControlInfo>();
		KeyValuePair<Control, Firaxis.ATF.DockContent>[] array = m_dockContent.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<Control, Firaxis.ATF.DockContent> keyValuePair = array[i];
			list.Add((SubControlInfo)keyValuePair.Key.Tag);
			keyValuePair.Value.Controls.Remove(keyValuePair.Key);
			keyValuePair.Value.Hide();
			keyValuePair.Value.Dispose();
		}
		m_dockContent.Clear();
		return list;
	}

	private void ReattachControlInfos(IEnumerable<SubControlInfo> infos)
	{
		int i = 0;
		for (int num = infos.Count(); i < num; i++)
		{
			SubControlInfo subControlInfo = infos.ElementAt(i);
			AddDockContextImpl(subControlInfo.Ctl, subControlInfo.Name, subControlInfo.Label, subControlInfo.Icon, subControlInfo.DockState).Show(m_dockPanel, subControlInfo.DockState);
		}
	}

	private void ThemeService_ThemeChanged(object sender, EventArgs e)
	{
		IEnumerable<SubControlInfo> infos = DetachControlInfos();
		m_dockPanel.Theme = m_themeService.ActiveTheme;
		ReattachControlInfos(infos);
		SkinService.ApplyActiveSkin(this);
	}

	private IDockContent StringToDockContent(string id)
	{
		foreach (Firaxis.ATF.DockContent value in m_dockContent.Values)
		{
			if (value.Name == id)
			{
				return value;
			}
		}
		return null;
	}

	private Icon GetComponentIcon(string iconName)
	{
		if (string.IsNullOrEmpty(iconName))
		{
			return null;
		}
		return ResourceUtil.GetIcon(iconName);
	}

	private Firaxis.ATF.DockContent AddDockContextImpl(Control key, string name, string text, string iconName, DockState state)
	{
		key.Tag = new SubControlInfo
		{
			Name = name,
			Ctl = key,
			Label = text,
			Icon = iconName,
			DockState = state
		};
		Firaxis.ATF.DockContent value = null;
		if (!m_dockContent.TryGetValue(key, out value))
		{
			Firaxis.ATF.DockContent dockContent = (m_dockContent[key] = new Firaxis.ATF.DockContent());
			value = dockContent;
		}
		value.Name = name;
		value.TabText = text;
		value.Text = text;
		value.ToolTipText = text;
		value.Controls.Add(key);
		value.ShowIcon = false;
		value.ShowTabText = true;
		value.CloseButtonVisible = false;
		value.CloseButton = false;
		return value;
	}

	private void SetAssetEditorLayoutState(string value)
	{
		m_dockPanel.SetLayoutState(value, m_dockContent);
	}

	private string GetAssetEditorLayoutState()
	{
		return m_dockPanel.GetLayoutState();
	}

	public void Bind(EditingContext context)
	{
		if (m_behaviorContext != null)
		{
			m_behaviorContext.SelectionChanged -= BehaviorProviderAdapter_SelectionChanged;
		}
		m_behaviorContext = context;
		m_behaviorAdapter = context.As<IBehaviorProviderAdapter>();
		m_timelineTreeControl.Bind(m_behaviorContext);
		m_triggerListControl.Bind(m_behaviorAdapter);
		UpdatePropertyGridContext();
		if (m_behaviorContext != null)
		{
			m_behaviorContext.SelectionChanged += BehaviorProviderAdapter_SelectionChanged;
		}
	}

	private void BehaviorProviderAdapter_SelectionChanged(object sender, EventArgs e)
	{
		UpdatePropertyGridContext();
	}

	private string GetSelectionDisplayName(Selection<object> selection)
	{
		if (selection == null || selection.Count == 0)
		{
			return string.Empty;
		}
		if (selection.All((object sItem) => sItem.Is<TriggerAdapter>()))
		{
			return "Trigger".Localize();
		}
		if (selection.All((object sItem) => sItem.Is<TrackAdapter>()))
		{
			return "Track".Localize();
		}
		if (selection.All((object sItem) => sItem.Is<TimelineBindingAdapter>()))
		{
			return "Timeline".Localize();
		}
		if (selection.All((object sItem) => sItem.Is<AnimationBindingAdapter>()))
		{
			return "Animation".Localize();
		}
		return string.Empty;
	}

	private void UpdatePropertyGridContext()
	{
		int num = m_behaviorContext?.Selection.Count ?? 0;
		if (num < 2)
		{
			IPropertyEditingContext propertyEditingContext = m_behaviorContext?.LastSelected?.As<IPropertyEditingContext>();
			m_propertyGrid.EditingContext = propertyEditingContext;
			if (propertyEditingContext == null)
			{
				UpdatePropertGridText("Properties".Localize());
			}
			else
			{
				UpdatePropertGridText("Properties: " + GetSelectionDisplayName(m_behaviorContext.Selection));
			}
		}
		else
		{
			m_propertyGrid.EditingContext = new MultiSelectPropertyEditingContext(m_behaviorContext, m_behaviorContext.Selection);
			UpdatePropertGridText($"Properties: {GetSelectionDisplayName(m_behaviorContext.Selection)}({num})");
		}
	}

	private void UpdatePropertGridText(string text)
	{
		m_dockContent[m_propertyGrid].Text = text;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (KeyValuePair<Control, Firaxis.ATF.DockContent> item in m_dockContent)
			{
				item.Value.Controls.Remove(item.Key);
				item.Value.Dispose();
				item.Key.Dispose();
			}
			m_dockContent.Clear();
			if (m_dockPanel != null)
			{
				m_dockPanel.Dispose();
				m_dockPanel = null;
			}
		}
		base.Dispose(disposing);
	}
}
