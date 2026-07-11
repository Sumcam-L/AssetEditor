using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace Sce.Atf.Dom;

[Export(typeof(IInitializable))]
[Export(typeof(DomRecorder))]
[PartCreationPolicy(CreationPolicy.Any)]
public class DomRecorder : TreeListViewEditor, IControlHostClient, IInitializable
{
	private enum DataType
	{
		Begin,
		Cancel,
		End,
		ChildAdded,
		ChildRemoved,
		AttributeChanged
	}

	private class DataItem
	{
		public readonly int TransactionNum;

		public readonly string Analysis;

		public EventArgs DomArgs;

		private List<DataItem> m_children;

		private DataType m_transactionType;

		private string m_transactionName;

		public string Description
		{
			get
			{
				switch (m_transactionType)
				{
				case DataType.Begin:
					return $"Transaction began : [{m_transactionName}]";
				case DataType.Cancel:
					return $"Transaction cancelled : [{m_transactionName}]";
				case DataType.End:
					return $"Transaction ended : [{m_transactionName}]";
				case DataType.ChildAdded:
				{
					ChildEventArgs e3 = (ChildEventArgs)DomArgs;
					return string.Format("[{0}; id {1}]: add to [{2}; id {3}] at index {4}", e3.Child, e3.Child.GetId() ?? e3.Child.GetHashCode().ToString("X"), e3.Parent, e3.Parent.GetId() ?? e3.Parent.GetHashCode().ToString("X"), e3.Index);
				}
				case DataType.ChildRemoved:
				{
					ChildEventArgs e2 = (ChildEventArgs)DomArgs;
					return string.Format("[{0}; id {1}]: remove from [{2} id {3}] at index {4}", e2.Child, e2.Child.GetId() ?? e2.Child.GetHashCode().ToString("X"), e2.Parent, e2.Parent.GetId() ?? e2.Parent.GetHashCode().ToString("X"), e2.Index);
				}
				case DataType.AttributeChanged:
				{
					AttributeEventArgs e = (AttributeEventArgs)DomArgs;
					return string.Format("[{0}; id {1}]: set [{2}] from [{3}] to [{4}]", e.DomNode, e.DomNode.GetId() ?? e.DomNode.GetHashCode().ToString("X"), e.AttributeInfo.Name, e.OldValue, e.NewValue);
				}
				default:
					throw new InvalidOperationException("unknown enum value");
				}
			}
		}

		public string ReportLine
		{
			get
			{
				if (string.IsNullOrEmpty(Analysis))
				{
					return $"#{TransactionNum}: {Description}";
				}
				return $"#{TransactionNum}: {Description}. {Analysis}";
			}
		}

		public bool HasChildren => m_children != null && m_children.Count > 0;

		public IEnumerable<DataItem> Children
		{
			get
			{
				IEnumerable<DataItem> children = m_children;
				return children ?? EmptyArray<DataItem>.Instance;
			}
		}

		public DataItem(int transactionNum, DataType transactionType, string transactionName)
		{
			TransactionNum = transactionNum;
			m_transactionType = transactionType;
			m_transactionName = transactionName;
			Analysis = string.Empty;
		}

		public DataItem(int transactionNum, DataType transactionType, EventArgs domArgs, string analysis)
		{
			TransactionNum = transactionNum;
			m_transactionType = transactionType;
			DomArgs = domArgs;
			Analysis = analysis;
		}
	}

	private class DataContainer : ITreeListView, IItemView, IObservableContext
	{
		private readonly List<DataItem> m_data = new List<DataItem>();

		private int m_transactionNum;

		private DataItem m_transactionParent;

		private static int s_dataImageIndex = -1;

		private static int s_folderImageIndex = -1;

		private static readonly string[] s_columnNames = new string[3]
		{
			"Trans.#".Localize("transaction number"),
			"Description".Localize(),
			"Analysis".Localize()
		};

		public DataItem this[int index] => m_data[index];

		public int Count => m_data.Count;

		public IEnumerable<object> Roots => m_data.AsIEnumerable<object>();

		public string[] ColumnNames => s_columnNames;

		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
		{
			add
			{
			}
			remove
			{
			}
		}

		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public event EventHandler Reloaded;

