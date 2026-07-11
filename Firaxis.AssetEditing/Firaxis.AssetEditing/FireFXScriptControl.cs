using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.FireFX;
using Firaxis.Collections;
using Firaxis.Utility;
using ScintillaNET;

namespace Firaxis.AssetEditing;

public class FireFXScriptControl : UserControl
{
	private const int kMarginLineNumbers = 0;

	private const int kMarginSymbols = 1;

	private const int kGutterBookmark = 0;

	private const int kGutterWarning = 1;

	private const int kGutterError = 2;

	private const int kLanguageKeywords = 0;

	private const int kTypeKeywords = 1;

	private const int kIntrinsicFunctions = 3;

	private const int kPreprocessorKeywords = 4;

	private FindForm findForm = new FindForm();

	private string quickSearchText = string.Empty;

	private Scintilla m_editor = new Scintilla();

	private IList<CompileIssue> m_scriptIssues = new List<CompileIssue>();

	private IFireFXScriptResource m_document;

	public string ScriptText
	{
		get
		{
			return m_editor.Text;
		}
		set
		{
			if (m_editor.Text != value)
			{
				m_editor.Text = value;
			}
		}
	}

	public IEnumerable<CompileIssue> ScriptIssues
	{
		get
		{
			return m_scriptIssues;
		}
		set
		{
			ClearIssues();
			AddIssues(value);
		}
	}

	public FireFXScriptControl()
	{
		m_editor.Dock = DockStyle.Fill;
		m_editor.HandleCreated += Editor_HandleCreated;
		m_editor.TextChanged += Editor_TextChanged;
		m_editor.KeyUp += Editor_KeyUp;
		ConfigureScintilla();
		base.Controls.Add(m_editor);
	}

	public FireFXScriptControl(IFireFXScriptResource scriptDoc)
		: this()
	{
		m_document = scriptDoc;
	}

	public void Bind(IFireFXScriptResource scriptDoc)
	{
		if (m_document != scriptDoc)
		{
			ClearIssues();
			if (m_document != null)
			{
				m_document.TextChanged -= Document_TextChanged;
			}
			m_document = scriptDoc;
			if (m_document != null)
			{
				m_document.TextChanged += Document_TextChanged;
			}
			UpdateEditorText();
		}
	}

	private void Document_TextChanged(object sender, EventArgs e)
	{
		UpdateEditorText();
	}

	private void Editor_HandleCreated(object sender, EventArgs e)
	{
		ConfigureScintilla();
		UpdateEditorText();
	}

	private void Editor_TextChanged(object sender, EventArgs e)
	{
		if (m_document != null)
		{
			m_document.Text = m_editor.Text;
		}
	}

	private void ConfigureScintilla()
	{
		m_editor.StyleResetDefault();
		m_editor.Styles[32].Font = "Consolas";
		m_editor.Styles[32].Size = 10;
		m_editor.StyleClearAll();
		m_editor.Styles[0].ForeColor = Color.Silver;
		m_editor.Styles[1].ForeColor = Color.FromArgb(0, 128, 0);
		m_editor.Styles[2].ForeColor = Color.FromArgb(0, 128, 0);
		m_editor.Styles[15].ForeColor = Color.FromArgb(128, 128, 128);
		m_editor.Styles[4].ForeColor = Color.Olive;
		m_editor.Styles[5].ForeColor = Color.Blue;
		m_editor.Styles[16].ForeColor = Color.Blue;
		m_editor.Styles[6].ForeColor = Color.FromArgb(163, 21, 21);
		m_editor.Styles[7].ForeColor = Color.FromArgb(163, 21, 21);
		m_editor.Styles[13].ForeColor = Color.FromArgb(163, 21, 21);
		m_editor.Styles[12].BackColor = Color.Pink;
		m_editor.Styles[10].ForeColor = Color.Purple;
		m_editor.Styles[9].ForeColor = Color.Gray;
		m_editor.Styles[19].ForeColor = Color.Maroon;
		m_editor.Lexer = Lexer.Cpp;
		m_editor.AssignCmdKey(Keys.Tab, Command.Tab);
		m_editor.AssignCmdKey(Keys.Tab | Keys.Shift, Command.BackTab);
		m_editor.AssignCmdKey(Keys.Tab | Keys.Control, Command.Cancel);
		m_editor.AssignCmdKey(Keys.F | Keys.Control, Command.Cancel);
		m_editor.AssignCmdKey(Keys.F | Keys.Shift | Keys.Control, Command.Cancel);
		m_editor.AssignCmdKey(Keys.F | Keys.Control | Keys.Alt, Command.Cancel);
		m_editor.AssignCmdKey(Keys.F | Keys.Shift | Keys.Control | Keys.Alt, Command.Cancel);
		m_editor.AssignCmdKey(Keys.F3 | Keys.Control, Command.Cancel);
		m_editor.Margins[0].Width = 24;
		Margin margin = m_editor.Margins[1];
		margin.Width = 16;
		margin.Type = MarginType.Symbol;
		margin.Mask = uint.MaxValue;
		margin.Cursor = MarginCursor.ReverseArrow;
		Marker marker = m_editor.Markers[1];
		marker.Symbol = MarkerSymbol.CircleMinus;
		marker.SetBackColor(Color.Yellow);
		Marker marker2 = m_editor.Markers[2];
		marker2.Symbol = MarkerSymbol.CircleMinus;
		marker2.SetBackColor(Color.Red);
		m_editor.SetKeywords(0, "emitter group local property const rtconst varying return root_emitter");
		m_editor.SetKeywords(1, "number point2d point3d color string spline vec4 float float2 float3 float4");
		m_editor.SetKeywords(3, "emit_rate emit_count export kill SPAWN RENDER SIM mix import global_import instance_import");
		m_editor.SetKeywords(4, "if ifdef ifndef else elif endif error pragma include define undef line");
	}

