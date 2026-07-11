using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ConsoleBox;

internal class ConsoleTextBox : TextBox, IConsoleTextBox
{
	private class CommandList
	{
		private int m_currentPosition = -1;

		private readonly List<string> m_commands = new List<string>();

		public string PreviousCommand
		{
			get
			{
				string result = string.Empty;
				if (m_currentPosition > 0)
				{
					result = m_commands[--m_currentPosition];
				}
				return result;
			}
		}

		public string NextCommand
		{
			get
			{
				string result = string.Empty;
				if (m_currentPosition < m_commands.Count - 1)
				{
					result = m_commands[++m_currentPosition];
				}
				else
				{
					m_currentPosition = m_commands.Count;
				}
				return result;
			}
		}

		private string LastCommand
		{
			get
			{
				int count = m_commands.Count;
				return (count > 0) ? m_commands[count - 1] : string.Empty;
			}
		}

		public void AddCommand(string cmd)
		{
			if (!string.IsNullOrEmpty(cmd))
			{
				if (LastCommand != cmd)
				{
					m_commands.Add(cmd);
				}
				m_currentPosition = m_commands.Count;
			}
		}
	}

	private class SuggestionListBox : ListBox
	{
		private string m_partial = string.Empty;

		public Action<string> InsertText { get; set; }

		public Action<int> RemoveText { get; set; }

		public Func<bool> Suggest { get; set; }

		public void SetSuggestions(string partial, IEnumerable<string> suggestions)
		{
			m_partial = partial;
			base.Items.Clear();
			base.Items.AddRange(suggestions.ToArray());
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			Complete();
		}

