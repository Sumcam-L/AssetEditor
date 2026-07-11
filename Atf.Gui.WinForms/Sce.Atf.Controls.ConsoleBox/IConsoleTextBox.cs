using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ConsoleBox;

public interface IConsoleTextBox
{
	string Prompt { get; set; }

	CmdHandler CommandHandler { set; }

	Color BackColor { get; set; }

	Color ForeColor { get; set; }

	Control Control { get; }

	SuggestionHandler SuggestionHandler { set; }

	void Clear();

	void EnterCommand(string cmd);

	void Write(string text);

	void WriteLine(string text);
}