	private void FindText(string textToFind)
	{
		m_editor.TargetStart = m_editor.CurrentPosition + textToFind.Length;
		m_editor.TargetEnd = m_editor.Text.Length;
		if (m_editor.SearchInTarget(textToFind) >= 0)
		{
			quickSearchText = textToFind;
			Scintilla editor = m_editor;
			int num = (m_editor.CurrentPosition = m_editor.TargetStart);
			int selectionStart = num;
			editor.SelectionStart = selectionStart;
			m_editor.SelectionEnd = m_editor.TargetEnd;
			m_editor.ScrollCaret();
			return;
		}
		m_editor.TargetStart = 0;
		m_editor.TargetEnd = m_editor.CurrentPosition;
		if (m_editor.SearchInTarget(textToFind) >= 0)
		{
			quickSearchText = textToFind;
			Scintilla editor2 = m_editor;
			int num = (m_editor.CurrentPosition = m_editor.TargetStart);
			int selectionStart2 = num;
			editor2.SelectionStart = selectionStart2;
			m_editor.SelectionEnd = m_editor.TargetEnd;
			m_editor.ScrollCaret();
		}
		else
		{
			NativeMethods.MessageBeep(NativeMethods.BeepType.Exclamation);
		}
	}

	private void FindAgain()
	{
		if (quickSearchText.Length != 0)
		{
			m_editor.TargetStart = m_editor.CurrentPosition + quickSearchText.Length;
			m_editor.TargetEnd = m_editor.Text.Length;
			if (m_editor.SearchInTarget(quickSearchText) >= 0)
			{
				Scintilla editor = m_editor;
				int num = (m_editor.CurrentPosition = m_editor.TargetStart);
				int selectionStart = num;
				editor.SelectionStart = selectionStart;
				m_editor.SelectionEnd = m_editor.TargetEnd;
				m_editor.ScrollCaret();
			}
			else
			{
				NativeMethods.MessageBeep(NativeMethods.BeepType.Exclamation);
			}
		}
	}

	private void Editor_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Control)
		{
			if (e.KeyCode == Keys.F)
			{
				if (!string.IsNullOrEmpty(m_editor.SelectedText))
				{
					findForm.FindWhat = m_editor.SelectedText;
				}
				else if (!string.IsNullOrEmpty(quickSearchText))
				{
					findForm.FindWhat = quickSearchText;
				}
				if (findForm.ShowDialog() == DialogResult.OK)
				{
					FindText(findForm.FindWhat);
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.F3)
			{
				FindText(m_editor.SelectedText);
				e.Handled = true;
			}
		}
		else if (!string.IsNullOrEmpty(quickSearchText) && e.KeyCode == Keys.F3)
		{
			FindAgain();
			e.Handled = true;
		}
	}

	private void UpdateEditorText()
	{
		int selectionStart = m_editor.SelectionStart;
		int selectionEnd = m_editor.SelectionEnd;
		int firstVisibleLine = m_editor.FirstVisibleLine;
		int currentPosition = m_editor.CurrentPosition;
		string text = m_document?.Text ?? string.Empty;
		int length = text.Length;
		m_editor.Text = text;
		m_editor.EmptyUndoBuffer();
		m_editor.FirstVisibleLine = Math.Min(firstVisibleLine, length);
		m_editor.CurrentPosition = Math.Min(currentPosition, length);
		m_editor.SelectionStart = Math.Min(selectionStart, length);
		m_editor.SelectionEnd = Math.Min(selectionEnd, length);
	}

	private void ClearIssues()
	{
		m_scriptIssues.Clear();
		m_editor.Lines.ForEach(delegate(Line line)
		{
			RemoveIssueMarkers(line);
		});
	}

	private void RemoveIssueMarkers(Line line)
	{
		uint num = line.MarkerGet();
		if ((num & 2) == 2)
		{
			line.MarkerDelete(1);
		}
		if ((num & 4) == 4)
		{
			line.MarkerDelete(2);
		}
	}

	private void AddIssues(IEnumerable<CompileIssue> issues)
	{
		issues.ForEach(delegate(CompileIssue issue)
		{
			AddIssueMarker(issue);
		});
	}

	private void AddIssueMarker(CompileIssue issue)
	{
		m_scriptIssues.Add(issue);
		Line line = m_editor.Lines[(int)issue.LineNo];
		if (issue.Type == CompileIssueType.Error)
		{
			line.MarkerAdd(2);
		}
		else if (issue.Type == CompileIssueType.Warning)
		{
			line.MarkerAdd(1);
		}
	}
}
