using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Collections;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BaseInstanceEntityDocument : CompositeDomDocument, IInstanceEntityDocument, IImportableDocument, IEntityDocument, IProjectSpecificDocument, IDocument, IResource, IVersionProvider
{
	private Image m_dirtyImage;

	private IImportService m_importService;

	private IInstanceEntity m_instanceEntity;

	public ICivTechService CivTechService { get; set; }

	public IDocumentClient DocumentClient { get; set; }

	public IImportService ImportService
	{
		get
		{
			return m_importService;
		}
		set
		{
			if (m_importService != null)
			{
				m_importService.DocumentImported -= HandleDocumentImported;
			}
			m_importService = value;
			if (m_importService != null)
			{
				m_importService.DocumentImported += HandleDocumentImported;
			}
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			if (InstanceEntity == null)
			{
				return false;
			}
			if (!CivTechService.IsFromActiveProjectOrDependencies(Uri))
			{
				return true;
			}
			if (!CivTechService.IsFromModDependencies(Uri))
			{
				if (!VersionService.IsLocalBuild())
				{
					return Version > VersionService.ApplicationVersion;
				}
				return false;
			}
			return true;
		}
	}

	public IAssetPreviewer PreviewerService => base.DomNode.As<InstanceEntityAdapter>()?.PreviewerService;

	public override string Type => DocumentClient.Info.FileType;

	public IVersionService VersionService { get; set; }

	protected string GamePantry => CivTechService.PrimaryProject.Paths.GamePantry;

	protected IProjectConfig ProjectConfig => CivTechService.PrimaryProject.Config;

	protected string ProjectName => CivTechService.PrimaryProject.Name;

	public IInstanceEntity InstanceEntity
	{
		get
		{
			return m_instanceEntity;
		}
		set
		{
			if (m_instanceEntity != value)
			{
				m_instanceEntity = value;
			}
		}
	}

	public IInstanceSet InstanceSet { get; set; }

	public Version Version => InstanceEntity.Version;

	public bool IsReadyForExport(ICollection<string> errorMessages)
	{
		bool flag = IsImportableEntity(errorMessages) && HasSourceFileSet(errorMessages) && HasValidSourceFile(errorMessages);
		if (Dirty && flag)
		{
			flag = HasValidSourceObject(errorMessages);
		}
		return flag;
	}

	public bool HasNameSet(string startingName)
	{
		if (InstanceEntity == null)
		{
			return false;
		}
		string name = InstanceEntity.Name;
		if (name.StartsWith(startingName, StringComparison.CurrentCultureIgnoreCase))
		{
			return false;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Uri.LocalPath);
		return Path.Combine(Path.GetDirectoryName(Uri.LocalPath), fileNameWithoutExtension).EndsWith(name);
	}

	public bool IsBasedOnEntity(InstanceType entityType, string entityName)
	{
		if (InstanceEntity.Type == entityType)
		{
			return InstanceEntity.Name == entityName;
		}
		return false;
	}

	public virtual bool MeetsSavePreconditions(ICollection<string> errorMessages)
	{
		if (EntityIsValid(errorMessages))
		{
			return ClassIsValid(errorMessages);
		}
		return false;
	}

	public override void UpdateControlInfo()
	{
		BaseEntityPropertyContext baseEntityPropertyContext = this.As<BaseEntityPropertyContext>();
		if (baseEntityPropertyContext.ControlInfo != null)
		{
			string localPath = Uri.LocalPath;
			string text = ((InstanceEntity != null) ? (InstanceEntity.Name + InstanceEntity.XMLExtension) : Path.GetFileName(localPath));
			if (Dirty)
			{
				text += "*";
			}
			if (IsReadOnly)
			{
				text += "(Read Only)";
			}
			baseEntityPropertyContext.ControlInfo.Name = text;
			baseEntityPropertyContext.ControlInfo.Description = localPath;
		}
	}

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			ImportService = null;
			base.DomNode.AttributeChanged -= HandleNameChange;
			base.DomNode.As<BaseEntityPropertyContext>().Dispose();
		}
	}

	protected virtual void DocumentSpecificReimportStep()
	{
	}

	protected virtual void HandleDocumentImported(object sender, DocumentImportedEventArgs e)
	{
		if (e.Document != this || !e.Successful)
		{
			return;
		}
		InstanceEntityAdapter adapter = base.DomNode.As<InstanceEntityAdapter>();
		TransactionContext transactionContext = base.DomNode.As<TransactionContext>();
		HistoryContext historyContext = base.DomNode.As<HistoryContext>();
		if (adapter != null)
		{
			using (historyContext?.SuspendRecording())
			{
				if (transactionContext == null || transactionContext.InTransaction)
				{
					adapter.Update(updateUI: true);
				}
				else
				{
					transactionContext.DoTransaction(delegate
					{
						adapter.Update();
					}, "Reconcile adapter state on import.");
				}
			}
			Dirty = true;
		}
		DocumentSpecificReimportStep();
	}

	protected override void OnDirtyChanged(EventArgs e)
	{
		UpdateControlInfo();
		base.OnDirtyChanged(e);
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += HandleNameChange;
	}

	protected override void OnUriChanged(UriChangedEventArgs e)
	{
		UpdateControlInfo();
		UpdateAdapterNameFromURI();
		base.OnUriChanged(e);
	}

	private bool ClassIsValid(ICollection<string> errorMessages)
	{
		IClassEntity classEntity = ProjectConfig.Classes.FindForInstance(InstanceEntity);
		if (classEntity == null)
		{
			errorMessages.Add("Assigned class cannot be found in the project config.");
			return false;
		}
		return HasAllDataFiles(classEntity, errorMessages);
	}

	private bool EntityIsValid(ICollection<string> errorMessages)
	{
		if (InstanceEntity == null || string.IsNullOrEmpty(InstanceEntity.ClassName))
		{
			errorMessages.Add("Cannot save an entity without a class being assigned.");
			return false;
		}
		return true;
	}

	private Image GetDirtyImage()
	{
		if (m_dirtyImage == null)
		{
			m_dirtyImage = ImageHelper.GetSolidTriangleImage(Color.Red, 16);
		}
		return m_dirtyImage;
	}

	private Uri GetNewUri(string newName)
	{
		InstanceEntityAdapter instanceEntityAdapter = base.DomNode.As<InstanceEntityAdapter>();
		if (instanceEntityAdapter == null)
		{
			BugSubmitter.SilentAssert(instanceEntityAdapter != null, "Failed to convert an instance entity document to an instance entity adapter!  @assign bwhitman");
			return null;
		}
		InstanceType instanceType = instanceEntityAdapter.InstanceType;
		string projectNameFromUri = CivTechService.ProjectMapService.GetProjectNameFromUri(Uri);
		ProjectEnvironment project = CivTechService.AllProjectsMap[projectNameFromUri];
		return new Uri(CivTechService.ProjectMapService.LayeredPantry.GetPantryPath(newName, instanceType, project));
	}

	private void HandleNameChange(object sender, AttributeEventArgs e)
	{
		InstanceEntityAdapter instanceEntityAdapter = base.DomNode.As<InstanceEntityAdapter>();
		BugSubmitter.Assert(instanceEntityAdapter != null, "Unable to convert the Document DomNode to an InstanceEntityAdapter.  Ensure that the ATF extensions are properly configured. @assign bwhitman");
		if (instanceEntityAdapter.MatchesNameAttribute(e.AttributeInfo))
		{
			Uri newUri = GetNewUri((string)e.NewValue);
			if (!Uri.Equals(newUri))
			{
				Uri = newUri;
			}
		}
	}

	private bool HasAllDataFiles(IClassEntity classEntity, ICollection<string> errorMessages)
	{
		if (classEntity.DataFiles.Any())
		{
			foreach (IClassDataFile classDataFile in classEntity.DataFiles)
			{
				if (!InstanceEntity.DataFiles.Any((IInstanceDataFile df) => df.ID == classDataFile.ID))
				{
					errorMessages.Add("Entity is missing expected data file with the ID of {0} and the extension of {1}.\nEnsure a source file is selected and perform an import (Ctrl+Shift+I).", classDataFile.ID, classDataFile.Extension);
					return false;
				}
			}
		}
		return true;
	}

	private bool HasSourceFileSet(ICollection<string> errorMessages)
	{
		if (string.IsNullOrWhiteSpace(base.DomNode.As<ImportedEntityAdapter>().SourceFilePath))
		{
			errorMessages.Add("Cannot import a document unless it has a source file path set.");
			return false;
		}
		return true;
	}

	private bool HasValidSourceFile(ICollection<string> errorMessages)
	{
		ImportedEntityAdapter importedEntityAdapter = base.DomNode.As<ImportedEntityAdapter>();
		if (importedEntityAdapter != null)
		{
			if (!File.Exists(CivTechService.PrimaryProject.VersionControl.GetLocalPath(importedEntityAdapter.SourceFilePath)))
			{
				errorMessages.Add("I can't find the source file for '" + importedEntityAdapter.Name + "': \n\n" + importedEntityAdapter.SourceFilePath);
				return false;
			}
			return true;
		}
		return false;
	}

	private bool HasValidSourceObject(ICollection<string> errorMessages)
	{
		ImportedEntityAdapter importedEntityAdapter = base.DomNode.As<ImportedEntityAdapter>();
		string value = importedEntityAdapter.SourceObjectName.Trim();
		IEnumerable<string> source = from obj in importedEntityAdapter.GetCurrentSourceObjects()
			select obj.Trim();
		int num;
		if (source.Any() || !string.IsNullOrEmpty(value))
		{
			num = (source.Contains(value) ? 1 : 0);
			if (num == 0)
			{
				errorMessages.Add("You must select a source object before performing an export.");
			}
		}
		else
		{
			num = 1;
		}
		return (byte)num != 0;
	}

	private bool IsImportableEntity(ICollection<string> errorMessages)
	{
		if (base.DomNode.As<ImportedEntityAdapter>() == null)
		{
			errorMessages.Add("The document that is open does not support import.");
			return false;
		}
		return true;
	}

	private void UpdateAdapterNameFromURI()
	{
		if (CivTechService != null)
		{
			string localPath = Uri.LocalPath;
			EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, localPath);
			global::DatabaseWrapper.DatabaseWrapper.CreateInstanceEntitySubdirectories(ProjectName, entityIDFromPath);
			this.As<INamedAdapter>().Name = entityIDFromPath.Name;
		}
	}
}
