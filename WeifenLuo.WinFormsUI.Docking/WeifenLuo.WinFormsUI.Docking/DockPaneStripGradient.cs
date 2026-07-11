using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPaneStripGradientConverter))]
public class DockPaneStripGradient
{
	private DockPanelGradient m_dockStripGradient = new DockPanelGradient();

	private TabGradient m_activeTabGradient = new TabGradient();

	private TabGradient m_inactiveTabGradient = new TabGradient();

	private TabGradient m_hoverTabGradient = new TabGradient();

	public DockPanelGradient DockStripGradient
	{
		get
		{
			return m_dockStripGradient;
		}
		set
		{
			m_dockStripGradient = value;
		}
	}

	public TabGradient ActiveTabGradient
	{
		get
		{
			return m_activeTabGradient;
		}
		set
		{
			m_activeTabGradient = value;
		}
	}

	public TabGradient HoverTabGradient
	{
		get
		{
			return m_hoverTabGradient;
		}
		set
		{
			m_hoverTabGradient = value;
		}
	}

	public TabGradient InactiveTabGradient
	{
		get
		{
			return m_inactiveTabGradient;
		}
		set
		{
			m_inactiveTabGradient = value;
		}
	}
}
