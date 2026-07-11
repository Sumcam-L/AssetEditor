namespace Firaxis.ATF;

public class GridViewEditorFactory : IATFEditorFactory
{
	public IATFEditor CreateEditor()
	{
		return new GridViewEditor();
	}
}
