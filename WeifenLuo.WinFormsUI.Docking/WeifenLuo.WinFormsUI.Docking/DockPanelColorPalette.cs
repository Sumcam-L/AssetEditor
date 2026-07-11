using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockPanelColorPalette
{
	public AutoHideStripPalette AutoHideStripDefault { get; } = new AutoHideStripPalette();

	public AutoHideStripPalette AutoHideStripHovered { get; } = new AutoHideStripPalette();

	public ButtonPalette OverflowButtonDefault { get; } = new ButtonPalette();

	public HoveredButtonPalette OverflowButtonHovered { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette OverflowButtonPressed { get; } = new HoveredButtonPalette();

	public TabPalette TabSelectedActive { get; } = new TabPalette();

	public TabPalette TabSelectedInactive { get; } = new TabPalette();

	public UnselectedTabPalette TabUnselected { get; } = new UnselectedTabPalette();

	public TabPalette TabUnselectedHovered { get; } = new TabPalette();

	public HoveredButtonPalette TabButtonSelectedActiveHovered { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette TabButtonSelectedActivePressed { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette TabButtonSelectedInactiveHovered { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette TabButtonSelectedInactivePressed { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette TabButtonUnselectedTabHoveredButtonHovered { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette TabButtonUnselectedTabHoveredButtonPressed { get; } = new HoveredButtonPalette();

	public MainWindowPalette MainWindowActive { get; } = new MainWindowPalette();

	public MainWindowStatusBarPalette MainWindowStatusBarDefault { get; } = new MainWindowStatusBarPalette();

	public ToolWindowCaptionPalette ToolWindowCaptionActive { get; } = new ToolWindowCaptionPalette();

	public ToolWindowCaptionPalette ToolWindowCaptionInactive { get; } = new ToolWindowCaptionPalette();

	public HoveredButtonPalette ToolWindowCaptionButtonActiveHovered { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette ToolWindowCaptionButtonPressed { get; } = new HoveredButtonPalette();

	public HoveredButtonPalette ToolWindowCaptionButtonInactiveHovered { get; } = new HoveredButtonPalette();

	public ToolWindowTabPalette ToolWindowTabSelectedActive { get; } = new ToolWindowTabPalette();

	public ToolWindowTabPalette ToolWindowTabSelectedInactive { get; } = new ToolWindowTabPalette();

	public ToolWindowUnselectedTabPalette ToolWindowTabUnselected { get; } = new ToolWindowUnselectedTabPalette();

	public ToolWindowTabPalette ToolWindowTabUnselectedHovered { get; } = new ToolWindowTabPalette();

	public Color ToolWindowBorder { get; set; }

	public Color ToolWindowSeparator { get; set; }

	public DockTargetPalette DockTarget { get; } = new DockTargetPalette();

	public CommandBarMenuPalette CommandBarMenuDefault { get; } = new CommandBarMenuPalette();

	public CommandBarMenuPopupPalette CommandBarMenuPopupDefault { get; } = new CommandBarMenuPopupPalette();

	public CommandBarMenuPopupDisabledPalette CommandBarMenuPopupDisabled { get; } = new CommandBarMenuPopupDisabledPalette();

	public CommandBarMenuPopupHoveredPalette CommandBarMenuPopupHovered { get; } = new CommandBarMenuPopupHoveredPalette();

	public CommandBarMenuTopLevelHeaderPalette CommandBarMenuTopLevelHeaderHovered { get; } = new CommandBarMenuTopLevelHeaderPalette();

	public CommandBarToolbarPalette CommandBarToolbarDefault { get; } = new CommandBarToolbarPalette();

	public CommandBarToolbarButtonCheckedPalette CommandBarToolbarButtonChecked { get; } = new CommandBarToolbarButtonCheckedPalette();

	public CommandBarToolbarButtonCheckedHoveredPalette CommandBarToolbarButtonCheckedHovered { get; } = new CommandBarToolbarButtonCheckedHoveredPalette();

	public CommandBarToolbarButtonPalette CommandBarToolbarButtonDefault { get; } = new CommandBarToolbarButtonPalette();

	public CommandBarToolbarButtonHoveredPalette CommandBarToolbarButtonHovered { get; } = new CommandBarToolbarButtonHoveredPalette();

	public CommandBarToolbarButtonPressedPalette CommandBarToolbarButtonPressed { get; } = new CommandBarToolbarButtonPressedPalette();

	public CommandBarToolbarOverflowButtonPalette CommandBarToolbarOverflowHovered { get; } = new CommandBarToolbarOverflowButtonPalette();

	public CommandBarToolbarOverflowButtonPalette CommandBarToolbarOverflowPressed { get; } = new CommandBarToolbarOverflowButtonPalette();

	public VisualStudioColorTable ColorTable { get; }

	public DockPanelColorPalette(IPaletteFactory factory)
	{
		factory.Initialize(this);
	}
}
