using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class CustomColorTable : ProfessionalColorTable
{
	public Color SettableButtonSelectedHighlight { get; set; }

	public Color SettableButtonSelectedHighlightBorder { get; set; }

	public Color SettableButtonPressedHighlight { get; set; }

	public Color SettableButtonPressedHighlightBorder { get; set; }

	public Color SettableButtonCheckedHighlight { get; set; }

	public Color SettableButtonCheckedHighlightBorder { get; set; }

	public Color SettableButtonPressedBorder { get; set; }

	public Color SettableButtonSelectedBorder { get; set; }

	public Color SettableButtonCheckedGradientBegin { get; set; }

	public Color SettableButtonCheckedGradientMiddle { get; set; }

	public Color SettableButtonCheckedGradientEnd { get; set; }

	public Color SettableButtonSelectedGradientBegin { get; set; }

	public Color SettableButtonSelectedGradientMiddle { get; set; }

	public Color SettableButtonSelectedGradientEnd { get; set; }

	public Color SettableButtonPressedGradientBegin { get; set; }

	public Color SettableButtonPressedGradientMiddle { get; set; }

	public Color SettableButtonPressedGradientEnd { get; set; }

	public Color SettableCheckBackground { get; set; }

	public Color SettableCheckSelectedBackground { get; set; }

	public Color SettableCheckPressedBackground { get; set; }

	public Color SettableGripDark { get; set; }

	public Color SettableGripLight { get; set; }

	public Color SettableImageMarginGradientBegin { get; set; }

	public Color SettableImageMarginGradientMiddle { get; set; }

	public Color SettableImageMarginGradientEnd { get; set; }

	public Color SettableImageMarginRevealedGradientBegin { get; set; }

	public Color SettableImageMarginRevealedGradientMiddle { get; set; }

	public Color SettableImageMarginRevealedGradientEnd { get; set; }

	public Color SettableMenuStripGradientBegin { get; set; }

	public Color SettableMenuStripGradientEnd { get; set; }

	public Color SettableMenuItemSelected { get; set; }

	public Color SettableMenuItemBorder { get; set; }

	public Color SettableMenuBorder { get; set; }

	public Color SettableMenuItemSelectedGradientBegin { get; set; }

	public Color SettableMenuItemSelectedGradientEnd { get; set; }

	public Color SettableMenuItemPressedGradientBegin { get; set; }

	public Color SettableMenuItemPressedGradientMiddle { get; set; }

	public Color SettableMenuItemPressedGradientEnd { get; set; }

	public Color SettableRaftingContainerGradientBegin { get; set; }

	public Color SettableRaftingContainerGradientEnd { get; set; }

	public Color SettableSeparatorDark { get; set; }

	public Color SettableSeparatorLight { get; set; }

	public Color SettableStatusStripGradientBegin { get; set; }

	public Color SettableStatusStripGradientEnd { get; set; }

	public Color SettableToolStripBorder { get; set; }

	public Color SettableToolStripDropDownBackground { get; set; }

	public Color SettableToolStripGradientBegin { get; set; }

	public Color SettableToolStripGradientMiddle { get; set; }

	public Color SettableToolStripGradientEnd { get; set; }

	public Color SettableToolStripContentPanelGradientBegin { get; set; }

	public Color SettableToolStripContentPanelGradientEnd { get; set; }

	public Color SettableToolStripPanelGradientBegin { get; set; }

	public Color SettableToolStripPanelGradientEnd { get; set; }

	public Color SettableOverflowButtonGradientBegin { get; set; }

	public Color SettableOverflowButtonGradientMiddle { get; set; }

	public Color SettableOverflowButtonGradientEnd { get; set; }

	public override Color ButtonSelectedHighlight => SettableButtonSelectedHighlight;

	public override Color ButtonSelectedHighlightBorder => SettableButtonSelectedHighlightBorder;

	public override Color ButtonPressedHighlight => SettableButtonPressedHighlight;

	public override Color ButtonPressedHighlightBorder => SettableButtonPressedHighlightBorder;

	public override Color ButtonCheckedHighlight => SettableButtonCheckedHighlight;

	public override Color ButtonCheckedHighlightBorder => SettableButtonCheckedHighlightBorder;

	public override Color ButtonPressedBorder => SettableButtonPressedBorder;

	public override Color ButtonSelectedBorder => SettableButtonSelectedBorder;

	public override Color ButtonCheckedGradientBegin => SettableButtonCheckedGradientBegin;

	public override Color ButtonCheckedGradientMiddle => SettableButtonCheckedGradientMiddle;

	public override Color ButtonCheckedGradientEnd => SettableButtonCheckedGradientEnd;

	public override Color ButtonSelectedGradientBegin => SettableButtonSelectedGradientBegin;

	public override Color ButtonSelectedGradientMiddle => SettableButtonSelectedGradientMiddle;

	public override Color ButtonSelectedGradientEnd => SettableButtonSelectedGradientEnd;

	public override Color ButtonPressedGradientBegin => SettableButtonPressedGradientBegin;

	public override Color ButtonPressedGradientMiddle => SettableButtonPressedGradientMiddle;

	public override Color ButtonPressedGradientEnd => SettableButtonPressedGradientEnd;

	public override Color CheckBackground => SettableCheckBackground;

	public override Color CheckSelectedBackground => SettableCheckSelectedBackground;

	public override Color CheckPressedBackground => SettableCheckPressedBackground;

	public override Color GripDark => SettableGripDark;

	public override Color GripLight => SettableGripLight;

	public override Color ImageMarginGradientBegin => SettableImageMarginGradientBegin;

	public override Color ImageMarginGradientMiddle => SettableImageMarginGradientMiddle;

	public override Color ImageMarginGradientEnd => SettableImageMarginGradientEnd;

	public override Color ImageMarginRevealedGradientBegin => SettableImageMarginGradientBegin;

	public override Color ImageMarginRevealedGradientMiddle => SettableImageMarginRevealedGradientMiddle;

	public override Color ImageMarginRevealedGradientEnd => SettableImageMarginRevealedGradientEnd;

	public override Color MenuStripGradientBegin => SettableMenuStripGradientBegin;

	public override Color MenuStripGradientEnd => SettableMenuStripGradientEnd;

	public override Color MenuItemSelected => SettableMenuItemSelected;

	public override Color MenuItemBorder => SettableMenuItemBorder;

	public override Color MenuBorder => SettableMenuBorder;

	public override Color MenuItemSelectedGradientBegin => SettableMenuItemSelectedGradientBegin;

	public override Color MenuItemSelectedGradientEnd => SettableMenuItemSelectedGradientEnd;

	public override Color MenuItemPressedGradientBegin => SettableMenuItemPressedGradientBegin;

	public override Color MenuItemPressedGradientMiddle => SettableMenuItemPressedGradientMiddle;

	public override Color MenuItemPressedGradientEnd => SettableMenuItemPressedGradientEnd;

	public override Color RaftingContainerGradientBegin => SettableRaftingContainerGradientBegin;

	public override Color RaftingContainerGradientEnd => SettableRaftingContainerGradientEnd;

	public override Color SeparatorDark => SettableSeparatorDark;

	public override Color SeparatorLight => SettableSeparatorLight;

	public override Color StatusStripGradientBegin => SettableStatusStripGradientBegin;

	public override Color StatusStripGradientEnd => SettableStatusStripGradientEnd;

	public override Color ToolStripBorder => SettableToolStripBorder;

	public override Color ToolStripDropDownBackground => SettableToolStripDropDownBackground;

	public override Color ToolStripGradientBegin => SettableToolStripGradientBegin;

	public override Color ToolStripGradientMiddle => SettableToolStripGradientMiddle;

	public override Color ToolStripGradientEnd => SettableToolStripGradientEnd;

	public override Color ToolStripContentPanelGradientBegin => SettableToolStripContentPanelGradientBegin;

	public override Color ToolStripContentPanelGradientEnd => SettableToolStripContentPanelGradientEnd;

	public override Color ToolStripPanelGradientBegin => SettableToolStripPanelGradientBegin;

	public override Color ToolStripPanelGradientEnd => SettableToolStripPanelGradientEnd;

	public override Color OverflowButtonGradientBegin => SettableOverflowButtonGradientBegin;

	public override Color OverflowButtonGradientMiddle => SettableOverflowButtonGradientMiddle;

	public override Color OverflowButtonGradientEnd => SettableOverflowButtonGradientEnd;

	public CustomColorTable()
	{
		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();
		int length = "Settable".Length;
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!(propertyInfo.PropertyType != typeof(Color)) && propertyInfo.Name.StartsWith("Settable"))
			{
				string key = propertyInfo.Name.Remove(0, length);
				dictionary.Add(key, propertyInfo);
			}
		}
		ProfessionalColorTable professionalColorTable = new ProfessionalColorTable();
		PropertyInfo[] properties2 = professionalColorTable.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo[] array2 = properties2;
		foreach (PropertyInfo propertyInfo2 in array2)
		{
			if (dictionary.TryGetValue(propertyInfo2.Name, out var value))
			{
				value.SetValue(this, propertyInfo2.GetValue(professionalColorTable, null), null);
			}
		}
		SettableToolStripBorder = Color.Transparent;
		SettableImageMarginGradientBegin = Color.Transparent;
		SettableImageMarginGradientMiddle = Color.Transparent;
		SettableImageMarginGradientEnd = Color.Transparent;
	}
}
