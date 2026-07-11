using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Applications;

public struct ControlGradient
{
	[DefaultValue(typeof(SystemColors), "Control")]
	public Color StartColor { get; set; }

	[DefaultValue(typeof(SystemColors), "Control")]
	public Color EndColor { get; set; }

	[DefaultValue(LinearGradientMode.Vertical)]
	public LinearGradientMode LinearGradientMode { get; set; }

	[DefaultValue(typeof(SystemColors), "ControlText")]
	public Color TextColor { get; set; }
}