		public DataContainer()
		{
			if (s_dataImageIndex == -1)
			{
				s_dataImageIndex = ResourceUtil.GetImageList16().Images.IndexOfKey(Resources.DataImage);
			}
			if (s_folderImageIndex == -1)
			{
				s_folderImageIndex = ResourceUtil.GetImageList16().Images.IndexOfKey(Resources.FolderImage);
			}
		}

		public void AddTransactionBegin(string transactionName)
		{
			m_transactionNum++;
			DataItem item = (m_transactionParent = new DataItem(m_transactionNum, DataType.Begin, transactionName));
			m_data.Add(item);
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item));
		}

		public void AddTransactionEnd(string transactionName)
		{
			DataItem item = new DataItem(m_transactionNum, DataType.End, transactionName);
			m_data.Add(item);
			m_transactionParent = null;
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item));
		}

		public void AddTransactionCancel(string transactionName)
		{
			DataItem item = new DataItem(m_transactionNum, DataType.Cancel, transactionName);
			m_data.Add(item);
			m_transactionParent = null;
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item));
		}

		public void AddDomEvent(DataType dataType, EventArgs e, string analysis)
		{
			int transactionNum = ((m_transactionParent != null) ? m_transactionNum : (-1));
			DataItem item = new DataItem(transactionNum, dataType, e, analysis);
			m_data.Add(item);
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item));
		}

		public void Clear()
		{
			m_data.Clear();
			this.Reloaded.Raise(this, EventArgs.Empty);
		}

		public IEnumerable<object> GetChildren(object parent)
		{
			DataItem dataParent = parent.As<DataItem>();
			if (dataParent == null || !dataParent.HasChildren)
			{
				yield break;
			}
			foreach (DataItem child in dataParent.Children)
			{
				yield return child;
			}
		}

		public void GetInfo(object obj, ItemInfo info)
		{
			DataItem dataItem = obj.As<DataItem>();
			if (dataItem != null)
			{
				info.Label = dataItem.TransactionNum.ToString();
				info.Properties = new object[2] { dataItem.Description, dataItem.Analysis };
				info.AllowLabelEdit = false;
				info.IsLeaf = !dataItem.HasChildren;
				info.ImageIndex = (dataItem.HasChildren ? s_folderImageIndex : s_dataImageIndex);
			}
		}
	}

	private IContextRegistry m_contextRegistry;

	private IDocumentRegistry m_documentRegistry;

	private readonly UserControl m_uberControl;

	private readonly DataContainer m_data = new DataContainer();

	private bool m_analysisEnabled;

	private IValidationContext m_validationContext;

	public int NumLogEvents => m_data.Count;

	[Import(AllowDefault = true)]
	public IDocumentRegistry DocumentRegistry
	{
		get
		{
			return m_documentRegistry;
		}
		set
		{
			if (m_documentRegistry != null)
			{
				m_documentRegistry.DocumentRemoved -= m_documentRegistry_DocumentRemoved;
			}
			m_documentRegistry = value;
			if (m_documentRegistry != null)
			{
				m_documentRegistry.DocumentRemoved += m_documentRegistry_DocumentRemoved;
			}
		}
	}

	[Import(AllowDefault = true)]
	public IContextRegistry ContextRegistry
	{
		get
		{
			return m_contextRegistry;
		}
		set
		{
			if (m_contextRegistry != null)
			{
				m_contextRegistry.ActiveContextChanged -= m_contextRegistry_ActiveContextChanged;
			}
			m_contextRegistry = value;
			if (m_contextRegistry != null)
			{
				m_contextRegistry.ActiveContextChanged += m_contextRegistry_ActiveContextChanged;
			}
		}
	}

	public IValidationContext ValidationContext
	{
		get
		{
			return m_validationContext;
		}
		set
		{
			if (m_validationContext != null)
			{
				m_validationContext.Beginning -= m_validationContext_Beginning;
				m_validationContext.Cancelled -= m_validationContext_Cancelled;
				m_validationContext.Ended -= m_validationContext_Ended;
			}
			m_validationContext = value;
			if (m_validationContext != null)
			{
				m_validationContext.Beginning += m_validationContext_Beginning;
				m_validationContext.Cancelled += m_validationContext_Cancelled;
				m_validationContext.Ended += m_validationContext_Ended;
			}
		}
	}

	[ImportingConstructor]
	public DomRecorder(IControlHostService controlHostService)
		: base(TreeListView.Style.VirtualList)
	{
		m_uberControl = new UserControl
		{
			Dock = DockStyle.Fill
		};
		int num = 2;
		int y = 2;
		Button button = new Button();
		button.Text = "Copy All".Localize();
		button.Width = TextRenderer.MeasureText(button.Text, button.Font).Width + 20;
		button.Location = new Point(num, y);
		button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		button.Click += CopyBtnClick;
		m_uberControl.Controls.Add(button);
		num += button.Width + 2;
		button = new Button();
		button.Text = "Clear".Localize("Pressing this button clears the contents of a list");
		button.Width = TextRenderer.MeasureText(button.Text, button.Font).Width + 20;
		button.Location = new Point(num, y);
		button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		button.Click += ClearBtnClick;
		m_uberControl.Controls.Add(button);
		num += button.Width + 2;
		CheckBox checkBox = new CheckBox();
		checkBox.Text = "Deep Analysis".Localize("a checkbox that enables or disables a time-consuming analysis");
		checkBox.Location = new Point(num, y);
		checkBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		checkBox.Click += AnalysisClick;
		checkBox.Height = button.Height;
		m_uberControl.Controls.Add(checkBox);
		y = button.Height;
		base.View = m_data;
		base.TreeListView.Columns[0].Width = 60;
		base.TreeListView.Columns[1].Width = 600;
		base.TreeListView.Columns[2].Width = 400;
		base.TreeListViewAdapter.RetrieveVirtualItem += TreeListViewAdapter_RetrieveVirtualItem;
		base.TreeListView.Name = "DOM Recorder".Localize("DOM means Document Object Model");
		base.TreeListView.Control.Location = new Point(0, y + 2);
		base.TreeListView.Control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		base.TreeListView.Control.Width = m_uberControl.Width;
		base.TreeListView.Control.Height = m_uberControl.Height - y - 2;
		m_uberControl.Controls.Add(base.TreeListView);
		ControlInfo controlInfo = new ControlInfo(base.TreeListView.Name, "Records DOM events on the active context and displays them".Localize(), StandardControlGroup.Bottom, "https://github.com/SonyWWS/ATF/search?utf8=%E2%9C%93&q=DomRecorder+or+Recorder".Localize())
		{
			ControlVisibility = ControlInitialVisibility.AlwaysHidden,
			MenuText = "Tool Debug\\DOM Recorder",
			MenuGroupOverride = StandardCommandGroup.UILayout
		};
		controlHostService.RegisterControl(m_uberControl, controlInfo, this);
		DomNode.DiagnosticAttributeChanged += m_root_AttributeChanged;
		DomNode.DiagnosticChildInserted += m_root_ChildInserted;
		DomNode.DiagnosticChildRemoved += m_root_ChildRemoved;
	}

	public IEnumerable<string> GetLogEvents(int maxEvents)
	{
		int startIndex = ((maxEvents != -1) ? Math.Max(0, m_data.Count - maxEvents) : 0);
		for (int i = startIndex; i < m_data.Count; i++)
		{
			DataItem item = m_data[i];
			yield return item.ReportLine;
		}
	}

	public void Clear()
	{
		m_data.Clear();
	}

	public virtual void Initialize()
	{
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	private void m_validationContext_Beginning(object sender, EventArgs e)
	{
		m_data.AddTransactionBegin(GetCurrentTransactionName());
	}

	private void m_validationContext_Cancelled(object sender, EventArgs e)
	{
		m_data.AddTransactionCancel(GetCurrentTransactionName());
	}

	private void m_validationContext_Ended(object sender, EventArgs e)
	{
		m_data.AddTransactionEnd(GetCurrentTransactionName());
	}

	private void m_root_ChildRemoved(object sender, ChildEventArgs e)
	{
		string analysis = (m_analysisEnabled ? AnalyzeRemoved(e) : string.Empty);
		m_data.AddDomEvent(DataType.ChildRemoved, e, analysis);
	}

	private void m_root_ChildInserted(object sender, ChildEventArgs e)
	{
		string analysis = (m_analysisEnabled ? AnalyzeInserted(e) : string.Empty);
		m_data.AddDomEvent(DataType.ChildAdded, e, analysis);
	}

	private void m_root_AttributeChanged(object sender, AttributeEventArgs e)
	{
		string analysis = (m_analysisEnabled ? AnalyzeAttributeChanged(e) : string.Empty);
		m_data.AddDomEvent(DataType.AttributeChanged, e, analysis);
	}

	private string AnalyzeRemoved(ChildEventArgs e)
	{
		return AnalyzeListeners(e.Parent.GetChildRemovedHandlers());
	}

	private string AnalyzeInserted(ChildEventArgs e)
	{
		return AnalyzeListeners(e.Parent.GetChildInsertedHandlers());
	}

	private string AnalyzeAttributeChanged(AttributeEventArgs e)
	{
		return AnalyzeListeners(e.DomNode.GetAttributeChangedHandlers());
	}

	private string AnalyzeListeners<T>(IEnumerable<EventHandler<T>> eventHandlers) where T : EventArgs
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		MethodInfo methodInfo = null;
		Dictionary<MethodInfo, List<object>> dictionary = new Dictionary<MethodInfo, List<object>>();
		foreach (EventHandler<T> eventHandler in eventHandlers)
		{
			num++;
			if (dictionary.TryGetValue(eventHandler.Method, out var value))
			{
				if (value.Find((object o) => o == eventHandler.Target) != null)
				{
					methodInfo = eventHandler.Method;
				}
				else
				{
					value.Add(eventHandler.Target);
				}
			}
			else
			{
				dictionary.Add(eventHandler.Method, new List<object>(new object[1] { eventHandler.Target }));
			}
			if (eventHandler.Target is IHistoryContext)
			{
				num2++;
				if (eventHandler.Target is HistoryContext { Recording: not false })
				{
					num3++;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("# of listeners: {0}", num);
		if (methodInfo != null)
		{
			stringBuilder.AppendFormat(". DUPLICATE LISTENER: {0} on {1}", methodInfo.Name, methodInfo.DeclaringType);
		}
		if (num2 > 0)
		{
			stringBuilder.AppendFormat(". # of IHistoryContext: {0}", num2);
			stringBuilder.AppendFormat(". # recording: {0}", num3);
		}
		return stringBuilder.ToString();
	}

	private void m_contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		ValidationContext = m_contextRegistry.ActiveContext.As<IValidationContext>();
	}

	private void m_documentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		Clear();
	}

	private void TreeListViewAdapter_RetrieveVirtualItem(object sender, TreeListViewAdapter.RetrieveVirtualNodeAdapter e)
	{
		e.Item = m_data[e.ItemIndex];
	}

	private IEnumerable<DataItem> GetItemsForReport()
	{
		bool foundSelection = false;
		foreach (DataItem item in Selection)
		{
			foundSelection = true;
			yield return item;
		}
		if (foundSelection)
		{
			yield break;
		}
		foreach (DataItem root in m_data.Roots)
		{
			yield return root;
		}
	}

	private void CopyBtnClick(object sender, EventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (DataItem item in GetItemsForReport())
		{
			stringBuilder.AppendLine(item.ReportLine);
		}
		string text = stringBuilder.ToString();
		if (!string.IsNullOrEmpty(text))
		{
			Clipboard.SetText(text);
		}
	}

	private void ClearBtnClick(object sender, EventArgs e)
	{
		Clear();
	}

	private void AnalysisClick(object sender, EventArgs e)
	{
		m_analysisEnabled = ((CheckBox)sender).Checked;
	}

	private string GetCurrentTransactionName()
	{
		if (m_validationContext == null)
		{
			return string.Empty;
		}
		TransactionContext transactionContext = m_validationContext.As<TransactionContext>();
		if (transactionContext == null)
		{
			return string.Empty;
		}
		return transactionContext.TransactionName;
	}
}
