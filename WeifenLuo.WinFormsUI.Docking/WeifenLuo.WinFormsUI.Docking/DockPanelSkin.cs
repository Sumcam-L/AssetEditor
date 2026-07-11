using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPanelSkinConverter))]
public class DockPanelSkin
{
	private AutoHideStripSkin m_autoHideStripSkin = new AutoHideStripSkin();

	private DockPaneStripSkin m_dockPaneStripSkin = new DockPaneStripSkin();

	public AutoHideStripSkin AutoHideStripSkin
	{
		get
		{
			return m_autoHideStripSkin;
		}
		set
		{
			m_autoHideStripSkin = value;
		}
	}

	public DockPaneStripSkin DockPaneStripSkin
	{
		get
		{
			return m_dockPaneStripSkin;
		}
		set
		{
			m_dockPaneStripSkin = value;
		}
	}
}
