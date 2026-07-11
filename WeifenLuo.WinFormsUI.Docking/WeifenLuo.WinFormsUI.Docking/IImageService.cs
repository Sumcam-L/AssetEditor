using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public interface IImageService
{
	Bitmap Dockindicator_PaneDiamond { get; }

	Bitmap Dockindicator_PaneDiamond_Fill { get; }

	Bitmap Dockindicator_PaneDiamond_Hotspot { get; }

	Bitmap DockIndicator_PaneDiamond_HotspotIndex { get; }

	Image DockIndicator_PanelBottom { get; }

	Image DockIndicator_PanelFill { get; }

	Image DockIndicator_PanelLeft { get; }

	Image DockIndicator_PanelRight { get; }

	Image DockIndicator_PanelTop { get; }

	Bitmap DockPane_Close { get; }

	Bitmap DockPane_List { get; }

	Bitmap DockPane_Dock { get; }

	Bitmap DockPaneActive_AutoHide { get; }

	Bitmap DockPane_Option { get; }

	Bitmap DockPane_OptionOverflow { get; }

	Bitmap DockPaneActive_Close { get; }

	Bitmap DockPaneActive_Dock { get; }

	Bitmap DockPaneActive_Option { get; }

	Bitmap DockPaneHover_Close { get; }

	Bitmap DockPaneHover_List { get; }

	Bitmap DockPaneHover_Dock { get; }

	Bitmap DockPaneActiveHover_AutoHide { get; }

	Bitmap DockPaneHover_Option { get; }

	Bitmap DockPaneHover_OptionOverflow { get; }

	Bitmap DockPanePress_Close { get; }

	Bitmap DockPanePress_List { get; }

	Bitmap DockPanePress_Dock { get; }

	Bitmap DockPanePress_AutoHide { get; }

	Bitmap DockPanePress_Option { get; }

	Bitmap DockPanePress_OptionOverflow { get; }

	Bitmap DockPaneActiveHover_Close { get; }

	Bitmap DockPaneActiveHover_Dock { get; }

	Bitmap DockPaneActiveHover_Option { get; }

	Image TabActive_Close { get; }

	Image TabInactive_Close { get; }

	Image TabLostFocus_Close { get; }

	Image TabHoverActive_Close { get; }

	Image TabHoverInactive_Close { get; }

	Image TabHoverLostFocus_Close { get; }

	Image TabPressActive_Close { get; }

	Image TabPressInactive_Close { get; }

	Image TabPressLostFocus_Close { get; }
}
