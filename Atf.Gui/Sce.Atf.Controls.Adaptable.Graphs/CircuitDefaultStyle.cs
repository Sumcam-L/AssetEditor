namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitDefaultStyle
{
	private static EdgeStyle s_edgeStyle = EdgeStyle.Default;

	private static bool s_showExpandedGroupPins;

	private static bool s_showVirtualLinks = true;

	public static EdgeStyle EdgeStyle
	{
		get
		{
			return s_edgeStyle;
		}
		set
		{
			s_edgeStyle = value;
		}
	}

	public static bool ShowExpandedGroupPins
	{
		get
		{
			return s_showExpandedGroupPins;
		}
		set
		{
			s_showExpandedGroupPins = value;
		}
	}

	public static bool ShowVirtualLinks
	{
		get
		{
			return s_showVirtualLinks;
		}
		set
		{
			s_showVirtualLinks = value;
		}
	}
}
