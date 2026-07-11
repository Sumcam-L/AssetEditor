using System.Collections.Generic;

namespace Firaxis.ATF;

public static class EditorCatalog
{
	private static IDictionary<string, IATFEditorFactory> s_atfEditors;

	static EditorCatalog()
	{
		s_atfEditors = new Dictionary<string, IATFEditorFactory>();
		s_atfEditors["KeyFrame"] = new KeyFrameEditorFactory();
		s_atfEditors["GridView"] = new GridViewEditorFactory();
	}

	public static IATFEditor CreateEditor(string editorName)
	{
		return s_atfEditors[editorName].CreateEditor();
	}

	public static IEnumerable<string> GetSupportedEditors()
	{
		return s_atfEditors.Keys;
	}

	public static bool IsEditorSupported(string editorName)
	{
		return s_atfEditors.ContainsKey(editorName);
	}
}
