using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public class RenderStateStack
{
	private readonly List<RenderState> m_contents = new List<RenderState>();

	private RenderState m_composedRenderState;

	public int Count => m_contents.Count;

	public RenderState ComposedRenderState
	{
		get
		{
			if (m_composedRenderState == null)
			{
				return null;
			}
			return m_composedRenderState.Clone() as RenderState;
		}
	}

	public void Push(RenderState renderState)
	{
		m_contents.Add(renderState);
		ComputeComposedRenderState();
	}

	public void Pop()
	{
		m_contents.RemoveAt(m_contents.Count - 1);
		ComputeComposedRenderState();
	}

	public RenderState Peek()
	{
		if (m_contents.Count == 0)
		{
			return null;
		}
		RenderState renderState = m_contents[m_contents.Count - 1];
		return renderState.Clone() as RenderState;
	}

	public void Remove(RenderState renderState)
	{
		m_contents.Remove(renderState);
		ComputeComposedRenderState();
	}

	public bool Contains(RenderState renderState)
	{
		return m_contents.Contains(renderState);
	}

	private void ComputeComposedRenderState()
	{
		if (m_contents.Count == 0)
		{
			m_composedRenderState = null;
			return;
		}
		int num = m_contents.Count - 1;
		m_composedRenderState = m_contents[num].Clone() as RenderState;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			m_composedRenderState.ComposeFrom(m_contents[num2]);
		}
	}
}
