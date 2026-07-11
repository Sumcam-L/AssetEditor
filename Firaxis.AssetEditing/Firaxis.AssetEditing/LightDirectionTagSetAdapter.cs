using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LightDirectionTagSetAdapter : EntityComponentAdapterBase
{
	public IList<LightDirectionTagAdapter> LightDirectionTags { get; private set; }

	public void Update()
	{
		UnregisterFromEvents();
		IList<LightDirectionTagAdapter> list = new List<LightDirectionTagAdapter>();
		foreach (IEnvironmentLightDirectionTag dirTag in (base.EntityAdapter.InstanceEntity as IEnvironmentLightInstance).DirectionTags)
		{
			LightDirectionTagAdapter lightDirectionTagAdapter = LightDirectionTags.FirstOrDefault((LightDirectionTagAdapter adp) => adp.Name == dirTag.Name);
			if (lightDirectionTagAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.LightDirectionTagType.Type);
				domNode.InitializeExtensions();
				lightDirectionTagAdapter = domNode.As<LightDirectionTagAdapter>();
				LightDirectionTags.Add(lightDirectionTagAdapter);
			}
			lightDirectionTagAdapter.Update(dirTag);
			list.Add(lightDirectionTagAdapter);
		}
		foreach (var entryAdapter in LightDirectionTags.Except(list).ToArray())
		{
			LightDirectionTags.Remove(entryAdapter);
		}
		RegisterForEvents();
	}

	protected override void OnNodeSet()
	{
		LightDirectionTags = new DomNodeListAdapter<LightDirectionTagAdapter>(base.DomNode, EntitySchema.LightDirectionTagSetType.LightDirectionTagsChild);
		RegisterForEvents();
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.LightDirectionTagSetType.LightDirectionTagsChild)
		{
			LightDirectionTagAdapter lightDirectionTagAdapter = e.Child.As<LightDirectionTagAdapter>();
			IEnvironmentLightInstance environmentLightInstance = base.EntityAdapter.InstanceEntity as IEnvironmentLightInstance;
			IEnvironmentLightDirectionTag environmentLightDirectionTag = environmentLightInstance.AddDirectionTag(lightDirectionTagAdapter.XPosition, lightDirectionTagAdapter.YPosition, lightDirectionTagAdapter.ZPosition);
			environmentLightDirectionTag.Name = lightDirectionTagAdapter.Name;
			environmentLightDirectionTag.SetIntensity(lightDirectionTagAdapter.Red, lightDirectionTagAdapter.Green, lightDirectionTagAdapter.Blue);
			environmentLightDirectionTag.CastsShadows = lightDirectionTagAdapter.CastsShadows;
			environmentLightDirectionTag.AngularFalloff = lightDirectionTagAdapter.AngularFalloff;
			environmentLightDirectionTag.Diameter = lightDirectionTagAdapter.Diameter;
			lightDirectionTagAdapter.Update(environmentLightDirectionTag);
			base.BatchChangelist?.CreateLightDirectionTagChangedEvent(environmentLightInstance, lightDirectionTagAdapter.DirectionTag);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.LightDirectionTagSetType.LightDirectionTagsChild)
		{
			LightDirectionTagAdapter lightDirectionTagAdapter = e.Child.As<LightDirectionTagAdapter>();
			IEnvironmentLightInstance environmentLightInstance = base.EntityAdapter.InstanceEntity as IEnvironmentLightInstance;
			base.BatchChangelist?.CreateLightTagDirectionRemovedEvent(environmentLightInstance, lightDirectionTagAdapter.DirectionTag.Name);
			environmentLightInstance.RemoveDirectionTag(lightDirectionTagAdapter.DirectionTag);
		}
	}

	private void RegisterForEvents()
	{
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterFromEvents()
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}
}
