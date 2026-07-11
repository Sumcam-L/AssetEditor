namespace Sce.Atf.Rendering;

public class ObjectStats
{
	private int m_nPrimitives;

	private int m_nVertices;

	public int PrimCount
	{
		get
		{
			return m_nPrimitives;
		}
		set
		{
			m_nPrimitives = value;
		}
	}

	public int VertexCount
	{
		get
		{
			return m_nVertices;
		}
		set
		{
			m_nVertices = value;
		}
	}
}
