using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class TreeListControl : TreeControl
{
	public class NodeEditEventArgs : EventArgs
	{
		public readonly Node Node;

		public readonly DataEditor EditedData;

		public NodeEditEventArgs(Node node, DataEditor editedData)
		{
			Node = node;
			EditedData = editedData;
		}
	}

	private Node m_dataEditNode;

	private readonly TextBox m_editTextBox;

	private DataEditor m_editData;

	private readonly TreeListView.ColumnCollection m_columns;

	private Pen m_seperatorPen;

	private Point m_firstPoint;

	private Point m_currentPoint;

	private int m_currentColumn;

	private int[] m_oldColumnWidths;

	private int m_treeWidth = 200;

	private bool m_columnResizing;

	private bool m_autoResizeColumns = true;

	public TreeListView.ColumnCollection Columns => m_columns;

	public bool AutoResizeColumns
	{
		get
		{
			return m_autoResizeColumns;
		}
		set
		{
			m_autoResizeColumns = value;
		}
	}

	public int TreeWidth
	{
		get
		{
			return m_treeWidth;
		}
		set
		{
			m_treeWidth = value;
		}
	}

	private TreeListItemRenderer TreeListItemRenderer => base.ItemRenderer as TreeListItemRenderer;

	public event EventHandler<NodeEditEventArgs> NodeDataEdited;

	public TreeListControl()
		: this(Style.Tree, null)
	{
	}

	public TreeListControl(Style style)
		: this(style, null)
	{
	}

	public TreeListControl(Style style, TreeListItemRenderer itemRenderer)
		: base(style, itemRenderer)
	{
		m_columns = new TreeListView.ColumnCollection();
		SuspendLayout();
		m_editTextBox = new TextBox();
		m_editTextBox.Visible = false;
		m_editTextBox.BorderStyle = BorderStyle.None;
		m_editTextBox.KeyDown += TextBoxKeyDown;
		m_editTextBox.KeyPress += TextBoxKeyPress;
		m_editTextBox.LostFocus += TextBoxLostFocus;
		base.ContentVerticalOffset = base.FontHeight + 2;
		base.Controls.Add(m_editTextBox);
		m_seperatorPen = new Pen(Color.DarkGray, 1f);
		ResumeLayout();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		int num = TreeWidth;
		if (Columns.Count > 0)
		{
			for (int i = 0; i < Columns.Count; i++)
			{
				TreeListView.Column column = Columns[i];
				Rectangle rectangle = new Rectangle(num + 3, base.Margin.Top, column.ActualWidth, base.ContentVerticalOffset);
				if (i == Columns.Count - 1)
				{
					rectangle.Width = base.ActualClientSize.Width - num;
				}
				e.Graphics.DrawString(column.Label, Font, TreeListItemRenderer.TextBrush, rectangle);
				e.Graphics.DrawLine(m_seperatorPen, num, base.Margin.Top, num, base.ContentVerticalOffset);
				num += column.ActualWidth;
			}
			e.Graphics.DrawLine(m_seperatorPen, 0, base.ContentVerticalOffset + 2, base.ActualClientSize.Width, base.ContentVerticalOffset + 2);
		}
		Region clip = e.Graphics.Clip;
		Rectangle clipRectangle = e.ClipRectangle;
		clipRectangle.Width = TreeWidth;
		e.Graphics.SetClip(clipRectangle);
		base.OnPaint(e);
		e.Graphics.Clip = clip;
	}

	protected override Rectangle GetLabelEditBounds(NodeLayoutInfo info)
	{
		Rectangle labelEditBounds = base.GetLabelEditBounds(info);
		labelEditBounds.Width = TreeWidth - labelEditBounds.Left;
		return labelEditBounds;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && m_seperatorPen != null)
		{
			m_seperatorPen.Dispose();
			m_seperatorPen = null;
		}
	}

	private void TextBoxLostFocus(object sender, EventArgs e)
	{
		EndDataEdit();
	}

	private void TextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			EndDataEdit();
			e.Handled = true;
			m_handleKeyUp = true;
		}
		else if (e.KeyCode == Keys.Escape)
		{
			AbortDataEdit();
			e.Handled = true;
			m_handleKeyUp = true;
		}
	}

	private void TextBoxKeyPress(object sender, KeyPressEventArgs e)
	{
		e.Handled = m_dataEditNode == null;
	}

	private void AbortDataEdit()
	{
		m_editData = null;
		m_dataEditNode = null;
		m_editTextBox.Hide();
	}

	protected virtual void OnNodeDataEdited(NodeEditEventArgs e)
	{
		this.NodeDataEdited.Raise(this, e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		m_columnResizing = false;
		Cursor = Cursors.Default;
		if (e.Button == MouseButtons.Left)
		{
			if (m_editData != null && m_editData.EditingMode == DataEditor.EditMode.BySlider)
			{
				EndDataEdit();
				return;
			}
			Point p = new Point(e.X, e.Y);
			HitRecord hitRecord = Pick(p);
			if (hitRecord.Node != null)
			{
				DataEditor dataEditor = GetDataEditor(hitRecord.Node, p);
				if (dataEditor != null && !dataEditor.ReadOnly && dataEditor.EditingMode == DataEditor.EditMode.ByTextBox)
				{
					BeginDataEdit(hitRecord.Node, dataEditor);
					return;
				}
			}
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		EndDataEdit();
		if (e.Button == MouseButtons.Left)
		{
			m_firstPoint = (m_currentPoint = new Point(e.X, e.Y));
			m_currentColumn = HitColumnSeperator(e.X, e.Y);
			if (m_currentColumn >= 0)
			{
				AutoResizeColumns = false;
				m_columnResizing = true;
				m_oldColumnWidths = new int[Columns.Count];
				for (int i = 0; i < Columns.Count; i++)
				{
					m_oldColumnWidths[i] = Columns[i].ActualWidth;
				}
				ResizeColumn();
			}
			else
			{
				Point p = new Point(e.X, e.Y);
				HitRecord hitRecord = Pick(p);
				if (hitRecord.Node != null)
				{
					DataEditor dataEditor = GetDataEditor(hitRecord.Node, p);
					if (dataEditor != null && !dataEditor.ReadOnly)
					{
						TreeListItemRenderer.TrackingEditor = dataEditor;
						if (dataEditor.EditingMode == DataEditor.EditMode.BySlider)
						{
							BeginDataEdit(hitRecord.Node, dataEditor);
						}
						else if (dataEditor.EditingMode == DataEditor.EditMode.ByExternalControl)
						{
							dataEditor.FinishDataEdit = EndDataEdit;
							BeginDataEdit(hitRecord.Node, dataEditor);
						}
						dataEditor.OnMouseDown(e);
						Invalidate();
						return;
					}
				}
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		m_currentPoint = new Point(e.X, e.Y);
		if (HitColumnSeperator(e.X, e.Y) >= 0)
		{
			Cursor = Cursors.SizeWE;
		}
		else if (!m_columnResizing)
		{
			Cursor = Cursors.Default;
		}
		if (e.Button == MouseButtons.Left)
		{
			if (m_columnResizing)
			{
				ResizeColumn();
			}
			else if (TreeListItemRenderer.TrackingEditor != null && TreeListItemRenderer.TrackingEditor.WantsMouseTracking())
			{
				Point clientPoint = new Point(e.X, e.Y);
				Node nodeAt = GetNodeAt(clientPoint);
				if (nodeAt != null)
				{
					TreeListItemRenderer.TrackingEditor.OnMouseMove(e);
					Invalidate();
				}
			}
		}
		base.OnMouseMove(e);
	}

	private int HitColumnSeperator(int x, int y)
	{
		if (y < base.ContentVerticalOffset && Columns.Count > 0)
		{
			int num = TreeWidth;
			for (int i = 0; i < Columns.Count; i++)
			{
				TreeListView.Column column = Columns[i];
				if (x <= num + 2 && x >= num - 2)
				{
					return i;
				}
				num += column.ActualWidth;
			}
		}
		return -1;
	}

	private void ResizeColumn()
	{
		if (m_currentPoint.X > base.ActualClientSize.Width || m_currentPoint.X < base.Margin.Left || m_currentColumn < 0)
		{
			return;
		}
		int num = m_currentPoint.X - m_firstPoint.X;
		if (m_currentColumn == 0)
		{
			m_treeWidth = m_currentPoint.X;
		}
		else
		{
			int num2 = m_oldColumnWidths[m_currentColumn] - num;
			if (num2 >= 8)
			{
				int num3 = m_oldColumnWidths[m_currentColumn - 1] + num;
				if (num3 < 8)
				{
					num2 += num3;
					if (num2 < 8)
					{
						return;
					}
					num3 = 8;
				}
				Columns[m_currentColumn - 1].ActualWidth = num3;
				Columns[m_currentColumn].ActualWidth = num2;
			}
		}
		Invalidate();
	}

	internal DataEditor GetDataEditor(Node node, Point p)
	{
		TreeListControl treeListControl = node.TreeControl as TreeListControl;
		ItemInfo itemInfo = new WinFormsItemInfo();
		treeListControl.TreeListItemRenderer.ItemView.GetInfo(node.Tag, itemInfo);
		if (itemInfo.Properties.Length != treeListControl.Columns.Count)
		{
			return null;
		}
		int num = treeListControl.TreeWidth;
		for (int i = 0; i < treeListControl.Columns.Count; i++)
		{
			TreeListView.Column column = treeListControl.Columns[i];
			if (p.X >= num && p.X <= num + column.ActualWidth)
			{
				DataEditor dataEditor = itemInfo.Properties[i] as DataEditor;
				if (dataEditor != null)
				{
					foreach (NodeLayoutInfo item in base.NodeLayout)
					{
						if (item.Node == node)
						{
							dataEditor.TextBox = m_editTextBox;
							dataEditor.Bounds = GetEditArea(item, dataEditor);
							dataEditor.SetEditingMode(p);
							break;
						}
					}
				}
				return dataEditor;
			}
			num += column.ActualWidth;
		}
		return null;
	}

	private void BeginDataEdit(Node node, DataEditor editData)
	{
		EndDataEdit();
		if (editData.ReadOnly)
		{
			return;
		}
		m_editData = editData;
		m_dataEditNode = node;
		foreach (NodeLayoutInfo item in base.NodeLayout)
		{
			if (item.Node == m_dataEditNode)
			{
				m_editData.TextBox = m_editTextBox;
				m_editData.Bounds = GetEditArea(item, m_editData);
				m_editData.BeginDataEdit();
				Invalidate();
				break;
			}
		}
	}

	private void EndDataEdit()
	{
		if (m_dataEditNode != null)
		{
			if (m_editData.EndDataEdit())
			{
				OnNodeDataEdited(new NodeEditEventArgs(m_dataEditNode, m_editData));
			}
			m_dataEditNode = null;
			m_editData = null;
			TreeListItemRenderer.TrackingEditor = null;
		}
		m_editTextBox.Hide();
	}

	private Rectangle GetEditArea(NodeLayoutInfo nodeLayoutInfo, DataEditor dataEditor)
	{
		int rowHeight = GetRowHeight(nodeLayoutInfo.Node);
		int num = TreeWidth;
		int num2 = -1;
		for (int i = 0; i < Columns.Count; i++)
		{
			if (Columns[i].Label == dataEditor.Name)
			{
				num2 = Columns[i].ActualWidth;
				break;
			}
			num += Columns[i].ActualWidth;
		}
		return new Rectangle(num, nodeLayoutInfo.Y, num2, rowHeight);
	}
}
