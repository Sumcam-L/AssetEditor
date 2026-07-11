using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.ATF;

public class DockHostControl : UserControl
{
	private struct SubControlInfo
	{
		public string Name;

		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;

		public bool CanClose;
	}

	protected readonly IThemeService ThemeService;

	private DockPanel m_dockPanel = new DockPanel();

	private IDictionary<Control, DockContent> m_dockContent;

	private int m_contentUpdateCount;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams obj = base.CreateParams;
			obj.ExStyle |= 33554432;
			return obj;
		}
	}

	public string LayoutState
	{
		get
		{
			return m_dockPanel.GetLayoutState();
		}
		set
		{
			if (!(m_dockPanel.GetLayoutState() == value))
			{
				m_dockPanel.SetLayoutState(value, m_dockContent);
			}
		}
	}

	public DockHostControl(IThemeService themeSvc)
	{
		ThemeService = themeSvc;
		ThemeService.ThemeChanged += ThemeService_ThemeChanged;
		m_dockContent = new Dictionary<Control, DockContent>();
		m_dockPanel.Theme = ThemeService.ActiveTheme;
		m_dockPanel.Dock = DockStyle.Fill;
		m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
		m_dockPanel.LargeDocumentIcon = false;
		base.Controls.Add(m_dockPanel);
	}

	public void AddDockContext(Control key, string dockName, string text, string iconName, DockState state, bool canClose)
	{
		AddDockContext(key, dockName, text, iconName, state, canClose, show: true);
	}

	public void AddDockContext(Control key, string dockName, string text, string iconName, DockState state, bool canClose, bool show)
	{
		key.Tag = new SubControlInfo
		{
			Name = dockName,
			Ctl = key,
			Label = text,
			Icon = iconName,
			DockState = state,
			CanClose = canClose
		};
		DockContent dockContent = new DockContent();
		dockContent.Name = dockName;
		dockContent.Text = text;
		dockContent.ToolTipText = text;
		dockContent.Controls.Add(key);
		if (!string.IsNullOrEmpty(iconName))
		{
			dockContent.Icon = GetComponentIcon(iconName);
			dockContent.ShowIcon = true;
			dockContent.ShowTabText = false;
			m_dockPanel.ShowDocumentIcon = true;
		}
		else
		{
			dockContent.Icon = null;
			dockContent.ShowIcon = false;
			dockContent.ShowTabText = true;
			m_dockPanel.ShowDocumentIcon = false;
		}
		dockContent.CloseButtonVisible = false;
		dockContent.CloseButton = false;
		if (canClose)
		{
			dockContent.CloseButton = true;
			dockContent.CloseButtonVisible = true;
		}
		else
		{
			dockContent.CloseButton = false;
			dockContent.CloseButtonVisible = false;
		}
		m_dockContent[key] = dockContent;
		if (show)
		{
			m_dockContent[key].Show(m_dockPanel, state);
		}
	}

	public void UpdateDockContextText(Control key, string text)
	{
		m_dockContent[key].Text = text;
	}

	public void ShowDockContext(Control key, DockState state)
	{
		m_dockContent[key].Show(m_dockPanel, state);
	}

	public void BeginContentUpdate()
	{
		if (m_contentUpdateCount++ == 0)
		{
			SuspendLayout();
			m_dockPanel.SuspendLayout();
		}
	}

	public void EndContentUpdate()
	{
		if (m_contentUpdateCount <= 0)
		{
			return;
		}
		if (--m_contentUpdateCount == 0)
		{
			m_dockPanel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
			Invalidate();
		}
	}

	public void RemoveAllContent()
	{
		foreach (KeyValuePair<Control, DockContent> item in m_dockContent)
		{
			item.Value.Controls.Remove(item.Key);
			item.Value.Hide();
			item.Value.Dispose();
			item.Key.Dispose();
		}
		m_dockContent.Clear();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			RemoveAllContent();
			m_dockPanel?.Dispose();
			m_dockPanel = null;
		}
		base.Dispose(disposing);
	}

	private IEnumerable<SubControlInfo> DetachControlInfos()
	{
		IList<SubControlInfo> list = new List<SubControlInfo>();
		KeyValuePair<Control, DockContent>[] array = m_dockContent.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<Control, DockContent> keyValuePair = array[i];
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
		foreach (SubControlInfo info in infos)
		{
			AddDockContext(info.Ctl, info.Name, info.Label, info.Icon, info.DockState, info.CanClose);
		}
	}

	private void ThemeService_ThemeChanged(object sender, EventArgs e)
	{
		IEnumerable<SubControlInfo> infos = DetachControlInfos();
		m_dockPanel.Theme = ThemeService.ActiveTheme;
		ReattachControlInfos(infos);
	}

	private IDockContent StringToDockContent(string id)
	{
		foreach (DockContent value in m_dockContent.Values)
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
}
