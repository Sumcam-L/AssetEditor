using System.Windows.Forms;

namespace ScintillaNET;

public class IndicatorClickEventArgs : IndicatorReleaseEventArgs
{
	public Keys Modifiers { get; private set; }

	public IndicatorClickEventArgs(Scintilla scintilla, Keys modifiers, int bytePosition)
		: base(scintilla, bytePosition)
	{
		Modifiers = modifiers;
	}
}
