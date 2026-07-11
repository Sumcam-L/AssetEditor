using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public abstract class ImportedEntityAdapter : InstanceEntityAdapter, ISourceObjectAdapter
{
	private SourceFileModel m_currentSourceFile;

	private List<string> m_currentSourceObjects = new List<string>();

	private bool m_userChangedName;

	public DateTime ExportedTime
	{
		get
		{
			return GetAttribute<DateTime>(ExportedTimeAttribute);
		}
		protected set
		{
			SetAttribute(ExportedTimeAttribute, value);
		}
	}

	public abstract IImportedEntity ImportedEntity { get; }

	public DateTime ImportedTime
	{
		get
		{
			return GetAttribute<DateTime>(ImportedTimeAttribute);
		}
		protected set
		{
			SetAttribute(ImportedTimeAttribute, value);
		}
	}

	public string SourceFilePath
	{
		get
		{
			return GetChild<EntitySourceFilePathAdapter>(SourceFilePathChild).Path;
		}
		set
		{
			GetChild<EntitySourceFilePathAdapter>(SourceFilePathChild).Path = value;
		}
	}

	public string SourceObjectName
	{
		get
		{
			return GetAttribute<string>(SourceObjectNameAttribute);
		}
		set
		{
			SetAttribute(SourceObjectNameAttribute, value);
		}
	}

	protected abstract AttributeInfo ExportedTimeAttribute { get; }

	protected abstract AttributeInfo ImportedTimeAttribute { get; }

	protected abstract ChildInfo SourceFilePathChild { get; }

	protected abstract AttributeInfo SourceObjectNameAttribute { get; }

	public string[] GetCurrentSourceObjects()
	{
		if (m_currentSourceObjects.Count == 0)
		{
			IContentExporter exporter = ExporterService.GetExporter(Path.GetExtension(SourceFilePath));
			if (exporter != null && exporter.SupportedInstanceTypes.Contains(InstanceType))
			{
				string localPath = base.CivTechService.PrimaryProject.VersionControl.GetLocalPath(SourceFilePath);
				IEnumerable<string> collection = from x in exporter.GetSourceObjectNames(localPath, InstanceType)
					orderby x
					select x;
				m_currentSourceObjects.AddRange(collection);
			}
		}
		return m_currentSourceObjects.ToArray();
	}

	public string GetSmartName(string sourceFilePath)
	{
		SourceFileModel sourceFileModel = new SourceFileModel(sourceFilePath, InstanceType);
		if (sourceFileModel.SourceObjects.Count() == 1)
		{
			return sourceFileModel.SourceObjects.First().SmartName;
		}
		return Name;
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (SourceFilePath != ImportedEntity.SourceFilePath)
		{
			SourceFilePath = ImportedEntity.SourceFilePath;
		}
		if (SourceObjectName != ImportedEntity.SourceObjectName)
		{
			SourceObjectName = ImportedEntity.SourceObjectName;
		}
		try
		{
			if (ImportedTime != ImportedEntity.ImportedTime)
			{
				ImportedTime = ImportedEntity.ImportedTime;
			}
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Entity's imported time has not been set. Reason=" + ex.Message);
		}
		try
		{
			if (ExportedTime != ImportedEntity.ExportedTime)
			{
				ExportedTime = ImportedEntity.ExportedTime;
			}
		}
		catch (ArgumentOutOfRangeException ex2)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Entity's exported time has not been set. Reason=" + ex2.Message);
		}
	}

	protected virtual bool CanPerformImport()
	{
		if (base.DomNode.As<IEntityDocument>().IsReadOnly)
		{
			return false;
		}
		bool num = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(InstanceEntity) != null;
		bool flag = HasSourceFile();
		bool flag2 = HasSourceObject();
		if (!num)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Cannot perform an import because there is no valid class assigned to the entity.");
		}
		if (!flag)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Cannot perform an import because the source file is not set or does not exist on disk.");
		}
		if (!flag2)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Cannot perform an import because the source file object has not been set.");
		}
		return num && flag && flag2;
	}

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (IsMyAttribute(e.AttributeInfo))
		{
			IEntityDocument entityDocument = base.DomNode.As<IEntityDocument>();
			EditingContext editingContext = base.DomNode.As<EditingContext>();
			if (entityDocument.IsReadOnly && editingContext != null && editingContext.InTransaction)
			{
				MessageBoxes.Show("Can not modify assets that are not part of the active project \"" + base.CivTechService.PrimaryProject.Name + "\"", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new InvalidTransactionException("Can not modify assets that are not part of the active project \"" + base.CivTechService.PrimaryProject.Name + "\"");
			}
		}
		base.DomNode.As<EditingContext>();
		bool flag = false;
		bool flag2 = false;
		if (e.AttributeInfo == NameAttribute)
		{
			m_userChangedName = true;
		}
		else if (e.AttributeInfo == SourceObjectNameAttribute)
		{
			flag = true;
			if (ImportedEntity.SourceObjectName != SourceObjectName)
			{
				flag2 = true;
				if (GetCurrentSourceObjects().Contains(SourceObjectName))
				{
					ImportedEntity.SourceObjectName = SourceObjectName;
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Cannot switch Source Object to {0} because it is not available in the current source file.", SourceObjectName);
					SourceObjectName = ImportedEntity.SourceObjectName;
				}
			}
		}
		else if (e.AttributeInfo == EntitySchema.EntitySourceFilePathType.PathAttribute)
		{
			flag = true;
			if (ImportedEntity.SourceFilePath != SourceFilePath)
			{
				if (string.IsNullOrEmpty(SourceFilePath))
				{
					SourceFilePath = ImportedEntity.SourceFilePath;
					Outputs.WriteLine(OutputMessageType.Error, "Cannot clear the source file.  If you are trying to reimport the file, press the Reimport button (or CTRL + Shift+ I) to perform an import.");
				}
				else if (!File.Exists(SourceFilePath))
				{
					Outputs.WriteLine(OutputMessageType.Error, "No file exists at path ({0}).  Reverting to previous value.", SourceFilePath);
					SourceFilePath = ImportedEntity.SourceFilePath;
				}
				else
				{
					m_currentSourceFile = new SourceFileModel(SourceFilePath, InstanceType);
					if (ExporterService.GetExporter(Path.GetExtension(SourceFilePath), InstanceType) == null)
					{
						Outputs.WriteLine(OutputMessageType.Error, "Cannot use {0} as a source file as there is not a valid exporter for that file type and this instance type.", SourceFilePath);
						SourceFilePath = ImportedEntity.SourceFilePath;
						return;
					}
					m_currentSourceObjects.Clear();
					m_currentSourceObjects.AddRange(m_currentSourceFile.SourceObjects.Select((SourceObjectModel obj) => obj.SourceObjectName));
					ImportedEntity.SourceFilePath = SourceFilePath;
					if (m_currentSourceObjects.Count == 1)
					{
						flag2 = true;
						string sourceObjectName = m_currentSourceObjects[0];
						ImportedEntity.SourceObjectName = sourceObjectName;
						UnregisterFromDomChanges();
						SourceObjectName = sourceObjectName;
						RegisterForDomChanges();
					}
					else if (string.IsNullOrEmpty(ImportedEntity.SourceObjectName) && !m_currentSourceObjects.Contains(ImportedEntity.SourceObjectName))
					{
						ImportedEntity.SourceObjectName = string.Empty;
					}
				}
			}
		}
		if (flag)
		{
			if (flag2)
			{
				AutomaticallySetName();
				PerformImport();
			}
		}
		else
		{
			base.HandleDomNodeAttributeChanged(sender, e);
		}
	}

	protected override void OnClassChange()
	{
		if (!string.IsNullOrEmpty(SourceFilePath) && CanPerformImport())
		{
			PerformImport();
		}
		base.OnClassChange();
	}

	protected override void OnNodeSet()
	{
		DomNode domNode = new DomNode(EntitySchema.EntitySourceFilePathType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(SourceFilePathChild, domNode);
		base.OnNodeSet();
	}

	protected virtual void PerformImport()
	{
		base.DomNode.As<BaseEntityPropertyContext>().ImportService.Import(base.DomNode.As<IInstanceEntityDocument>());
	}

	private void AutomaticallySetName()
	{
		if (m_canChangeClass && !m_userChangedName && m_currentSourceFile != null)
		{
			SourceObjectModel sourceObjectModel = m_currentSourceFile.SourceObjects.FirstOrDefault((SourceObjectModel model) => model.SourceObjectName == SourceObjectName);
			if (sourceObjectModel != null)
			{
				Name = sourceObjectModel.SmartName;
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Selected a source object that does not exist in the source file.  Cannot update the entity name automatically.");
			}
		}
	}

	private bool HasSourceFile()
	{
		string text = SourceFilePath;
		if (!string.IsNullOrEmpty(SourceFilePath))
		{
			text = base.CivTechService.PrimaryProject.VersionControl.GetLocalPath(SourceFilePath);
		}
		if (!string.IsNullOrEmpty(text))
		{
			return File.Exists(text);
		}
		return false;
	}

	private bool HasSourceObject()
	{
		if (m_currentSourceObjects.Count > 1)
		{
			return !string.IsNullOrEmpty(SourceObjectName);
		}
		return true;
	}

	private bool IsMyAttribute(AttributeInfo ai)
	{
		if (ai != NameAttribute && ai != SourceObjectNameAttribute && ai != ImportedTimeAttribute && ai != ExportedTimeAttribute)
		{
			return ai == EntitySchema.EntitySourceFilePathType.PathAttribute;
		}
		return true;
	}
}
