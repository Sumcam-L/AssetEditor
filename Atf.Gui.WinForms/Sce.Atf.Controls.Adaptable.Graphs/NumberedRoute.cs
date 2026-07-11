namespace Sce.Atf.Controls.Adaptable.Graphs;

public class NumberedRoute : IEdgeRoute
{
	private int m_index;

	public int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public virtual bool AllowFanIn => true;

	public virtual bool AllowFanOut => true;
}
