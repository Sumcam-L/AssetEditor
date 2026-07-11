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

public class BehaviorAdapter : InstanceEntityAdapter, IBehaviorProviderAdapter, IAnimatableEntityAdapter, IInstanceEntityAdapter, INamedAdapter, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, IAttachmentPointNameProvider, IAssetBrowserTypeProvider, IDisposable, ICommandClient
{
	private IInstanceSet _instanceSet;

	private IBehaviorClass _behaviorClass;

	private ISelectionContext m_selectionContext;

	public IBehaviorDataProvider BehaviorData => Behavior;

	public IEnumerable<string> AllowedBehaviorClasses
	{
		get
		{
			if (Behavior == null || string.IsNullOrEmpty(Behavior.ClassName))
			{
				return Enumerable.Empty<string>();
			}
			return new string[1] { Behavior.ClassName };
		}
	}

	public IEnumerable<string> AllowedTriggerLightClasses
	{
		get
		{
			if (BehaviorClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return BehaviorClass.AllowedTriggerClasses;
		}
	}

	public IEnumerable<string> AllowedTriggerVFXClasses
	{
		get
		{
			if (BehaviorClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return BehaviorClass.AllowedTriggerClasses;
		}
	}

	public IEnumerable<IAssetArtDefReference> AllowedBehaviorArtDefs
	{
		get
		{
			if (BehaviorClass == null)
			{
				return Enumerable.Empty<IAssetArtDefReference>();
			}
			return BehaviorClass.ArtDefReferences;
		}
	}

	public IEnumerable<string> ReferenceGeometryNames => Behavior.ReferenceGeometries;

	public IAnimatable AnimationData => Behavior;

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

	public IBehaviorInstance Behavior => InstanceEntity as IBehaviorInstance;

	public IDSGInstance DSGInst { get; private set; }

	public IBehaviorClass BehaviorClass
	{
		get
		{
			if (_behaviorClass == null && Behavior != null)
			{
				_behaviorClass = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(Behavior) as IBehaviorClass;
			}
			return _behaviorClass;
		}
	}

	public string DSG
	{
		get
		{
			return GetAttribute<string>(EntitySchema.BehaviorType.DSGAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.BehaviorType.DSGAttribute, value);
		}
	}

	public AnimationBindingSetAdapter AnimationBindingSet { get; private set; }

	public AttachmentPointSetAdapter AttachmentPointSet { get; private set; }

	public TimelineSetAdapter TimelineSet { get; private set; }

	public TimelineBindingSetAdapter TimelineBindingSet { get; private set; }

	public BehaviorSetAdapter BehaviorSet { get; private set; }

	public GeometryReferenceSetAdapter GeometryReferenceSet { get; private set; }

	public IEnumerable<string> AttachementPointNames => BehaviorData.AttachmentPointSet.Items.Select((IAttachmentPoint ap) => ap.Name);

	private ITransactionContext TransactionContext => (ITransactionContext)GetAdapter(typeof(ITransactionContext));

	public virtual bool InTransaction => TransactionContext?.InTransaction ?? false;

	public int PendingOperationCount => TransactionContext?.PendingOperationCount ?? 0;

	public virtual ICommandClient CommandClient => this;

	public virtual IEnumerable<CommandInfo> Commands => Enumerable.Empty<CommandInfo>();

	public virtual ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

	public virtual string DefaultSortPropertyName => "Name";

	public virtual IEnumerable<object> Items => m_selectionContext?.Selection;

	public virtual IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public IEnumerable<object> Selection
	{
		get
		{
			return m_selectionContext.Selection;
		}
		set
		{
			m_selectionContext.Selection = value;
		}
	}

	public object LastSelected => m_selectionContext.LastSelected;

	public int SelectionCount => m_selectionContext.SelectionCount;

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_DSG };

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			IBehaviorClass behaviorClass = BehaviorClass;
			if (behaviorClass == null)
			{
				return Enumerable.Empty<string>();
			}
			return behaviorClass.AllowedDSGClasses;
		}
	}

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);

	protected override AttributeInfo NameAttribute => EntitySchema.BehaviorType.NameAttribute;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.BehaviorType.DescriptionAttribute;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.BehaviorType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.BehaviorType.CookParameterSetChild;

	protected override ChildInfo DataFilesChild => EntitySchema.BehaviorType.DataFilesChild;

	protected override ChildInfo TagsChild => EntitySchema.BehaviorType.TagsChild;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public BehaviorAdapter()
	{
		InitializeSelectionContext();
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

	protected override void OnClassChange()
	{
		base.OnClassChange();
		_behaviorClass = null;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		AnimationBindingSet = this.CreateComponentAdapter<AnimationBindingSetAdapter>(EntitySchema.AnimationBindingSetType.Type, EntitySchema.BehaviorType.AnimationBindingSetChild);
		AttachmentPointSet = this.CreateComponentAdapter<AttachmentPointSetAdapter>(EntitySchema.AttachmentPointSetType.Type, EntitySchema.BehaviorType.AttachmentPointSetChild);
		TimelineSet = this.CreateComponentAdapter<TimelineSetAdapter>(EntitySchema.TimelineSetType.Type, EntitySchema.BehaviorType.TimelineSetChild);
		TimelineBindingSet = this.CreateComponentAdapter<TimelineBindingSetAdapter>(EntitySchema.TimelineBindingSetType.Type, EntitySchema.BehaviorType.TimelineBindingSetChild);
		GeometryReferenceSet = this.CreateComponentAdapter<GeometryReferenceSetAdapter>(EntitySchema.GeometryReferenceSetType.Type, EntitySchema.BehaviorType.GeometryReferenceSetChild);
		BehaviorSet = this.CreateComponentAdapter<BehaviorSetAdapter>(EntitySchema.BehaviorSetType.Type, EntitySchema.BehaviorType.BehaviorSetChild);
	}

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.BehaviorType.DSGAttribute)
		{
			Behavior.DSGName = DSG;
			DSGInst = InstanceSet.LoadEntityIfUnique<IDSGInstance>(DSG);
			AnimationBindingSet.Update();
			base.DomNode.As<BaseEntityPropertyContext>().BatchChangelist?.CreateAssetDSGChangedEvent(Behavior, DSG);
		}
		else
		{
			base.HandleDomNodeAttributeChanged(sender, e);
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (Behavior.DSGName != DSG)
		{
			DSG = Behavior.DSGName;
		}
		if (!string.IsNullOrEmpty(DSG))
		{
			DSGInst = InstanceSet.LoadEntityIfUnique<IDSGInstance>(DSG);
		}
		else
		{
			DSGInst = null;
		}
		AnimationBindingSet.Update();
		TimelineBindingSet.Update();
		AttachmentPointSet.Update();
		TimelineSet.Update();
		BehaviorSet.Update();
		GeometryReferenceSet.Update();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			ShutdownSelectionContext();
			if (_instanceSet != null)
			{
				_instanceSet.Dispose();
				_instanceSet = null;
			}
		}
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

	public TransactionSuspensionReceipt SuspendTransactions()
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

	private void InitializeSelectionContext()
	{
		m_selectionContext = new SelectionContext();
		m_selectionContext.SelectionChanging += SelectionContext_SelectionChanging;
		m_selectionContext.SelectionChanged += SelectionContext_SelectionChanged;
	}

	private void ShutdownSelectionContext()
	{
		m_selectionContext.SelectionChanging -= SelectionContext_SelectionChanging;
		m_selectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
		m_selectionContext = null;
	}

	private void SelectionContext_SelectionChanging(object sender, EventArgs e)
	{
		this.SelectionChanging.Raise(sender, e);
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		this.SelectionChanged.Raise(sender, e);
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selectionContext.GetSelection<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selectionContext.GetLastSelected<T>();
	}

	public bool SelectionContains(object item)
	{
		return m_selectionContext.SelectionContains(item);
	}
}
