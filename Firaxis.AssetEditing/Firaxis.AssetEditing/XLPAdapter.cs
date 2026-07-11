using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class XLPAdapter : DomNodeAdapter, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum XLPCommandGroups
	{
		CollectionOperations,
		EntryOperations
	}

	private enum Command
	{
		AddNewEntry,
		AddExistingEntry,
		RemoveEntry,
		ReimportEntry,
		OpenEntry
	}

	private IList<CommandInfo> m_commands = new List<CommandInfo>();

	private ISelectionContext m_selectionContext = new SelectionContext();

	private static StandardCommandTag<Command, XLPAdapter> m_newEntryCommandTag = new StandardCommandTag<Command, XLPAdapter>(Command.AddNewEntry, delegate(XLPAdapter adapter)
	{
		adapter.AddNewEntriesFromCloud();
	}, (XLPAdapter adapter) => adapter.CanAddNewEntriesFromCloud(), delegate
	{
	});

	private static StandardCommandTag<Command, XLPAdapter> m_addEntryCommandTag = new StandardCommandTag<Command, XLPAdapter>(Command.AddExistingEntry, delegate(XLPAdapter adapter)
	{
		adapter.AddEntryFromCloud();
	}, (XLPAdapter adapter) => adapter.CanAddEntryFromCloud(), delegate
	{
	});

	private static StandardCommandTag<Command, XLPAdapter> m_openEntriesCommandTag = new StandardCommandTag<Command, XLPAdapter>(Command.OpenEntry, delegate(XLPAdapter adapter)
	{
		adapter.OpenSelectedEntries();
	}, (XLPAdapter adapter) => adapter.CanOpenSelectedEntries(), delegate
	{
	});

	private static StandardCommandTag<Command, XLPAdapter> m_removeEntriesCommandTag = new StandardCommandTag<Command, XLPAdapter>(Command.RemoveEntry, delegate(XLPAdapter adapter)
	{
		adapter.RemoveSelectedEntries();
	}, (XLPAdapter adapter) => adapter.CanRemoveSelectedEntries(), delegate
	{
	});

	private static StandardCommandTag<Command, XLPAdapter> m_reimportEntriesCommandTag = new StandardCommandTag<Command, XLPAdapter>(Command.ReimportEntry, delegate(XLPAdapter adapter)
	{
		adapter.ReimportSelectedEntries();
	}, (XLPAdapter adapter) => adapter.CanReimportSelectedEntries(), delegate
	{
	});

	public string ClassName
	{
		get
		{
			return GetAttribute<string>(XLPSchema.XLPType.ClassNameAttribute);
		}
		set
		{
			SetAttribute(XLPSchema.XLPType.ClassNameAttribute, value);
		}
	}

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => m_commands;

	public ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

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

	public IList<XLPEntryAdapter> Elements { get; private set; }

	public bool InTransaction => base.DomNode.GetRoot().As<XLPContext>().InTransaction;

	public int PendingOperationCount => base.DomNode.GetRoot().As<XLPContext>().PendingOperationCount;

	public IEnumerable<object> Items => Elements;

	public object LastSelected => m_selectionContext.LastSelected;

	public string ModuleName
	{
		get
		{
			return GetAttribute<string>(XLPSchema.XLPType.ModuleNameAttribute);
		}
		set
		{
			SetAttribute(XLPSchema.XLPType.ModuleNameAttribute, value);
		}
	}

	public string PackageName
	{
		get
		{
			return GetAttribute<string>(XLPSchema.XLPType.PackageNameAttribute);
		}
		set
		{
			SetAttribute(XLPSchema.XLPType.PackageNameAttribute, value);
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in XLPSchema.XLPEntryType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

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

	public int SelectionCount => m_selectionContext.SelectionCount;

	public IXLP XLP { get; set; }

	public IXLPClass XLPClass { get; private set; }

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public event EventHandler SelectionChanged;

	public event EventHandler SelectionChanging;

	public XLPAdapter()
	{
		m_commands.Add(new CommandInfo(m_addEntryCommandTag, StandardMenu.Edit, XLPCommandGroups.CollectionOperations, "Add Existing".Localize("Name of a command"), "Adds an entry for an existing entity to the XLP".Localize(), Keys.None, Firaxis.ATF.Resources.AddExistingEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_newEntryCommandTag, StandardMenu.Edit, XLPCommandGroups.CollectionOperations, "Add New".Localize("Name of a command"), "Creates new entity and adds an entry for it to the XLP.".Localize(), Keys.None, Firaxis.ATF.Resources.AddNewEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_removeEntriesCommandTag, StandardMenu.Edit, XLPCommandGroups.CollectionOperations, "Remove".Localize("Name of a command"), "Removes existing entries from the XLP".Localize(), Keys.None, Sce.Atf.Resources.RemoveImage, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_reimportEntriesCommandTag, StandardMenu.Edit, XLPCommandGroups.EntryOperations, "Reimport".Localize("Name of a command"), "Reimports entities associated with the selected entries.".Localize(), Keys.None, "file_refresh", CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_openEntriesCommandTag, StandardMenu.Edit, XLPCommandGroups.EntryOperations, "Open".Localize("Name of a command"), "Open entities associated with the selected entries.".Localize(), Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		if (this.ItemChanged != null && this.SelectionChanging != null)
		{
			_ = this.SelectionChanged;
		}
	}

	public void AddEntry(string entryName, string entityName)
	{
		if (Elements.FirstOrDefault((XLPEntryAdapter el) => el.EntryID == entryName) != null)
		{
			throw new InvalidTransactionException("Entry " + entryName + " already exists in XLP!");
		}
		DomNode domNode = new DomNode(XLPSchema.XLPEntryType.Type);
		domNode.InitializeExtensions();
		XLPEntryAdapter xLPEntryAdapter = domNode.As<XLPEntryAdapter>();
		xLPEntryAdapter.EntryID = entryName;
		xLPEntryAdapter.ObjectName = entityName;
		Elements.Add(xLPEntryAdapter);
	}

	public void AssignDefaultPlatforms()
	{
		if (HasAnyAllowedPlatform())
		{
			return;
		}
		foreach (Platforms usablePlatform in PlatformsAssistant.GetUsablePlatforms())
		{
			XLP.AllowPlatform(usablePlatform);
		}
	}

	public void Begin(string transactionName)
	{
		base.DomNode.GetRoot().As<XLPContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.GetRoot().As<XLPContext>().Cancel();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.GetRoot().As<ITransactionContext>().SuspendTransactions();
	}

	public bool CanDoCommand(object commandTag)
	{
		return ((StandardCommandTag<Command, XLPAdapter>)commandTag).CanDoCommand(this);
	}

	public void DoCommand(object commandTag)
	{
		((StandardCommandTag<Command, XLPAdapter>)commandTag).DoCommand(this);
	}

	public void End()
	{
		base.DomNode.GetRoot().As<XLPContext>().End();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selectionContext.GetLastSelected<T>();
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selectionContext.GetSelection<T>();
	}

	public void RemoveEntry(string entryName)
	{
		XLPEntryAdapter xLPEntryAdapter = Elements.FirstOrDefault((XLPEntryAdapter el) => el.EntryID == entryName);
		if (xLPEntryAdapter == null)
		{
			throw new InvalidTransactionException("No entry " + entryName + " exists in XLP!");
		}
		Elements.Remove(xLPEntryAdapter);
	}

	public bool SelectionContains(object item)
	{
		return m_selectionContext.SelectionContains(item);
	}

	public void Update(IXLP xlp)
	{
		UnregisterForDomChanges();
		XLP = xlp;
		if (xlp.ClassName != ClassName)
		{
			ClassName = xlp.ClassName;
		}
		if (xlp.Package != PackageName)
		{
			PackageName = xlp.Package;
		}
		UpdateNativeXLPClass();
		IList<XLPEntryAdapter> list = new List<XLPEntryAdapter>();
		foreach (IXLPEntry entry in xlp.XLPEntries)
		{
			XLPEntryAdapter xLPEntryAdapter = Elements.FirstOrDefault((XLPEntryAdapter xea) => xea.EntryID == entry.ID);
			if (xLPEntryAdapter == null)
			{
				DomNode domNode = new DomNode(XLPSchema.XLPEntryType.Type);
				domNode.InitializeExtensions();
				xLPEntryAdapter = domNode.As<XLPEntryAdapter>();
				xLPEntryAdapter.EntryID = entry.ID;
				xLPEntryAdapter.ObjectName = entry.ObjectName;
				Elements.Add(xLPEntryAdapter);
			}
			list.Add(xLPEntryAdapter);
			xLPEntryAdapter.Update(xlp, entry);
		}
		foreach (var entryAdapter in Elements.Except(list).ToArray())
		{
			Elements.Remove(entryAdapter);
		}
		RegisterForDomChanges();
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
		((StandardCommandTag<Command, XLPAdapter>)commandTag).UpdateCommand(this, commandState);
	}

	protected override void OnNodeSet()
	{
		Elements = new DomNodeListAdapter<XLPEntryAdapter>(base.DomNode, XLPSchema.XLPType.EntriesChild);
		XLPContext xLPContext = base.DomNode.As<XLPContext>();
		xLPContext.ItemInserted += MainContext_ItemInserted;
		xLPContext.ItemRemoved += MainContext_ItemRemoved;
		xLPContext.ItemChanged += MainContext_ItemChanged;
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	protected void RaiseItemChanged(object item)
	{
		this.ItemChanged?.Invoke(this, new ItemChangedEventArgs<object>(item));
	}

	protected void RaiseItemInserted(int idx, object inserted)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(idx, inserted));
	}

	protected void RaiseItemRemoved(int idx, object removed)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(idx, removed));
	}

	protected void RaiseReloaded()
	{
		this.Reloaded?.Invoke(this, EventArgs.Empty);
	}

	private static void AddUndoOperation(string name, XLPAdapter adapter, Action action)
	{
		adapter.DomNode.GetRoot().As<XLPContext>().DoTransaction(action, name.Localize());
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		base.DomNode.As<XLPContext>();
		if (e.AttributeInfo == XLPSchema.XLPType.ClassNameAttribute)
		{
			XLP.ClassName = ClassName;
			UpdateNativeXLPClass();
		}
		else if (e.AttributeInfo == XLPSchema.XLPType.PackageNameAttribute)
		{
			XLP.Package = PackageName;
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == XLPSchema.XLPEntryType.Type)
		{
			XLPEntryAdapter xLPEntryAdapter = e.Child.As<XLPEntryAdapter>();
			xLPEntryAdapter.XLPEntry = XLP.AddEntry(xLPEntryAdapter.EntryID, xLPEntryAdapter.ObjectName);
			RaiseItemInserted(e.Index, e.Child);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == XLPSchema.XLPEntryType.Type)
		{
			XLPEntryAdapter xLPEntryAdapter = e.Child.As<XLPEntryAdapter>();
			XLP.RemoveEntry(xLPEntryAdapter.EntryID);
			RaiseItemRemoved(e.Index, e.Child);
		}
	}

	private bool HasAnyAllowedPlatform()
	{
		foreach (Platforms usablePlatform in PlatformsAssistant.GetUsablePlatforms())
		{
			if (XLP.IsPlatformAllowed(usablePlatform))
			{
				return true;
			}
		}
		return false;
	}

	private void MainContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		RaiseItemChanged(e.Item);
	}

	private void MainContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		RaiseItemInserted(e.Index, e.Item);
	}

	private void MainContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		RaiseItemRemoved(e.Index, e.Item);
	}

	private void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomChanges()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	private void UpdateNativeXLPClass()
	{
		XLPDocument xLPDocument = base.DomNode.As<XLPDocument>();
		XLPClass = xLPDocument.CivTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass xlpc) => xlpc.Name == XLP.ClassName);
		if (XLPClass != null)
		{
			ModuleName = XLPClass.CookModuleName;
		}
		else
		{
			ModuleName = "Invalid XLP class!".Localize();
		}
	}
}
