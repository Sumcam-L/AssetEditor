using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public class TraverseNode
{
	private IRenderObject m_renderObject;

	private Matrix4F m_transform;

	private SceneNode[] m_graphPath;

	private RenderState m_renderState = new RenderState();

	private Box m_worldSpaceBoundingBox;

	public IRenderObject RenderObject
	{
		get
		{
			return m_renderObject;
		}
		set
		{
			m_renderObject = value;
		}
	}

	public Matrix4F Transform
	{
		get
		{
			return m_transform;
		}
		set
		{
			m_transform = value;
		}
	}

	public SceneNode[] GraphPath
	{
		get
		{
			return m_graphPath;
		}
		set
		{
			m_graphPath = value;
		}
	}

	public RenderState RenderState
	{
		get
		{
			return m_renderState;
		}
		set
		{
			m_renderState = value;
		}
	}

	public Box WorldSpaceBoundingBox
	{
		get
		{
			return m_worldSpaceBoundingBox;
		}
		set
		{
			m_worldSpaceBoundingBox = value;
		}
	}

	public TraverseNode()
	{
	}

	public TraverseNode(IRenderObject renderObject, Matrix4F transform, Stack<SceneNode> graphPath, RenderState renderState)
	{
		Init(renderObject, transform, graphPath, renderState);
	}

	public void Init(IRenderObject renderObject, Matrix4F transform, Stack<SceneNode> graphPath, RenderState renderState)
	{
		m_renderObject = renderObject;
		m_transform = transform;
		m_renderState.Init(renderState);
		m_graphPath = graphPath.ToArray();
	}

	public void Reset()
	{
		m_renderObject = null;
		m_transform = null;
		m_graphPath = null;
	}
}
