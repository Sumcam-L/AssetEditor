using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class XLPContext : EditingContext, IPropertyEditingContext, IObservableContext, IXLPEditorContext, IDisposable, IInstancingContext
{
	private class XLPEntryCopyData
	{
		public readonly string XLPClassName;

		public readonly string EntryName;

		public readonly string ObjectName;

		public XLPEntryCopyData(string className, string entryName, string objectName)
		{
			XLPClassName = className;
			EntryName = entryName;
			ObjectName = objectName;
		}
	}

	private ControlInfo m_controlInfo;

	private XLPDocument m_document;

	private XLPEditorControl m_gui;

	public AssetBrowserFileCommands AssetBrowserCommands { get; set; }

	public IAssetBrowserDialogService AssetBrowserService { get; set; }

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
			m_controlInfo = value;
		}
	}

	public XLPDocument Doc
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

	public XLPEditorControl GUI { get; internal set; }

	public IImportService ImportService { get; set; }

	public IEnumerable<object> Items => new List<object> { base.DomNode };

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public IEntityCacheService EntityCacheService { get; set; }

	public IPlatformSelectorContext PlatformSelectorContext => base.DomNode.As<PlatformSelectorContext>();

	IPropertyEditingContext IXLPEditorContext.XLPContext => this;

	IPropertyEditingListContext IXLPEditorContext.XLPEntriesContext => base.DomNode.As<IPropertyEditingListContext>();

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public XLPContext()
	{
		_ = this.Reloaded;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	public bool CanCopy()
	{
		return base.Selection.Any();
	}

	public bool CanDelete()
	{
		return base.Selection.Any();
	}

	public bool CanInsert(object dataObject)
	{
		if (!(dataObject is IDataObject dataObject2))
		{
			return false;
		}
		if (!(dataObject2.GetData(typeof(List<XLPEntryCopyData>)) is List<XLPEntryCopyData> list))
		{
			return false;
		}
		if (list.Count == 0)
		{
			return false;
		}
		XLPEntryCopyData xLPEntryCopyData = list[0];
		IXLP xLP = m_document.XLP;
		return xLPEntryCopyData.XLPClassName == xLP.ClassName;
	}

	public object Copy()
	{
		List<XLPEntryCopyData> list = new List<XLPEntryCopyData>();
		string className = m_document.XLP.ClassName;
		foreach (XLPEntryAdapter item2 in base.Selection)
		{
			XLPEntryCopyData item = new XLPEntryCopyData(className, item2.EntryID, item2.ObjectName);
			list.Add(item);
		}
		return list;
	}

	public void Delete()
	{
		ICollection<string> entryNamesToRemove = new List<string>();
		foreach (XLPEntryAdapter item in base.Selection)
		{
			entryNamesToRemove.Add(item.EntryID);
		}
		IXLP xlp = m_document.XLP;
		Action transaction = delegate
		{
			foreach (string item2 in entryNamesToRemove)
			{
				xlp.RemoveEntry(item2);
			}
		};
		string text = string.Format("Removed the following entries: {0}", string.Join(", ", entryNamesToRemove));
		this.DoTransaction(transaction, text);
		Outputs.WriteLine(OutputMessageType.Info, text);
	}

	public void Insert(object dataObject)
	{
		if (CanDelete())
		{
			Delete();
		}
		List<XLPEntryCopyData> sourceData = (dataObject as IDataObject).GetData(typeof(List<XLPEntryCopyData>)) as List<XLPEntryCopyData>;
		IXLP xlp = m_document.XLP;
		IEnumerable<XLPEntryCopyData> uniqueData = DuplicateXLPEntryData(sourceData, xlp);
		ICollection<string> values = uniqueData.Select((XLPEntryCopyData data) => data.EntryName).ToList();
		Action transaction = delegate
		{
			foreach (XLPEntryCopyData item in uniqueData)
			{
				xlp.AddEntry(item.EntryName, item.ObjectName);
			}
		};
		string text = string.Format("Pasted the following entries: {0}", string.Join(", ", values));
		this.DoTransaction(transaction, text);
		Outputs.WriteLine(OutputMessageType.Info, text);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (bDisposing && m_gui != null)
		{
			m_gui.Dispose();
			m_gui = null;
		}
	}

	protected override void OnCancelled()
	{
		base.DomNode.As<XLPAdapter>().Update(m_document.XLP);
	}

	protected override void OnEnded()
	{
		IDisposable disposable = null;
		if (Doc.IsReadOnly)
		{
			disposable = SuspendRecording();
		}
		base.OnEnded();
		disposable?.Dispose();
		if (Doc.IsReadOnly && InTransaction)
		{
			MessageBoxes.Show("Can not modify assets that are not part of the active project \"" + CivTechService.PrimaryProject.Name + "\"", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException("Can not modify assets that are not part of the active project \"" + CivTechService.PrimaryProject.Name + "\"");
		}
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child));
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child));
	}

	private IEnumerable<XLPEntryCopyData> DuplicateXLPEntryData(List<XLPEntryCopyData> sourceData, IXLP targetXLP)
	{
		ICollection<XLPEntryCopyData> collection = new List<XLPEntryCopyData>(sourceData.Count);
		foreach (XLPEntryCopyData sourceDatum in sourceData)
		{
			string entryName = XLPAdapterExtensions.GenerateUniqueEntryName(sourceDatum.EntryName, targetXLP);
			XLPEntryCopyData item = new XLPEntryCopyData(sourceDatum.XLPClassName, entryName, sourceDatum.ObjectName);
			collection.Add(item);
		}
		return collection;
	}
}
