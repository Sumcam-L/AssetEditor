using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(ListViewEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public class ListViewEditor : IInitializable
{
	[Import(AllowDefault = true)]
	private IStatusService m_statusService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private readonly ICommandService m_commandService;

	private readonly ListView m_listView;

	private readonly ListViewAdapter m_listViewAdapter;

	private readonly Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();

	private Point m_mouseDownPoint;

	private bool m_dragging;

	public IListView ListView
	{
		get
		{
			return m_listViewAdapter.ListView;
		}
		set
		{
			m_listViewAdapter.ListView = value;
		}
	}

	public ListView Control => m_listView;

	public ListViewAdapter ListViewAdapter => m_listViewAdapter;

	public object LastHit => m_listViewAdapter.LastHit;

	internal string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Columns");
			xmlDocument.AppendChild(xmlElement);
			foreach (KeyValuePair<string, int> columnWidth in m_columnWidths)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Column");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("Name", columnWidth.Key);
				xmlElement2.SetAttribute("Width", columnWidth.Value.ToString());
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement == null || documentElement.Name != "Columns")
			{
				throw new Exception("Invalid GridModelControl settings");
			}
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Column");
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("Name");
				if (!string.IsNullOrEmpty(attribute))
				{
					string attribute2 = item.GetAttribute("Width");
					if (attribute2 != null && int.TryParse(attribute2, out var result))
					{
						m_columnWidths[attribute] = result;
					}
				}
			}
			Control.SuspendLayout();
			foreach (ColumnHeader column in Control.Columns)
			{
				string text = column.Text;
				if (!string.IsNullOrEmpty(text))
				{
					if (m_columnWidths.TryGetValue(text, out var value2))
					{
						column.Width = value2;
					}
					else
					{
						m_columnWidths[text] = column.Width;
					}
				}
			}
			Control.ResumeLayout();
		}
	}

	protected ICommandService CommandService => m_commandService;

	protected IStatusService StatusService => m_statusService;

	protected IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders => m_contextMenuCommandProviders.GetValues();

	public event EventHandler LastHitChanged;

	[ImportingConstructor]
	public ListViewEditor(ICommandService commandService)
	{
		m_commandService = commandService;
		Configure(out m_listView, out m_listViewAdapter);
		m_listView.MouseDown += listView_MouseDown;
		m_listView.MouseMove += listView_MouseMove;
		m_listView.MouseUp += listView_MouseUp;
		m_listView.MouseLeave += listView_MouseLeave;
		m_listView.DragOver += listView_DragOver;
		m_listView.DragDrop += listView_DragDrop;
		m_listView.AfterLabelEdit += listView_AfterLabelEdit;
		m_listView.ColumnWidthChanged += listView_ColumnWidthChanged;
		m_listViewAdapter.LastHitChanged += listViewAdapter_LastHitChanged;
	}

	protected virtual void Configure(out ListView listView, out ListViewAdapter listViewAdapter)
	{
		listView = new ListView();
		listView.SmallImageList = ResourceUtil.GetImageList16();
		listView.LargeImageList = ResourceUtil.GetImageList32();
		listViewAdapter = new ListViewAdapter(listView);
	}

	public virtual void Initialize()
	{
		if (m_settingsService != null && Control != null && !string.IsNullOrEmpty(Control.Name))
		{
			m_settingsService.RegisterSettings(Control.Name, new BoundPropertyDescriptor(this, () => Settings, "Settings", "", ""));
		}
	}

	protected virtual void OnLastHitChanged(EventArgs e)
	{
		this.LastHitChanged.Raise(this, e);
	}

	protected virtual void OnStartDrag(IEnumerable<object> items)
	{
		m_listView.DoDragDrop(items, DragDropEffects.All);
	}

	protected virtual void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			IEnumerable<object> commands = ContextMenuCommandProviders.GetCommands(ListView, m_listViewAdapter.LastHit);
			Point screenPoint = m_listView.PointToScreen(new Point(e.X, e.Y));
			m_commandService.RunContextMenu(commands, screenPoint);
		}
	}

	protected virtual void OnDragOver(DragEventArgs e)
	{
		if (ApplicationUtil.CanInsert(m_listViewAdapter.ListView, null, e.Data))
		{
			e.Effect = DragDropEffects.Move;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	protected virtual void OnDragDrop(DragEventArgs e)
	{
		ApplicationUtil.Insert(m_listViewAdapter.ListView, null, e.Data, "Drag and Drop".Localize(), m_statusService);
	}

	protected virtual void OnNodeLabelEdited(LabelEditEventArgs e)
	{
		IListView listView = m_listViewAdapter.ListView;
		ListViewItem item = m_listView.Items[e.Item];
		INamingContext namingContext = listView.As<INamingContext>();
		if (namingContext != null && namingContext.CanSetName(item.Tag))
		{
			ITransactionContext context = listView.As<ITransactionContext>();
			context.DoTransaction(delegate
			{
				namingContext.SetName(item.Tag, e.Label);
			}, "Edit Label".Localize());
		}
	}

	protected virtual void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
	{
		ColumnHeader columnHeader = Control.Columns[e.ColumnIndex];
		string text = columnHeader.Text;
		if (!string.IsNullOrEmpty(text))
		{
			m_columnWidths[text] = columnHeader.Width;
		}
	}

	private void listViewAdapter_LastHitChanged(object sender, EventArgs e)
	{
		OnLastHitChanged(EventArgs.Empty);
	}

	private void listView_MouseDown(object sender, MouseEventArgs e)
	{
		m_mouseDownPoint = new Point(e.X, e.Y);
	}

	private void listView_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && !m_dragging && m_listView.AllowDrop)
		{
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(e.X - m_mouseDownPoint.X) > dragSize.Width || Math.Abs(e.Y - m_mouseDownPoint.Y) > dragSize.Height)
			{
				m_dragging = true;
			}
		}
		if (!m_dragging)
		{
			return;
		}
		object[] array = null;
		if ((System.Windows.Forms.Control.ModifierKeys & Keys.Alt) != Keys.None)
		{
			object itemAt = m_listViewAdapter.GetItemAt(new Point(e.X, e.Y));
			if (itemAt != null)
			{
				array = new object[1] { itemAt };
			}
		}
		else
		{
			array = m_listViewAdapter.GetSelectedItems();
		}
		if (array != null)
		{
			OnStartDrag(array);
		}
	}

	private void listView_MouseUp(object sender, MouseEventArgs e)
	{
		m_dragging = false;
		OnMouseUp(e);
	}

	private void listView_MouseLeave(object sender, EventArgs e)
	{
		m_dragging = false;
	}

	private void listView_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void listView_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		OnNodeLabelEdited(e);
	}

	private void listView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		OnColumnWidthChanged(e);
	}
}