		protected override bool IsInputKey(Keys keyData)
		{
			return keyData == Keys.Tab || base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			switch (e.KeyData)
			{
			case Keys.Back:
				RemoveText(1);
				Suggest();
				break;
			case Keys.Down:
				if (SelectedIndex == base.Items.Count - 1)
				{
					SelectedIndex = 0;
					e.Handled = true;
				}
				break;
			case Keys.Up:
				if (SelectedIndex == 0)
				{
					SelectedIndex = base.Items.Count - 1;
					e.Handled = true;
				}
				break;
			case Keys.Tab:
			case Keys.Return:
				Complete();
				break;
			case Keys.Escape:
			case Keys.Left:
			case Keys.Right:
				Hide();
				break;
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			char keyChar = e.KeyChar;
			if (keyChar >= ' ' && keyChar <= '~')
			{
				string text = keyChar.ToString();
				if (s_triggers.Contains(keyChar))
				{
					Complete(text);
					return;
				}
				InsertText(text);
				Suggest();
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			Hide();
		}

		private void Complete(string extra = null)
		{
			if (base.SelectedItem != null)
			{
				RemoveText(m_partial.Length);
				string obj = base.SelectedItem.ToString();
				InsertText(obj);
			}
			if (string.IsNullOrEmpty(extra))
			{
				Hide();
				return;
			}
			InsertText(extra);
			Suggest();
		}
	}

	private CmdHandler m_commandHandler;

	private SuggestionHandler m_suggestionHandler;

	private readonly CommandList m_commandHistory = new CommandList();

	private bool m_multiline;

	private string m_multilinePrompt = "... ";

	private string m_prompt = ">>> ";

	private int m_promptIndex;

	private int m_spacesPerIndent = 4;

	private readonly SuggestionListBox m_suggestionListBox;

	private static readonly char[] s_delimiters = new char[4] { ' ', '\n', '\r', '\t' };

	private static readonly Regex s_nextWord = new Regex("\\w+\\s*|\\W+", RegexOptions.Compiled);

	private static readonly char[] s_triggers = new char[2] { '.', '[' };

	[Browsable(true)]
	[Category("Appearance")]
	[DefaultValue(">>> ")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public string Prompt
	{
		get
		{
			return m_prompt;
		}
		set
		{
			string text = value;
			if (string.IsNullOrEmpty(text))
			{
				text = " ";
			}
			m_prompt = text;
			m_multilinePrompt = new string('.', m_prompt.Length - 1);
			m_multilinePrompt += " ";
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[DefaultValue(4)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int SpacesPerIndent
	{
		get
		{
			return m_spacesPerIndent;
		}
		set
		{
			m_spacesPerIndent = value;
		}
	}

	public CmdHandler CommandHandler
	{
		set
		{
			m_commandHandler = value ?? new CmdHandler(DefaultCommandHandler);
		}
	}

	public SuggestionHandler SuggestionHandler
	{
		set
		{
			m_suggestionHandler = value ?? new SuggestionHandler(DefaultSuggestionHandler);
		}
	}

	public Control Control => this;

	private bool IsCaretAtWritablePosition => CaretOffsetFromPrompt >= 0 && CaretOffsetFromCurrentLine >= 0;

	private bool IsCaretAtEnd => CaretOffsetFromCurrentLine == GetCurrentLine().Length;

	private int CaretIndex
	{
		get
		{
			User32.GetCaretPos(out var lpPoint);
			int charIndexFromPosition = GetCharIndexFromPosition(lpPoint);
			if (charIndexFromPosition != base.SelectionStart)
			{
				return base.SelectionStart + SelectionLength;
			}
			return charIndexFromPosition;
		}
	}

	private int CaretOffsetFromCurrentLine
	{
		get
		{
			int caretIndex = CaretIndex;
			int lineFromCharIndex = GetLineFromCharIndex(caretIndex);
			return caretIndex - GetFirstCharIndexFromLine(lineFromCharIndex) - m_prompt.Length;
		}
	}

	private int CaretOffsetFromPrompt => CaretIndex - m_promptIndex;

	public ConsoleTextBox()
	{
		Font = new Font("Lucida Console", 8f, FontStyle.Regular, GraphicsUnit.Point, 0);
		MaxLength = 0;
		Multiline = true;
		base.ScrollBars = ScrollBars.Vertical;
		ShortcutsEnabled = true;
		base.WordWrap = true;
		base.BorderStyle = BorderStyle.None;
		m_suggestionListBox = new SuggestionListBox
		{
			InsertText = InsertTextAtCaret,
			ItemHeight = 11,
			RemoveText = RemoveTextBeforeCaret,
			Suggest = Suggest
		};
		m_suggestionListBox.Hide();
		base.Controls.Add(m_suggestionListBox);
		WritePrompt();
		m_commandHandler = DefaultCommandHandler;
	}

	public new void Clear()
	{
		Text = string.Empty;
		WritePrompt();
	}

	public new void Copy()
	{
		string selectedText = GetSelectedText();
		if (!string.IsNullOrEmpty(selectedText))
		{
			Clipboard.SetText(selectedText);
		}
	}

	public new void Cut()
	{
		Copy();
		RemoveSelection();
	}

	public void EnterCommand(string cmd)
	{
		if (!string.IsNullOrEmpty(cmd))
		{
			WriteLine(cmd);
			ExecuteCommand(cmd);
		}
	}

	public void Write(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			MoveCaretToEnd();
			AppendText(text);
		}
	}

	public void WriteLine(string text)
	{
		Write(text);
		AppendText(Environment.NewLine);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Control)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Return)
			{
				e.SuppressKeyPress = true;
			}
			return;
		}
		e.Handled = e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Home || e.KeyCode == Keys.End;
		switch (e.KeyCode)
		{
		case Keys.Left:
			PrevChar(MoveLeft);
			break;
		case Keys.Right:
			NextChar(MoveRight);
			break;
		case Keys.Up:
		{
			string previousCommand = m_commandHistory.PreviousCommand;
			if (previousCommand.Length > 0)
			{
				ReplaceTextAtPrompt(previousCommand);
			}
			break;
		}
		case Keys.Down:
		{
			string nextCommand = m_commandHistory.NextCommand;
			ReplaceTextAtPrompt(nextCommand);
			break;
		}
		case Keys.End:
			Next(MoveRight, GetCurrentLine().Length - CaretOffsetFromCurrentLine);
			break;
		case Keys.Home:
			Prev(MoveLeft, CaretOffsetFromCurrentLine);
			break;
		default:
			MoveCaretToWritablePosition();
			break;
		}
		base.OnKeyDown(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		switch (e.KeyChar)
		{
		case '\b':
			e.Handled = true;
			if (SelectionLength > 0)
			{
				RemoveSelection();
			}
			else
			{
				PrevChar(RemoveTextBeforeCaret);
			}
			break;
		case '\r':
		{
			e.Handled = true;
			string textAtPrompt = GetTextAtPrompt();
			AppendText(Environment.NewLine);
			SubmitCommand(textAtPrompt);
			break;
		}
		}
		base.OnKeyPress(e);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (msg.Msg == 256 || msg.Msg == 260)
		{
			switch (keyData)
			{
			case Keys.Back | Keys.Control:
				m_suggestionListBox.Hide();
				PrevWord(RemoveTextBeforeCaret);
				return true;
			case Keys.Delete:
				m_suggestionListBox.Hide();
				if (SelectionLength > 0)
				{
					RemoveSelection();
				}
				else
				{
					NextChar(RemoveTextAfterCaret);
				}
				return true;
			case Keys.Delete | Keys.Control:
				m_suggestionListBox.Hide();
				NextWord(RemoveTextAfterCaret);
				return true;
			case Keys.End | Keys.Control:
			case Keys.End | Keys.Shift | Keys.Control:
				m_suggestionListBox.Hide();
				MoveRight(TextLength - CaretIndex);
				return true;
			case Keys.Return | Keys.Control:
			{
				m_suggestionListBox.Hide();
				string currentLine = GetCurrentLine();
				InsertTextAtCaret(Environment.NewLine);
				Indent(currentLine);
				break;
			}
			case Keys.Home | Keys.Control:
			case Keys.Home | Keys.Shift | Keys.Control:
				m_suggestionListBox.Hide();
				MoveLeft(CaretOffsetFromPrompt);
				return true;
			case Keys.Left | Keys.Control:
			case Keys.Left | Keys.Shift | Keys.Control:
				m_suggestionListBox.Hide();
				PrevWord(MoveLeft);
				return true;
			case Keys.Right | Keys.Control:
			case Keys.Right | Keys.Shift | Keys.Control:
				m_suggestionListBox.Hide();
				NextWord(MoveRight);
				return true;
			case Keys.Space | Keys.Control:
				Suggest();
				return true;
			}
		}
		return ProcessCmdKey(ref msg, keyData);
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 768:
			Cut();
			return;
		case 769:
			Copy();
			return;
		case 770:
		{
			string text = Clipboard.GetText();
			InsertTextAtCaret(text);
			return;
		}
		case 12:
			if (!IsCaretAtWritablePosition)
			{
				MoveCaretToEnd();
			}
			break;
		case 771:
			return;
		}
		base.WndProc(ref m);
	}

	private void ExecuteCommand(string cmd)
	{
		m_commandHistory.AddCommand(cmd);
		if (cmd.Length > 0)
		{
			using StringReader stringReader = new StringReader(cmd);
			for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
			{
				if (IsCompoundStatement(text))
				{
					StringBuilder stringBuilder = new StringBuilder();
					do
					{
						stringBuilder.AppendLine(text);
						text = stringReader.ReadLine();
					}
					while (!string.IsNullOrEmpty(text));
					m_commandHandler(stringBuilder.ToString());
				}
				else
				{
					m_commandHandler(text);
				}
			}
		}
		if (TextLength > m_prompt.Length)
		{
			WritePrompt();
		}
		m_multiline = false;
	}

	private void SubmitCommand(string cmd)
	{
		string text = cmd.TrimEnd(' ');
		if (text.EndsWith(Environment.NewLine))
		{
			if (m_multiline)
			{
				ExecuteCommand(text);
			}
			else if (TextLength > m_prompt.Length)
			{
				WritePrompt();
			}
		}
		else if (IsCompoundStatement(text) || m_multiline)
		{
			m_multiline = true;
			WriteMultilinePrompt();
			string lastLine = text;
			int num = text.LastIndexOf(Environment.NewLine);
			if (num != -1)
			{
				lastLine = text.Substring(num);
				lastLine = lastLine.TrimStart('\r', '\n');
			}
			Indent(lastLine);
		}
		else
		{
			ExecuteCommand(text);
		}
	}

	private bool Suggest()
	{
		m_suggestionListBox.Items.Clear();
		if (IsCaretAtWritablePosition)
		{
			string currentLine = GetCurrentLine();
			int caretOffsetFromCurrentLine = CaretOffsetFromCurrentLine;
			int num = ((caretOffsetFromCurrentLine > 0) ? currentLine.LastIndexOfAny(s_triggers, caretOffsetFromCurrentLine - 1) : (-1));
			int num2;
			string partial;
			IEnumerable<string> source;
			if (num > -1)
			{
				num2 = num;
				partial = currentLine.Substring(num + 1, caretOffsetFromCurrentLine - num - 1);
				int num3 = currentLine.LastIndexOfAny(s_delimiters, num) + 1;
				string text = currentLine.Substring(num3, num - num3);
				if (string.IsNullOrWhiteSpace(text))
				{
					m_suggestionListBox.Hide();
					return false;
				}
				string text2 = currentLine[num].ToString();
				string quoteType = string.Empty;
				if (text2 == "[")
				{
					quoteType = (partial.StartsWith("'''") ? "'''" : (partial.StartsWith("\"\"\"") ? "\"\"\"" : (partial.StartsWith("'") ? "'" : "\"")));
				}
				source = from suggestion in m_suggestionHandler(text, text2)
					select string.Format("{0}{1}{0}", quoteType, StringUtil.EscapeQuotes(suggestion));
			}
			else
			{
				num2 = ((caretOffsetFromCurrentLine > 0) ? currentLine.LastIndexOf(' ', caretOffsetFromCurrentLine - 1) : (-1));
				partial = currentLine.Substring(num2 + 1, caretOffsetFromCurrentLine - num2 - 1);
				source = m_suggestionHandler(string.Empty, string.Empty);
			}
			IEnumerable<string> suggestions = source.Where((string suggestion) => suggestion.StartsWith(partial, StringComparison.InvariantCultureIgnoreCase)).Distinct();
			m_suggestionListBox.SetSuggestions(partial, suggestions);
			int count = m_suggestionListBox.Items.Count;
			if (count > 0)
			{
				int lineFromCharIndex = GetLineFromCharIndex(CaretIndex);
				int index = GetFirstCharIndexFromLine(lineFromCharIndex) + m_prompt.Length + num2;
				Point positionFromCharIndex = GetPositionFromCharIndex(index);
				int num4 = Math.Min(count, 8);
				int itemHeight = m_suggestionListBox.ItemHeight;
				int num5 = base.Height - itemHeight;
				if (positionFromCharIndex.Y < num5 - 3)
				{
					int val = Math.Max(positionFromCharIndex.Y - 8, num5 - positionFromCharIndex.Y);
					val = Math.Min(val, itemHeight * num4 + 4);
					int num6 = ((positionFromCharIndex.Y >= val) ? (positionFromCharIndex.Y - val) : (positionFromCharIndex.Y + base.FontHeight));
					int num7 = 0;
					foreach (object item in m_suggestionListBox.Items)
					{
						int num8 = TextRenderer.MeasureText(item.ToString(), m_suggestionListBox.Font).Width;
						if (num8 > num7)
						{
							num7 = num8;
						}
					}
					m_suggestionListBox.Bounds = new Rectangle(positionFromCharIndex.X + 3, num6, num7 + 25, val);
					m_suggestionListBox.SelectedIndex = 0;
					m_suggestionListBox.Show();
					m_suggestionListBox.Focus();
				}
				return true;
			}
		}
		m_suggestionListBox.Hide();
		return false;
	}

	private void WriteMultilinePrompt()
	{
		if (TextLength > 0 && Text[TextLength - 1] != '\n')
		{
			AppendText(Environment.NewLine);
		}
		AppendText(m_multilinePrompt);
		MoveCaretToEnd();
	}

	private void WritePrompt()
	{
		if (TextLength > 0 && Text[TextLength - 1] != '\n')
		{
			AppendText(Environment.NewLine);
		}
		AppendText(m_prompt);
		MoveCaretToEnd();
		m_promptIndex = TextLength;
	}

	private void MoveCaretToEnd()
	{
		base.SelectionStart = TextLength;
		ScrollToCaret();
	}

	private void MoveCaretToPrompt()
	{
		MoveLeft((CaretOffsetFromPrompt < 0) ? CaretOffsetFromPrompt : CaretOffsetFromCurrentLine);
	}

	private void MoveCaretToWritablePosition()
	{
		if (CaretOffsetFromPrompt < 0)
		{
			MoveLeft(CaretOffsetFromPrompt);
		}
		else if (CaretOffsetFromCurrentLine < 0)
		{
			MoveLeft(CaretOffsetFromCurrentLine);
		}
	}

	private string GetSelectedText()
	{
		string text = Text.Substring(base.SelectionStart, SelectionLength);
		if (text.Contains(Environment.NewLine))
		{
			text = text.Replace(Environment.NewLine + m_prompt, Environment.NewLine);
			text = text.Replace(Environment.NewLine + m_multilinePrompt, Environment.NewLine);
		}
		return text;
	}

	private string GetTextAtPrompt()
	{
		string text = Text.Substring(m_promptIndex, TextLength - m_promptIndex);
		if (text.Contains(Environment.NewLine))
		{
			text = text.Replace(Environment.NewLine + m_multilinePrompt, Environment.NewLine);
		}
		return text;
	}

	private void Indent(string lastLine)
	{
		MoveCaretToPrompt();
		string newValue = new string(' ', SpacesPerIndent);
		int num = lastLine.Replace("\t", newValue).TakeWhile(char.IsWhiteSpace).Count();
		if (IsCompoundStatement(lastLine))
		{
			num += SpacesPerIndent;
		}
		string text = new string(' ', num);
		InsertTextAtCaret(text);
	}

	private void AdjustSelection(int count)
	{
		if (count == 0)
		{
			return;
		}
		if ((SelectionLength == 0 && count < 0) || CaretIndex == base.SelectionStart)
		{
			int num = SelectionLength - count;
			if (num < 0)
			{
				base.SelectionStart += SelectionLength;
			}
			else
			{
				base.SelectionStart += count;
			}
			SelectionLength = Math.Abs(num);
			Point point = ((num < 0) ? GetPositionFromCharIndex(base.SelectionStart + SelectionLength) : GetPositionFromCharIndex(base.SelectionStart));
			User32.SetCaretPos(point.X, point.Y);
			return;
		}
		int num2 = SelectionLength + count;
		if (num2 < 0)
		{
			base.SelectionStart += num2;
		}
		SelectionLength = Math.Abs(num2);
		if (num2 < 0)
		{
			Point positionFromCharIndex = GetPositionFromCharIndex(base.SelectionStart);
			User32.SetCaretPos(positionFromCharIndex.X, positionFromCharIndex.Y);
		}
	}

	private string GetCurrentLine()
	{
		int lineFromCharIndex = GetLineFromCharIndex(CaretIndex);
		int num = GetFirstCharIndexFromLine(lineFromCharIndex) + m_prompt.Length;
		int num2 = Text.IndexOf(Environment.NewLine, num);
		int length = ((num2 < 0) ? (TextLength - num) : (num2 - num));
		return Text.Substring(num, length);
	}

	private static bool IsCompoundStatement(string text)
	{
		string text2 = text.Trim();
		if (text2.StartsWith("@"))
		{
			return true;
		}
		if ((text2.StartsWith("if ") || text2.StartsWith("while ") || text2.StartsWith("for ") || text2.StartsWith("try ") || text2.StartsWith("with ") || text2.StartsWith("def ") || text2.StartsWith("class ")) && text2.EndsWith(":"))
		{
			return true;
		}
		return false;
	}

	private void MoveCaret(int count)
	{
		if ((Control.ModifierKeys & Keys.Shift) == 0)
		{
			int num = base.SelectionStart;
			if (CaretIndex > num)
			{
				num += SelectionLength;
			}
			num += count;
			SelectionLength = 0;
			base.SelectionStart = num;
		}
		else
		{
			AdjustSelection(count);
		}
	}

	private void MoveLeft(int count)
	{
		MoveCaret(-count);
	}

	private void MoveRight(int count)
	{
		MoveCaret(count);
	}

	private void Next(Action<int> action, int count)
	{
		if (CaretOffsetFromPrompt < 0)
		{
			action(-CaretOffsetFromPrompt);
		}
		else if (IsCaretAtEnd && base.SelectionStart != TextLength)
		{
			action(m_multilinePrompt.Length + 2);
		}
		else if (CaretOffsetFromCurrentLine < 0)
		{
			action(-CaretOffsetFromCurrentLine);
		}
		else
		{
			action(count);
		}
	}

	private void NextChar(Action<int> action)
	{
		Next(action, 1);
	}

	private void NextWord(Action<int> action)
	{
		int count = 0;
		if (IsCaretAtWritablePosition && !IsCaretAtEnd)
		{
			string input = GetCurrentLine().Substring(CaretOffsetFromCurrentLine);
			MatchCollection matchCollection = s_nextWord.Matches(input);
			if (matchCollection.Count > 0)
			{
				Match match = matchCollection[0];
				count = match.Length;
			}
		}
		Next(action, count);
	}

	private void Prev(Action<int> action, int count)
	{
		if (CaretOffsetFromPrompt <= 0)
		{
			action(CaretOffsetFromPrompt);
		}
		else if (CaretOffsetFromCurrentLine > 0)
		{
			action(count);
		}
		else
		{
			action(m_multilinePrompt.Length + CaretOffsetFromCurrentLine + 2);
		}
	}

	private void PrevChar(Action<int> action)
	{
		Prev(action, 1);
	}

	private void PrevWord(Action<int> action)
	{
		int count = 0;
		if (CaretOffsetFromPrompt > 0 && CaretOffsetFromCurrentLine > 0)
		{
			string input = GetCurrentLine().Substring(0, CaretOffsetFromCurrentLine);
			MatchCollection matchCollection = s_nextWord.Matches(input);
			if (matchCollection.Count > 0)
			{
				Match match = matchCollection[matchCollection.Count - 1];
				count = match.Length;
			}
		}
		Prev(action, count);
	}

	private static void DefaultCommandHandler(string cmd)
	{
	}

	private static IEnumerable<string> DefaultSuggestionHandler(string obj, string trigger)
	{
		yield break;
	}

	private void InsertTextAtCaret(string text)
	{
		RemoveSelection();
		if (text.Contains(Environment.NewLine))
		{
			text = text.Replace(Environment.NewLine, Environment.NewLine + m_multilinePrompt);
			m_multiline = true;
		}
		int num = Math.Max(CaretIndex, m_promptIndex);
		Text = Text.Insert(num, text);
		base.SelectionStart = num + text.Length;
		ScrollToCaret();
	}

	private void RemoveSelection()
	{
		if (SelectionLength > 0)
		{
			RemoveText(base.SelectionStart, SelectionLength);
		}
	}

	private void RemoveText(int startIndex, int count)
	{
		int num = MathUtil.Clamp(startIndex, m_promptIndex, TextLength);
		int num2 = MathUtil.Clamp(startIndex + count, m_promptIndex, TextLength);
		int num3 = num2 - num;
		if (num3 > 0)
		{
			Text = Text.Remove(num, num3);
		}
		else
		{
			SelectionLength = 0;
		}
		base.SelectionStart = num;
		string textAtPrompt = GetTextAtPrompt();
		m_multiline = textAtPrompt.Contains(Environment.NewLine);
		ScrollToCaret();
	}

	private void RemoveTextAfterCaret(int count)
	{
		RemoveText(CaretIndex, count);
	}

	private void RemoveTextBeforeCaret(int count)
	{
		RemoveText(CaretIndex - count, count);
	}

	private void ReplaceTextAtPrompt(string text)
	{
		m_multiline = text.Contains(Environment.NewLine);
		if (m_multiline)
		{
			text = text.Replace(Environment.NewLine, Environment.NewLine + m_multilinePrompt);
		}
		Select(m_promptIndex, TextLength - m_promptIndex);
		SelectedText = text;
	}
}
