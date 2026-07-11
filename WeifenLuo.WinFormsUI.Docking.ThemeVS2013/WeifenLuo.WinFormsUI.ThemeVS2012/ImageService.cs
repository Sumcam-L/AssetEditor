using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

public class ImageService : IImageService
{
	private readonly DockPanelColorPalette _palette;

	public Bitmap Dockindicator_PaneDiamond { get; internal set; }

	public Bitmap Dockindicator_PaneDiamond_Fill { get; internal set; }

	public Bitmap Dockindicator_PaneDiamond_Hotspot { get; internal set; }

	public Bitmap DockIndicator_PaneDiamond_HotspotIndex { get; internal set; }

	public Image DockIndicator_PanelBottom { get; internal set; }

	public Image DockIndicator_PanelFill { get; internal set; }

	public Image DockIndicator_PanelLeft { get; internal set; }

	public Image DockIndicator_PanelRight { get; internal set; }

	public Image DockIndicator_PanelTop { get; internal set; }

	public Bitmap DockPane_Close { get; internal set; }

	public Bitmap DockPane_List { get; internal set; }

	public Bitmap DockPane_Dock { get; internal set; }

	public Bitmap DockPaneActive_AutoHide { get; internal set; }

	public Bitmap DockPane_Option { get; internal set; }

	public Bitmap DockPane_OptionOverflow { get; internal set; }

	public Bitmap DockPaneActive_Close { get; }

	public Bitmap DockPaneActive_Dock { get; }

	public Bitmap DockPaneActive_Option { get; }

	public Bitmap DockPaneHover_Close { get; internal set; }

	public Bitmap DockPaneHover_List { get; internal set; }

	public Bitmap DockPaneHover_Dock { get; internal set; }

	public Bitmap DockPaneActiveHover_AutoHide { get; internal set; }

	public Bitmap DockPaneHover_Option { get; internal set; }

	public Bitmap DockPaneHover_OptionOverflow { get; internal set; }

	public Bitmap DockPanePress_Close { get; internal set; }

	public Bitmap DockPanePress_List { get; internal set; }

	public Bitmap DockPanePress_Dock { get; internal set; }

	public Bitmap DockPanePress_AutoHide { get; internal set; }

	public Bitmap DockPanePress_Option { get; internal set; }

	public Bitmap DockPanePress_OptionOverflow { get; internal set; }

	public Bitmap DockPaneActiveHover_Close { get; }

	public Bitmap DockPaneActiveHover_Dock { get; }

	public Bitmap DockPaneActiveHover_Option { get; }

	public Image TabHoverActive_Close { get; internal set; }

	public Image TabActive_Close { get; internal set; }

	public Image TabInactive_Close { get; internal set; }

	public Image TabHoverInactive_Close { get; internal set; }

	public Image TabHoverLostFocus_Close { get; internal set; }

	public Image TabLostFocus_Close { get; internal set; }

	public Image TabPressActive_Close { get; }

	public Image TabPressInactive_Close { get; }

	public Image TabPressLostFocus_Close { get; }

