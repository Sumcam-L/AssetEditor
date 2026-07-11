using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPaneStripGradientConverter))]
public class DockPaneStripToolWindowGradient : DockPaneStripGradient
{
	private TabGradient m_activeCaptionGradient = new TabGradient();

	private TabGradient m_inactiveCaptionGradient = new TabGradient();

	public TabGradient ActiveCaptionGradient
	{
		get
		{
			return m_activeCaptionGradient;
		}
		set
		{
			m_activeCaptionGradient = value;
		}
	}

	public TabGradient InactiveCaptionGradient
	{
		get
		{
			return m_inactiveCaptionGradient;
		}
		set
		{
			m_inactiveCaptionGradient = value;
		}
	}
}
