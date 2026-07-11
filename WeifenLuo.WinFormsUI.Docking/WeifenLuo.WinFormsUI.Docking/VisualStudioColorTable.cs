using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public class VisualStudioColorTable : ProfessionalColorTable
{
	private DockPanelColorPalette _palette;

	public Color ButtonCheckedHoveredBorder => _palette.CommandBarToolbarButtonCheckedHovered.Border;

	public Color ButtonCheckedHoveredBackground => _palette.CommandBarMenuPopupHovered.CheckmarkBackground;

	public Color ButtonCheckedBorder => _palette.CommandBarToolbarButtonChecked.Border;

	public override Color ButtonCheckedGradientBegin => _palette.CommandBarToolbarButtonChecked.Background;

	public override Color ButtonCheckedGradientMiddle => _palette.CommandBarToolbarButtonChecked.Background;

	public override Color ButtonCheckedGradientEnd => _palette.CommandBarToolbarButtonChecked.Background;

	public override Color CheckBackground => _palette.CommandBarMenuPopupDefault.CheckmarkBackground;

	public override Color CheckSelectedBackground => _palette.CommandBarMenuPopupHovered.CheckmarkBackground;

	public override Color CheckPressedBackground => _palette.CommandBarMenuPopupHovered.CheckmarkBackground;

	public override Color ButtonPressedBorder => _palette.CommandBarMenuTopLevelHeaderHovered.Border;

	public override Color ButtonPressedGradientBegin => _palette.CommandBarToolbarButtonPressed.Background;

	public override Color ButtonPressedGradientMiddle => _palette.CommandBarToolbarButtonPressed.Background;

	public override Color ButtonPressedGradientEnd => _palette.CommandBarToolbarButtonPressed.Background;

	public override Color MenuItemPressedGradientBegin => _palette.CommandBarMenuPopupDefault.BackgroundTop;

	public override Color MenuItemPressedGradientMiddle => _palette.CommandBarMenuPopupDefault.BackgroundTop;

	public override Color MenuItemPressedGradientEnd => _palette.CommandBarMenuPopupDefault.BackgroundTop;

	public override Color ButtonSelectedBorder => _palette.CommandBarToolbarButtonChecked.Border;

	public override Color ButtonSelectedGradientBegin => _palette.CommandBarMenuTopLevelHeaderHovered.Background;

	public override Color ButtonSelectedGradientMiddle => _palette.CommandBarMenuTopLevelHeaderHovered.Background;

	public override Color ButtonSelectedGradientEnd => _palette.CommandBarMenuTopLevelHeaderHovered.Background;

	public override Color MenuItemSelected => _palette.CommandBarMenuPopupHovered.ItemBackground;

	public override Color MenuItemSelectedGradientBegin => _palette.CommandBarMenuTopLevelHeaderHovered.Background;

	public override Color MenuItemSelectedGradientEnd => _palette.CommandBarMenuTopLevelHeaderHovered.Background;

	public override Color GripDark => _palette.CommandBarToolbarDefault.Grip;

	public override Color GripLight => _palette.CommandBarToolbarDefault.Grip;

	public override Color ImageMarginGradientBegin => _palette.CommandBarMenuPopupDefault.IconBackground;

	public override Color ImageMarginGradientMiddle => _palette.CommandBarMenuPopupDefault.IconBackground;

	public override Color ImageMarginGradientEnd => _palette.CommandBarMenuPopupDefault.IconBackground;

	public override Color MenuStripGradientBegin => _palette.CommandBarMenuDefault.Background;

	public override Color MenuStripGradientEnd => _palette.CommandBarMenuDefault.Background;

	public override Color MenuItemBorder => _palette.CommandBarMenuTopLevelHeaderHovered.Border;

	public override Color MenuBorder => _palette.CommandBarMenuPopupDefault.Border;

	public override Color SeparatorDark => _palette.CommandBarToolbarDefault.Separator;

	public override Color SeparatorLight => _palette.CommandBarToolbarDefault.SeparatorAccent;

	public override Color StatusStripGradientBegin => _palette.MainWindowStatusBarDefault.Background;

	public override Color StatusStripGradientEnd => _palette.MainWindowStatusBarDefault.Background;

	public override Color ToolStripBorder => _palette.CommandBarToolbarDefault.Border;

	public override Color ToolStripDropDownBackground => _palette.CommandBarMenuPopupDefault.BackgroundBottom;

	public override Color ToolStripGradientBegin => _palette.CommandBarToolbarDefault.Background;

	public override Color ToolStripGradientMiddle => _palette.CommandBarToolbarDefault.Background;

	public override Color ToolStripGradientEnd => _palette.CommandBarToolbarDefault.Background;

	public override Color OverflowButtonGradientBegin => _palette.CommandBarToolbarDefault.OverflowButtonBackground;

	public override Color OverflowButtonGradientMiddle => _palette.CommandBarToolbarDefault.OverflowButtonBackground;

	public override Color OverflowButtonGradientEnd => _palette.CommandBarToolbarDefault.OverflowButtonBackground;

	public VisualStudioColorTable(DockPanelColorPalette palette)
	{
		_palette = palette;
	}
}
