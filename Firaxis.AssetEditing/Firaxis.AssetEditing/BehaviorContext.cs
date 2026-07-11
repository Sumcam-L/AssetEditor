using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BehaviorContext : BaseEntityPropertyContext, IBehaviorEditorContext, IEntityEditorContext, IObservableContext, IAssetBrowserServiceProvider, ITriggerInstancingContext, IInstancingContext
{
	public BehaviorEditor BehaviorEditor { get; set; }

	public IAssetBrowserDialogService AssetBrowserService { get; set; }

	public IBehaviorInstance BehaviorInstance => base.DomNode.As<BaseInstanceEntityDocument>().InstanceEntity as IBehaviorInstance;

	public IPropertyEditingListContext AttachmentsContext => base.DomNode.As<BehaviorAdapter>().AttachmentPointSet;

	public IPropertyEditingListContext AnimationSetContext => base.DomNode.As<BehaviorAdapter>().AnimationBindingSet;

	public IPropertyEditingListContext GeometryReferenceSetContext => base.DomNode.As<BehaviorAdapter>().GeometryReferenceSet;

	public IPropertyEditingListContext BehaviorSetContext => base.DomNode.As<BehaviorAdapter>().BehaviorSet;

	public bool HasAnimations
	{
		get
		{
			BehaviorAdapter behaviorAdapter = base.DomNode.As<BehaviorAdapter>();
			if (global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, behaviorAdapter.InstanceEntity) is IBehaviorClass behaviorClass)
			{
				return behaviorClass.AllowedAnimationClasses.Any();
			}
			return false;
		}
	}

	public bool HasGeometryReferences
	{
		get
		{
			BehaviorAdapter behaviorAdapter = base.DomNode.As<BehaviorAdapter>();
			if (global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, behaviorAdapter.InstanceEntity) is IBehaviorClass behaviorClass)
			{
				return behaviorClass.AllowedGeometryClasses.Any();
			}
			return false;
		}
	}

	public bool HasBehaviors => true;

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.TimelineType.AnimationNameAttribute)
		{
			OnReloaded(sender, new TimelineReloadEvent(e.DomNode.As<TimelineAdapter>()));
		}
		else
		{
			base.HandleDomNodeAttributeChanged(sender, e);
		}
	}

	public bool CanCopy()
	{
		return TriggerInstancingHelper.CanCopy(base.Selection);
	}

	public object Copy()
	{
		return TriggerInstancingHelper.Copy(base.Selection);
	}

	public bool CanInsert(object dataObject)
	{
		return TriggerInstancingHelper.CanInsert(base.Selection, dataObject);
	}

	public void Insert(object dataObject)
	{
		TriggerInstancingHelper.Insert(base.Selection, dataObject);
	}

	public void InsertAtTime(float time, object dataObject)
	{
		TriggerInstancingHelper.InsertAtTime(base.Selection, time, dataObject);
	}

	public bool CanDelete()
	{
		return TriggerInstancingHelper.CanDelete(base.Selection);
	}

	public void Delete()
	{
		TriggerInstancingHelper.Delete(base.Selection);
	}
}
