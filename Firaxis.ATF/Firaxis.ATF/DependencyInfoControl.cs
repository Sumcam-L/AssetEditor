using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class DependencyInfoControl : UserControl
{
	private readonly ICivTechService m_civTechService;

	private readonly IDocumentService m_DocumentService;

	private readonly ICommandService m_commandService;

	private DependencyFileCommands m_DependencyFileCommands;

	private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

	private IContainer components;

	private SplitContainer splitter;

	public TreeView dependantTree;

	public TreeView dependencyTree;

	private Label label2;

	private Label label1;

	public DependencyInfoControl(ICivTechService civTechSvc, IDocumentService docSvc, ICommandService comSvc, IEnumerable<Lazy<IDocumentClient>> docClients)
	{
		InitializeComponent();
		m_civTechService = civTechSvc;
		m_DocumentService = docSvc;
		m_commandService = comSvc;
		m_documentClients = docClients;
		Initialize();
	}

	private void Initialize()
	{
		m_DependencyFileCommands = new DependencyFileCommands(m_commandService, m_DocumentService, m_documentClients, this);
	}

	public void SetActiveDocument(IDocument doc)
	{
		dependantTree.BeginUpdate();
		dependencyTree.BeginUpdate();
		ClearDependentTree();
		ClearDependencyTree();
		if (doc != null)
		{
			FillDependentTreeTwoDeep(doc.Uri, dependantTree.Nodes);
			FillDependencyTreeTwoDeep(doc.Uri, dependencyTree.Nodes);
		}
		dependantTree.EndUpdate();
		dependencyTree.EndUpdate();
	}

	private void ClearDependencyTree()
	{
		dependencyTree.Nodes.Clear();
	}

	private void ClearDependentTree()
	{
		dependantTree.Nodes.Clear();
	}

	private void dependantTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
	{
		foreach (TreeNode node in e.Node.Nodes)
		{
			Uri uri = (Uri)node.Tag;
			if (node.Nodes.Count == 0)
			{
				FillDependentTreeTwoDeep(uri, node.Nodes);
			}
		}
	}

	private void dependencyTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
	{
		foreach (TreeNode node in e.Node.Nodes)
		{
			Uri uri = (Uri)node.Tag;
			if (node.Nodes.Count == 0)
			{
				FillDependencyTreeTwoDeep(uri, node.Nodes);
			}
		}
	}

	private void FillDependencyTreeTwoDeep(Uri uri, TreeNodeCollection nodes)
	{
		foreach (Uri dependency in m_civTechService.GetWorkspaceDependencyRegistry(uri).GetDependencies(uri))
		{
			TreeNode treeNode = nodes.Add(dependency.LocalPath);
			treeNode.Tag = dependency;
			foreach (Uri dependency2 in m_civTechService.GetWorkspaceDependencyRegistry(dependency).GetDependencies(dependency))
			{
				treeNode.Nodes.Add(dependency2.LocalPath).Tag = dependency2;
			}
		}
	}

	private void FillDependentTreeTwoDeep(Uri uri, TreeNodeCollection nodes)
	{
		foreach (Uri dependent in m_civTechService.GetWorkspaceDependencyRegistry(uri).GetDependents(uri))
		{
			TreeNode treeNode = nodes.Add(dependent.LocalPath);
			treeNode.Tag = dependent;
			foreach (Uri dependent2 in m_civTechService.GetWorkspaceDependencyRegistry(dependent).GetDependents(dependent))
			{
				treeNode.Nodes.Add(dependent2.LocalPath).Tag = dependent2;
			}
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
		this.splitter = new System.Windows.Forms.SplitContainer();
		this.label2 = new System.Windows.Forms.Label();
		this.dependantTree = new System.Windows.Forms.TreeView();
		this.label1 = new System.Windows.Forms.Label();
		this.dependencyTree = new System.Windows.Forms.TreeView();
		((System.ComponentModel.ISupportInitialize)this.splitter).BeginInit();
		this.splitter.Panel1.SuspendLayout();
		this.splitter.Panel2.SuspendLayout();
		this.splitter.SuspendLayout();
		base.SuspendLayout();
		this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitter.Location = new System.Drawing.Point(0, 0);
		this.splitter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.splitter.Name = "splitter";
		this.splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitter.Panel1.Controls.Add(this.label2);
		this.splitter.Panel1.Controls.Add(this.dependantTree);
		this.splitter.Panel2.Controls.Add(this.label1);
		this.splitter.Panel2.Controls.Add(this.dependencyTree);
		this.splitter.Size = new System.Drawing.Size(842, 649);
		this.splitter.SplitterDistance = 327;
		this.splitter.SplitterWidth = 6;
		this.splitter.TabIndex = 0;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 6);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(97, 20);
		this.label2.TabIndex = 1;
		this.label2.Text = "Dependents";
		this.dependantTree.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.dependantTree.Location = new System.Drawing.Point(4, 31);
		this.dependantTree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.dependantTree.Name = "dependantTree";
		this.dependantTree.Size = new System.Drawing.Size(830, 289);
		this.dependantTree.TabIndex = 0;
		this.dependantTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(dependantTree_BeforeExpand);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(4, 6);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(112, 20);
		this.label1.TabIndex = 1;
		this.label1.Text = "Dependencies";
		this.dependencyTree.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.dependencyTree.Location = new System.Drawing.Point(4, 31);
		this.dependencyTree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.dependencyTree.Name = "dependencyTree";
		this.dependencyTree.Size = new System.Drawing.Size(830, 277);
		this.dependencyTree.TabIndex = 0;
		this.dependencyTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(dependencyTree_BeforeExpand);
		base.AutoScaleDimensions = new System.Drawing.SizeF(9f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitter);
		base.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		base.Name = "DependencyInfoControl";
		base.Size = new System.Drawing.Size(842, 649);
		this.splitter.Panel1.ResumeLayout(false);
		this.splitter.Panel1.PerformLayout();
		this.splitter.Panel2.ResumeLayout(false);
		this.splitter.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitter).EndInit();
		this.splitter.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
