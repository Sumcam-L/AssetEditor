using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPaneStripConverter))]
public class DockPaneStripSkin
{
	private DockPaneStripGradient m_DocumentGradient = new DockPaneStripGradient();

	private DockPaneStripToolWindowGradient m_ToolWindowGradient = new DockPaneStripToolWindowGradient();

	private Font m_textFont = SystemFonts.MenuFont;

	public DockPaneStripGradient DocumentGradient
	{
		get
		{
			return m_DocumentGradient;
		}
		set
		{
			m_DocumentGradient = value;
		}
	}

	public DockPaneStripToolWindowGradient ToolWindowGradient
	{
		get
		{
			return m_ToolWindowGradient;
		}
		set
		{
			m_ToolWindowGradient = value;
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
