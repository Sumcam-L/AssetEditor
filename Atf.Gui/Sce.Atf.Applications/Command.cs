namespace Sce.Atf.Applications;

public abstract class Command
{
	private string m_description;

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
		}
	}

	public Command()
		: this(string.Empty)
	{
	}

	public Command(string description)
	{
		m_description = description;
	}

	public abstract void Do();

	public abstract void Undo();
}
