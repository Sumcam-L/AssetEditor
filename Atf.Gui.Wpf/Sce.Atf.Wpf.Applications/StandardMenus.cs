using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public static class StandardMenus
{
	public static MenuDef File = new MenuDef(StandardMenu.File, "_File".Localize(), "File Commands".Localize());

	public static MenuDef Edit = new MenuDef(StandardMenu.Edit, "_Edit".Localize(), "Editing Commands".Localize());

	public static MenuDef View = new MenuDef(StandardMenu.View, "_View".Localize(), "View Commands".Localize());

	public static MenuDef Modify = new MenuDef(StandardMenu.Modify, "Modify".Localize(), "Modify Commands".Localize());

	public static MenuDef Format = new MenuDef(StandardMenu.Format, "_Format".Localize(), "Formatting Commands".Localize());

	public static MenuDef Window = new MenuDef(StandardMenu.Window, "_Window".Localize(), "Window Management Commands".Localize());

	public static MenuDef Help = new MenuDef(StandardMenu.Help, "_Help".Localize(), "Help Commands".Localize());
}
