using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.ATF;

public class EntityShadowDocument : IShadowEntityDocument, IEntityDocument, IProjectSpecificDocument, IDocument, IResource, IShadowDocument, IInvisibleDocument, IImportableDocument
{
	private IInstanceEntity m_instanceEntity;

	public ICivTechService CivTechService { get; set; }

	public string Type => "ShadowDocument";

	public Uri Uri { get; set; }

	public bool Dirty
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool IsReadOnly => false;

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

	public event EventHandler DirtyChanged;

	public event EventHandler<UriChangedEventArgs> UriChanged;

	public EntityShadowDocument()
	{
		if (this.UriChanged != null)
		{
			_ = this.DirtyChanged;
		}
	}

	public bool IsReadyForExport(ICollection<string> errorMessages)
	{
		if (HasSourceFileSet(errorMessages))
		{
			return HasValidSourceFile(errorMessages);
		}
		return false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && InstanceSet != null)
		{
			InstanceSet.Dispose();
			InstanceSet = null;
		}
	}

	private bool HasSourceFileSet(ICollection<string> errorMessages)
	{
		if (InstanceEntity is IImportedEntity importedEntity && string.IsNullOrWhiteSpace(importedEntity.SourceFilePath))
		{
			errorMessages.Add("Cannot import a document unless it has a source file path set.");
			return false;
		}
		return true;
	}

	private bool HasValidSourceFile(ICollection<string> errorMessages)
	{
		if (InstanceEntity is IImportedEntity importedEntity)
		{
			if (!File.Exists(CivTechService.PrimaryProject.VersionControl.GetLocalPath(importedEntity.SourceFilePath)))
			{
				errorMessages.Add("I can't find the source file for '" + importedEntity.Name + "': \n\n" + importedEntity.SourceFilePath);
				return false;
			}
			return true;
		}
		return false;
	}
}
