using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class CompositeCommand : Command
{
	private readonly List<Command> m_commands;

	public CompositeCommand(IEnumerable<Command> commands)
		: this(null, commands)
	{
	}

	public CompositeCommand(string description, IEnumerable<Command> commands)
		: base(description)
	{
		m_commands = new List<Command>(commands);
	}

	public override void Do()
	{
		for (int i = 0; i < m_commands.Count; i++)
		{
			Command command = m_commands[i];
			bool flag = false;
			try
			{
				command.Do();
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					for (i--; i >= 0; i--)
					{
						Command command2 = m_commands[i];
						command2.Undo();
					}
				}
			}
		}
	}

	public override void Undo()
	{
		for (int num = m_commands.Count - 1; num >= 0; num--)
		{
			m_commands[num].Undo();
		}
	}

	public void Add(Command command)
	{
		if (command is CompositeCommand compositeCommand)
		{
			m_commands.AddRange(compositeCommand.m_commands);
		}
		else
		{
			m_commands.Add(command);
		}
	}

	public Command Optimize()
	{
		if (m_commands.Count > 1)
		{
			return this;
		}
		if (m_commands.Count == 0)
		{
			return null;
		}
		Command command = m_commands[0];
		command.Description = base.Description;
		return command;
	}
}
