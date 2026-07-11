using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Interop;

internal class CommandClientAdapter : ICommandClient
{
	private HashSet<ICommandItem> m_commands = new HashSet<ICommandItem>();

	private ICommandClient m_adaptee;

	public CommandClientAdapter(ICommandClient adaptee)
	{
		m_adaptee = adaptee;
	}

	public void AddCommand(ICommandItem command)
	{
		lock (m_commands)
		{
			m_commands.Add(command);
		}
	}

	public ICommandItem RemoveCommand(object commandTag)
	{
		ICommandItem commandItem = m_commands.FirstOrDefault((ICommandItem x) => CommandComparer.TagsEqual(x.CommandTag, commandTag));
		if (commandItem != null)
		{
			lock (m_commands)
			{
				m_commands.Remove(commandItem);
			}
		}
		return commandItem;
	}

	public void UpdateCommands()
	{
		ICommandItem[] array;
		lock (m_commands)
		{
			array = m_commands.ToArray();
		}
		ICommandItem[] array2 = array;
		foreach (ICommandItem commandItem in array2)
		{
			CommandState commandState = new CommandState(commandItem.Text, commandItem.IsChecked);
			m_adaptee.UpdateCommand(commandItem.CommandTag, commandState);
			if (commandState.Text != commandItem.Text)
			{
				commandItem.Text = commandState.Text;
			}
			if (commandState.Check != commandItem.IsChecked)
			{
				commandItem.IsChecked = commandState.Check;
			}
		}
	}

	public bool CanDoCommand(object command)
	{
		ICommandItem commandItem = command as ICommandItem;
		Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
		return m_adaptee.CanDoCommand(commandItem.CommandTag);
	}

	public void DoCommand(object command)
	{
		ICommandItem commandItem = command as ICommandItem;
		Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
		m_adaptee.DoCommand(commandItem.CommandTag);
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
		throw new InvalidOperationException("CommandClientAdapter.UpdateCommand() - WPF shouldn't ever be calling this method, and suggests a non-WPF app is erroneously using CommandClientAdapter.");
	}
}
