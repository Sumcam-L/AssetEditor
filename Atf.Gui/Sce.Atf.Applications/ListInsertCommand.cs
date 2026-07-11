using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class ListInsertCommand<T> : Command
{
	private IList<T> m_list;

	private T m_element;

	private int m_index;

	public ListInsertCommand(string commandName, IList<T> list, T element, int index)
		: base(commandName)
	{
		m_list = list;
		m_element = element;
		m_index = index;
	}

	public override void Do()
	{
		m_list.Insert(m_index, m_element);
	}

	public override void Undo()
	{
		m_list.RemoveAt(m_index);
	}
}
