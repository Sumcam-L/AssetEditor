using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefContext : EditingContext, IObservableContext, ISelectionContext, IInstancingContext, IDisposable
{
	private class ArtDefElementCopyData
	{
		public readonly string ArtDefElementName;

		public readonly string ArtDefElementXML;

		public readonly IArtDefElementTemplate ElementTemplate;

		public readonly Uri SourceUri;

		public ArtDefElementCopyData(string name, string xml, IArtDefElementTemplate template, Uri sourceUri)
		{
			ArtDefElementName = name;
			ArtDefElementXML = xml;
			ElementTemplate = template;
			SourceUri = sourceUri;
		}
	}

	private ArtDefDocument m_document;

	private ControlInfo m_controlInfo;

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

	public ArtDefDocument Doc
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

	public ArtDefSetTreeLister GUI { get; set; }

	public ICommandService CommandService { get; internal set; }

	public IDocumentRegistry DocumentRegistry { get; internal set; }

	public IControlHostService ControlHostService { get; internal set; }

	public IContextRegistry ContextRegistry { get; internal set; }

	public ArtDefCommands Commands { get; internal set; }

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public ArtDefContext()
	{
		base.UndoSelection = false;
		_ = this.Reloaded;
	}

	protected override void OnNodeSet()
	{
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.As<ArtDefSetAdapter>().TemplateChanged += ArtDefContext_TemplateChanged;
		base.OnNodeSet();
	}

	private void ArtDefContext_TemplateChanged(object sender, ItemChangedEventArgs<string> e)
	{
		if (base.Selection.LastSelected != null)
		{
			base.Selection.Clear();
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (base.Selection.LastSelected != null && ((AdaptablePath<object>)base.Selection.LastSelected).Last == e.Child)
		{
			base.Selection.Clear();
		}
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child));
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child));
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	public static IList<string> GetLineage(DomNode dn)
	{
		DomNode domNode = dn;
		IList<string> list = new List<string>();
		while (domNode != null)
		{
			INamedAdapter namedAdapter = domNode.As<INamedAdapter>();
			if (namedAdapter != null)
			{
				list.Insert(0, namedAdapter.Name);
			}
			domNode = domNode.Parent;
		}
		return list;
	}

	public static string GenerateUniqueName(IEnumerable<IArtDefElement> elements, string baseName)
	{
		int num = 0;
		string uniqueName = baseName;
		while (elements.Any((IArtDefElement element) => element.Name == uniqueName))
		{
			num++;
			uniqueName = baseName + num.ToString("D3");
		}
		return uniqueName;
	}

	public static string GenerateUniqueName(IEnumerable<IValue> values, string baseName)
	{
		int num = 0;
		string uniqueName = string.Empty;
		do
		{
			num++;
			uniqueName = baseName + num.ToString("D3");
		}
		while (values.Any((IValue value) => value.ParameterName == uniqueName));
		return uniqueName;
	}

	public static IArtDefTemplate FindArtDefTemplate(IProjectConfig projConfig, string artDefTmplName)
	{
		return projConfig.ArtDefTemplates.Items.FirstOrDefault((IArtDefTemplate elem) => elem.Name == artDefTmplName);
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
			MessageBoxes.Show("Can not modify assets that are not part of the active project \"" + Doc.CivTechService.PrimaryProject.Name + "\"", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException("Can not modify assets that are not part of the active project");
		}
	}

	public bool CanCopy()
	{
		if (base.Selection.LastSelected.Is<ArtDefElementAdapter>())
		{
			return true;
		}
		if (base.Selection.LastSelected.Is<ArtDefCollectionAdapter>())
		{
			if (base.Selection.LastSelected.Is<ArtDefElementAdapter>())
			{
				return true;
			}
			if (base.Selection.LastSelected.Is<ArtDefCollectionAdapter>())
			{
				return GUI.GetCurrentSubselection().Any();
			}
		}
		return false;
	}

	public object Copy()
	{
		object lastSelected = base.Selection.LastSelected;
		if (lastSelected.Is<ArtDefElementAdapter>() && base.Selection.Count == 1)
		{
			return CreateCopyData(lastSelected);
		}
		if (lastSelected.Is<ArtDefElementAdapter>())
		{
			return CopySelection();
		}
		if (lastSelected.Is<ArtDefCollectionAdapter>())
		{
			IList<ArtDefElementCopyData> list = new List<ArtDefElementCopyData>();
			{
				foreach (object item2 in GUI.GetCurrentSubselection())
				{
					ArtDefElementCopyData item = CreateCopyData(item2);
					list.Add(item);
				}
				return list;
			}
		}
		return null;
	}

	public IEnumerable<object> CopySelection()
	{
		List<object> list = new List<object>();
		foreach (object item2 in base.Selection)
		{
			if (item2.Is<ArtDefElementAdapter>())
			{
				list.Add(CreateCopyData(item2));
			}
			else
			{
				if (!item2.Is<ArtDefCollectionAdapter>())
				{
					continue;
				}
				IList<ArtDefElementCopyData> list2 = new List<ArtDefElementCopyData>();
				foreach (object item3 in GUI.GetCurrentSubselection())
				{
					ArtDefElementCopyData item = CreateCopyData(item3);
					list2.Add(item);
				}
				list.Add(list2);
			}
		}
		return list;
	}

	private ArtDefElementCopyData CreateCopyData(object elementAdapter)
	{
		ArtDefElementAdapter artDefElementAdapter = elementAdapter.As<ArtDefElementAdapter>();
		bool condition = artDefElementAdapter != null;
		string fmtText = "Attempted to copy element of type \"{0}\" in ArtDefContext @summary Element not of type ArtDefElementAdater! @assign bwhitman";
		object[] array = new object[1];
		int num = 0;
		object obj;
		if (elementAdapter == null)
		{
			obj = null;
		}
		else
		{
			Type type = elementAdapter.GetType();
			obj = ((type != null) ? type.Name : null);
		}
		array[num] = obj;
		BugSubmitter.Assert(condition, fmtText, array);
		IArtDefElement artDefElement = artDefElementAdapter.ArtDefElement;
		string name = artDefElement.Name;
		string xml = artDefElement.SerializeToXML();
		IArtDefElementTemplate artDefElementTmpl = artDefElementAdapter.ArtDefElementTmpl;
		IDocument document = artDefElementAdapter.DomNode.GetRoot().As<IDocument>();
		BugSubmitter.Assert(document != null, "Element adapter \"{0}\" does not have a document root node @summary ArtDefElement adapter does not have a document root node @assign bwhitman", name);
		return new ArtDefElementCopyData(name, xml, artDefElementTmpl, document.Uri);
	}

	private ArtDefElementCopyData GetFirstObject(IDataObject datObj)
	{
		ArtDefElementCopyData artDefElementCopyData = datObj.GetData(typeof(ArtDefElementCopyData)) as ArtDefElementCopyData;
		if (artDefElementCopyData == null)
		{
			if (datObj.GetData(typeof(List<ArtDefElementCopyData>)) is List<ArtDefElementCopyData> { Count: >0 } list)
			{
				artDefElementCopyData = list[0];
			}
			else if (datObj.GetData(typeof(List<object>)) is List<object> { Count: >0 } list2)
			{
				artDefElementCopyData = list2[0] as ArtDefElementCopyData;
			}
		}
		return artDefElementCopyData;
	}

	private IEnumerable<ArtDefElementCopyData> GetAllAdapters(IDataObject datObj)
	{
		if (!(datObj.GetData(typeof(ArtDefElementCopyData)) is ArtDefElementCopyData artDefElementCopyData))
		{
			if (!(datObj.GetData(typeof(List<ArtDefElementCopyData>)) is List<ArtDefElementCopyData> list))
			{
				yield break;
			}
			foreach (ArtDefElementCopyData item in list)
			{
				yield return item;
			}
		}
		else
		{
			yield return artDefElementCopyData;
		}
	}

	private ArtDefCollectionAdapter GetTargetCollectionSelection()
	{
		ArtDefCollectionAdapter result;
		if ((result = base.Selection.LastSelected.As<ArtDefCollectionAdapter>()) == null)
		{
			result = base.Selection.LastSelected.As<ArtDefElementAdapter>()?.DomNode?.Parent?.As<ArtDefCollectionAdapter>();
		}
		return result;
	}

	public bool CanInsert(object dataObject)
	{
		if (!(dataObject is IDataObject datObj))
		{
			return false;
		}
		ArtDefElementCopyData firstObject = GetFirstObject(datObj);
		if (firstObject == null)
		{
			return false;
		}
		ArtDefCollectionAdapter targetCollectionSelection = GetTargetCollectionSelection();
		if (targetCollectionSelection == null)
		{
			return false;
		}
		return targetCollectionSelection.ArtDefElementsTemplate == firstObject.ElementTemplate;
	}

	public void Insert(object dataObject)
	{
		ArtDefCollectionAdapter targetCollectionSelection = GetTargetCollectionSelection();
		if (targetCollectionSelection == null)
		{
			return;
		}
		IEnumerable<ArtDefElementCopyData> enumerable = new List<ArtDefElementCopyData> { dataObject as ArtDefElementCopyData };
		if (enumerable.First() == null)
		{
			enumerable = dataObject as List<ArtDefElementCopyData>;
		}
		if (enumerable == null && ((dataObject is IDataObject dataObject2) ? dataObject2.GetData(typeof(List<object>)) : null) is List<object> list)
		{
			enumerable = list.ConvertAll((object x) => (ArtDefElementCopyData)x);
		}
		if (enumerable == null)
		{
			if (!(dataObject is IDataObject datObj))
			{
				return;
			}
			enumerable = GetAllAdapters(datObj);
		}
		IArtDefCollection artDefCollection = targetCollectionSelection.ArtDefCollection;
		using IArtDef artDef = Context.Get<CivTechContext>().CreateInstance<IArtDef>(new object[1] { Doc.CivTechService.PrimaryProject.Config });
		IArtDefCollection artDefCollection2 = artDef.AddCollection(artDefCollection.CollectionName);
		Uri sourceUri = null;
		foreach (ArtDefElementCopyData item in enumerable)
		{
			string name = GenerateUniqueName(artDefCollection.Elements, item.ArtDefElementName);
			string artDefElementXML = item.ArtDefElementXML;
			IArtDefElement artDefElement = artDefCollection2.AddElement(name);
			if (artDefElement.DeserializeFromXML(artDefElementXML))
			{
				artDefElement.Name = name;
			}
			else
			{
				artDefCollection2.RemoveElement(artDefElement.Name);
			}
			sourceUri = item.SourceUri;
		}
		AddElementsFromNative(targetCollectionSelection, artDefCollection2, sourceUri);
	}

	private void AddElementsFromNative(ArtDefCollectionAdapter collectionAdapter, IArtDefCollection nativeCollection, Uri sourceUri)
	{
		foreach (IArtDefElement element in nativeCollection.Elements)
		{
			if (string.IsNullOrWhiteSpace(element.Name))
			{
				string text = BuildPasteErrorMessage(collectionAdapter, sourceUri);
				BugSubmitter.SilentAssert(!string.IsNullOrWhiteSpace(element.Name), text + " @assign dgurley @summary Source data does not meet current criteria.");
				continue;
			}
			ArtDefElementAdapter artDefElementAdapter = collectionAdapter.AddElement(element.Name, -1);
			foreach (IValue item in element.Fields.Items)
			{
				artDefElementAdapter.FindField(item.ParameterName).CopyValue(item);
			}
			foreach (IArtDefCollection childCollection in element.Children)
			{
				ArtDefCollectionAdapter artDefCollectionAdapter = artDefElementAdapter.Collections.FirstOrDefault((ArtDefCollectionAdapter child) => child.Name == childCollection.CollectionName);
				if (artDefCollectionAdapter == null)
				{
					IArtDefElementTemplate artDefElementTemplate = artDefElementAdapter.ArtDefElementTmpl.Children.FirstOrDefault((IArtDefElementTemplate template) => template.Name == childCollection.CollectionName);
					if (artDefElementTemplate != null)
					{
						artDefCollectionAdapter = artDefElementAdapter.AddCollection(childCollection.CollectionName, artDefElementTemplate);
					}
				}
				if (artDefCollectionAdapter != null)
				{
					AddElementsFromNative(artDefCollectionAdapter, childCollection, sourceUri);
				}
			}
		}
	}

	private string BuildPasteErrorMessage(ArtDefCollectionAdapter adapter, Uri sourceUri)
	{
		StringBuilder stringBuilder = new StringBuilder(adapter.Name);
		DomNode parent = adapter.DomNode.Parent;
		while (parent != null && !parent.Is<ArtDefSetAdapter>())
		{
			string text = " ---> ";
			ArtDefElementAdapter artDefElementAdapter = parent.As<ArtDefElementAdapter>();
			if (artDefElementAdapter != null)
			{
				text = artDefElementAdapter.Name + text;
			}
			else
			{
				ArtDefCollectionAdapter artDefCollectionAdapter = parent.As<ArtDefCollectionAdapter>();
				if (artDefCollectionAdapter != null)
				{
					text = artDefCollectionAdapter.Name + text;
				}
			}
			stringBuilder.Insert(0, text);
			parent = parent.Parent;
		}
		stringBuilder.Insert(0, string.Format("An element from the source {0} has an empty name.{1}{1}", sourceUri.LocalPath, Environment.NewLine));
		return stringBuilder.ToString();
	}

	public bool CanDelete()
	{
		if (base.Selection.LastSelected != null)
		{
			return base.Selection.LastSelected.Is<ArtDefElementAdapter>();
		}
		return false;
	}

	public void Delete()
	{
		ArtDefElementAdapter artDefElementAdapter = base.Selection.LastSelected.As<ArtDefElementAdapter>();
		artDefElementAdapter.DomNode.Parent.As<ArtDefCollectionAdapter>().RemoveElement(artDefElementAdapter.Name);
	}

	public void FixupBLPReferences()
	{
		ArtDefSetAdapter artDefSetAdapter = base.DomNode.As<ArtDefSetAdapter>();
		IArtDef artDef = artDefSetAdapter.ArtDef;
		this.DoTransaction(delegate
		{
			artDef.VisitAllValues(FixupBLPValueVisitor);
		}, "Fixup BLP References");
	}

	private void FixupBLPValueVisitor(IValue value)
	{
		IXLPRegistry xLPRegistry = CivTechRegistry.XLPRegistry;
		Action<string> outputDelegate = delegate(string message)
		{
			Outputs.WriteLine(OutputMessageType.Info, message);
		};
		switch (value.ParameterType)
		{
		case Firaxis.CivTech.AssetObjects.ValueType.VT_BLP_ENTRY:
		{
			IBLPEntryValue blpEntryValue2 = (IBLPEntryValue)value;
			xLPRegistry.FixupBLPReference(blpEntryValue2, outputDelegate);
			break;
		}
		case Firaxis.CivTech.AssetObjects.ValueType.VT_COLLECTION:
		{
			ICollectionValue collectionValue = (ICollectionValue)value;
			if (collectionValue.EntryValueType != Firaxis.CivTech.AssetObjects.ValueType.VT_BLP_ENTRY)
			{
				break;
			}
			{
				foreach (IBLPEntryValue item in collectionValue.Items)
				{
					xLPRegistry.FixupBLPReference(item, outputDelegate);
				}
				break;
			}
		}
		}
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			base.DomNode.ChildInserted -= DomNode_ChildInserted;
			base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
			base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
			base.DomNode.As<ArtDefSetAdapter>().TemplateChanged -= ArtDefContext_TemplateChanged;
			base.History.Clear();
			m_document = null;
			m_controlInfo = null;
			GUI?.Dispose();
			GUI = null;
		}
	}
}
