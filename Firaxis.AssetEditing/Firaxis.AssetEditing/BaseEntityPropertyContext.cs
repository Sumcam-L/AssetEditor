using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BaseEntityPropertyContext : EditingContext, IEntityEditorContext, IObservableContext, IPropertyEditingContext, IDisposable
{
	private ControlInfo m_controlInfo;

	private BaseInstanceEntityDocument m_document;

	public AssetBrowserFileCommands AssetDocumentCommands { get; set; }

	public ICivTechService CivTechService { get; set; }

	public IFileWatcherService FileWatchService { get; set; }

	public ControlInfo ControlInfo
	{
		get
		{
			return m_controlInfo;
		}
		set
		{
			if (m_controlInfo != value)
			{
				m_controlInfo = value;
			}
		}
	}

	public IPropertyEditingListContext CookParametersContext => base.DomNode.As<InstanceEntityAdapter>().CookParameterSet;

	public BaseInstanceEntityDocument Doc
	{
		get
		{
			return m_document;
		}
		set
		{
			if (m_document != value)
			{
				m_document = value;
			}
		}
	}

	public IDocumentRegistryMediator DocumentRegistryMediator { get; set; }

	public IPropertyEditingContext EntityContext => this;

	public EntityEditorControlBase GUI { get; set; }

	public bool HasCookParameters
	{
		get
		{
			InstanceEntityAdapter instanceEntityAdapter = base.DomNode.As<InstanceEntityAdapter>();
			return global::DatabaseWrapper.DatabaseWrapper.GetClass(CivTechService.PrimaryProject.Name, instanceEntityAdapter.InstanceEntity)?.CookParameters.Items.Any() ?? false;
		}
	}

	public IImportService ImportService { get; set; }

	public virtual IEnumerable<object> Items
	{
		get
		{
			yield return base.DomNode;
		}
	}

	public virtual IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public BatchEntitySourceControlService SourceControl { get; set; }

	public IEntityCacheService EntityCacheService { get; set; }

	public ITunerQueueService TunerQueueService { get; set; }

	public bool HotLoadOnReimport => HotLoadService?.HotLoadOnReimport ?? true;

	public IHotLoadService HotLoadService { get; set; }

	public IEntityChangeList BatchChangelist => base.DomNode.As<IEntityPreviewComponent>()?.EntityChanges;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public BaseEntityPropertyContext()
	{
		_ = this.Reloaded;
	}

	public virtual void OnReloaded()
	{
		this.Reloaded.Raise(this, new EventArgs());
	}

	public virtual void PerformReload()
	{
		UnregisterForDomChanges();
		base.DomNode?.As<InstanceEntityAdapter>()?.Update();
		OnReloaded();
		RegisterForDomChanges();
	}

	protected virtual void OnReloaded(object sender, EventArgs args)
	{
		this.Reloaded.Raise(this, args);
	}

	protected virtual void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
	}

	protected override void OnBeginning()
	{
		if (Doc != null && Doc.IsReadOnly)
		{
			string message = "Can not modify assets that are not part of the active project \"" + Doc.CivTechService.PrimaryProject.Name + "\"";
			MessageBoxes.Show(message, "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException(message);
		}
		base.OnBeginning();
	}

	protected override void OnEnded()
	{
		IDisposable disposable = null;
		if (Doc != null && Doc.IsReadOnly)
		{
			disposable = SuspendRecording();
		}
		base.OnEnded();
		disposable?.Dispose();
		if (Doc != null && Doc.IsReadOnly && InTransaction)
		{
			MessageBoxes.Show("Can not modify assets that are not part of the active project \"" + Doc.CivTechService.PrimaryProject.Name + "\"", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException("Can not modify assets that are not part of the active project \"" + Doc.CivTechService.PrimaryProject.Name + "\"");
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterForDomChanges();
	}

	private void RegisterForDomChanges()
	{
		UnregisterForDomChanges();
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	private void UnregisterForDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			GUI?.Bind(null);
			GUI?.Dispose();
			GUI = null;
		}
	}
}
