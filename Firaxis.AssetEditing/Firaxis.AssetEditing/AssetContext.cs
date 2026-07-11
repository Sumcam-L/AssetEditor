using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AssetContext : BaseEntityPropertyContext, IAssetEditorContext, IEntityEditorContext, IObservableContext, IAssetBrowserServiceProvider, ITriggerInstancingContext, IInstancingContext
{
	public AssetEditor AssetEditor { get; set; }

	public IAssetBrowserDialogService AssetBrowserService { get; set; }

	public IPropertyEditingListContext AttachmentsContext => base.DomNode.As<AssetAdapter>().AttachmentPointSet;

	public IModelInstanceStateContext GeometrySetContext => base.DomNode.As<AssetAdapter>().GeometrySet;

	public virtual bool HasGeometries
	{
		get
		{
			IAssetClass assetClass = AssetClass;
			if (assetClass == null)
			{
				return false;
			}
			return assetClass.AllowedGeometryClasses?.Any() == true;
		}
	}

	public virtual bool HasParticleEffects
	{
		get
		{
			IAssetClass assetClass = AssetClass;
			if (assetClass == null)
			{
				return false;
			}
			return assetClass.AllowedParticleEffectClasses?.Any() == true;
		}
	}

	public IPropertyEditingListContext ParticleEffectsContext => base.DomNode.As<AssetAdapter>().ParticleEffectsSet;

	public virtual bool HasAnimations
	{
		get
		{
			IAssetClass assetClass = AssetClass;
			if (assetClass == null)
			{
				return false;
			}
			return assetClass.AllowedAnimationClasses?.Any() == true;
		}
	}

	public IPropertyEditingListContext AnimationSetContext => base.DomNode.As<AssetAdapter>().AnimationBindingSet;

	public virtual bool HasBehaviors
	{
		get
		{
			IAssetClass assetClass = AssetClass;
			if (assetClass == null)
			{
				return false;
			}
			return assetClass.AllowedBehaviorClasses?.Any() == true;
		}
	}

	public IPropertyEditingListContext BehaviorSetContext => base.DomNode.As<AssetAdapter>().BehaviorSet;

	public virtual bool HasSplines
	{
		get
		{
			IAssetClass assetClass = AssetClass;
			if (assetClass == null)
			{
				return false;
			}
			return assetClass.AllowedSplineClasses?.Any() == true;
		}
	}

	public IPropertyEditingListContext SplineSetContext => base.DomNode.As<AssetAdapter>().SplineSet;

	private IAssetClass AssetClass
	{
		get
		{
			AssetAdapter assetAdapter = base.DomNode.As<AssetAdapter>();
			if (assetAdapter == null)
			{
				return null;
			}
			return global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, assetAdapter.InstanceEntity) as IAssetClass;
		}
	}

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

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			base.GUI?.Bind(null);
			base.GUI?.Dispose();
			base.GUI = null;
		}
		base.Dispose(bDisposing);
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
