using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(DockPaneTabGradientConverter))]
public class TabGradient : DockPanelGradient
{
	private Color m_textColor = SystemColors.ControlText;

	[DefaultValue(typeof(SystemColors), "ControlText")]
	public Color TextColor
	{
		get
		{
			return m_textColor;
		}
		set
		{
			m_textColor = value;
		}
	}
}
