using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class AnimationBindingSetAdapter : AnimatableComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		AddEntriesFromSource,
		ReimportEntries,
		OpenEntries,
		PreviewEntrySource,
		RenderSourceObject,
		ClearBoundAnimations,
		BindAnimationsAutomatically
	}

	private struct AnimationSetCommandTag
	{
		public Command Command;

		public AnimationSetCommandTag(Command command)
		{
			Command = command;
		}
	}

	private IList<CommandInfo> m_commands = new List<CommandInfo>();

	private ISelectionContext m_selectionContext = new SelectionContext();

	private static AnimationSetCommandTag AddEntriesFromSourceCommandTag = new AnimationSetCommandTag(Command.AddEntriesFromSource);

	private static AnimationSetCommandTag ReimportEntriesCommandTag = new AnimationSetCommandTag(Command.ReimportEntries);

	private static AnimationSetCommandTag OpenEntriesCommandTag = new AnimationSetCommandTag(Command.OpenEntries);

	private static AnimationSetCommandTag OpenEntriesSourceCommandTag = new AnimationSetCommandTag(Command.PreviewEntrySource);

	private static AnimationSetCommandTag RenderSourceObjectCommandTag = new AnimationSetCommandTag(Command.RenderSourceObject);

	private static AnimationSetCommandTag ClearBoundAnimationsTag = new AnimationSetCommandTag(Command.ClearBoundAnimations);

	private static AnimationSetCommandTag TryAutomaticAnimationBindingTag = new AnimationSetCommandTag(Command.BindAnimationsAutomatically);

	private InstanceType[] m_typeFilter = new InstanceType[1] { InstanceType.IT_ANIMATION };

	public IList<AnimationBindingAdapter> AnimationBindings { get; private set; }

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => m_commands;

	public string DefaultSortPropertyName
	{
		get
		{
			System.ComponentModel.PropertyDescriptor propertyDescriptor = PropertyDescriptors.FirstOrDefault();
			if (propertyDescriptor == null)
			{
				return string.Empty;
			}
			return propertyDescriptor.Name;
		}
	}

	public ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

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

	public IEnumerable<object> Items => base.AnimatableEntityAdapter.AnimationBindingSet.AnimationBindings;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			PropertyDescriptorCollection tag = EntitySchema.AnimationBindingType.Type.GetTag<PropertyDescriptorCollection>();
			if (tag == null)
			{
				yield break;
			}
			foreach (object item in tag)
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public bool InTransaction => base.AnimatableEntityAdapter.As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.AnimatableEntityAdapter.As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public AnimationBindingSetAdapter()
	{
		m_commands.Add(new CommandInfo(AddEntriesFromSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add New".Localize("Name of a command"), "Creates new animations from the source file.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.AddNewEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(TryAutomaticAnimationBindingTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Bind Automatically".Localize("Name of a command"), "Attempts to bind each unbound animation automatically.".Localize(), Sce.Atf.Input.Keys.None, Resources.FillMaterialsIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ClearBoundAnimationsTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Clear Animations".Localize("Name of a command"), "Clears all bound animations.".Localize(), Sce.Atf.Input.Keys.None, Resources.ClearMaterialsIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ReimportEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Reimport".Localize("Name of a command"), "Reimports the selected animations.".Localize(), Sce.Atf.Input.Keys.None, Resources.ReimportFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open".Localize("Name of a command"), "Opens the documents of the selected animations.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenEntriesSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open Source File".Localize("Name of a command"), "Opens the selected animation's Source File.".Localize(), Sce.Atf.Input.Keys.None, Resources.OpenSourceFileIcon, CommandVisibility.All));
		if (this.SelectionChanging != null && this.SelectionChanged != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	private void RegisterForDomChanges()
	{
		UnregisterFromDomChanges();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.AnimationBindingType.AnimationNameAttribute)
		{
			string text = (string)e.NewValue;
			AnimationBindingAdapter animationBindingAdapter = sender.As<DomNode>().As<AnimationBindingAdapter>() ?? e.DomNode.As<AnimationBindingAdapter>();
			if (!string.IsNullOrEmpty(text))
			{
				base.BehaviorProvider.BehaviorData.AnimationBindings.Bind(animationBindingAdapter.SlotName, text);
				base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			}
			else
			{
				base.BehaviorProvider.BehaviorData.AnimationBindings.Unbind(animationBindingAdapter.SlotName);
				base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			}
			TimelineAdapter timelineAdapter = base.BehaviorProvider.TimelineSet.FindTimeline(animationBindingAdapter.SlotName);
			if (timelineAdapter != null)
			{
				OnReloaded(new TimelineReloadEvent(timelineAdapter));
			}
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.AnimationBindingSetType.AnimationBindingChild)
		{
			e.Child.As<AnimationBindingAdapter>();
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			OnItemRemoved(e.Index, e.Child);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.AnimationBindingSetType.AnimationBindingChild)
		{
			e.Child.As<AnimationBindingAdapter>();
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			OnItemInserted(e.Index, e.Child);
		}
	}

	public bool HasAnimationSlot(string name)
	{
		return AnimationBindings.Any((AnimationBindingAdapter binding) => binding.SlotName == name);
	}

	public AnimationBindingAdapter FindOrCreateAnimationBinding(string name)
	{
		AnimationBindingAdapter animationBindingAdapter = AnimationBindings.FirstOrDefault((AnimationBindingAdapter binding) => binding.SlotName == name);
		if (animationBindingAdapter == null)
		{
			animationBindingAdapter = AddAnimationBinding(name);
		}
		return animationBindingAdapter;
	}

	public AnimationBindingAdapter AddAnimationBinding(string name)
	{
		if (AnimationBindings.FirstOrDefault((AnimationBindingAdapter binding) => binding.SlotName == name) != null)
		{
			throw new System.Exception("AnimationBinding slot named \"" + name + "\" already exists!");
		}
		DomNode domNode = new DomNode(EntitySchema.AnimationBindingType.Type);
		domNode.InitializeExtensions();
		AnimationBindingAdapter animationBindingAdapter = domNode.As<AnimationBindingAdapter>();
		animationBindingAdapter.SlotName = name;
		AnimationBindings.Add(animationBindingAdapter);
		return animationBindingAdapter;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		AnimationBindings = new DomNodeListAdapter<AnimationBindingAdapter>(base.DomNode, EntitySchema.AnimationBindingSetType.AnimationBindingChild);
		RegisterForDomChanges();
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (IAnimationBinding binding in base.AnimatableEntityAdapter.AnimationData.AnimationBindings.Bindings)
		{
			dictionary[binding.SlotName] = binding.AnimationName;
		}
		IList<AnimationBindingAdapter> list = new List<AnimationBindingAdapter>();
		if (base.AnimatableEntityAdapter.DSGInst != null)
		{
			foreach (string slotName in base.AnimatableEntityAdapter.DSGInst.GetAnimationSlots())
			{
				AnimationBindingAdapter animationBindingAdapter = AnimationBindings.FirstOrDefault((AnimationBindingAdapter uab) => uab.SlotName == slotName);
				if (animationBindingAdapter == null)
				{
					DomNode domNode = new DomNode(EntitySchema.AnimationBindingType.Type);
					domNode.InitializeExtensions();
					animationBindingAdapter = domNode.As<AnimationBindingAdapter>();
					AnimationBindings.Add(animationBindingAdapter);
				}
				string animationName = (dictionary.ContainsKey(slotName) ? dictionary[slotName] : string.Empty);
				animationBindingAdapter.Update(slotName, animationName);
				list.Add(animationBindingAdapter);
			}
		}
		foreach (var entryAdapter in AnimationBindings.Except(list).ToArray())
		{
			entryAdapter.AnimationName = string.Empty;
			AnimationBindings.Remove(entryAdapter);
		}
		RegisterForDomChanges();
		OnReloaded(EventArgs.Empty);
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

	public bool CanDoCommand(object commandTag)
	{
		AnimationSetCommandTag animationSetCommandTag = (AnimationSetCommandTag)commandTag;
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly && animationSetCommandTag.Command != Command.OpenEntries && animationSetCommandTag.Command != Command.PreviewEntrySource)
		{
			return false;
		}
		if (animationSetCommandTag.Command == Command.OpenEntries || animationSetCommandTag.Command == Command.PreviewEntrySource || animationSetCommandTag.Command == Command.RenderSourceObject)
		{
			return Selection.Any();
		}
		if (animationSetCommandTag.Command == Command.ReimportEntries)
		{
			if (Selection.Any())
			{
				return CanReimportSelection();
			}
			return false;
		}
		return !string.IsNullOrEmpty(base.AnimatableEntityAdapter.InstanceEntity.ClassName);
	}

	private bool CanReimportSelection()
	{
		ICivTechService civTechService = base.EntityAdapter.CivTechService;
		foreach (object item in Selection)
		{
			AnimationBindingAdapter animationBindingAdapter = item.As<AnimationBindingAdapter>();
			string entityPath = civTechService.GetEntityPath(animationBindingAdapter.AnimationName, InstanceType.IT_ANIMATION);
			if (string.IsNullOrEmpty(entityPath))
			{
				return false;
			}
			Uri uri = null;
			try
			{
				uri = new Uri(entityPath);
			}
			catch (System.Exception)
			{
				return false;
			}
			if (!civTechService.IsFromActiveProjectOrDependencies(uri))
			{
				return false;
			}
		}
		return true;
	}

	public void DoCommand(object commandTag)
	{
		switch (((AnimationSetCommandTag)commandTag).Command)
		{
		case Command.AddEntriesFromSource:
			AddAnimationsToCloud();
			break;
		case Command.ReimportEntries:
			ReimportSelectedAnimations();
			break;
		case Command.OpenEntries:
			OpenSelectedEntries();
			break;
		case Command.PreviewEntrySource:
			PreviewSelectedAnimationSourceFiles();
			break;
		case Command.RenderSourceObject:
			RenderSelectedEntitiesSourceObjects();
			break;
		case Command.ClearBoundAnimations:
			ClearBoundAnimations();
			break;
		case Command.BindAnimationsAutomatically:
			TryBindAnimationsAutomatically();
			break;
		default:
			MessageBox.Show("Reached an invalid command tag.");
			break;
		}
	}

	private void AddAnimationsToCloud()
	{
		BaseEntityPropertyContext baseEntityPropertyContext = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>();
		if (!(baseEntityPropertyContext.CivTechService.PrimaryProject.Config.Classes.FindForInstance(base.AnimatableEntityAdapter.InstanceEntity) is IAnimatableClass animatableClass))
		{
			string caption = "Failed to add animations";
			MessageBox.Show("The animatable class is no longer valid.  Can not add animations to this asset!", caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		IEnumerable<string> animationNames;
		using (IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { baseEntityPropertyContext.CivTechService.GetActivePantryPaths() }))
		{
			animationNames = (from res in SetAdapterHelper.LaunchMiniImporter(baseEntityPropertyContext.CivTechService, baseEntityPropertyContext.FileWatchService, entitySet, baseEntityPropertyContext.EntityCacheService, animatableClass.AllowedAnimationClasses, InstanceType.IT_ANIMATION)
				select res.Name).ToArray();
		}
		if (base.AnimatableEntityAdapter.DSGInst != null)
		{
			IEnumerable<string> animationSlots = base.AnimatableEntityAdapter.DSGInst.GetAnimationSlots();
			IDictionary<string, string> currentAssignments = MatchAnimationsToSlots(animationNames, animationSlots);
			UpdateWithStringAssigner(animationNames, animationSlots, currentAssignments);
		}
	}

	private IDictionary<string, string> MatchAnimationsToSlots(IEnumerable<string> animationNames, IEnumerable<string> slotNames)
	{
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = animationNames.OrderBy((string x) => x).ToArray();
		string[] source = array.Select((string name) => name.ToLower()).ToArray();
		foreach (string item in slotNames.OrderBy((string x) => x))
		{
			string lowerName = item.ToLower();
			string value = source.FirstOrDefault((string name) => name.Contains(lowerName));
			if (string.IsNullOrEmpty(value))
			{
				string trimmed = lowerName.Replace("_", "");
				value = source.FirstOrDefault((string name) => name.Contains(trimmed));
			}
			if (string.IsNullOrEmpty(value))
			{
				string[] array2 = lowerName.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries);
				int num = 0;
				while (string.IsNullOrEmpty(value) && num < array2.Length)
				{
					string part = array2[num];
					if (part.Length > 1)
					{
						value = source.FirstOrDefault((string name) => name.Contains(part));
					}
					num++;
				}
			}
			if (!string.IsNullOrEmpty(value))
			{
				int num2 = source.IndexOf(value);
				string value2 = array[num2];
				dictionary[item] = value2;
			}
		}
		return dictionary;
	}

	private void UpdateWithStringAssigner(IEnumerable<string> animationNames, IEnumerable<string> slotNames, IDictionary<string, string> currentAssignments)
	{
		StringAssignerViewModel stringAssignerViewModel = new StringAssignerViewModel(slotNames, animationNames, currentAssignments);
		stringAssignerViewModel.Title = "Animation Assigner";
		stringAssignerViewModel.SlotLabel = "Slot Name";
		stringAssignerViewModel.DomainLabel = "Animations";
		if (DialogService.ShowDialog(stringAssignerViewModel))
		{
			IDictionary<string, string> assignedSlots = stringAssignerViewModel.GetAssignedSlots();
			DoBatchAnimationChange(assignedSlots);
		}
	}

	private void DoBatchAnimationChange(IDictionary<string, string> assignedValues)
	{
		GetTransactionContext().DoTransaction(delegate
		{
			foreach (KeyValuePair<string, string> assignedValue in assignedValues)
			{
				base.AnimatableEntityAdapter.AnimationBindingSet.FindOrCreateAnimationBinding(assignedValue.Key).AnimationName = assignedValue.Value;
			}
		}, "Update Assigned Animations");
	}

	private void OpenSelectedEntries()
	{
		AssetBrowserFileCommands assetDocumentCommands = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>().AssetDocumentCommands;
		foreach (string item in from adp in Selection.OfType<AnimationBindingAdapter>()
			where !string.IsNullOrEmpty(adp.AnimationName)
			select adp.AnimationName)
		{
			assetDocumentCommands.OpenExistingDocument(InstanceType.IT_ANIMATION, item);
		}
	}

	private void ReimportSelectedAnimations()
	{
		IEnumerable<string> selectedAnimationNames = GetSelectedAnimationNames();
		IEntityDocument entityDocument = base.AnimatableEntityAdapter.As<IEntityDocument>();
		BaseEntityPropertyContext entityContext = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>();
		IEnumerable<EntityID> entityIDs = SetAdapterHelper.GetSelectedEntityIDs(selectedAnimationNames, InstanceType.IT_ANIMATION);
		using (entityContext.SuspendRecording())
		{
			entityContext.DoTransaction(delegate
			{
				entityContext.BatchChangelist?.AddGenericEntityChangedEvents(entityIDs);
				SetAdapterHelper.ImportSelectedEntities(entityIDs, entityContext, recurseIntoChildren: false);
			}, "Reimporting animations.");
		}
		entityContext.AssetDocumentCommands.OpenExistingDocument(entityDocument.InstanceEntity.Type, entityDocument.InstanceEntity.Name);
		if (base.HotLoadOnReimport)
		{
			List<Uri> list = new List<Uri> { entityDocument.Uri };
			list.AddRange(StaticMethods.GetEntityURIs(entityContext.CivTechService, entityIDs));
			base.TunerQueueService?.AddFilesToQueue(list);
		}
	}

	private IEnumerable<string> GetSelectedAnimationNames()
	{
		List<string> list = new List<string>(Selection.Count());
		foreach (object item in Selection)
		{
			AnimationBindingAdapter animationBindingAdapter = item.As<AnimationBindingAdapter>();
			if (animationBindingAdapter != null && !string.IsNullOrEmpty(animationBindingAdapter.AnimationName))
			{
				list.Add(animationBindingAdapter.AnimationName);
			}
		}
		return list;
	}

	private void PreviewSelectedAnimationSourceFiles()
	{
		BaseEntityPropertyContext baseEntityPropertyContext = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>();
		_ = baseEntityPropertyContext.CivTechService.PrimaryProject.Paths.GamePantry;
		IEnumerable<string> enumerable = from adp in Selection.OfType<AnimationBindingAdapter>()
			where !string.IsNullOrEmpty(adp.AnimationName)
			select adp.AnimationName;
		using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { baseEntityPropertyContext.CivTechService.GetActivePantryPaths() });
		foreach (string item in enumerable)
		{
			if (instanceSet.LoadEntityIfUnique<IInstanceEntity>(item) == null)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to preview sources with.", item, EnumToStringConverter.GetNameFromType(InstanceType.IT_ANIMATION));
			}
		}
		foreach (IAnimationInstance item2 in instanceSet.Items.OfType<IAnimationInstance>())
		{
			OpenEntitySourceFileForPreview(baseEntityPropertyContext.CivTechService, item2);
		}
	}

	private void RenderSelectedEntitiesSourceObjects()
	{
		BaseEntityPropertyContext baseEntityPropertyContext = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>();
		_ = baseEntityPropertyContext.CivTechService.PrimaryProject.Paths.GamePantry;
		IEnumerable<string> enumerable = from adp in Selection.OfType<AnimationBindingAdapter>()
			where !string.IsNullOrEmpty(adp.AnimationName)
			select adp.AnimationName;
		if (enumerable.Count() < 1)
		{
			return;
		}
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "Pick the folder to put the Renders";
		folderBrowserDialog.ShowDialog();
		if (string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
		{
			return;
		}
		using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { baseEntityPropertyContext.CivTechService.GetActivePantryPaths() });
		foreach (string item in enumerable)
		{
			if (instanceSet.LoadEntityIfUnique<IInstanceEntity>(item) == null)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to preview sources with.", item, EnumToStringConverter.GetNameFromType(InstanceType.IT_ANIMATION));
			}
		}
		foreach (IAnimationInstance item2 in instanceSet.Items.OfType<IAnimationInstance>())
		{
			RenderEntitySourceObject(item2, folderBrowserDialog.SelectedPath);
		}
	}

	private void OpenEntitySourceFileForPreview(ICivTechService civTechSvc, IImportedEntity entity)
	{
		_ = base.AnimatableEntityAdapter.As<BaseEntityPropertyContext>().CivTechService.PrimaryProject.Paths.GamePantry;
		CivTechHelper.OpenSourceFile(civTechSvc, entity);
	}

	private void RenderEntitySourceObject(IImportedEntity entity, string outputPath)
	{
	}

	private void ClearBoundAnimations()
	{
		IEnumerable<string> enumerable = AnimationBindings.Select((AnimationBindingAdapter adp) => adp.SlotName);
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in enumerable)
		{
			dictionary[item] = string.Empty;
		}
		DoBatchAnimationChange(dictionary);
	}

	private void TryBindAnimationsAutomatically()
	{
		IEnumerable<string> slotNames = (from adp in AnimationBindings
			where string.IsNullOrEmpty(adp.AnimationName)
			select adp.SlotName).ToArray();
		IEnumerable<string> filteredAnimationsFromCloud = GetFilteredAnimationsFromCloud();
		IDictionary<string, string> currentAssignments = MatchAnimationsToSlots(filteredAnimationsFromCloud, slotNames);
		UpdateWithStringAssigner(filteredAnimationsFromCloud, slotNames, currentAssignments);
	}

	private IEnumerable<string> GetFilteredAnimationsFromCloud()
	{
		IInstanceEntity instanceEntity = base.AnimatableEntityAdapter.InstanceEntity;
		ICivTechService civTechService = base.EntityAdapter.CivTechService;
		if (!(civTechService.PrimaryProject.Config.Classes.FindForInstance(instanceEntity) is IAnimatableClass animatableClass))
		{
			return Enumerable.Empty<string>();
		}
		string name = instanceEntity.Name;
		return CivTechRegistry.EntityQueryService.FindFilesByName(civTechService.GetActiveProjects(), name, animatableClass.AllowedAnimationClasses, m_typeFilter).InstanceItems[InstanceType.IT_ANIMATION];
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}

	protected virtual void OnItemChanged(object item)
	{
		this.ItemChanged?.Invoke(this, new ItemChangedEventArgs<object>(item));
	}

	protected virtual void OnReloaded(EventArgs args)
	{
		this.Reloaded?.Invoke(this, args);
	}

	public void Begin(string transactionName)
	{
		base.AnimatableEntityAdapter.As<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.AnimatableEntityAdapter.As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.AnimatableEntityAdapter.As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.AnimatableEntityAdapter.As<ITransactionContext>().SuspendTransactions();
	}
}
