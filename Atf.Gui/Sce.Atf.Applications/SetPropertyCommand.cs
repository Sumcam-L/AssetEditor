using System.ComponentModel;

namespace Sce.Atf.Applications;

public class SetPropertyCommand : Command
{
	private readonly object m_component;

	private readonly PropertyDescriptor m_descriptor;

	private readonly object m_oldValue;

	private readonly object m_newValue;

	public SetPropertyCommand(string commandName, object component, string propertyName, object oldValue, object newValue)
		: base(commandName)
	{
		m_component = component;
		m_descriptor = TypeDescriptor.GetProperties(component)[propertyName];
		m_oldValue = oldValue;
		m_newValue = newValue;
	}

	public SetPropertyCommand(string commandName, object component, PropertyDescriptor descriptor, object oldValue, object newValue)
		: base(commandName)
	{
		m_component = component;
		m_descriptor = descriptor;
		m_oldValue = oldValue;
		m_newValue = newValue;
	}

	public override void Do()
	{
		m_descriptor.SetValue(m_component, m_newValue);
	}

	public override void Undo()
	{
		m_descriptor.SetValue(m_component, m_oldValue);
	}
}
