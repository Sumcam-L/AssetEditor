using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public sealed class NestedDockingStatus
{
	private DockPane m_dockPane;

	private NestedPaneCollection m_nestedPanes;

	private DockPane m_previousPane;

	private DockAlignment m_alignment;

	private double m_proportion = 0.5;

	private bool m_isDisplaying;

	private DockPane m_displayingPreviousPane;

	private DockAlignment m_displayingAlignment;

	private double m_displayingProportion = 0.5;

	private Rectangle m_logicalBounds = Rectangle.Empty;

	private Rectangle m_paneBounds = Rectangle.Empty;

	private Rectangle m_splitterBounds = Rectangle.Empty;

	public DockPane DockPane => m_dockPane;

	public NestedPaneCollection NestedPanes => m_nestedPanes;

	public DockPane PreviousPane => m_previousPane;

	public DockAlignment Alignment => m_alignment;

	public double Proportion => m_proportion;

	public bool IsDisplaying => m_isDisplaying;

	public DockPane DisplayingPreviousPane => m_displayingPreviousPane;

	public DockAlignment DisplayingAlignment => m_displayingAlignment;

	public double DisplayingProportion => m_displayingProportion;

	public Rectangle LogicalBounds => m_logicalBounds;

	public Rectangle PaneBounds => m_paneBounds;

	public Rectangle SplitterBounds => m_splitterBounds;

	internal NestedDockingStatus(DockPane pane)
	{
		m_dockPane = pane;
	}

	internal void SetStatus(NestedPaneCollection nestedPanes, DockPane previousPane, DockAlignment alignment, double proportion)
	{
		m_nestedPanes = nestedPanes;
		m_previousPane = previousPane;
		m_alignment = alignment;
		m_proportion = proportion;
	}

	internal void SetDisplayingStatus(bool isDisplaying, DockPane displayingPreviousPane, DockAlignment displayingAlignment, double displayingProportion)
	{
		m_isDisplaying = isDisplaying;
		m_displayingPreviousPane = displayingPreviousPane;
		m_displayingAlignment = displayingAlignment;
		m_displayingProportion = displayingProportion;
	}

	internal void SetDisplayingBounds(Rectangle logicalBounds, Rectangle paneBounds, Rectangle splitterBounds)
	{
		m_logicalBounds = logicalBounds;
		m_paneBounds = paneBounds;
		m_splitterBounds = splitterBounds;
	}
}
