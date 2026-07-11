using System;
using Firaxis.CivTech.TextureExport;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EnvironmentLightChangingOperation : TransactionContext.Operation
{
	private EnvironmentLightContext m_context;

	private ICubeMap m_redoCube;

	private ICubeMap m_undoCube;

	public EnvironmentLightChangingOperation(EnvironmentLightContext context, Action doAction)
	{
		m_context = context;
		m_undoCube = ((context.Cube != null) ? context.Cube.Clone() : null);
		doAction();
		m_redoCube = ((context.Cube != null) ? context.Cube.Clone() : null);
	}

	public override void Do()
	{
		m_context.Cube = m_redoCube.Clone();
		m_context.ApplyChanges();
	}

	public override void Undo()
	{
		m_context.Cube = m_undoCube.Clone();
		m_context.ApplyChanges();
	}
}
