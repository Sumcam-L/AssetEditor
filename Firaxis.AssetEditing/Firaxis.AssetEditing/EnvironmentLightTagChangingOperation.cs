using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EnvironmentLightTagChangingOperation : TransactionContext.Operation
{
	private EnvironmentLightContext m_context;

	private IAssetPreviewer m_previewerService;

	private IEntityChangeList m_redoChangelist;

	private string m_redoXML;

	private IEntityChangeList m_undoChangelist;

	private string m_undoXML;

	public EnvironmentLightTagChangingOperation(EnvironmentLightContext context, IEnvironmentLightDirectionTag lightTag, Action doAction)
	{
		m_context = context;
		_ = lightTag.Name;
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		m_previewerService = context.DomNode.As<InstanceEntityAdapter>().PreviewerService;
		m_undoChangelist = civTechContext.CreateInstance<IEntityChangeList>();
		m_redoChangelist = civTechContext.CreateInstance<IEntityChangeList>();
		m_undoXML = context.EnvironmentLight.SerializeIntoXML();
		m_undoChangelist.CreateLightDirectionTagChangedEvent(m_context.EnvironmentLight, lightTag);
		doAction();
		m_redoXML = context.EnvironmentLight.SerializeIntoXML();
		m_redoChangelist.CreateLightDirectionTagChangedEvent(m_context.EnvironmentLight, lightTag);
	}

	public override void Do()
	{
		m_context.EnvironmentLight.DeserializeFromXML(m_redoXML);
	}

	public override void Undo()
	{
		m_context.EnvironmentLight.DeserializeFromXML(m_undoXML);
	}
}
