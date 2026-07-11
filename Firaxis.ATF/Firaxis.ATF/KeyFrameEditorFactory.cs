namespace Firaxis.ATF;

public class KeyFrameEditorFactory : IATFEditorFactory
{
	public IATFEditor CreateEditor()
	{
		return new KeyFrameEditor();
	}
}
