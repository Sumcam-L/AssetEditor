using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public abstract class InstanceEntityAdapter : DomNodeAdapter, IFieldSerializer, IInstanceEntityAdapter, INamedAdapter
{
	private IList<DataFileAdapter> m_dataFiles;

	private IList<TextElementAdapter> m_tags;

	protected bool m_canChangeClass = true;

	private IClassEntity _lastClassEntity;

	private string m_oldCookVals = string.Empty;

	public IAssetPreviewer PreviewerService { get; set; }

	public IPreviewerWidgetService PreviewerWidgetService { get; set; }

	public virtual IInstanceEntity InstanceEntity { get; internal set; }

	public virtual InstanceType InstanceType => InstanceEntity?.Type ?? InstanceType.IT_INVALID;

	public IDocumentRegistry DocumentRegistry { get; set; }

	public ICivTechService CivTechService { get; set; }

	public IFileWatcherService FileWatcherService { get; set; }

	public bool CanChangeClass => m_canChangeClass;

	public bool ShouldChangeClass
	{
		get
		{
			IQueryService dependents = CivTechRegistry.EntityCacheService.GetDependents(Name, InstanceType);
			ICollection<string> collection = new List<string>();
			foreach (KeyValuePair<InstanceType, List<string>> instanceItem in dependents.InstanceItems)
			{
				foreach (string item in instanceItem.Value)
				{
					collection.Add(item);
				}
			}
			return collection.Count == 0;
		}
	}

	public IInstanceSet Instances => base.DomNode.As<IInstanceEntityDocument>().InstanceSet;

	private IClassEntity LastClassEntity
	{
		get
		{
			return _lastClassEntity;
		}
		set
		{
			if (_lastClassEntity != value)
			{
				_lastClassEntity = value;
				OnClassChange();
			}
		}
	}

	public virtual string Name
	{
		get
		{
			return GetAttribute<string>(NameAttribute);
		}
		set
		{
			SetAttribute(NameAttribute, value);
		}
	}

	public string Description
	{
		get
		{
			return GetAttribute<string>(DescriptionAttribute);
		}
		set
		{
			SetAttribute(DescriptionAttribute, value);
		}
	}

	public virtual string ClassName
	{
		get
		{
			return GetAttribute<string>(ClassNameAttribute);
		}
		set
		{
			SetAttribute(ClassNameAttribute, value);
		}
	}

	public IList<DataFileAdapter> DataFiles => m_dataFiles;

	public CookParameterSetAdapter CookParameterSet { get; private set; }

	public IList<TextElementAdapter> Tags => m_tags;

	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract AttributeInfo DescriptionAttribute { get; }

	protected abstract AttributeInfo ClassNameAttribute { get; }

	protected abstract ChildInfo CookParametersChild { get; }

	protected abstract ChildInfo DataFilesChild { get; }

	protected abstract ChildInfo TagsChild { get; }

	public ISerializable FieldSerializer => InstanceEntity;

	public event EventHandler<DataFilesEventArgs> DataFilesChanging;

	public event EventHandler<DataFilesEventArgs> DataFilesChanged;

	public InstanceEntityAdapter()
	{
	}

	protected override void OnNodeSet()
	{
		CookParameterSet = this.CreateComponentAdapter<CookParameterSetAdapter>(EntitySchema.CookParametersSetType.Type, CookParametersChild);
		m_dataFiles = new DomNodeListAdapter<DataFileAdapter>(base.DomNode, DataFilesChild);
		m_tags = new DomNodeListAdapter<TextElementAdapter>(base.DomNode, TagsChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	protected virtual void AssignPropertiesFromEntity(bool updateUI)
	{
		AssignBasicPropertiesFromEntity();
		AssignTextAndDatFilesFromEntity();
		AssignCookParametersFromEntity(updateUI);
	}

	protected virtual void AssignBasicPropertiesFromEntity()
	{
		m_canChangeClass = global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(CivTechService.PrimaryProject.Name, InstanceEntity);
		if (InstanceEntity.Name != Name)
		{
			Name = InstanceEntity.Name;
		}
		if (InstanceEntity.Description != Description)
		{
			Description = InstanceEntity.Description;
		}
		if (InstanceEntity.ClassName != ClassName)
		{
			ClassName = InstanceEntity.ClassName;
		}
	}

	protected virtual void AssignCookParametersFromEntity(bool updateUI)
	{
		IClassEntity classEntity = global::DatabaseWrapper.DatabaseWrapper.GetClass(CivTechService.PrimaryProject.Name, InstanceType, InstanceEntity.ClassName);
		if (classEntity != null)
		{
			CookParameterSet.Update(InstanceEntity.CookParameters, classEntity.CookParameters, updateUI);
		}
	}

	protected virtual void AssignTextAndDatFilesFromEntity()
	{
		UpdateTextList(InstanceEntity, InstanceEntity.Tags, Tags, InstanceEntity.AddTag, InstanceEntity.RemoveTag);
		UpdateDataFiles(InstanceEntity);
		_lastClassEntity = global::DatabaseWrapper.DatabaseWrapper.GetClass(CivTechService.PrimaryProject.Name, InstanceType, InstanceEntity.ClassName);
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
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	public virtual void Update(bool updateUI = false)
	{
		UnregisterFromDomChanges();
		AssignPropertiesFromEntity(updateUI);
		RegisterForDomChanges();
	}

	public virtual bool PreNameChange(string newValue)
	{
		return IsNameChangeValid(newValue);
	}

	protected virtual void OnNameChange(string oldName, string newName)
	{
		IEnumerable<DataFileInfo> dataFiles = InstanceEntity.DataFiles.Select((IInstanceDataFile df) => new DataFileInfo(df.ID, df.RelativePath, new Uri(InstanceEntity.GetDataFilePath(df.RelativePath))));
		InstanceEntity.Name = newName;
		BugSubmitter.SilentAssert(!base.DomNode.As<IEntityDocument>().IsReadOnly, "IntanceEntityAdapter.PreNameChange failed to prevent modifying readonly document {0} named {1}! @summary IntanceEntityAdapter.PreNameChange failed to prevent modifying a readonly document! @assign bwhitman", InstanceEntity.Type, InstanceEntity.Name);
		IClassEntity entityClass = global::DatabaseWrapper.DatabaseWrapper.GetClass(CivTechService.PrimaryProject.Name, InstanceEntity);
		if (entityClass != null)
		{
			bool flag = InstanceEntity.DataFiles.Any();
			bool flag2 = entityClass.DataFiles.Any();
			if (flag || flag2)
			{
				this.DataFilesChanging.Raise(this, new DataFilesEventArgs(dataFiles));
				if (flag)
				{
					_ = CivTechService.PrimaryProject.Paths.GamePantry;
					ICollection<Tuple<string, string, bool>> collection = new List<Tuple<string, string, bool>>();
					foreach (IInstanceDataFile dataFile in InstanceEntity.DataFiles)
					{
						IClassDataFile classDataFile = entityClass.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == dataFile.ID);
						if (classDataFile == null)
						{
							Outputs.WriteLine(OutputMessageType.Info, "Stripping data file with ID \"{0}\" that no longer exists in the class entity \"{1}\" configuration.", dataFile.ID, entityClass.Name);
						}
						else
						{
							string dataFilePath = InstanceEntity.GetDataFilePath(dataFile.RelativePath);
							string item = dataFilePath.Replace(oldName, newName);
							collection.Add(Tuple.Create(dataFilePath, item, classDataFile.IsGenerated));
						}
					}
					string errorMessage = string.Empty;
					if (!CopyEntityDataFiles(collection, out errorMessage))
					{
						BaseEntityPropertyContext baseEntityPropertyContext = base.DomNode.As<BaseEntityPropertyContext>();
						if (baseEntityPropertyContext != null && baseEntityPropertyContext.InTransaction)
						{
							throw new InvalidTransactionException(errorMessage);
						}
						MessageBoxes.Show("Failed to rename one or more data files. You may need to fix your environment with P4V in order to perform this name change.\n\nError:\n" + errorMessage, "Failed to rename data files", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
				InstanceEntity.PopulateDataFiles(entityClass);
				IEnumerable<DataFileInfo> source = InstanceEntity.DataFiles.Select((IInstanceDataFile df) => new DataFileInfo(df.ID, df.RelativePath, new Uri(InstanceEntity.GetDataFilePath(df.RelativePath))));
				this.DataFilesChanged.Raise(this, new DataFilesEventArgs(source.Where((DataFileInfo result) => !entityClass.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == result.ID).IsGenerated)));
			}
		}
		AssignPropertiesFromEntity(updateUI: true);
	}

	private bool CopyEntityDataFiles(IEnumerable<Tuple<string, string, bool>> dataFiles, out string errorMessage)
	{
		try
		{
			foreach (Tuple<string, string, bool> dataFile in dataFiles)
			{
				string item = dataFile.Item1;
				string item2 = dataFile.Item2;
				if (!dataFile.Item3 && File.Exists(item))
				{
					if (File.Exists(item2))
					{
						errorMessage = "A data file with the new name (" + item2 + ") already exists.  The entity name change is being reverted.";
						return false;
					}
					File.Copy(item, item2);
					new FileInfo(item2).IsReadOnly = false;
				}
			}
		}
		catch (System.Exception ex)
		{
			errorMessage = "An exception occurred while trying to copy the data file to its new location.  Exception message: \n\n" + ex.Message;
			return false;
		}
		errorMessage = string.Empty;
		return true;
	}

	public virtual void PostNameChange(string oldName, string newName)
	{
	}

	public virtual bool IsNameChangeValid(string newValue)
	{
		if (newValue == InstanceEntity.Name)
		{
			return true;
		}
		if (Path.IsPathRooted(newValue))
		{
			return false;
		}
		BaseInstanceEntityDocument baseInstanceEntityDocument = base.DomNode.As<BaseInstanceEntityDocument>();
		string projectNameFromUri = CivTechService.ProjectMapService.GetProjectNameFromUri(baseInstanceEntityDocument.Uri);
		ProjectEnvironment project = CivTechService.AllProjectsMap[projectNameFromUri];
		string pantryPath = CivTechService.ProjectMapService.LayeredPantry.GetPantryPath(newValue, InstanceType, project);
		if (string.IsNullOrEmpty(pantryPath))
		{
			return false;
		}
		Uri newDocumentUri = new Uri(pantryPath);
		if (DocumentRegistry.Documents.Where((IDocument doc) => doc.Uri.Equals(newDocumentUri)).Any())
		{
			return false;
		}
		string name = InstanceEntity.Name;
		InstanceEntity.Name = newValue;
		bool result = global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(CivTechService.PrimaryProject.Name, InstanceEntity);
		InstanceEntity.Name = name;
		return result;
	}

	public void UpdateClassName(string className)
	{
		PreClassNameChange();
		ClassName = className;
		PostClassNameChange();
	}

	public virtual void PreClassNameChange()
	{
		foreach (string item in global::DatabaseWrapper.DatabaseWrapper.BuildDefaultTagList(InstanceEntity))
		{
			RemoveTag(item);
		}
		if (CookParameterSet != null && CookParameterSet.ValueSet != null)
		{
			m_oldCookVals = CookParameterSet.ValueSet.SerializeIntoXML();
		}
	}

	protected virtual void OnClassChange()
	{
		base.DomNode.As<BaseEntityPropertyContext>().OnReloaded();
	}

	public virtual void PostClassNameChange()
	{
		foreach (string item in global::DatabaseWrapper.DatabaseWrapper.BuildDefaultTagList(InstanceEntity))
		{
			AddTag(item);
		}
		RemoveCookParameters();
		IClassEntity classEntity = CivTechService.PrimaryProject.Config.Classes.FindForInstance(InstanceEntity);
		if (classEntity != null)
		{
			AddParametersFromConfig(classEntity, CookParameterSet);
			if (CookParameterSet != null && CookParameterSet.ValueSet != null)
			{
				CookParameterSet.ValueSet.UpdateFromXML(m_oldCookVals);
				CookParameterSet.Update(CookParameterSet.ValueSet, classEntity.CookParameters, updateUI: true);
			}
		}
		m_oldCookVals = string.Empty;
	}

	private TextElementAdapter AddTag(string tagName)
	{
		DomNode domNode = new DomNode(BaseSchema.TextElementType.Type);
		domNode.InitializeExtensions();
		TextElementAdapter textElementAdapter = domNode.As<TextElementAdapter>();
		textElementAdapter.Text = tagName;
		Tags.Add(textElementAdapter);
		return textElementAdapter;
	}

	private void RemoveTag(string tagName)
	{
		TextElementAdapter[] array = Tags.Where((TextElementAdapter oldTag) => oldTag.Text == tagName).ToArray();
		foreach (TextElementAdapter item in array)
		{
			Tags.Remove(item);
		}
	}

	private void RemoveCookParameters()
	{
		if (CookParameterSet != null && CookParameterSet.Fields != null)
		{
			while (CookParameterSet.Fields.Count > 0)
			{
				CookParameterSet.Fields.RemoveAt(0);
			}
		}
	}

	private void AddParametersFromConfig(IClassEntity config, CookParameterSetAdapter cookParams)
	{
		if (config.CookParameters == null)
		{
			return;
		}
		foreach (IParameter item in config.CookParameters.Items)
		{
			cookParams.AddParameter(item);
		}
	}

	protected virtual void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == NameAttribute)
		{
			string oldName = (string)e.OldValue;
			string newName = (string)e.NewValue;
			OnNameChange(oldName, newName);
		}
		else if (e.AttributeInfo == ClassNameAttribute)
		{
			if (InstanceEntity.ClassName != ClassName)
			{
				IEntityDocument entityDocument = base.DomNode.As<IEntityDocument>();
				if (CivTechService.IsFromActiveProjectOrDependencies(entityDocument.Uri))
				{
					InstanceEntity.ClassName = ClassName;
					CookParameterSet.ValueSet = InstanceEntity.CookParameters;
					OnClassChange();
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Cannot change the class of an entity that is not from the current project \"{0}\"", CivTechService.PrimaryProject.Name);
					ClassName = InstanceEntity.ClassName;
				}
			}
		}
		else if (e.AttributeInfo == DescriptionAttribute)
		{
			InstanceEntity.Description = Description;
		}
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		base.DomNode.As<EditingContext>();
		if (e.ChildInfo.Type == BaseSchema.TextElementType.Type)
		{
			TextElementAdapter textElementAdapter = e.Child.As<TextElementAdapter>();
			if (e.ChildInfo.Name == "Tags")
			{
				InstanceEntity.RemoveTag(textElementAdapter.Text);
			}
		}
	}

	protected void UpdateTextList(IInstanceEntity entity, IEnumerable<string> newText, IList<TextElementAdapter> textList, EventHandler<AttributeEventArgs> handler)
	{
		IList<TextElementAdapter> list = new List<TextElementAdapter>();
		foreach (string tag in newText)
		{
			TextElementAdapter textElementAdapter = textList.FirstOrDefault((TextElementAdapter tea) => tea.Text == tag);
			if (textElementAdapter == null)
			{
				DomNode domNode = new DomNode(BaseSchema.TextElementType.Type);
				domNode.InitializeExtensions();
				textElementAdapter = domNode.As<TextElementAdapter>();
				textElementAdapter.Text = tag;
				if (handler != null)
				{
					domNode.AttributeChanged += handler;
				}
				textList.Add(textElementAdapter);
			}
			list.Add(textElementAdapter);
		}
		foreach (var entryAdapter in textList.Except(list).ToArray())
		{
			textList.Remove(entryAdapter);
			entryAdapter.DomNode.AttributeChanged -= handler;
		}
	}

	protected void UpdateTextList(IInstanceEntity entity, IEnumerable<string> newText, IList<TextElementAdapter> textList, Action<string> entityAddFunction = null, Action<string> entityRemoveFunction = null)
	{
		EventHandler<AttributeEventArgs> handler = null;
		if (entityAddFunction != null && entityRemoveFunction != null)
		{
			handler = delegate(object s, AttributeEventArgs e)
			{
				base.DomNode.As<EditingContext>().DoTransaction(delegate
				{
					entityRemoveFunction(e.OldValue.ToString());
					entityAddFunction(e.NewValue.ToString());
				}, "Update Tag");
			};
		}
		UpdateTextList(entity, newText, textList, handler);
	}

	protected void UpdateDataFiles(IInstanceEntity entity)
	{
		IList<DataFileAdapter> list = new List<DataFileAdapter>();
		foreach (IInstanceDataFile dataFile in entity.DataFiles)
		{
			DataFileAdapter dataFileAdapter = DataFiles.FirstOrDefault((DataFileAdapter udf) => udf.RelativePath == dataFile.RelativePath);
			if (dataFileAdapter == null)
			{
				DomNode domNode = new DomNode(BaseSchema.DataFileType.Type);
				domNode.InitializeExtensions();
				dataFileAdapter = domNode.As<DataFileAdapter>();
				dataFileAdapter.ID = dataFile.ID;
				dataFileAdapter.RelativePath = dataFile.RelativePath;
				DataFiles.Add(dataFileAdapter);
			}
			list.Add(dataFileAdapter);
		}
		foreach (var entryAdapter in DataFiles.Except(list).ToArray())
		{
			DataFiles.Remove(entryAdapter);
		}
	}

	public bool MatchesNameAttribute(AttributeInfo attribute)
	{
		return attribute == NameAttribute;
	}
}
