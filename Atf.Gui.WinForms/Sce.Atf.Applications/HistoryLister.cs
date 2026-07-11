using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(HistoryLister))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HistoryLister : IInitializable, IDisposable
{
	private class RetrieveCommandListItemEventArgs : EventArgs
	{
		public readonly int ItemIndex;

		public CommandListItem Item;

		public RetrieveCommandListItemEventArgs(int index)
		{
			ItemIndex = index;
		}
	}

	private class DrawCommandListItemEventArgs : EventArgs
	{
		public readonly Graphics Graphics;

		public readonly CommandListItem Item;

		public DrawCommandListItemEventArgs(Graphics graphics, CommandListItem item)
		{
			Graphics = graphics;
			Item = item;
		}
	}

	private class CommandListItem
	{
		public readonly string Text;

		public Rectangle Bounds { get; private set; }

		public int Index { get; private set; }

		public CommandListItem(string text)
		{
			Text = text;
		}

		public void SetBounds(Rectangle bounds)
		{
			Bounds = bounds;
		}

		public void SetIndex(int index)
		{
			Index = index;
		}
	}

	private class CommandList : Control
	{
		public EventHandler<DrawCommandListItemEventArgs> DrawItem;

		public EventHandler<RetrieveCommandListItemEventArgs> RetrieveCommandListItem;

		private VScrollBar m_vScrollBar;

		private int m_virtualListSize;

		public int VirtualListSize
		{
			get
			{
				return m_virtualListSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (value != m_virtualListSize)
				{
					m_virtualListSize = value;
					UpdateScrollBar();
					Invalidate();
				}
			}
		}

		public CommandList()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			m_vScrollBar = new VScrollBar();
			m_vScrollBar.Dock = DockStyle.Right;
			base.Controls.Add(m_vScrollBar);
			base.SizeChanged += delegate
			{
				UpdateScrollBar();
			};
			m_vScrollBar.ValueChanged += delegate
			{
				Invalidate();
			};
		}

		public CommandListItem GetItemAt(int x, int y)
		{
			if (y < 0 || y >= base.ClientSize.Height || VirtualListSize == 0)
			{
				return null;
			}
			int topIndex = GetTopIndex();
			int itemWidth = GetItemWidth();
			int itemHeight = GetItemHeight();
			int num = y / itemHeight;
			int num2 = topIndex + num;
			if (num2 >= VirtualListSize)
			{
				return null;
			}
			RetrieveCommandListItemEventArgs e = new RetrieveCommandListItemEventArgs(num2);
			OnRetrieveCommandListItem(e);
			e.Item.SetIndex(num2);
			e.Item.SetBounds(new Rectangle(0, num * itemHeight, itemWidth, itemHeight));
			return e.Item;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (VirtualListSize != 0)
			{
				int itemWidth = GetItemWidth();
				int itemHeight = GetItemHeight();
				int num = ((base.ClientSize.Height % itemHeight == 0) ? (base.ClientSize.Height / itemHeight) : (base.ClientSize.Height / itemHeight + 1));
				int topIndex = GetTopIndex();
				int num2 = Math.Min(topIndex + num, VirtualListSize);
				Rectangle bounds = new Rectangle(0, 0, itemWidth, itemHeight);
				for (int i = topIndex; i < num2; i++)
				{
					RetrieveCommandListItemEventArgs e2 = new RetrieveCommandListItemEventArgs(i);
					OnRetrieveCommandListItem(e2);
					e2.Item.SetIndex(i);
					e2.Item.SetBounds(bounds);
					DrawCommandListItemEventArgs e3 = new DrawCommandListItemEventArgs(e.Graphics, e2.Item);
					OnDrawItem(e3);
					bounds.Y += itemHeight;
				}
			}
		}

		private int GetItemHeight()
		{
			return (int)Font.GetHeight() + 2;
		}

		private void OnRetrieveCommandListItem(RetrieveCommandListItemEventArgs e)
		{
			RetrieveCommandListItem.Raise(this, e);
		}

		private void OnDrawItem(DrawCommandListItemEventArgs e)
		{
			DrawItem.Raise(this, e);
		}

		private int GetTopIndex()
		{
			return m_vScrollBar.Visible ? m_vScrollBar.Value : 0;
		}

		private int GetItemWidth()
		{
			return m_vScrollBar.Visible ? (base.ClientSize.Width - m_vScrollBar.Width - 1) : (base.ClientSize.Width - 1);
		}

		private void UpdateScrollBar()
		{
			int itemHeight = GetItemHeight();
			m_vScrollBar.Minimum = 0;
			m_vScrollBar.Maximum = Math.Max(VirtualListSize - 1, 0);
			int num = base.ClientSize.Height / itemHeight;
			if (num < 0)
			{
				num = 1;
			}
			m_vScrollBar.LargeChange = num;
			m_vScrollBar.Visible = m_vScrollBar.Maximum > 0 && m_vScrollBar.LargeChange <= m_vScrollBar.Maximum;
			if (!m_vScrollBar.Visible)
			{
				m_vScrollBar.Value = 0;
			}
			else
			{
				m_vScrollBar.Value = Math.Min(m_vScrollBar.Value, m_vScrollBar.Maximum - m_vScrollBar.LargeChange);
			}
		}
	}

	private const int VK_SHIFT = 16;

	private const int VK_CONTROL = 17;

	private const int VK_MENU = 18;

	private Form m_historyDetails;

	private TreeView m_operationTree;

	private int m_selectedDetailIndex = 0;

	[Import(AllowDefault = false)]
	private IDocumentRegistry m_documentRegistry;

	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService;

	private CommandHistory m_commandHistory;

	private HistoryContext m_historyContext;

	private CommandList m_commandListbox;

	private SolidBrush m_fillBrush = new SolidBrush(Color.White);

	private SolidBrush m_textBrush = new SolidBrush(Color.White);

	private Color m_redoForeColor;

	private Color m_redoBackColor;

	private Color m_undoForeColor;

	private Color m_undoBackColor;

	private static bool CtrlPressed => GetAsyncKeyState(17) < 0;

	private static bool ShiftPressed => GetAsyncKeyState(16) < 0;

	private static bool AltPressed => GetAsyncKeyState(18) < 0;

	[DllImport("User32.dll")]
	private static extern short GetAsyncKeyState(int keyCode);

	private void ShowHistoryDetails()
	{
		if (m_historyDetails == null || !m_historyDetails.IsHandleCreated)
		{
			m_selectedDetailIndex = 0;
			m_historyDetails = new Form();
			m_operationTree = new TreeView();
			m_operationTree.Dock = DockStyle.Fill;
			m_historyDetails.Controls.Add(m_operationTree);
		}
		m_historyDetails.Show(m_commandListbox);
	}

	private void HideHistoryDetails()
	{
		m_historyDetails.Hide();
	}

	private bool AreHistoryDetailsVisible()
	{
		return m_historyDetails?.Visible ?? false;
	}

	private int GetActiveHistoryDetailIndex()
	{
		return m_selectedDetailIndex;
	}

	private void SetActiveHistoryDetailIndex(int idx)
	{
		if (m_selectedDetailIndex == idx)
		{
			return;
		}
		m_operationTree.BeginUpdate();
		m_operationTree.Nodes.Clear();
		m_selectedDetailIndex = idx;
		Command command = m_commandHistory[idx - 1];
		if (command is TransactionCommandBase transactionCommandBase)
		{
			transactionCommandBase.Operations.ForEach(delegate(TransactionContext.Operation op)
			{
				m_operationTree.Nodes.Add(op.ToString());
			});
		}
		else
		{
			m_operationTree.Nodes.Add(command.Description);
		}
		m_operationTree.EndUpdate();
	}

	void IInitializable.Initialize()
	{
		m_commandListbox = new CommandList();
		m_commandListbox.MouseDown += delegate(object sender, MouseEventArgs e)
		{
			CommandListItem itemAt = m_commandListbox.GetItemAt(e.X, e.Y);
			if (itemAt != null)
			{
				if (CtrlPressed && ShiftPressed && AltPressed)
				{
					if (!AreHistoryDetailsVisible())
					{
						ShowHistoryDetails();
						SetActiveHistoryDetailIndex(itemAt.Index);
					}
					else
					{
						int activeHistoryDetailIndex = GetActiveHistoryDetailIndex();
						if (activeHistoryDetailIndex == itemAt.Index)
						{
							HideHistoryDetails();
						}
						else
						{
							SetActiveHistoryDetailIndex(itemAt.Index);
						}
					}
				}
				else
				{
					int num = m_commandHistory.Current - 1;
					int num2 = itemAt.Index - 1;
					if (num2 <= num)
					{
						while (num2 < num)
						{
							m_historyContext.Undo();
							num = m_commandHistory.Current - 1;
						}
					}
					else
					{
						while (num2 >= m_commandHistory.Current)
						{
							m_historyContext.Redo();
						}
					}
				}
			}
		};
		CommandList commandListbox = m_commandListbox;
		commandListbox.RetrieveCommandListItem = (EventHandler<RetrieveCommandListItemEventArgs>)Delegate.Combine(commandListbox.RetrieveCommandListItem, (EventHandler<RetrieveCommandListItemEventArgs>)delegate(object sender, RetrieveCommandListItemEventArgs e)
		{
			e.Item = ((e.ItemIndex == 0) ? new CommandListItem("< Clean State >".Localize()) : new CommandListItem(m_commandHistory[e.ItemIndex - 1].Description));
		});
		CommandList commandListbox2 = m_commandListbox;
		commandListbox2.DrawItem = (EventHandler<DrawCommandListItemEventArgs>)Delegate.Combine(commandListbox2.DrawItem, (EventHandler<DrawCommandListItemEventArgs>)delegate(object sender, DrawCommandListItemEventArgs e)
		{
			if (e.Item.Index >= 0)
			{
				int num = e.Item.Index - 1;
				Rectangle bounds = e.Item.Bounds;
				CommandListItem item = e.Item;
				if (num >= m_commandHistory.Current)
				{
					m_textBrush.Color = m_redoForeColor;
					m_fillBrush.Color = m_redoBackColor;
				}
				else
				{
					m_textBrush.Color = m_undoForeColor;
					m_fillBrush.Color = m_undoBackColor;
				}
				e.Graphics.FillRectangle(m_fillBrush, bounds);
				e.Graphics.DrawString(item.Text, m_commandListbox.Font, m_textBrush, bounds, StringFormat.GenericDefault);
			}
		});
		ControlInfo controlInfo = new ControlInfo("History", "Undo/Redo stack", StandardControlGroup.Right);
		m_controlHostService.RegisterControl(m_commandListbox, controlInfo, null);
		m_documentRegistry.ActiveDocumentChanged += m_documentRegistry_ActiveDocumentChanged;
		m_commandListbox.BackColorChanged += delegate
		{
			ComputeColors();
		};
		m_commandListbox.ForeColorChanged += delegate
		{
			ComputeColors();
		};
		ComputeColors();
	}

	private void m_documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		if (m_commandHistory != null)
		{
			m_commandHistory.CommandDone -= m_commandHistory_CommandDone;
			m_commandHistory.CommandUndone -= m_commandHistory_CommandUndone;
			m_commandHistory.Cleared -= m_commandHistory_Cleared;
		}
		m_historyContext = m_documentRegistry.GetActiveDocument<HistoryContext>();
		m_commandHistory = ((m_historyContext == null) ? null : m_historyContext.History);
		if (m_commandHistory != null)
		{
			m_commandHistory.CommandDone += m_commandHistory_CommandDone;
			m_commandHistory.CommandUndone += m_commandHistory_CommandUndone;
			m_commandHistory.Cleared += m_commandHistory_Cleared;
		}
		if (m_commandHistory != null && m_commandHistory.Count > 0)
		{
			m_commandListbox.VirtualListSize = m_commandHistory.Count + 1;
		}
		else
		{
			m_commandListbox.VirtualListSize = 0;
		}
	}

	private void m_commandHistory_Cleared(object sender, EventArgs e)
	{
		m_commandListbox.VirtualListSize = 0;
		m_commandListbox.Invalidate();
	}

	private void m_commandHistory_CommandUndone(object sender, EventArgs e)
	{
		m_commandListbox.Invalidate();
	}

	private void m_commandHistory_CommandDone(object sender, EventArgs e)
	{
		m_commandListbox.VirtualListSize = m_commandHistory.Count + 1;
		m_commandListbox.Invalidate();
	}

	private void ComputeColors()
	{
		m_undoForeColor = m_commandListbox.ForeColor;
		m_undoBackColor = m_commandListbox.BackColor;
		m_redoForeColor = ((m_undoForeColor.GetBrightness() > 0.5f) ? ControlPaint.Dark(m_undoForeColor, 0.3f) : ControlPaint.Light(m_undoForeColor, 0.3f));
		m_redoBackColor = ((m_undoBackColor.GetBrightness() > 0.5f) ? ControlPaint.Dark(m_undoBackColor, 0.15f) : ControlPaint.Light(m_undoBackColor, 0.15f));
	}

	public void Dispose()
	{
		m_fillBrush?.Dispose();
		m_textBrush?.Dispose();
	}
}
