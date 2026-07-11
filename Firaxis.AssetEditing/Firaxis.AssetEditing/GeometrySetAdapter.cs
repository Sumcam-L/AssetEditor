using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class GeometrySetAdapter : AssetComponentAdapterBase, IModelInstanceStateContext, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ITreeListView, IItemView, ICommandClient, ISelectionContext, IValidationContext
{
	private enum Command
	{
		AddEntriesFromCloud,
		AddEntriesFromSource,
		RemoveEntries,
		ReimportEntries,
		OpenEntries,
		PreviewEntrySource,
		ClearBoundMaterials,
		AutomaticallyBindMaterials
	}

	private struct GeometrySetCommandTag
	{
		public Command Command;

		public GeometrySetCommandTag(Command command)
		{
			Command = command;
		}
	}

	private struct FieldInfo
	{
		public readonly FieldPropertyDescriptorBase descriptor;

		public readonly string name;

		public FieldInfo(FieldPropertyDescriptorBase desc, string n)
		{
			descriptor = desc;
			name = n;
		}
	}

	private static GeometrySetCommandTag AddEntriesFromCloudCommandTag = new GeometrySetCommandTag(Command.AddEntriesFromCloud);

	private static GeometrySetCommandTag AddEntriesFromSourceCommandTag = new GeometrySetCommandTag(Command.AddEntriesFromSource);

	private static GeometrySetCommandTag RemoveEntriesCommandTag = new GeometrySetCommandTag(Command.RemoveEntries);

	private static GeometrySetCommandTag ReimportEntriesCommandTag = new GeometrySetCommandTag(Command.ReimportEntries);

	private static GeometrySetCommandTag OpenEntriesCommandTag = new GeometrySetCommandTag(Command.OpenEntries);

	private static GeometrySetCommandTag OpenEntriesSourceCommandTag = new GeometrySetCommandTag(Command.PreviewEntrySource);

	private static GeometrySetCommandTag AutomaticallyBindMaterialsCommandTag = new GeometrySetCommandTag(Command.AutomaticallyBindMaterials);

	private static GeometrySetCommandTag ClearBoundMaterialsCommandTag = new GeometrySetCommandTag(Command.ClearBoundMaterials);

	private bool _initialUpdate = true;

	private ISelectionContext m_selectionContext = new SelectionContext();

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	public IList<ModelInstanceAdapter> ModelInstances { get; private set; }

	public IGeometrySet GeometrySet => base.AssetAdapter.Asset.GeometrySet;

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

	public IEnumerable<object> Items
	{
		get
		{
			foreach (ModelInstanceAdapter item in GetSelection<ModelInstanceAdapter>())
			{
				IEnumerable<PrimGroupStateAdapter> enumerable = item?.PrimGroups;
				IEnumerable<PrimGroupStateAdapter> enumerable2 = enumerable ?? Enumerable.Empty<PrimGroupStateAdapter>();
				foreach (PrimGroupStateAdapter item2 in enumerable2)
				{
					yield return item2;
				}
			}
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			IList<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
			foreach (System.ComponentModel.PropertyDescriptor item in EntitySchema.PrimGroupStateType.Type.GetTag<PropertyDescriptorCollection>())
			{
				list.Add(item);
			}
			int num = Selection.Count();
			if (num > 1)
			{
				return list.Concat(GetIntersectionOfDescriptors());
			}
			if (num == 1)
			{
				return list.Concat(GetAllDescriptorsForInstance());
			}
			return list;
		}
	}

	public IEnumerable<object> Roots => ModelInstances;

	public string[] ColumnNames => new string[1] { "Model Instance".Localize() };

	public bool InTransaction => base.AssetAdapter.As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.AssetAdapter.As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public event EventHandler Beginning;

	public event EventHandler Cancelled;

	public event EventHandler Ending;

	public event EventHandler Ended;

	private string EnsureUniqueModelName(string inModelName)
	{
		int num = 0;
		string outModelName = inModelName;
		while (ModelInstances.Any((ModelInstanceAdapter mi) => mi.Name == outModelName))
		{
			num++;
			outModelName = $"{inModelName}{num}";
		}
		return outModelName;
	}

	private ModelInstanceAdapter AddModelInstance(string desiredModelName, IGeometryInstance geo)
	{
		DomNode domNode = new DomNode(EntitySchema.ModelInstanceType.Type);
		domNode.InitializeExtensions();
		string text = desiredModelName;
		if (string.IsNullOrEmpty(text))
		{
			text = geo.ModelName;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = geo.Name;
		}
		ModelInstanceAdapter modelInstanceAdapter = domNode.As<ModelInstanceAdapter>();
		modelInstanceAdapter.GeoName = geo.Name;
		modelInstanceAdapter.Name = EnsureUniqueModelName(text);
		modelInstanceAdapter.GeometryInstance = geo;
		modelInstanceAdapter.GeometryClassName = geo?.ClassName;
		ModelInstances.Add(modelInstanceAdapter);
		return modelInstanceAdapter;
	}

	public GeometrySetAdapter()
	{
		m_commands.Add(new CommandInfo(AddEntriesFromSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add New".Localize("Name of a command"), "Creates new geometries from the source file and adds them to the asset.".Localize(), Keys.None, Firaxis.ATF.Resources.AddNewEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(AddEntriesFromCloudCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add Existing".Localize("Name of a command"), "Adds the selected geometries to the asset.".Localize(), Keys.None, Firaxis.ATF.Resources.AddExistingEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(RemoveEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Remove".Localize("Name of a command"), "Removes the selected geometries from the Asset.".Localize(), Keys.None, Resources.RemoveItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ReimportEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Reimport".Localize("Name of a command"), "Reimports the selected geometries.".Localize(), Keys.None, Resources.ReimportFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open".Localize("Name of a command"), "Opens the documents of the selected geometries.".Localize(), Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenEntriesSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open Source File".Localize("Name of a command"), "Opens the selected geometry's Source File.".Localize(), Keys.None, Resources.OpenSourceFileIcon, CommandVisibility.All));
		object groupTag = 3;
		m_commands.Add(new CommandInfo(AutomaticallyBindMaterialsCommandTag, StandardMenu.Edit, groupTag, "Automatically Bind Materials".Localize("Name of a command"), "Automatically binds unbound materials on all geometries with materials from the cloud.".Localize(), Keys.None, Resources.FillMaterialsIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ClearBoundMaterialsCommandTag, StandardMenu.Edit, groupTag, "Clear Bound Materials".Localize("Name of a command"), "Clears all bound materials on all geometry.".Localize(), Keys.None, Resources.ClearMaterialsIcon, CommandVisibility.All));
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		ModelInstances = new DomNodeListAdapter<ModelInstanceAdapter>(base.DomNode, EntitySchema.GeometrySetType.ModelInstanceChild);
		m_selectionContext.SelectionChanging += selection_Changing;
		m_selectionContext.SelectionChanged += selection_Changed;
	}

	protected override void OnParentNodeSet()
	{
		RegisterForDomChanges();
		base.OnParentNodeSet();
	}

	protected virtual void RegisterForDomChanges()
	{
		UnregisterFromDomChanges();
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
	}

	private void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.ModelInstanceType.Type)
		{
			if (SelectionContains(e.Child))
			{
				m_selectionContext.Remove(e.Child);
			}
			string name = e.Child.As<ModelInstanceAdapter>().Name;
			GeometrySet.RemoveModelInstance(name);
			base.BatchChangelist?.CreateModelInstanceRemovedEvent(base.AssetAdapter.Asset, name);
			OnItemRemoved(e.Index, e.Child);
		}
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.ModelInstanceType.Type)
		{
			ModelInstanceAdapter modelInstanceAdapter = e.Child.As<ModelInstanceAdapter>();
			IGeometryInstance geometryInstance = (modelInstanceAdapter.GeometryInstance = base.AssetAdapter.InstanceSet.LoadEntityIfUnique<IGeometryInstance>(modelInstanceAdapter.GeoName));
			IGeometryInstance geometryInstance3 = geometryInstance;
			modelInstanceAdapter.GeometryClassName = geometryInstance3?.ClassName;
			modelInstanceAdapter.ModelInstance = GeometrySet.AddModelInstance(modelInstanceAdapter.Name, geometryInstance3);
			base.BatchChangelist?.CreateModelInstanceChangedEvent(base.AssetAdapter.Asset, modelInstanceAdapter.Name, modelInstanceAdapter.GeoName, modelInstanceAdapter.ModelInstance);
			OnItemInserted(e.Index, e.Child);
		}
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		this.SelectionChanged?.Invoke(this, e);
	}

	private void selection_Changing(object sender, EventArgs e)
	{
		this.SelectionChanging?.Invoke(this, e);
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		_ = base.AssetAdapter.InstanceSet;
		Context.EnsureCreated<CivTechContext>();
		IList<ModelInstanceAdapter> list = new List<ModelInstanceAdapter>();
		foreach (IModelInstance model in base.AssetAdapter.Asset.GeometrySet.ModelInstances)
		{
			ModelInstanceAdapter modelInstanceAdapter = ModelInstances.FirstOrDefault((ModelInstanceAdapter umi) => umi.Name == model.Name);
			if (modelInstanceAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.ModelInstanceType.Type);
				domNode.InitializeExtensions();
				modelInstanceAdapter = domNode.As<ModelInstanceAdapter>();
				modelInstanceAdapter.Name = model.Name;
				modelInstanceAdapter.GeoName = model.GeoName;
				ModelInstances.Add(modelInstanceAdapter);
				if (!_initialUpdate)
				{
					base.BatchChangelist?.CreateModelInstanceChangedEvent(base.AssetAdapter.Asset, model.Name, model.GeoName, model);
				}
			}
			modelInstanceAdapter.Update(model);
			list.Add(modelInstanceAdapter);
		}
		if (!_initialUpdate)
		{
			foreach (var entryAdapter in ModelInstances.Except(list).ToArray())
			{
				base.BatchChangelist?.CreateModelInstanceRemovedEvent(base.AssetAdapter.Asset, entryAdapter.Name);
				ModelInstances.Remove(entryAdapter);
			}
		}
		_initialUpdate = false;
		RegisterForDomChanges();
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

	public void RefreshSelection()
	{
		IEnumerable<object> selection = Selection.ToList();
		Selection = new List<object>();
		m_selectionContext.Selection = selection;
	}

	private IEnumerable<System.ComponentModel.PropertyDescriptor> GetIntersectionOfDescriptors()
	{
		Dictionary<int, FieldInfo> dictionary = new Dictionary<int, FieldInfo>();
		foreach (ModelInstanceAdapter item in GetSelection<ModelInstanceAdapter>())
		{
			IEnumerable<IFieldValueAdapter> enumerable = (item.PrimGroups?.FirstOrDefault())?.Fields;
			foreach (IFieldValueAdapter item2 in enumerable ?? Enumerable.Empty<IFieldValueAdapter>())
			{
				foreach (FieldPropertyDescriptorBase item3 in item2.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
				{
					int key = item2.Name.GetHashCode() ^ item3.GetType().GetHashCode();
					dictionary[key] = new FieldInfo(item3, item2.Name);
				}
			}
		}
		foreach (FieldInfo value in dictionary.Values)
		{
			yield return value.descriptor;
		}
	}

	private IEnumerable<System.ComponentModel.PropertyDescriptor> GetAllDescriptorsForInstance()
	{
		IEnumerable<IFieldValueAdapter> enumerable = (GetSelection<ModelInstanceAdapter>().FirstOrDefault()?.PrimGroups?.FirstOrDefault())?.Fields;
		IEnumerable<IFieldValueAdapter> enumerable2 = enumerable ?? Enumerable.Empty<IFieldValueAdapter>();
		foreach (IFieldValueAdapter item in enumerable2)
		{
			foreach (FieldPropertyDescriptorBase item2 in item.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
			{
				yield return item2;
			}
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		if (base.ParentAsset == null || string.IsNullOrEmpty(base.AssetAdapter.Asset.ClassName))
		{
			return false;
		}
		GeometrySetCommandTag geometrySetCommandTag = (GeometrySetCommandTag)commandTag;
		if (base.AssetAdapter.As<IEntityDocument>().IsReadOnly && geometrySetCommandTag.Command != Command.OpenEntries && geometrySetCommandTag.Command != Command.PreviewEntrySource)
		{
			return false;
		}
		if (geometrySetCommandTag.Command == Command.AddEntriesFromCloud || geometrySetCommandTag.Command == Command.AddEntriesFromSource)
		{
			return true;
		}
		if (geometrySetCommandTag.Command == Command.ReimportEntries)
		{
			if (Selection.Any())
			{
				return CanReimportSelection();
			}
			return false;
		}
		if (geometrySetCommandTag.Command == Command.OpenEntries || geometrySetCommandTag.Command == Command.RemoveEntries || geometrySetCommandTag.Command == Command.PreviewEntrySource || geometrySetCommandTag.Command == Command.AutomaticallyBindMaterials || geometrySetCommandTag.Command == Command.ClearBoundMaterials)
		{
			return Selection.Any();
		}
		return false;
	}

	private bool CanReimportSelection()
	{
		ICivTechService civTechService = base.AssetAdapter.CivTechService;
		foreach (object item in Selection)
		{
			ModelInstanceAdapter modelInstanceAdapter = item.As<ModelInstanceAdapter>();
			string entityPath = civTechService.GetEntityPath(modelInstanceAdapter.GeoName, InstanceType.IT_GEOMETRY);
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
		base.AssetAdapter.As<AssetContext>();
		switch (((GeometrySetCommandTag)commandTag).Command)
		{
		case Command.AddEntriesFromCloud:
			AddEntriesFromCloud();
			break;
		case Command.AddEntriesFromSource:
			AddEntriesFromSource();
			break;
		case Command.RemoveEntries:
			RemoveSelectedModelsFromAsset();
			break;
		case Command.ReimportEntries:
			ReimportSelectedEntries();
			break;
		case Command.OpenEntries:
			OpenDocumentsForSelectedGeometry();
			break;
		case Command.PreviewEntrySource:
			PreviewSelectedGeometrySources();
			break;
		case Command.ClearBoundMaterials:
			ClearBoundMaterials();
			break;
		case Command.AutomaticallyBindMaterials:
			AutomaticallyBindMaterials();
			break;
		}
	}

	public void ClearBoundMaterials()
	{
		using (new Sce.Atf.WaitCursor())
		{
			base.ParentContext.DoTransaction(delegate
			{
				foreach (object item in Selection)
				{
					foreach (PrimGroupStateAdapter primGroup in item.As<ModelInstanceAdapter>().PrimGroups)
					{
						ClearPrimGroupMaterials(primGroup);
					}
				}
			}, "Clear all bound materials.".Localize());
		}
	}

	private void ClearPrimGroupMaterials(PrimGroupStateAdapter primGroup)
	{
		(from wc in primGroup.Fields.OfType<ObjectFieldValueAdapter>()
			where wc.ObjectParameter.ObjectType == InstanceType.IT_MATERIAL
			select wc).ForEach(delegate(ObjectFieldValueAdapter fe)
		{
			fe.ObjectName = string.Empty;
		});
	}

	public void AutomaticallyBindMaterials()
	{
		using (new Sce.Atf.WaitCursor())
		{
			base.ParentContext.DoTransaction(delegate
			{
				using IInstanceSet tempSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() });
				foreach (object item in Selection)
				{
					ModelInstanceAdapter modelAdapter = item.As<ModelInstanceAdapter>();
					BindMaterialsToModel(tempSet, modelAdapter);
				}
			}, "Automatically bind empty materials.".Localize());
		}
	}

	private void BindMaterialsToModel(IInstanceSet tempSet, ModelInstanceAdapter modelAdapter)
	{
		if (tempSet.LoadEntityIfUnique(modelAdapter.GeoName, InstanceType.IT_GEOMETRY) is IGeometryInstance geo)
		{
			AssignMaterialsToModel(modelAdapter, geo, base.ProjectConfig.Classes, tempSet, null, base.LocalPantry);
			return;
		}
		Outputs.WriteLine(OutputMessageType.Error, "Could not load the referenced Geometry {0}", modelAdapter.GeoName);
	}

	private void AddEntriesFromCloud()
	{
		IEnumerable<KeyValuePair<string, InstanceType>> entities = SelectGeometryFromAssetBrowser();
		if (!entities.Any())
		{
			return;
		}
		base.ParentContext.DoTransaction(delegate
		{
			using IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() });
			foreach (KeyValuePair<string, InstanceType> item in entities.Where((KeyValuePair<string, InstanceType> pair) => pair.Value == InstanceType.IT_GEOMETRY))
			{
				EntityID entityID = new EntityID(item.Key, item.Value);
				ReloadModelInAsset(entityID, entitySet);
			}
		}, "Adding geometry from the cloud.".Localize());
	}

	private IEnumerable<KeyValuePair<string, InstanceType>> SelectGeometryFromAssetBrowser()
	{
		IDictionary<string, InstanceType> dictionary = new Dictionary<string, InstanceType>();
		IEnumerable<string> allowedGeometryClasses = base.AssetAdapter.AssetClass.AllowedGeometryClasses;
		base.AssetAdapter.As<AssetContext>().AssetBrowserService.OpenEntities(dictionary, new InstanceType[1] { InstanceType.IT_GEOMETRY }, allowedGeometryClasses);
		return dictionary;
	}

	private void ReloadModelInAsset(EntityID entityID, IInstanceSet entitySet)
	{
		IGeometryInstance geometryInstance = entitySet.LoadByName<IGeometryInstance>(entityID.Name);
		if (geometryInstance == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Could not load the geometry with the name {0}.  It will not be added to the asset.", entityID.Name);
			return;
		}
		IEnumerable<IPrimGroupStateInformation> savedInformation = RemoveModelAndCaptureGroupState(entityID.Name);
		if (AddModelToAsset(geometryInstance, savedInformation, entitySet) == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Model '{0}' failed to be added to the asset. Most likely there is already a geometry with the same model name in the Asset", geometryInstance.ModelName);
		}
	}

	public void ReloadModelInAsset(IGeometryInstance changedGeometry, IInstanceSet entitySet)
	{
		if (changedGeometry == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Could not load the geometry with the name {0}.  It will not be added to the asset.", changedGeometry.Name);
			return;
		}
		base.ParentContext.As<IEntityPreviewComponent>();
		base.ParentContext.DoTransaction(delegate
		{
			IEnumerable<IPrimGroupStateInformation> savedInformation = RemoveModelAndCaptureGroupState(changedGeometry.Name);
			if (AddModelToAsset(changedGeometry, savedInformation, entitySet) == null)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Model '{0}' failed to be added to the asset. Most likely there is already a geometry with the same model name in the Asset", changedGeometry.ModelName);
			}
		}, "Reload model in asset.");
	}

	private void AddEntriesFromSource()
	{
		AssetContext context = base.AssetAdapter.As<AssetContext>();
		IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() });
		try
		{
			IEnumerable<EntityID> importedEntities = GetImportedEntities(entitySet);
			IEnumerable<EntityID> validGeos = importedEntities.Where((EntityID id) => id.Type == InstanceType.IT_GEOMETRY && !string.IsNullOrWhiteSpace(id.Name)).ToArray();
			if (!validGeos.Any())
			{
				return;
			}
			context.DoTransaction(delegate
			{
				foreach (EntityID item in validGeos)
				{
					ReloadModelInAsset(item, entitySet);
				}
			}, "Add geometries to asset from source file.".Localize());
		}
		finally
		{
			if (entitySet != null)
			{
				entitySet.Dispose();
			}
		}
	}

	private IEnumerable<EntityID> GetImportedEntities(IInstanceSet entitySet)
	{
		IEnumerable<string> allowedGeometryClasses = base.AssetAdapter.AssetClass.AllowedGeometryClasses;
		BaseEntityPropertyContext baseEntityPropertyContext = base.EntityAdapter.As<BaseEntityPropertyContext>();
		return SetAdapterHelper.LaunchMiniImporter(baseEntityPropertyContext.CivTechService, baseEntityPropertyContext.FileWatchService, entitySet, baseEntityPropertyContext.EntityCacheService, allowedGeometryClasses, InstanceType.IT_GEOMETRY);
	}

	private IEnumerable<IPrimGroupStateInformation> RemoveModelAndCaptureGroupState(string geoName)
	{
		ModelInstanceAdapter modelInstanceAdapter = ModelInstances.FirstOrDefault((ModelInstanceAdapter modIns) => modIns.GeoName == geoName);
		if (modelInstanceAdapter == null)
		{
			return Enumerable.Empty<IPrimGroupStateInformation>();
		}
		IEnumerable<IPrimGroupStateInformation> primGroupInformation = modelInstanceAdapter.ModelInstance.GetPrimGroupInformation(Context.EnsureCreated<CivTechContext>().CreateInstance<IPrimGroupStateInformation>);
		ModelInstances.Remove(modelInstanceAdapter);
		return primGroupInformation;
	}

	private void AssignMaterialsToModel(ModelInstanceAdapter modelInstance, IGeometryInstance geo, IClassSet classes, IInstanceSet instanceSet, IEnumerable<string> materialNames, string gamePantry)
	{
		IGeometryClass obj = classes.FindForInstance(geo) as IGeometryClass;
		HashSet<string> hashSet = null;
		hashSet = ((materialNames == null) ? new HashSet<string>() : new HashSet<string>(materialNames));
		IParameterSet groupParameters = obj.GroupParameters;
		foreach (PrimGroupStateAdapter primGroup in modelInstance.PrimGroups)
		{
			primGroup.AddDefaultValuesAsNecessary(groupParameters);
			foreach (ObjectFieldValueAdapter item in from val in primGroup.Fields.OfType<ObjectFieldValueAdapter>()
				where val.ObjectParameter != null && val.ObjectParameter.ObjectType == InstanceType.IT_MATERIAL
				select val)
			{
				IObjectParameter objParam = groupParameters.FindByName(item.Parameter.Name) as IObjectParameter;
				string text = item.AssignDefaultMaterial(objParam, primGroup.GroupName, primGroup.StateName, hashSet, instanceSet, gamePantry);
				if (!string.IsNullOrEmpty(text))
				{
					hashSet.Add(text);
				}
			}
		}
	}

	private void AssignDefaultMaterialsToUnassignedPGS(ModelInstanceAdapter modelInstance, IGeometryInstance geo, IClassSet classes, IInstanceSet instanceSet, string gamePantry)
	{
		IParameterSet parameterSet = ((classes.FindForInstance(geo) is IGeometryClass geometryClass) ? geometryClass.GroupParameters : null);
		if (parameterSet == null)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (PrimGroupStateAdapter primGroup in modelInstance.PrimGroups)
		{
			foreach (ObjectFieldValueAdapter item in from val in primGroup.Fields.OfType<ObjectFieldValueAdapter>()
				where val.ObjectParameter != null && val.ObjectParameter.ObjectType == InstanceType.IT_MATERIAL && string.IsNullOrEmpty(val.ObjectName)
				select val)
			{
				IObjectParameter objParam = parameterSet.FindByName(item.Parameter.Name) as IObjectParameter;
				string text = item.AssignDefaultMaterial(objParam, primGroup.GroupName, primGroup.StateName, hashSet, instanceSet, gamePantry);
				if (!string.IsNullOrEmpty(text))
				{
					hashSet.Add(text);
				}
			}
		}
	}

	private void RestorePrimGroupState(ModelInstanceAdapter model, IEnumerable<IPrimGroupStateInformation> stateInformation)
	{
		foreach (IPrimGroupStateInformation item in stateInformation)
		{
			model.FindPrimGroupState(item.MeshName, item.GroupName, item.StateName)?.RestoreState(model.GeometryClassName, item.Values);
		}
	}

	private void PopulatePrimGroupStatesChangedEvents(IEntityChangeList changeList, ModelInstanceAdapter model, IInstanceEntity parentEntity)
	{
		foreach (PrimGroupStateAdapter primGroup in model.PrimGroups)
		{
			foreach (IFieldValueAdapter field in primGroup.Fields)
			{
				changeList.CreateTriGroupParameterChangedEvent(parentEntity, model.Name, primGroup.MeshName, primGroup.GroupName, primGroup.StateName, field.Parameter.Name, field.Value);
			}
		}
	}

	public ModelInstanceAdapter AddModelToAsset(IGeometryInstance newGeometry, IEnumerable<IPrimGroupStateInformation> savedInformation, IInstanceSet entitySet)
	{
		string modelName = (string.IsNullOrEmpty(newGeometry.ModelName) ? newGeometry.Name : newGeometry.ModelName);
		if (ModelInstances.Where((ModelInstanceAdapter mi) => mi.Name == modelName).Any())
		{
			return null;
		}
		ModelInstanceAdapter modelInstanceAdapter = AddModelInstance(newGeometry.ModelName, newGeometry);
		if (modelInstanceAdapter == null)
		{
			return null;
		}
		AddPrimGroupsFromClass(newGeometry, modelInstanceAdapter);
		RestorePrimGroupState(modelInstanceAdapter, savedInformation);
		AssignDefaultMaterialsToUnassignedPGS(modelInstanceAdapter, newGeometry, base.ProjectConfig.Classes, entitySet, base.LocalPantry);
		base.BatchChangelist?.CreateModelInstanceChangedEvent(base.AssetAdapter.Asset, modelName, newGeometry.Name, modelInstanceAdapter.ModelInstance);
		OnReloaded();
		return modelInstanceAdapter;
	}

	public void RemoveModelFromAsset(string modelName)
	{
		ModelInstanceAdapter modelInstanceAdapter = ModelInstances.FirstOrDefault((ModelInstanceAdapter mi) => mi.Name == modelName);
		if (modelInstanceAdapter == null)
		{
			return;
		}
		modelInstanceAdapter.FreezeUpdates();
		PrimGroupStateAdapter[] array = modelInstanceAdapter.PrimGroups.ToArray();
		foreach (PrimGroupStateAdapter primGroupStateAdapter in array)
		{
			primGroupStateAdapter.FreezeUpdates();
			IFieldValueAdapter[] array2 = primGroupStateAdapter.Fields.ToArray();
			foreach (IFieldValueAdapter item in array2)
			{
				primGroupStateAdapter.Fields.Remove(item);
			}
			modelInstanceAdapter.PrimGroups.Remove(primGroupStateAdapter);
		}
		modelInstanceAdapter.ClearModelInstancePrimGroups();
		bool condition = modelInstanceAdapter.Is<DomNode>();
		string fmtText = "Invalid ModelInstanceAdapter in GeometrySetAdapter.RemoveModelFromAsset(\"%s\"), where matching adapter was of type %s with a DomNodeType of %s @assign bwhitman @summary ModelInstanceAdapter was not adaptable to a DomNode!";
		object[] array3 = new object[3]
		{
			modelName,
			modelInstanceAdapter.GetType(),
			null
		};
		int num3 = 2;
		array3[num3] = modelInstanceAdapter.DomNode?.Type;
		BugSubmitter.SilentAssert(condition, fmtText, array3);
		ModelInstances.Remove(modelInstanceAdapter);
		base.BatchChangelist?.CreateModelInstanceRemovedEvent(base.AssetAdapter.Asset, modelName);
		OnReloaded();
	}

	private void AddPrimGroupsFromClass(IGeometryInstance newGeometry, ModelInstanceAdapter modelInst)
	{
		IGeometryClass geometryClass = base.ProjectConfig.Classes.FindForInstance(newGeometry) as IGeometryClass;
		foreach (IGeoMesh geometryMesh in newGeometry.GeometryMeshes)
		{
			foreach (IGeoPrimGroup geoPrimGroup in geometryMesh.GeoPrimGroups)
			{
				foreach (IAssetClassState state in base.AssetAdapter.AssetClass.States)
				{
					PrimGroupStateAdapter primGroupStateAdapter = modelInst.AddPrimGroupState(geometryMesh.Name, geoPrimGroup.Name, state.Name);
					if (geometryClass != null)
					{
						primGroupStateAdapter.AddDefaultValuesAsNecessary(geometryClass.GroupParameters);
					}
				}
			}
		}
	}

	private void OpenDocumentsForSelectedGeometry()
	{
		AssetContext assetContext = base.AssetAdapter.As<AssetContext>();
		foreach (string selectedGeometryName in GetSelectedGeometryNames())
		{
			assetContext.AssetDocumentCommands.OpenExistingDocument(InstanceType.IT_GEOMETRY, selectedGeometryName);
		}
	}

	public void ReimportSelectedEntries()
	{
		IEnumerable<string> selectedGeometryNames = GetSelectedGeometryNames();
		IEntityDocument entityDocument = base.AssetAdapter.As<IEntityDocument>();
		BaseEntityPropertyContext entityContext = base.AssetAdapter.As<BaseEntityPropertyContext>();
		IEnumerable<EntityID> entityIDs = SetAdapterHelper.GetSelectedEntityIDs(selectedGeometryNames, InstanceType.IT_GEOMETRY);
		using (entityContext.SuspendRecording())
		{
			entityContext.DoTransaction(delegate
			{
				entityContext.BatchChangelist?.AddGenericEntityChangedEvents(entityIDs);
				SetAdapterHelper.ImportSelectedEntities(entityIDs, entityContext, recurseIntoChildren: false);
			}, "Reimporting geometry.");
		}
		entityContext.AssetDocumentCommands.OpenExistingDocument(entityDocument.InstanceEntity.Type, entityDocument.InstanceEntity.Name);
		if (base.HotLoadOnReimport)
		{
			List<Uri> list = new List<Uri> { entityDocument.Uri };
			list.AddRange(StaticMethods.GetEntityURIs(entityContext.CivTechService, entityIDs));
			base.TunerQueueService?.AddFilesToQueue(list);
		}
		RefreshSelection();
	}

	private IEnumerable<string> GetSelectedGeometryNames()
	{
		List<string> list = new List<string>(Selection.Count());
		foreach (object item in Selection)
		{
			ModelInstanceAdapter modelInstanceAdapter = item.As<ModelInstanceAdapter>();
			if (modelInstanceAdapter != null && !string.IsNullOrEmpty(modelInstanceAdapter.GeoName))
			{
				list.Add(modelInstanceAdapter.GeoName);
			}
		}
		return list;
	}

	private void RemoveSelectedModelsFromAsset()
	{
		base.ParentContext.DoTransaction(delegate
		{
			object[] array = Selection.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				ModelInstanceAdapter modelInstanceAdapter = array[i].As<ModelInstanceAdapter>();
				RemoveModelFromAsset(modelInstanceAdapter.Name);
			}
		}, "Remove models from asset.".Localize());
	}

	public void RemoveModelsFromAsset(IEnumerable<string> modelNamesToRemove)
	{
		base.ParentContext.DoTransaction(delegate
		{
			foreach (string item in modelNamesToRemove)
			{
				RemoveModelFromAsset(item);
			}
		}, "Remove models from asset.".Localize());
	}

	private void PreviewSelectedGeometrySources()
	{
		using IInstanceSet instances = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() });
		IEnumerable<EntityID> entities = from name in GetSelectedGeometryNames()
			select new EntityID(name, InstanceType.IT_GEOMETRY);
		foreach (IGeometryInstance item in (IEnumerable<IGeometryInstance>)instances.LoadEntities(base.EntityAdapter.CivTechService.ProjectMapService.LayeredPantry, entities))
		{
			OpenEntitySourceFileForPreview(item);
		}
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

	protected virtual void OnReloaded()
	{
		this.Reloaded?.Invoke(this, EventArgs.Empty);
	}

	public IEnumerable<object> GetChildren(object parent)
	{
		yield break;
	}

	private ModelInstanceAdapter GetModelInstanceAdapter(object item)
	{
		ModelInstanceAdapter modelInstanceAdapter = item as ModelInstanceAdapter;
		if (modelInstanceAdapter == null && item is DomNode adaptable)
		{
			modelInstanceAdapter = adaptable.As<ModelInstanceAdapter>();
		}
		return modelInstanceAdapter;
	}

	public void GetInfo(object item, ItemInfo info)
	{
		ModelInstanceAdapter modelInstanceAdapter = GetModelInstanceAdapter(item);
		if (modelInstanceAdapter != null)
		{
			info.AllowLabelEdit = false;
			info.IsLeaf = true;
			info.Label = $"{modelInstanceAdapter.GeoName} ({modelInstanceAdapter.Name}); Vertex Count: {modelInstanceAdapter.VertexCount}; Primitive Count: {modelInstanceAdapter.PrimitiveCount}; Bone Count {modelInstanceAdapter.BoneCount}";
		}
	}

	public void Begin(string transactionName)
	{
		base.AssetAdapter.As<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.AssetAdapter.As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.AssetAdapter.As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.AssetAdapter.As<ITransactionContext>().SuspendTransactions();
	}

	private void OpenEntitySourceFileForPreview(IImportedEntity entity)
	{
		CivTechHelper.OpenSourceFile(base.AssetAdapter.CivTechService, entity);
	}

	protected virtual void OnValidationBeginning()
	{
		this.Beginning?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnValidationCancelled()
	{
		this.Cancelled?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnValidationEnding()
	{
		this.Ending?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnValidationEnded()
	{
		this.Ended?.Invoke(this, EventArgs.Empty);
	}

	public void AddGeometryToModelInstances(IGeometryInstance geometryToAdd)
	{
		AssetContext assetContext = base.AssetAdapter.As<AssetContext>();
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		assetContext.DoTransaction(delegate
		{
			IEnumerable<IPrimGroupStateInformation> enumerable = null;
			IModelInstance modelInstance = base.AssetAdapter.Asset.GeometrySet.FindModelInstance(geometryToAdd.Name);
			if (modelInstance != null && string.Compare(modelInstance.GeoName, geometryToAdd.Name) == 0)
			{
				enumerable = modelInstance.GetPrimGroupInformation(civTechContext.CreateInstance<IPrimGroupStateInformation>);
				RemoveModelFromAsset(modelInstance.Name);
			}
			using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { assetContext.CivTechService.GetActivePantryPaths() });
			ModelInstanceAdapter modelInstanceAdapter = AddModelInstance(geometryToAdd.ModelName, geometryToAdd);
			if (modelInstanceAdapter != null)
			{
				AddPrimGroupsFromClass(geometryToAdd, modelInstanceAdapter);
				AssignMaterialsToModel(modelInstanceAdapter, geometryToAdd, assetContext.CivTechService.PrimaryProject.Config.Classes, instanceSet, null, assetContext.CivTechService.PrimaryProject.Paths.GamePantry);
				if (enumerable != null)
				{
					RestorePrimGroupState(modelInstanceAdapter, enumerable);
				}
				IEntityChangeList batchChangelist = base.BatchChangelist;
				if (batchChangelist != null)
				{
					PopulatePrimGroupStatesChangedEvents(batchChangelist, modelInstanceAdapter, base.AssetAdapter.Asset);
					batchChangelist.CreateEntityChangedEvent(base.EntityAdapter.InstanceType, base.EntityAdapter.InstanceEntity.Name);
				}
				OnReloaded();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Model '{0}' failed to be added to the asset. Most likely there is already a geometry with the same model name in the Asset", geometryToAdd.ModelName);
			}
		}, "Add Geometry");
	}
}
