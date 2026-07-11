using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class ListRemoveCommand<T> : Command
{
	private IList<T> m_list;

	private T m_element;

	private int m_index;

	public ListRemoveCommand(string commandName, IList<T> list, T element, int index)
		: base(commandName)
	{
		m_list = list;
		m_element = element;
		m_index = index;
	}

	public override void Do()
	{
		m_list.RemoveAt(m_index);
	}

	public override void Undo()
	{
		m_list.Insert(m_index, m_element);
	}
}
