using System;

namespace Sce.Atf.Rendering;

public class RenderStateGuardian
{
	public delegate void RenderStateSetHandler(RenderState newRenderState, RenderState oldRenderState);

	private readonly RenderStateSetHandler[] m_renderStateSetters = new RenderStateSetHandler[32];

	private bool m_reset = true;

	private readonly RenderState m_oldRenderState = new RenderState();

	public void Reset()
	{
		m_reset = true;
	}

	public void Clear()
	{
		for (int i = 0; i < 32; i++)
		{
			m_renderStateSetters[i] = null;
		}
	}

	public void Commit(RenderState renderState)
	{
		renderState.CommitAllBitsToGuardian(this);
		m_oldRenderState.Init(renderState);
		m_reset = false;
	}

	public RenderStateSetHandler TryGetHandler(int renderStateBit)
	{
		int num = MathUtil.LogBase2(renderStateBit);
		return m_renderStateSetters[num];
	}

	public void RegisterRenderStateHandler(int renderStateBit, RenderStateSetHandler handler)
	{
		if (!MathUtil.OnlyOneBitSet(renderStateBit))
		{
			throw new ArgumentException("RenderStateSetHandlers can only be set on keys with only one bit set.");
		}
		if (TryGetHandler(renderStateBit) != null)
		{
			throw new InvalidOperationException("Key " + renderStateBit + " is already associated with a handler.");
		}
		int num = MathUtil.LogBase2(renderStateBit);
		m_renderStateSetters[num] = handler;
	}

	public void SetRenderState(int renderStateBit, RenderState renderState)
	{
		TryGetHandler(renderStateBit)?.Invoke(renderState, (!m_reset) ? m_oldRenderState : null);
	}

	public void SetRenderStateByIndex(int renderStateIndex, RenderState renderState)
	{
		m_renderStateSetters[renderStateIndex]?.Invoke(renderState, (!m_reset) ? m_oldRenderState : null);
	}
}
