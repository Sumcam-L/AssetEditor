using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Reflection;
using Sce.Atf;

namespace Firaxis.AssetPreviewing;

public class PreviewerCacheControl : UserControl
{
	private IContainer components = null;

	private ListView lvCachedEntities;

	private TreeView tvEntityXML;

	private SplitContainer splCachedEntities;

	private ColumnHeader colType;

	private ColumnHeader colName;

	private ColumnHeader colURI;

	public ListView CachedEntities => lvCachedEntities;

	public TreeView EntityXML => tvEntityXML;

	private IPreviewerCacheService PreviewerCacheService { get; set; }

	public PreviewerCacheControl(IPreviewerCacheService pcs)
	{
		PreviewerCacheService = pcs;
		InitializeComponent();
	}

	public void AddCachedAsset(InstanceType insType, string insName, Uri uri)
	{
		ListViewItem listViewItem = CachedEntities.Items.Add(uri.LocalPath, ReflectionHelper.GetDisplayName(insType), string.Empty);
		listViewItem.SubItems.Add(insName);
		listViewItem.SubItems.Add(uri.LocalPath);
		listViewItem.Tag = uri.LocalPath;
	}

	public void RemoveCachedAsset(Uri uri)
	{
		CachedEntities.Items.RemoveByKey(uri.LocalPath);
	}

	private void lvCachedEntities_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		if (lvCachedEntities.SelectedIndices.Count > 0)
		{
			ListViewItem listViewItem = lvCachedEntities.Items[lvCachedEntities.SelectedIndices[0]];
			string key = (string)listViewItem.Tag;
			string cachedXML = PreviewerCacheService.GetCachedXML(key);
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(cachedXML);
				tvEntityXML.BeginUpdate();
				tvEntityXML.Nodes.Clear();
				tvEntityXML.Nodes.Add(new TreeNode(xmlDocument.DocumentElement.Name));
				TreeNode treeNode = new TreeNode();
				treeNode = tvEntityXML.Nodes[0];
				AddNode(xmlDocument.DocumentElement, treeNode);
				tvEntityXML.EndUpdate();
				tvEntityXML.ExpandAll();
				return;
			}
			catch (XmlException ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
				return;
			}
			catch (System.Exception ex2)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex2.Message);
				return;
			}
		}
		tvEntityXML.Nodes.Clear();
	}

	private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
	{
		if (inXmlNode.HasChildNodes)
		{
			XmlNodeList childNodes = inXmlNode.ChildNodes;
			for (int i = 0; i <= childNodes.Count - 1; i++)
			{
				XmlNode xmlNode = inXmlNode.ChildNodes[i];
				inTreeNode.Nodes.Add(new TreeNode(xmlNode.Name));
				TreeNode inTreeNode2 = inTreeNode.Nodes[i];
				AddNode(xmlNode, inTreeNode2);
			}
		}
		else
		{
			inTreeNode.Text = inXmlNode.OuterXml.Trim();
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
		this.lvCachedEntities = new System.Windows.Forms.ListView();
		this.colType = new System.Windows.Forms.ColumnHeader();
		this.colName = new System.Windows.Forms.ColumnHeader();
		this.colURI = new System.Windows.Forms.ColumnHeader();
		this.tvEntityXML = new System.Windows.Forms.TreeView();
		this.splCachedEntities = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)this.splCachedEntities).BeginInit();
		this.splCachedEntities.Panel1.SuspendLayout();
		this.splCachedEntities.Panel2.SuspendLayout();
		this.splCachedEntities.SuspendLayout();
		base.SuspendLayout();
		this.lvCachedEntities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.colType, this.colName, this.colURI });
		this.lvCachedEntities.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lvCachedEntities.FullRowSelect = true;
		this.lvCachedEntities.HideSelection = false;
		this.lvCachedEntities.Location = new System.Drawing.Point(0, 0);
		this.lvCachedEntities.MultiSelect = false;
		this.lvCachedEntities.Name = "lvCachedEntities";
		this.lvCachedEntities.ShowItemToolTips = true;
		this.lvCachedEntities.Size = new System.Drawing.Size(421, 184);
		this.lvCachedEntities.TabIndex = 0;
		this.lvCachedEntities.UseCompatibleStateImageBehavior = false;
		this.lvCachedEntities.View = System.Windows.Forms.View.Details;
		this.lvCachedEntities.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(lvCachedEntities_ItemSelectionChanged);
		this.colType.Text = "Type";
		this.colType.Width = 81;
		this.colName.Text = "Name";
		this.colName.Width = 176;
		this.colURI.Text = "URI";
		this.colURI.Width = 400;
		this.tvEntityXML.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tvEntityXML.Location = new System.Drawing.Point(0, 0);
		this.tvEntityXML.Name = "tvEntityXML";
		this.tvEntityXML.Size = new System.Drawing.Size(421, 180);
		this.tvEntityXML.TabIndex = 1;
		this.splCachedEntities.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splCachedEntities.Location = new System.Drawing.Point(0, 0);
		this.splCachedEntities.Name = "splCachedEntities";
		this.splCachedEntities.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splCachedEntities.Panel1.Controls.Add(this.lvCachedEntities);
		this.splCachedEntities.Panel2.Controls.Add(this.tvEntityXML);
		this.splCachedEntities.Size = new System.Drawing.Size(421, 368);
		this.splCachedEntities.SplitterDistance = 184;
		this.splCachedEntities.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splCachedEntities);
		base.Name = "PreviewerCacheControl";
		base.Size = new System.Drawing.Size(421, 368);
		this.splCachedEntities.Panel1.ResumeLayout(false);
		this.splCachedEntities.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splCachedEntities).EndInit();
		this.splCachedEntities.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