	public ImageService(ThemeBase theme)
	{
		_palette = theme.ColorPalette;
		Dockindicator_PaneDiamond_Hotspot = Resources.Dockindicator_PaneDiamond_Hotspot;
		DockIndicator_PaneDiamond_HotspotIndex = Resources.DockIndicator_PaneDiamond_HotspotIndex;
		Color glyphArrow = _palette.DockTarget.GlyphArrow;
		Color border = _palette.DockTarget.Border;
		Color buttonBorder = _palette.DockTarget.ButtonBorder;
		Color background = _palette.DockTarget.Background;
		Color buttonBackground = _palette.DockTarget.ButtonBackground;
		Color glyphBorder = _palette.DockTarget.GlyphBorder;
		Color glyphBackground = _palette.DockTarget.GlyphBackground;
		bool flag = glyphBackground.ToArgb() != buttonBackground.ToArgb();
		using (Bitmap layerArrow = ImageServiceHelper.GetLayerImage(glyphArrow, 32, theme.PaintingService))
		{
			using Bitmap layerWindow = ImageServiceHelper.GetLayerImage(glyphBorder, 32, theme.PaintingService);
			using Bitmap layerCore = (flag ? ImageServiceHelper.GetLayerImage(glyphBackground, 32, theme.PaintingService) : null);
			using Bitmap background2 = ImageServiceHelper.GetBackground(background, border, 40, theme.PaintingService);
			using Bitmap bitmap = ImageServiceHelper.GetDockIcon(Resources.MaskArrowBottom, layerArrow, Resources.MaskWindowBottom, layerWindow, Resources.MaskDock, buttonBackground, theme.PaintingService, Resources.MaskCoreBottom, layerCore, buttonBorder);
			using Bitmap bitmap2 = ImageServiceHelper.GetDockIcon(null, null, Resources.MaskWindowCenter, layerWindow, Resources.MaskDock, buttonBackground, theme.PaintingService, Resources.MaskCoreCenter, layerCore, buttonBorder);
			using Bitmap bitmap3 = ImageServiceHelper.GetDockIcon(Resources.MaskArrowLeft, layerArrow, Resources.MaskWindowLeft, layerWindow, Resources.MaskDock, buttonBackground, theme.PaintingService, Resources.MaskCoreLeft, layerCore, buttonBorder);
			using Bitmap bitmap4 = ImageServiceHelper.GetDockIcon(Resources.MaskArrowRight, layerArrow, Resources.MaskWindowRight, layerWindow, Resources.MaskDock, buttonBackground, theme.PaintingService, Resources.MaskCoreRight, layerCore, buttonBorder);
			using Bitmap bitmap5 = ImageServiceHelper.GetDockIcon(Resources.MaskArrowTop, layerArrow, Resources.MaskWindowTop, layerWindow, Resources.MaskDock, buttonBackground, theme.PaintingService, Resources.MaskCoreTop, layerCore, buttonBorder);
			DockIndicator_PanelBottom = ImageServiceHelper.GetDockImage(bitmap, background2);
			DockIndicator_PanelFill = ImageServiceHelper.GetDockImage(bitmap2, background2);
			DockIndicator_PanelLeft = ImageServiceHelper.GetDockImage(bitmap3, background2);
			DockIndicator_PanelRight = ImageServiceHelper.GetDockImage(bitmap4, background2);
			DockIndicator_PanelTop = ImageServiceHelper.GetDockImage(bitmap5, background2);
			using Bitmap five = ImageServiceHelper.GetFiveBackground(Resources.MaskDockFive, background, border, theme.PaintingService);
			Dockindicator_PaneDiamond = ImageServiceHelper.CombineFive(five, bitmap, bitmap2, bitmap3, bitmap4, bitmap5);
			Dockindicator_PaneDiamond_Fill = ImageServiceHelper.CombineFive(five, bitmap, bitmap2, bitmap3, bitmap4, bitmap5);
		}
		TabActive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabSelectedActive.Button, _palette.TabSelectedActive.Background);
		TabInactive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabUnselectedHovered.Button, _palette.TabUnselectedHovered.Background);
		TabLostFocus_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabSelectedInactive.Button, _palette.TabSelectedInactive.Background);
		TabHoverActive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonSelectedActiveHovered.Glyph, _palette.TabButtonSelectedActiveHovered.Background, _palette.TabButtonSelectedActiveHovered.Border);
		TabHoverInactive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonUnselectedTabHoveredButtonHovered.Glyph, _palette.TabButtonUnselectedTabHoveredButtonHovered.Background, _palette.TabButtonUnselectedTabHoveredButtonHovered.Border);
		TabHoverLostFocus_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonSelectedInactiveHovered.Glyph, _palette.TabButtonSelectedInactiveHovered.Background, _palette.TabButtonSelectedInactiveHovered.Border);
		TabPressActive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonSelectedActivePressed.Glyph, _palette.TabButtonSelectedActivePressed.Background, _palette.TabButtonSelectedActivePressed.Border);
		TabPressInactive_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonUnselectedTabHoveredButtonPressed.Glyph, _palette.TabButtonUnselectedTabHoveredButtonPressed.Background, _palette.TabButtonUnselectedTabHoveredButtonPressed.Border);
		TabPressLostFocus_Close = ImageServiceHelper.GetImage(Resources.MaskTabClose, _palette.TabButtonSelectedInactivePressed.Glyph, _palette.TabButtonSelectedInactivePressed.Background, _palette.TabButtonSelectedInactivePressed.Border);
		DockPane_List = ImageServiceHelper.GetImage(Resources.MaskTabList, _palette.OverflowButtonDefault.Glyph, _palette.MainWindowActive.Background);
		DockPane_OptionOverflow = ImageServiceHelper.GetImage(Resources.MaskTabOverflow, _palette.OverflowButtonDefault.Glyph, _palette.MainWindowActive.Background);
		DockPaneHover_List = ImageServiceHelper.GetImage(Resources.MaskTabList, _palette.OverflowButtonHovered.Glyph, _palette.OverflowButtonHovered.Background, _palette.OverflowButtonHovered.Border);
		DockPaneHover_OptionOverflow = ImageServiceHelper.GetImage(Resources.MaskTabOverflow, _palette.OverflowButtonHovered.Glyph, _palette.OverflowButtonHovered.Background, _palette.OverflowButtonHovered.Border);
		DockPanePress_List = ImageServiceHelper.GetImage(Resources.MaskTabList, _palette.OverflowButtonPressed.Glyph, _palette.OverflowButtonPressed.Background, _palette.OverflowButtonPressed.Border);
		DockPanePress_OptionOverflow = ImageServiceHelper.GetImage(Resources.MaskTabOverflow, _palette.OverflowButtonPressed.Glyph, _palette.OverflowButtonPressed.Background, _palette.OverflowButtonPressed.Border);
		DockPane_Close = ImageServiceHelper.GetImage(Resources.MaskToolWindowClose, _palette.ToolWindowCaptionInactive.Button, _palette.ToolWindowCaptionInactive.Background);
		DockPane_Dock = ImageServiceHelper.GetImage(Resources.MaskToolWindowDock, _palette.ToolWindowCaptionInactive.Button, _palette.ToolWindowCaptionInactive.Background);
		DockPane_Option = ImageServiceHelper.GetImage(Resources.MaskToolWindowOption, _palette.ToolWindowCaptionInactive.Button, _palette.ToolWindowCaptionInactive.Background);
		DockPaneActive_Close = ImageServiceHelper.GetImage(Resources.MaskToolWindowClose, _palette.ToolWindowCaptionActive.Button, _palette.ToolWindowCaptionActive.Background);
		DockPaneActive_Dock = ImageServiceHelper.GetImage(Resources.MaskToolWindowDock, _palette.ToolWindowCaptionActive.Button, _palette.ToolWindowCaptionActive.Background);
		DockPaneActive_Option = ImageServiceHelper.GetImage(Resources.MaskToolWindowOption, _palette.ToolWindowCaptionActive.Button, _palette.ToolWindowCaptionActive.Background);
		DockPaneActive_AutoHide = ImageServiceHelper.GetImage(Resources.MaskToolWindowAutoHide, _palette.ToolWindowCaptionActive.Button, _palette.ToolWindowCaptionActive.Background);
		DockPaneHover_Close = ImageServiceHelper.GetImage(Resources.MaskToolWindowClose, _palette.ToolWindowCaptionButtonInactiveHovered.Glyph, _palette.ToolWindowCaptionButtonInactiveHovered.Background, _palette.ToolWindowCaptionButtonInactiveHovered.Border);
		DockPaneHover_Dock = ImageServiceHelper.GetImage(Resources.MaskToolWindowDock, _palette.ToolWindowCaptionButtonInactiveHovered.Glyph, _palette.ToolWindowCaptionButtonInactiveHovered.Background, _palette.ToolWindowCaptionButtonInactiveHovered.Border);
		DockPaneHover_Option = ImageServiceHelper.GetImage(Resources.MaskToolWindowOption, _palette.ToolWindowCaptionButtonInactiveHovered.Glyph, _palette.ToolWindowCaptionButtonInactiveHovered.Background, _palette.ToolWindowCaptionButtonInactiveHovered.Border);
		DockPaneActiveHover_Close = ImageServiceHelper.GetImage(Resources.MaskToolWindowClose, _palette.ToolWindowCaptionButtonActiveHovered.Glyph, _palette.ToolWindowCaptionButtonActiveHovered.Background, _palette.ToolWindowCaptionButtonActiveHovered.Border);
		DockPaneActiveHover_Dock = ImageServiceHelper.GetImage(Resources.MaskToolWindowDock, _palette.ToolWindowCaptionButtonActiveHovered.Glyph, _palette.ToolWindowCaptionButtonActiveHovered.Background, _palette.ToolWindowCaptionButtonActiveHovered.Border);
		DockPaneActiveHover_Option = ImageServiceHelper.GetImage(Resources.MaskToolWindowOption, _palette.ToolWindowCaptionButtonActiveHovered.Glyph, _palette.ToolWindowCaptionButtonActiveHovered.Background, _palette.ToolWindowCaptionButtonActiveHovered.Border);
		DockPaneActiveHover_AutoHide = ImageServiceHelper.GetImage(Resources.MaskToolWindowAutoHide, _palette.ToolWindowCaptionButtonActiveHovered.Glyph, _palette.ToolWindowCaptionButtonActiveHovered.Background, _palette.ToolWindowCaptionButtonActiveHovered.Border);
		DockPanePress_Close = ImageServiceHelper.GetImage(Resources.MaskToolWindowClose, _palette.ToolWindowCaptionButtonPressed.Glyph, _palette.ToolWindowCaptionButtonPressed.Background, _palette.ToolWindowCaptionButtonPressed.Border);
		DockPanePress_Dock = ImageServiceHelper.GetImage(Resources.MaskToolWindowDock, _palette.ToolWindowCaptionButtonPressed.Glyph, _palette.ToolWindowCaptionButtonPressed.Background, _palette.ToolWindowCaptionButtonPressed.Border);
		DockPanePress_Option = ImageServiceHelper.GetImage(Resources.MaskToolWindowOption, _palette.ToolWindowCaptionButtonPressed.Glyph, _palette.ToolWindowCaptionButtonPressed.Background, _palette.ToolWindowCaptionButtonPressed.Border);
		DockPanePress_AutoHide = ImageServiceHelper.GetImage(Resources.MaskToolWindowAutoHide, _palette.ToolWindowCaptionButtonPressed.Glyph, _palette.ToolWindowCaptionButtonPressed.Background, _palette.ToolWindowCaptionButtonPressed.Border);
	}
}
