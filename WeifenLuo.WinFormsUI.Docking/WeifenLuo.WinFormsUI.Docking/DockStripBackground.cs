using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPaneTabGradientConverter))]
public class DockStripBackground
{
	private Color m_startColor = SystemColors.Control;

	private Color m_endColor = SystemColors.Control;

	[DefaultValue(typeof(SystemColors), "Control")]
	public Color StartColor
	{
		get
		{
			return m_startColor;
		}
		set
		{
			m_startColor = value;
		}
	}

	[DefaultValue(typeof(SystemColors), "Control")]
	public Color EndColor
	{
		get
		{
			return m_endColor;
		}
		set
		{
			m_endColor = value;
		}
	}
}
