using System.Windows;

namespace Sce.Atf.Wpf;

public static class Resources
{
	[ResourceDictionaryResource("Styles.xaml")]
	public static readonly string StylesDictionary;

	[ResourceDictionaryResource("PropertyEditors.xaml")]
	public static readonly string PropertyEditorsDictionary;

	[ImageResource("dialog_error.xaml")]
	public static readonly ResourceKey DialogErrorImageKey;

	[ImageResource("dialog_information.xaml")]
	public static readonly ResourceKey DialogInformationImageKey;

	[ImageResource("dialog_question.xaml")]
	public static readonly ResourceKey DialogQuestionImageKey;

	[ImageResource("dialog_warning.xaml")]
	public static readonly ResourceKey DialogWarningImageKey;

	public static readonly ResourceKey DialogRootBorderStyleKey;

	public static readonly ResourceKey SwitchToDialogKey;

	public static readonly ResourceKey ToolBarTrayStyleKey;

	public static readonly ResourceKey ToolBarStyleKey;

	public static readonly ResourceKey ToolBarButtonStyleKey;

	public static readonly ResourceKey ToolBarItemTemplateKey;

	public static readonly ResourceKey MenuStyleKey;

	public static readonly ResourceKey ContextMenuStyleKey;

	public static readonly ResourceKey SubMenuItemStyleKey;

	public static readonly ResourceKey CommandMenuItemStyleKey;

	public static readonly ResourceKey MenuSeparatorStyleKey;

	public static readonly ResourceKey MenuItemImageStyleKey;

	public static readonly ResourceKey StatusBarStyleKey;

	public static readonly ResourceKey StatusBarItemStyleKey;

	public static readonly ResourceKey AutoGreyStyleKey;

	public static readonly ResourceKey DialogButtonStyleKey;

	public static readonly ResourceKey ListViewItemStyleKey;

	public static readonly ResourceKey TreeViewStyleKey;

	public static readonly ResourceKey TreeViewItemStyleKey;

	public static readonly ResourceKey DefaultTreeViewItemTemplateKey;

	public static readonly ResourceKey TreeViewStateImageStyleKey;

	public static readonly ResourceKey TreeViewItemExpanderStyleKey;

	public static readonly ResourceKey TreeViewImageStyleKey;

	public static readonly ResourceKey TreeViewIconStyleKey;

	public static readonly ResourceKey TreeViewLabelTextBlockStyleKey;

	public static readonly ResourceKey TreeListViewStyleKey;

	public static readonly ResourceKey TreeListViewItemStyleKey;

	public static readonly ResourceKey TileViewStyleKey;

	public static readonly ResourceKey TileViewItemStyleKey;

	public static readonly ResourceKey RadioButtonListStyleKey;

	[ImageResource("error.ico")]
	public static readonly ResourceKey ErrorImageKey;

	[ImageResource("info.ico")]
	public static readonly ResourceKey InfoImageKey;

	[ImageResource("warning.ico")]
	public static readonly ResourceKey WarningImageKey;

	[ImageResource("clear.bmp")]
	public static readonly ResourceKey ClearImageKey;

	[ImageResource("Alphabetical.png")]
	public static readonly ResourceKey AlphabeticalImageKey;

	[ImageResource("ByCategory.png")]
	public static readonly ResourceKey ByCategoryImageKey;

	static Resources()
	{
		StylesDictionary = null;
		PropertyEditorsDictionary = null;
		DialogRootBorderStyleKey = new ComponentResourceKey(typeof(Resources), "DialogRootBorderStyle");
		SwitchToDialogKey = new ComponentResourceKey(typeof(Resources), "SwitchToDialog");
		ToolBarTrayStyleKey = new ComponentResourceKey(typeof(Resources), "ToolBarTrayStyle");
		ToolBarStyleKey = new ComponentResourceKey(typeof(Resources), "ToolBarStyle");
		ToolBarButtonStyleKey = new ComponentResourceKey(typeof(Resources), "ToolBarButtonStyle");
		ToolBarItemTemplateKey = new ComponentResourceKey(typeof(Resources), "ToolBarItemTemplate");
		MenuStyleKey = new ComponentResourceKey(typeof(Resources), "MenuStyle");
		ContextMenuStyleKey = new ComponentResourceKey(typeof(Resources), "ContextMenuStyle");
		SubMenuItemStyleKey = new ComponentResourceKey(typeof(Resources), "SubMenuItemStyle");
		CommandMenuItemStyleKey = new ComponentResourceKey(typeof(Resources), "CommandMenuItemStyle");
		MenuSeparatorStyleKey = new ComponentResourceKey(typeof(Resources), "MenuSeparatorStyle");
		MenuItemImageStyleKey = new ComponentResourceKey(typeof(Resources), "MenuItemImageStyle");
		StatusBarStyleKey = new ComponentResourceKey(typeof(Resources), "StatusBarStyle");
		StatusBarItemStyleKey = new ComponentResourceKey(typeof(Resources), "StatusBarItemStyle");
		AutoGreyStyleKey = new ComponentResourceKey(typeof(Resources), "AutoGreyStyle");
		DialogButtonStyleKey = new ComponentResourceKey(typeof(Resources), "DialogButtonStyle");
		ListViewItemStyleKey = new ComponentResourceKey(typeof(Resources), "ListViewItemStyle");
		TreeViewStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewStyle");
		TreeViewItemStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewItemStyle");
		DefaultTreeViewItemTemplateKey = new ComponentResourceKey(typeof(Resources), "DefaultTreeViewItemTemplate");
		TreeViewStateImageStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewStateImageStyle");
		TreeViewItemExpanderStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewItemExpanderStyle");
		TreeViewImageStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewImageStyle");
		TreeViewIconStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewIconStyle");
		TreeViewLabelTextBlockStyleKey = new ComponentResourceKey(typeof(Resources), "TreeViewLabelTextBlockStyle");
		TreeListViewStyleKey = new ComponentResourceKey(typeof(Resources), "TreeListViewStyle");
		TreeListViewItemStyleKey = new ComponentResourceKey(typeof(Resources), "TreeListViewItemStyle");
		TileViewStyleKey = new ComponentResourceKey(typeof(Resources), "TileView");
		TileViewItemStyleKey = new ComponentResourceKey(typeof(Resources), "TileViewItem");
		RadioButtonListStyleKey = new ComponentResourceKey(typeof(Resources), "RadioButtonListStyle");
		ErrorImageKey = null;
		InfoImageKey = null;
		WarningImageKey = null;
		ClearImageKey = null;
		AlphabeticalImageKey = null;
		ByCategoryImageKey = null;
		ResourceUtil.RegistrationStarted = true;
		ResourceUtil.Register(typeof(Sce.Atf.Resources), "Resources/");
		ResourceUtil.Register(typeof(Resources), "Resources/");
	}

	public static void Register()
	{
	}
}
