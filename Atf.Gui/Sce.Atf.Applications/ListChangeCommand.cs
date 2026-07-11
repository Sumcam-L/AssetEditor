using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class ListChangeCommand<T> : Command
{
	private IList<T> m_list;

	private T m_oldElement;

	private T m_newElement;

	private int m_index;

	public ListChangeCommand(string commandName, IList<T> list, T oldElement, T newElement, int index)
		: base(commandName)
	{
		m_list = list;
		m_oldElement = oldElement;
		m_newElement = newElement;
		m_index = index;
	}

	public override void Do()
	{
		m_list[m_index] = m_newElement;
	}

	public override void Undo()
	{
		m_list[m_index] = m_oldElement;
	}
}
