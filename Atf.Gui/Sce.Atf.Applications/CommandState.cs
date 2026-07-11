namespace Sce.Atf.Applications;

public class CommandState
{
	private string m_text = "";

	private bool m_check;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	public bool Check
	{
		get
		{
			return m_check;
		}
		set
		{
			m_check = value;
		}
	}

	public CommandState()
	{
	}

	public CommandState(string text, bool check)
	{
		m_text = text;
		m_check = check;
	}
}
