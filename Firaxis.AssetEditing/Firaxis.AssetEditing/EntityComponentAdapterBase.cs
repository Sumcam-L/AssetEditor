using System;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EntityComponentAdapterBase : DomNodeAdapter
{
	public InstanceEntityAdapter EntityAdapter { get; set; }

	protected IEntityChangeList BatchChangelist
	{
		get
		{
			BugSubmitter.SilentAssert(base.DomNode.Parent != null, "Attempting to access the root when the parent is not set.  @assign bwhitman");
			return base.DomNode.GetRoot().As<IEntityPreviewComponent>()?.EntityChanges;
		}
	}

	protected ITunerQueueService TunerQueueService
	{
		get
		{
			BugSubmitter.SilentAssert(base.DomNode.Parent != null, "Attempting to access the root when the parent is not set.  @assign bwhitman");
			return base.DomNode.GetRoot().As<BaseEntityPropertyContext>()?.TunerQueueService;
		}
	}

	protected bool HotLoadOnReimport
	{
		get
		{
			BugSubmitter.SilentAssert(base.DomNode.Parent != null, "Attempting to access the root when the parent is not set.  @assign bwhitman");
			return base.DomNode.GetRoot().As<BaseEntityPropertyContext>()?.HotLoadOnReimport ?? true;
		}
	}

	public override object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(typeof(ITransactionContext)))
		{
			return GetTransactionContext();
		}
		return base.GetAdapter(type);
	}

	protected TransactionContext GetTransactionContext()
	{
		return EntityAdapter.As<TransactionContext>();
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		EntityAdapter = base.DomNode.GetRoot().As<InstanceEntityAdapter>();
		if (EntityAdapter == null)
		{
			EntityComponentAdapterBase entityComponentAdapterBase = base.DomNode.GetRoot().As<EntityComponentAdapterBase>();
			if (entityComponentAdapterBase != null)
			{
				EntityAdapter = entityComponentAdapterBase.EntityAdapter;
			}
		}
		BugSubmitter.SilentAssert(EntityAdapter != null, "Entity adapter not set in EntityComponentAdapterBase.OnParentNodeSet  @assign bwhitman");
	}
}
