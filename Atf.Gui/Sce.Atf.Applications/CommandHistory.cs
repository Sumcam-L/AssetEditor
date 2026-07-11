using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class CommandHistory
{
	private readonly List<Command> m_commands;

	private readonly CommandCount m_commandCount;

	public int Current => m_commandCount.Current;

	public int Count => m_commands.Count;

	public Command this[int index] => m_commands[index];

	public bool CanUndo => m_commandCount.CanDecrement;

	public bool CanRedo => m_commandCount.Current < m_commands.Count;

	public bool Dirty
	{
		get
		{
			return m_commandCount.Dirty;
		}
		set
		{
			m_commandCount.Dirty = value;
		}
	}

	public string UndoDescription
	{
		get
		{
			string result = "";
			if (CanUndo)
			{
				Command command = m_commands[m_commandCount.Current - 1];
				result = command.Description;
			}
			return result;
		}
	}

	public string RedoDescription
	{
		get
		{
			string result = "";
			if (CanRedo)
			{
				Command command = m_commands[m_commandCount.Current];
				result = command.Description;
			}
			return result;
		}
	}

	public Command LastDone
	{
		get
		{
			int num = m_commandCount.Current - 1;
			return (num >= 0) ? m_commands[num] : null;
		}
	}

	private IEnumerable<Command> Commands => m_commands;

	public event EventHandler CommandDone;

	public event EventHandler CommandUndone;

	public event EventHandler DirtyChanged;

	public event EventHandler Cleared;

	public CommandHistory()
	{
		m_commands = new List<Command>();
		m_commandCount = new CommandCount();
		m_commandCount.DirtyChanged += commandCount_DirtyChanged;
	}

	public void Clear()
	{
		m_commandCount.Reset();
		m_commands.Clear();
		this.Cleared?.Invoke(this, EventArgs.Empty);
	}

	public void Add(Command command)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		ClearUndoneCommands();
		Command lastDone = LastDone;
		m_commands.Add(command);
		m_commandCount.Increment();
		OnCommandDone();
	}

	public Command Undo()
	{
		if (!CanUndo)
		{
			throw new InvalidOperationException("Can't undo");
		}
		Command command = m_commands[m_commandCount.Current - 1];
		m_commandCount.Undo();
		command.Undo();
		OnCommandUndone();
		return command;
	}

	public Command Redo()
	{
		if (!CanRedo)
		{
			throw new InvalidOperationException("Can't redo");
		}
		Command command = m_commands[m_commandCount.Current];
		m_commandCount.Redo();
		command.Do();
		OnCommandDone();
		return command;
	}

	public void Revert()
	{
		while (Dirty && CanUndo)
		{
			Undo();
		}
	}

	public Command[] Collapse()
	{
		ClearUndoneCommands();
		Command[] array = new Command[m_commands.Count];
		m_commands.CopyTo(array);
		Clear();
		return array;
	}

	private void ClearUndoneCommands()
	{
		int num = m_commandCount.Current - 1;
		int num2 = m_commands.Count - (num + 1);
		if (num2 > 0)
		{
			m_commands.RemoveRange(num + 1, num2);
		}
	}

	private void commandCount_DirtyChanged(object sender, EventArgs e)
	{
		this.DirtyChanged.Raise(this, e);
	}

	private void OnCommandDone()
	{
		this.CommandDone.Raise(this, EventArgs.Empty);
	}

	private void OnCommandUndone()
	{
		this.CommandUndone.Raise(this, EventArgs.Empty);
	}
}
