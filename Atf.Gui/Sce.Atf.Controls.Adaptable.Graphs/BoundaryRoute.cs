namespace Sce.Atf.Controls.Adaptable.Graphs;

public class BoundaryRoute : IEdgeRoute
{
	private float m_position;

	public float Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	bool IEdgeRoute.AllowFanIn => true;

	bool IEdgeRoute.AllowFanOut => true;

	public BoundaryRoute()
	{
	}

	public BoundaryRoute(float position)
	{
		m_position = position;
	}
}
