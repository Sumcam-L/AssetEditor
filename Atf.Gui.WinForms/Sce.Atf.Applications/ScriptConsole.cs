using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Controls.ConsoleBox;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(ScriptConsole))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ScriptConsole : IInitializable, IControlHostClient
{
	[Import(AllowDefault = true)]
	private IControlHostService m_controlHostService;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptService;

	private IConsoleTextBox m_consoleBox;

	private readonly Dictionary<Tuple<Type, string>, Func<IEnumerable<string>>> m_registeredSuggestors = new Dictionary<Tuple<Type, string>, Func<IEnumerable<string>>>();

	public Control Control => m_consoleBox.Control;

	public IConsoleTextBox ConsoleTextBox => m_consoleBox;

	[ImportingConstructor]
	public ScriptConsole()
	{
		m_consoleBox = new ConsoleTextBox();
		m_consoleBox.Control.Dock = DockStyle.Fill;
		m_consoleBox.CommandHandler = ProcessCommand;
		m_consoleBox.SuggestionHandler = Suggestions;
	}

	public ScriptConsole(ScriptingService scriptService, IControlHostService controlHostService)
		: this()
	{
		m_controlHostService = controlHostService;
		m_scriptService = scriptService;
	}

	public ScriptConsole(ScriptingService scriptService)
		: this()
	{
		m_scriptService = scriptService;
	}

	public void RegisterSuggestor(Type type, string trigger, Func<IEnumerable<string>> func)
	{
		m_registeredSuggestors.Add(new Tuple<Type, string>(type, trigger), func);
	}

	void IInitializable.Initialize()
	{
		if (m_scriptService != null && m_controlHostService != null)
		{
			m_controlHostService.RegisterControl(m_consoleBox.Control, m_scriptService.DisplayName, $"Interactive {m_scriptService.DisplayName} Console", StandardControlGroup.Bottom, null, this, "https://github.com/SonyWWS/ATF/wiki/Scripting-Applications-with-Python".Localize());
		}
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	private void ProcessCommand(string cmd)
	{
		if (!StringUtil.IsNullOrEmptyOrWhitespace(cmd))
		{
			cmd = cmd.Trim();
			if (cmd == "cls")
			{
				m_consoleBox.Clear();
			}
			else if (cmd.StartsWith("runfile "))
			{
				string text = cmd.Substring("runfile ".Length);
				text = text.Trim();
				string text2 = m_scriptService.ExecuteFile(text);
				m_consoleBox.Write(text2);
			}
			else
			{
				string text3 = m_scriptService.ExecuteStatement(cmd);
				m_consoleBox.Write(text3);
			}
			m_consoleBox.Control.Focus();
		}
	}

	private IEnumerable<string> Suggestions(string obj, string trigger)
	{
		IEnumerable<object> enumerable = null;
		try
		{
			enumerable = m_scriptService.ExecuteSilent($"dir({obj})") as IEnumerable<object>;
			if (!string.IsNullOrWhiteSpace(obj))
			{
				dynamic val = m_scriptService.ExecuteSilent($"{obj}.GetType()");
				if (m_registeredSuggestors.TryGetValue(new Tuple<Type, string>(val, trigger), out var value))
				{
					return value();
				}
			}
		}
		catch (Exception)
		{
		}
		return (enumerable != null) ? enumerable.Cast<string>() : Enumerable.Empty<string>();
	}
}
