namespace Sce.Atf.Wpf.Applications;

public class MenuDef
{
	public readonly object MenuTag;

	public readonly string Text;

	public readonly string Description;

	public MenuDef(object menuTag, string text, string description)
	{
		MenuTag = menuTag;
		Text = text;
		Description = description;
	}
}
