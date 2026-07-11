using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls.Properties;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class TriStateCheckboxTreeView : TreeView
{
	public enum CheckState
	{
		Unchecked,
		Checked,
		GreyChecked
	}

	private IContainer components;

	[Browsable(true)]
	public bool LockParentChildState { get; set; }

	[Browsable(false)]
	public new bool CheckBoxes
	{
		get
		{
			return base.CheckBoxes;
		}
		set
		{
			base.CheckBoxes = value;
		}
	}

	[Browsable(false)]
	public new int ImageIndex
	{
		get
		{
			return base.ImageIndex;
		}
		set
		{
			base.ImageIndex = value;
		}
	}

	[Browsable(false)]
	public new ImageList ImageList
	{
		get
		{
			return base.ImageList;
		}
		set
		{
			base.ImageList = value;
		}
	}

	[Browsable(false)]
	public new int SelectedImageIndex
	{
		get
		{
			return base.SelectedImageIndex;
		}
		set
		{
			base.SelectedImageIndex = value;
		}
	}

	public TriStateCheckboxTreeView()
	{
		InitializeComponent();
		ImageList = new ImageList();
		ImageList.ImageSize = new Size(16, 16);
		ImageList.Images.Add(Firaxis.Controls.Properties.Resources.unchecked_img);
		ImageList.Images.Add(Firaxis.Controls.Properties.Resources.checked_img);
		ImageList.Images.Add(Firaxis.Controls.Properties.Resources.partial_checked_img);
		ImageIndex = 0;
		SelectedImageIndex = 0;
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		NativeMethods.TV_HITTESTINFO lParam = new NativeMethods.TV_HITTESTINFO
		{
			pt = PointToClient(Control.MousePosition)
		};
		NativeMethods.SendMessageForHitTest(base.Handle, 4369, 0, ref lParam);
		if ((lParam.flags & NativeMethods.TVHT.ONITEMICON) == NativeMethods.TVHT.ONITEMICON)
		{
			TreeNode nodeAt = GetNodeAt(lParam.pt);
			if (nodeAt != null)
			{
				ChangeNodeState(nodeAt, TreeViewAction.ByMouse);
			}
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Space)
		{
			ChangeNodeState(base.SelectedNode, TreeViewAction.ByKeyboard);
		}
	}

	private void CheckNode(TreeNode node, CheckState state, TreeViewAction actionSource)
	{
		if (LockParentChildState)
		{
			InternalSetChecked(node, state, actionSource);
			{
				foreach (TreeNode node4 in node.Nodes)
				{
					CheckNode(node4, state, actionSource);
				}
				return;
			}
		}
		CheckState checkState = state;
		if (state == CheckState.Checked)
		{
			foreach (TreeNode node5 in node.Nodes)
			{
				if (GetChecked(node5) != CheckState.Checked)
				{
					checkState = CheckState.GreyChecked;
					break;
				}
			}
		}
		InternalSetChecked(node, checkState, actionSource);
	}

	private void ChangeParent(TreeNode node, TreeViewAction actionSource)
	{
		if (node == null)
		{
			return;
		}
		CheckState checkState = GetChecked(node.FirstNode);
		if (!LockParentChildState)
		{
			if (checkState != CheckState.Unchecked)
			{
				foreach (TreeNode node4 in node.Nodes)
				{
					if (GetChecked(node4) == CheckState.Unchecked)
					{
						checkState = CheckState.GreyChecked;
						break;
					}
				}
			}
		}
		else
		{
			foreach (TreeNode node5 in node.Nodes)
			{
				checkState &= GetChecked(node5);
			}
		}
		if (InternalSetChecked(node, checkState, actionSource))
		{
			ChangeParent(node.Parent, actionSource);
		}
	}

	protected void ChangeNodeState(TreeNode node, TreeViewAction actionSource)
	{
		BeginUpdate();
		CheckState checkState = ((node.ImageIndex == 0 || node.ImageIndex < 0) ? CheckState.Checked : CheckState.Unchecked);
		CheckNode(node, checkState, actionSource);
		ChangeParent(node.Parent, actionSource);
		EndUpdate();
	}

	private bool InternalSetChecked(TreeNode node, CheckState state, TreeViewAction updateAction)
	{
		TreeViewCancelEventArgs e = new TreeViewCancelEventArgs(node, cancel: false, updateAction);
		OnBeforeCheck(e);
		if (e.Cancel)
		{
			return false;
		}
		node.ImageIndex = (int)state;
		node.SelectedImageIndex = (int)state;
		OnAfterCheck(new TreeViewEventArgs(node, updateAction));
		return true;
	}

	public CheckState GetChecked(TreeNode node)
	{
		if (node.ImageIndex < 0)
		{
			return CheckState.Unchecked;
		}
		return (CheckState)node.ImageIndex;
	}

	public void SetChecked(TreeNode node, CheckState state)
	{
		if (InternalSetChecked(node, state, TreeViewAction.Unknown))
		{
			CheckNode(node, state, TreeViewAction.Unknown);
			ChangeParent(node.Parent, TreeViewAction.Unknown);
		}
	}
}
