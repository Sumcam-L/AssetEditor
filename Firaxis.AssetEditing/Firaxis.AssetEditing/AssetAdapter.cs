using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.Asset;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AssetAdapter : InstanceEntityAdapter, IBehaviorProviderAdapter, IAnimatableEntityAdapter, IInstanceEntityAdapter, INamedAdapter, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, IAttachmentPointNameProvider, IAssetBrowserTypeProvider, IDisposable, ICommandClient
{
	private IInstanceSet _instanceSet;

	private IAssetClass _assetClass;

	public IAnimatable AnimationData => Asset;

	public IInstanceSet InstanceSet
	{
		get
		{
			if (_instanceSet == null)
			{
				_instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.CivTechService.GetActivePantryPaths() });
			}
			return _instanceSet;
		}
	}

	public IAssetInstance Asset => InstanceEntity as IAssetInstance;

	public IDSGInstance DSGInst { get; private set; }

	public IBehaviorDataProvider BehaviorData => Asset;

	public IEnumerable<string> AllowedBehaviorClasses
	{
		get
		{
			if (AssetClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return AssetClass.AllowedBehaviorClasses;
		}
	}

	public IEnumerable<string> AllowedTriggerLightClasses
	{
		get
		{
			if (AssetClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return AssetClass.AllowedTriggerClasses;
		}
	}

	public IEnumerable<string> AllowedTriggerVFXClasses
	{
		get
		{
			if (AssetClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return AssetClass.AllowedTriggerClasses;
		}
	}

	public IEnumerable<IAssetArtDefReference> AllowedBehaviorArtDefs
	{
		get
		{
			if (AssetClass == null)
			{
				return Enumerable.Empty<IAssetArtDefReference>();
			}
			return AssetClass.ArtDefReferences;
		}
	}

	public IEnumerable<string> ReferenceGeometryNames
	{
		get
		{
			if (Asset == null || Asset.GeometrySet.ModelInstanceCount == 0)
			{
				return Enumerable.Empty<string>();
			}
			ISet<string> set = new HashSet<string>();
			foreach (IModelInstance modelInstance in Asset.GeometrySet.ModelInstances)
			{
				set.Add(modelInstance.GeoName);
			}
			return set;
		}
	}

	public IAssetClass AssetClass
	{
		get
		{
			if (_assetClass == null && Asset != null)
			{
				_assetClass = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(Asset) as IAssetClass;
			}
			return _assetClass;
		}
	}

	public string DSG
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AssetType.DSGAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AssetType.DSGAttribute, value);
		}
	}

	public AnimationBindingSetAdapter AnimationBindingSet { get; private set; }

	public AttachmentPointSetAdapter AttachmentPointSet { get; private set; }

	public TimelineSetAdapter TimelineSet { get; private set; }

	public TimelineBindingSetAdapter TimelineBindingSet { get; private set; }

	public GeometrySetAdapter GeometrySet { get; private set; }

	public ParticleEffectSetAdapter ParticleEffectsSet { get; private set; }

	public BehaviorSetAdapter BehaviorSet { get; private set; }

	public SplineSetAdapter SplineSet { get; private set; }

	public IEnumerable<string> AttachementPointNames => BehaviorData.AttachmentPointSet.Items.Select((IAttachmentPoint ap) => ap.Name);

	private ITransactionContext TransactionContext => (ITransactionContext)GetAdapter(typeof(ITransactionContext));

	public virtual bool InTransaction => TransactionContext?.InTransaction ?? false;

	public int PendingOperationCount => TransactionContext?.PendingOperationCount ?? 0;

	public virtual ICommandClient CommandClient => this;

	public virtual IEnumerable<CommandInfo> Commands => Enumerable.Empty<CommandInfo>();

	public virtual ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

	public virtual string DefaultSortPropertyName => "Name";

	public virtual IEnumerable<object> Items
	{
		get
		{
			ISelectionContext selectionContext = base.DomNode.As<ISelectionContext>();
			BugSubmitter.SilentAssert(selectionContext != null, "AssetAdapter could not get an ISelectionContext from its root context! @assign bwhitman");
			if (selectionContext != null && selectionContext.SelectionCount > 0)
			{
				if (selectionContext.LastSelected.Is<AnimationBindingAdapter>())
				{
					AnimationBindingAdapter bindingAdapter2 = selectionContext.LastSelected.As<AnimationBindingAdapter>();
					foreach (TriggerAdapter item in TimelineSet.Timelines.FirstOrDefault((TimelineAdapter tl) => tl.Name == bindingAdapter2.SlotName)?.Triggers)
					{
						yield return item.DomNode;
					}
					yield break;
				}
				if (selectionContext.LastSelected.Is<TimelineBindingAdapter>())
				{
					TimelineBindingAdapter bindingAdapter3 = selectionContext.LastSelected.As<TimelineBindingAdapter>();
					foreach (TriggerAdapter item2 in TimelineSet.Timelines.FirstOrDefault((TimelineAdapter tl) => tl.Name == bindingAdapter3.SlotName)?.Triggers)
					{
						yield return item2.DomNode;
					}
					yield break;
				}
			}
			foreach (TimelineAdapter timeline in TimelineSet.Timelines)
			{
				foreach (TriggerAdapter trigger in timeline.Triggers)
				{
					yield return trigger.DomNode;
				}
			}
		}
	}

	public virtual IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	protected override AttributeInfo NameAttribute => EntitySchema.AssetType.NameAttribute;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.AssetType.DescriptionAttribute;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.AssetType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.AssetType.CookParameterSetChild;

	protected override ChildInfo DataFilesChild => EntitySchema.AssetType.DataFilesChild;

	protected override ChildInfo TagsChild => EntitySchema.AssetType.TagsChild;

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_DSG };

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			if (AssetClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return AssetClass.AllowedDSGClasses;
		}
	}

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public AssetAdapter()
	{
		if (this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	public string GetAnimationName(string slotName)
	{
		string result = string.Empty;
		IAnimationBinding animationBinding = BehaviorData.AnimationBindings.FindBinding(slotName);
		if (animationBinding != null)
		{
			result = animationBinding.AnimationName;
		}
		return result;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		AnimationBindingSet = this.CreateComponentAdapter<AnimationBindingSetAdapter>(EntitySchema.AnimationBindingSetType.Type, EntitySchema.AssetType.AnimationBindingSetChild);
		AttachmentPointSet = this.CreateComponentAdapter<AttachmentPointSetAdapter>(EntitySchema.AttachmentPointSetType.Type, EntitySchema.AssetType.AttachmentPointSetChild);
		TimelineSet = this.CreateComponentAdapter<TimelineSetAdapter>(EntitySchema.TimelineSetType.Type, EntitySchema.AssetType.TimelineSetChild);
		TimelineBindingSet = this.CreateComponentAdapter<TimelineBindingSetAdapter>(EntitySchema.TimelineBindingSetType.Type, EntitySchema.AssetType.TimelineBindingSetChild);
		GeometrySet = this.CreateComponentAdapter<GeometrySetAdapter>(EntitySchema.GeometrySetType.Type, EntitySchema.AssetType.GeometrySetChild);
		ParticleEffectsSet = this.CreateComponentAdapter<ParticleEffectSetAdapter>(EntitySchema.ParticleEffectSetType.Type, EntitySchema.AssetType.ParticleEffectsSetChild);
		BehaviorSet = this.CreateComponentAdapter<BehaviorSetAdapter>(EntitySchema.BehaviorSetType.Type, EntitySchema.AssetType.BehaviorSetChild);
		SplineSet = this.CreateComponentAdapter<SplineSetAdapter>(EntitySchema.SplineSetType.Type, EntitySchema.AssetType.SplineSetChild);
		AnimationBindingSet.Reloaded += delegate(object s, EventArgs e)
		{
			this.Reloaded.Raise(s, e);
		};
	}

	protected override void OnClassChange()
	{
		base.OnClassChange();
		_assetClass = null;
		base.DomNode.As<AssetDocument>().RaisePreviewModuleChanged();
		base.DomNode.As<AssetContext>().OnReloaded();
	}

	protected override void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		base.HandleDomNodeChildInserted(sender, e);
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
	}

	protected override void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		base.HandleDomNodeChildRemoved(sender, e);
	}

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		base.HandleDomNodeAttributeChanged(sender, e);
		if (e.AttributeInfo == EntitySchema.AssetType.DSGAttribute)
		{
			BaseEntityPropertyContext baseEntityPropertyContext = base.DomNode.As<BaseEntityPropertyContext>();
			Asset.DSGName = DSG;
			DSGInst = InstanceSet.LoadEntityIfUnique<IDSGInstance>(DSG);
			UpdateAnimationBindingSet();
			baseEntityPropertyContext.BatchChangelist?.CreateAssetDSGChangedEvent(Asset, DSG);
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void UpdateAnimationBindingSet()
	{
		AnimationBindingSet.Update();
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (Asset.DSGName != DSG)
		{
			DSG = Asset.DSGName;
		}
		if (!string.IsNullOrEmpty(DSG))
		{
			DSGInst = InstanceSet.LoadEntityIfUnique<IDSGInstance>(DSG);
		}
		else
		{
			DSGInst = null;
		}
		AttachmentPointSet.EntityAdapter = this;
		TimelineSet.Update();
		AnimationBindingSet.Update();
		TimelineBindingSet.Update();
		AttachmentPointSet.Update();
		ParticleEffectsSet.Update();
		GeometrySet.Update();
		BehaviorSet.Update();
		SplineSet.InitializeFromNative(Asset.SplineSet);
	}

	public virtual void Begin(string transactionName)
	{
		TransactionContext?.Begin(transactionName);
	}

	public virtual void Cancel()
	{
		TransactionContext?.Cancel();
	}

	public virtual void End()
	{
		TransactionContext?.End();
	}

	public virtual TransactionSuspensionReceipt SuspendTransactions()
	{
		return TransactionContext?.SuspendTransactions();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return true;
	}

	public virtual void DoCommand(object commandTag)
	{
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _instanceSet != null)
		{
			_instanceSet.Dispose();
			_instanceSet = null;
		}
	}
}
