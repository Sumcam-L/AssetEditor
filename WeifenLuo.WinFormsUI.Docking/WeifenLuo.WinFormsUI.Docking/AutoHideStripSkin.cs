using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(AutoHideStripConverter))]
public class AutoHideStripSkin
{
	private DockPanelGradient m_dockStripGradient = new DockPanelGradient();

	private TabGradient m_TabGradient = new TabGradient();

	private DockStripBackground m_DockStripBackground = new DockStripBackground();

	private Font m_textFont = SystemFonts.MenuFont;

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

	public TabGradient TabGradient
	{
		get
		{
			return m_TabGradient;
		}
		set
		{
			m_TabGradient = value;
		}
	}

	public DockStripBackground DockStripBackground
	{
		get
		{
			return m_DockStripBackground;
		}
		set
		{
			m_DockStripBackground = value;
		}
	}

	[DefaultValue(typeof(SystemFonts), "MenuFont")]
	public Font TextFont
	{
		get
		{
			return m_textFont;
		}
		set
		{
			m_textFont = value;
		}
	}
}
