using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class ExploreFileControl : UserControl, IExploreFileTree
{
	private IContainer components = null;

	private SplitterControl splitterControl;

	private ExploreFileTree exploreFileTree;

	private ExploreFileList exploreFileList;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private ColumnHeader columnHeader3;

	public ExploreFileTree FileTree => exploreFileTree;

	public ExploreFileList FileList => exploreFileList;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string BaseDirectory
	{
		get
		{
			return FileTree.BaseDirectory;
		}
		set
		{
			FileTree.BaseDirectory = value;
		}
	}

	public ExploreFileControl()
	{
		InitializeComponent();
		FileTree.SelectedItemChanged += FileTree_SelectedItemChanged;
		FileList.DoubleClickedItem += FileList_DoubleClickedItem;
	}

	public void RebuildTree()
	{
		FileTree.RebuildTree();
	}

	private void FileTree_SelectedItemChanged(object sender, ScrollableTree.TreeNodeEventArgs e)
	{
		Update();
		FileList.PopulateList((FileTree.SelectedNodes.Count > 0) ? FileTree.SelectedNodes[0] : null);
	}

	private void FileList_DoubleClickedItem(object sender, ScrollableTree.TreeNodeEventArgs e)
	{
		if (e.Node.Item.Tag is ExploreFileTree.PathContainer)
		{
			FileTree.ExpandSelectNode(e.Node);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.Controls.ExploreFileControl));
		this.splitterControl = new Firaxis.Controls.SplitterControl();
		this.exploreFileTree = new Firaxis.Controls.ExploreFileTree();
		this.exploreFileList = new Firaxis.Controls.ExploreFileList();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		this.splitterControl.Panel1.SuspendLayout();
		this.splitterControl.Panel2.SuspendLayout();
		this.splitterControl.SuspendLayout();
		base.SuspendLayout();
		this.splitterControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitterControl.Location = new System.Drawing.Point(0, 0);
		this.splitterControl.Name = "splitterControl";
		this.splitterControl.Panel1.Controls.Add(this.exploreFileTree);
		this.splitterControl.Panel2.Controls.Add(this.exploreFileList);
		this.splitterControl.Size = new System.Drawing.Size(493, 178);
		this.splitterControl.SplitterDistance = 172;
		this.splitterControl.TabIndex = 0;
		this.exploreFileTree.BackColor = System.Drawing.Color.White;
		this.exploreFileTree.Dock = System.Windows.Forms.DockStyle.Fill;
		this.exploreFileTree.Location = new System.Drawing.Point(0, 0);
		this.exploreFileTree.Name = "exploreFileTree";
		this.exploreFileTree.ShowFiles = false;
		this.exploreFileTree.Size = new System.Drawing.Size(172, 178);
		this.exploreFileTree.TabIndex = 0;
		this.exploreFileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.columnHeader1, this.columnHeader2, this.columnHeader3 });
		this.exploreFileList.Dock = System.Windows.Forms.DockStyle.Fill;
		this.exploreFileList.FullRowSelect = true;
		this.exploreFileList.Location = new System.Drawing.Point(0, 0);
		this.exploreFileList.Name = "exploreFileList";
		this.exploreFileList.OwnerDraw = true;
		this.exploreFileList.Size = new System.Drawing.Size(317, 178);
		this.exploreFileList.TabIndex = 0;
		this.exploreFileList.UseCompatibleStateImageBehavior = false;
		this.exploreFileList.View = System.Windows.Forms.View.Details;
		this.columnHeader1.Text = "Name";
		this.columnHeader1.Width = 150;
		this.columnHeader2.Text = "Date";
		this.columnHeader2.Width = 90;
		this.columnHeader3.Text = "Size";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitterControl);
		base.Name = "ExploreFileControl";
		base.Size = new System.Drawing.Size(493, 178);
		this.splitterControl.Panel1.ResumeLayout(false);
		this.splitterControl.Panel2.ResumeLayout(false);
		this.splitterControl.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
