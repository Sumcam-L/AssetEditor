using System;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class StandardCommandTag<E, T> where E : struct where T : DomNodeAdapter
{
	public readonly E Command;

	private Action<T> m_doAction;

	private Func<T, bool> m_canDoFunction;

	private Action<T, CommandState> m_updateAction;

	public StandardCommandTag(E command, Action<T> doAction, Func<T, bool> canDoFunc, Action<T, CommandState> upAct)
	{
		Command = command;
		m_doAction = doAction;
		m_canDoFunction = canDoFunc;
		m_updateAction = upAct;
	}

	public bool CanDoCommand(T adapter)
	{
		return m_canDoFunction(adapter);
	}

	public void DoCommand(T adapter)
	{
		m_doAction(adapter);
	}

	public void UpdateCommand(T adapter, CommandState commandState)
	{
		m_updateAction(adapter, commandState);
	}
}
