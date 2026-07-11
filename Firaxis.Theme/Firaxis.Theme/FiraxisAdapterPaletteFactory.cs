using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisAdapterPaletteFactory : IPaletteFactory
{
	private FiraxisThemePalette Palette { get; set; }

	public FiraxisAdapterPaletteFactory(FiraxisThemePalette palette)
	{
		Palette = palette;
	}

	public void Initialize(DockPanelColorPalette palette)
	{
		palette.AutoHideStripDefault.Background = Palette.AutoHideTabPalette.Background;
		palette.AutoHideStripDefault.Border = Palette.AutoHideTabPalette.Border;
		palette.AutoHideStripDefault.Text = Palette.AutoHideTabPalette.Text;
		palette.AutoHideStripHovered.Background = Palette.AutoHideTabMouseOverPalette.Background;
		palette.AutoHideStripHovered.Border = Palette.AutoHideTabMouseOverPalette.Border;
		palette.AutoHideStripHovered.Text = Palette.AutoHideTabMouseOverPalette.Text;
		palette.CommandBarMenuDefault.Background = Palette.CommandBarMenuPalette.Background;
		palette.CommandBarMenuDefault.Text = Palette.CommandBarMenuPalette.Text;
		palette.CommandBarMenuPopupDefault.Arrow = Palette.CommandBarMenuPopupPalette.Arrow;
		palette.CommandBarMenuPopupDefault.BackgroundBottom = Palette.CommandBarMenuPopupPalette.BackgroundBottom;
		palette.CommandBarMenuPopupDefault.BackgroundTop = Palette.CommandBarMenuPopupPalette.BackgroundTop;
		palette.CommandBarMenuPopupDefault.Border = Palette.CommandBarMenuPopupPalette.Border;
		palette.CommandBarMenuPopupDefault.Checkmark = Palette.CommandBarMenuPopupPalette.Checkmark;
		palette.CommandBarMenuPopupDefault.CheckmarkBackground = Palette.CommandBarMenuPopupPalette.CheckmarkBackground;
		palette.CommandBarMenuPopupDefault.IconBackground = Palette.CommandBarMenuPopupPalette.IconBackground;
		palette.CommandBarMenuPopupDefault.Separator = Palette.CommandBarMenuPopupPalette.Separator;
		palette.CommandBarMenuPopupDisabled.Checkmark = Palette.CommandBarMenuPopupDisabledPalette.Checkmark;
		palette.CommandBarMenuPopupDisabled.CheckmarkBackground = Palette.CommandBarMenuPopupDisabledPalette.CheckmarkBackground;
		palette.CommandBarMenuPopupDisabled.Text = Palette.CommandBarMenuPopupDisabledPalette.Text;
		palette.CommandBarMenuPopupHovered.Arrow = Palette.CommandBarMenuPopupHoveredPalette.Arrow;
		palette.CommandBarMenuPopupHovered.Checkmark = Palette.CommandBarMenuPopupHoveredPalette.Checkmark;
		palette.CommandBarMenuPopupHovered.CheckmarkBackground = Palette.CommandBarMenuPopupHoveredPalette.CheckmarkBackground;
		palette.CommandBarMenuPopupHovered.ItemBackground = Palette.CommandBarMenuPopupHoveredPalette.ItemBackground;
		palette.CommandBarMenuPopupHovered.Text = Palette.CommandBarMenuPopupHoveredPalette.Text;
		palette.CommandBarMenuTopLevelHeaderHovered.Background = Palette.CommandBarMenuTopLevelHeaderHoveredPalette.Background;
		palette.CommandBarMenuTopLevelHeaderHovered.Border = Palette.CommandBarMenuTopLevelHeaderHoveredPalette.Border;
		palette.CommandBarMenuTopLevelHeaderHovered.Text = Palette.CommandBarMenuTopLevelHeaderHoveredPalette.Text;
		palette.CommandBarToolbarDefault.Background = Palette.CommandBarToolbarPalette.Background;
		palette.CommandBarToolbarDefault.Border = Palette.CommandBarToolbarPalette.Border;
		palette.CommandBarToolbarDefault.Grip = Palette.CommandBarToolbarPalette.Grip;
		palette.CommandBarToolbarDefault.OverflowButtonBackground = Palette.CommandBarToolbarPalette.OverflowButtonBackground;
		palette.CommandBarToolbarDefault.OverflowButtonGlyph = Palette.CommandBarToolbarPalette.OverflowButtonGlyph;
		palette.CommandBarToolbarDefault.Separator = Palette.CommandBarToolbarPalette.Separator;
		palette.CommandBarToolbarDefault.SeparatorAccent = Palette.CommandBarToolbarPalette.SeparatorAccent;
		palette.CommandBarToolbarDefault.Tray = Palette.CommandBarToolbarPalette.Tray;
		palette.CommandBarToolbarButtonChecked.Background = Palette.CommandBarToolbarButtonCheckedPalette.Background;
		palette.CommandBarToolbarButtonChecked.Border = Palette.CommandBarToolbarButtonCheckedPalette.Border;
		palette.CommandBarToolbarButtonChecked.Text = Palette.CommandBarToolbarButtonCheckedPalette.Text;
		palette.CommandBarToolbarButtonCheckedHovered.Border = Palette.CommandBarToolbarButtonCheckedHoveredPalette.Border;
		palette.CommandBarToolbarButtonCheckedHovered.Text = Palette.CommandBarToolbarButtonCheckedHoveredPalette.Text;
		palette.CommandBarToolbarButtonDefault.Arrow = Palette.CommandBarToolbarButtonPalette.Arrow;
		palette.CommandBarToolbarButtonHovered.Arrow = Palette.CommandBarToolbarButtonHoveredPalette.Arrow;
		palette.CommandBarToolbarButtonHovered.Separator = Palette.CommandBarToolbarButtonHoveredPalette.Separator;
		palette.CommandBarToolbarButtonPressed.Arrow = Palette.CommandBarToolbarButtonPressedPalette.Arrow;
		palette.CommandBarToolbarButtonPressed.Background = Palette.CommandBarToolbarButtonPressedPalette.Background;
		palette.CommandBarToolbarButtonPressed.Text = Palette.CommandBarToolbarButtonPressedPalette.Text;
		palette.CommandBarToolbarOverflowHovered.Background = Palette.CommandBarToolbarOverflowHoveredPalette.Background;
		palette.CommandBarToolbarOverflowHovered.Glyph = Palette.CommandBarToolbarOverflowHoveredPalette.Glyph;
		palette.CommandBarToolbarOverflowPressed.Background = Palette.CommandBarToolbarOverflowPressedPalette.Background;
		palette.CommandBarToolbarOverflowPressed.Glyph = Palette.CommandBarToolbarOverflowPressedPalette.Glyph;
		palette.OverflowButtonDefault.Glyph = Palette.OverflowButtonPalette.Glyph;
		palette.OverflowButtonHovered.Background = Palette.OverflowButtonHoveredPalette.Background;
		palette.OverflowButtonHovered.Border = Palette.OverflowButtonHoveredPalette.Border;
		palette.OverflowButtonHovered.Glyph = Palette.OverflowButtonHoveredPalette.Glyph;
		palette.OverflowButtonPressed.Background = Palette.OverflowButtonPressedPalette.Background;
		palette.OverflowButtonPressed.Border = Palette.OverflowButtonPressedPalette.Border;
		palette.OverflowButtonPressed.Glyph = Palette.OverflowButtonPressedPalette.Glyph;
		palette.TabSelectedActive.Background = Palette.TabSelectedActivePalette.Background;
		palette.TabSelectedActive.Button = Palette.TabSelectedActivePalette.Button;
		palette.TabSelectedActive.Text = Palette.TabSelectedActivePalette.Text;
		palette.TabSelectedInactive.Background = Palette.TabSelectedInactivePalette.Background;
		palette.TabSelectedInactive.Button = Palette.TabSelectedInactivePalette.Button;
		palette.TabSelectedInactive.Text = Palette.TabSelectedInactivePalette.Text;
		palette.TabUnselected.Text = Palette.TabUnselectedPalette.Text;
		palette.TabUnselected.Background = Palette.TabUnselectedPalette.Background;
		palette.TabUnselectedHovered.Background = Palette.TabUnselectedHoveredPalette.Background;
		palette.TabUnselectedHovered.Button = Palette.TabUnselectedHoveredPalette.Button;
		palette.TabUnselectedHovered.Text = Palette.TabUnselectedHoveredPalette.Text;
		palette.TabButtonSelectedActiveHovered.Background = Palette.TabButtonSelectedActiveHoveredPalette.Background;
		palette.TabButtonSelectedActiveHovered.Border = Palette.TabButtonSelectedActiveHoveredPalette.Border;
		palette.TabButtonSelectedActiveHovered.Glyph = Palette.TabButtonSelectedActiveHoveredPalette.Glyph;
		palette.TabButtonSelectedActivePressed.Background = Palette.TabButtonSelectedActivePressedPalette.Background;
		palette.TabButtonSelectedActivePressed.Border = Palette.TabButtonSelectedActivePressedPalette.Border;
		palette.TabButtonSelectedActivePressed.Glyph = Palette.TabButtonSelectedActivePressedPalette.Glyph;
		palette.TabButtonSelectedInactiveHovered.Background = Palette.TabButtonSelectedInactiveHoveredPalette.Background;
		palette.TabButtonSelectedInactiveHovered.Border = Palette.TabButtonSelectedInactiveHoveredPalette.Border;
		palette.TabButtonSelectedInactiveHovered.Glyph = Palette.TabButtonSelectedInactiveHoveredPalette.Glyph;
		palette.TabButtonSelectedInactivePressed.Background = Palette.TabButtonSelectedInactivePressedPalette.Background;
		palette.TabButtonSelectedInactivePressed.Border = Palette.TabButtonSelectedInactivePressedPalette.Border;
		palette.TabButtonSelectedInactivePressed.Glyph = Palette.TabButtonSelectedInactivePressedPalette.Glyph;
		palette.TabButtonUnselectedTabHoveredButtonHovered.Background = Palette.TabButtonUnselectedTabHoveredButtonHoveredPalette.Background;
		palette.TabButtonUnselectedTabHoveredButtonHovered.Border = Palette.TabButtonUnselectedTabHoveredButtonHoveredPalette.Border;
		palette.TabButtonUnselectedTabHoveredButtonHovered.Glyph = Palette.TabButtonUnselectedTabHoveredButtonHoveredPalette.Glyph;
		palette.TabButtonUnselectedTabHoveredButtonPressed.Background = Palette.TabButtonUnselectedTabHoveredButtonPressedPalette.Background;
		palette.TabButtonUnselectedTabHoveredButtonPressed.Border = Palette.TabButtonUnselectedTabHoveredButtonPressedPalette.Border;
		palette.TabButtonUnselectedTabHoveredButtonPressed.Glyph = Palette.TabButtonUnselectedTabHoveredButtonPressedPalette.Glyph;
		palette.MainWindowActive.Background = Palette.MainWindowActivePalette.Background;
		palette.MainWindowStatusBarDefault.Background = Palette.MainWindowStatusBarPalette.Background;
		palette.MainWindowStatusBarDefault.Highlight = Palette.MainWindowStatusBarPalette.Highlight;
		palette.MainWindowStatusBarDefault.HighlightText = Palette.MainWindowStatusBarPalette.HighlightText;
		palette.MainWindowStatusBarDefault.ResizeGrip = Palette.MainWindowStatusBarPalette.ResizeGrip;
		palette.MainWindowStatusBarDefault.ResizeGripAccent = Palette.MainWindowStatusBarPalette.ResizeGripAccent;
		palette.MainWindowStatusBarDefault.Text = Palette.MainWindowStatusBarPalette.Text;
		palette.ToolWindowCaptionActive.Background = Palette.ToolWindowCaptionActivePalette.Background;
		palette.ToolWindowCaptionActive.Button = Palette.ToolWindowCaptionActivePalette.Button;
		palette.ToolWindowCaptionActive.Grip = Palette.ToolWindowCaptionActivePalette.Grip;
		palette.ToolWindowCaptionActive.Text = Palette.ToolWindowCaptionActivePalette.Text;
		palette.ToolWindowCaptionInactive.Background = Palette.ToolWindowCaptionInactivePalette.Background;
		palette.ToolWindowCaptionInactive.Button = Palette.ToolWindowCaptionInactivePalette.Button;
		palette.ToolWindowCaptionInactive.Grip = Palette.ToolWindowCaptionInactivePalette.Grip;
		palette.ToolWindowCaptionInactive.Text = Palette.ToolWindowCaptionInactivePalette.Text;
		palette.ToolWindowCaptionButtonActiveHovered.Background = Palette.ToolWindowCaptionButtonActiveHoveredPalette.Background;
		palette.ToolWindowCaptionButtonActiveHovered.Border = Palette.ToolWindowCaptionButtonActiveHoveredPalette.Border;
		palette.ToolWindowCaptionButtonActiveHovered.Glyph = Palette.ToolWindowCaptionButtonActiveHoveredPalette.Glyph;
		palette.ToolWindowCaptionButtonPressed.Background = Palette.ToolWindowCaptionButtonPressedPalette.Background;
		palette.ToolWindowCaptionButtonPressed.Border = Palette.ToolWindowCaptionButtonPressedPalette.Border;
		palette.ToolWindowCaptionButtonPressed.Glyph = Palette.ToolWindowCaptionButtonPressedPalette.Glyph;
		palette.ToolWindowCaptionButtonInactiveHovered.Background = Palette.ToolWindowCaptionButtonInactiveHoveredPalette.Background;
		palette.ToolWindowCaptionButtonInactiveHovered.Border = Palette.ToolWindowCaptionButtonInactiveHoveredPalette.Border;
		palette.ToolWindowCaptionButtonInactiveHovered.Glyph = Palette.ToolWindowCaptionButtonInactiveHoveredPalette.Glyph;
		palette.ToolWindowTabSelectedActive.Background = Palette.ToolWindowTabSelectedActivePalette.Background;
		palette.ToolWindowTabSelectedActive.Text = Palette.ToolWindowTabSelectedActivePalette.Text;
		palette.ToolWindowTabSelectedInactive.Background = Palette.ToolWindowTabSelectedInactivePalette.Background;
		palette.ToolWindowTabSelectedInactive.Text = Palette.ToolWindowTabSelectedInactivePalette.Text;
		palette.ToolWindowTabUnselected.Text = Palette.ToolWindowTabUnselectedPalette.Text;
		palette.ToolWindowTabUnselected.Background = Palette.ToolWindowTabUnselectedPalette.Background;
		palette.ToolWindowTabUnselectedHovered.Background = Palette.ToolWindowTabUnselectedHoveredPalette.Background;
		palette.ToolWindowTabUnselectedHovered.Text = Palette.ToolWindowTabUnselectedHoveredPalette.Text;
		palette.ToolWindowSeparator = Palette.ToolWindowPalette.Separator;
		palette.ToolWindowBorder = Palette.ToolWindowPalette.Border;
		palette.DockTarget.Background = Palette.DockTargetPalette.Background;
		palette.DockTarget.Border = Palette.DockTargetPalette.Border;
		palette.DockTarget.ButtonBackground = Palette.DockTargetPalette.ButtonBackground;
		palette.DockTarget.ButtonBorder = Palette.DockTargetPalette.ButtonBorder;
		palette.DockTarget.GlyphBackground = Palette.DockTargetPalette.GlyphBackground;
		palette.DockTarget.GlyphArrow = Palette.DockTargetPalette.GlyphArrow;
		palette.DockTarget.GlyphBorder = Palette.DockTargetPalette.GlyphBorder;
	}
}
